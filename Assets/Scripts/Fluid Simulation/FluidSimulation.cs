using System.Collections;
using System.Linq;
using UnityEngine;

public class FluidSimulation
{
	public VectorField vectorField { get; private set; }
	public Vector2Int gridSize { get; private set; }
	public float cellRadius { get; private set; }
	public float cellDiameter { get; private set; }
	
	public float gravitationalConstant { get; private set; }
	public float fluidDensity { get; private set; }
	public float overrelaxation { get; private set; }
	public int iterations { get; private set; }
	
	public FluidSimulation(VectorField _vectorField, float _gravitationalConstant, float _fluidDensity, float _overrelaxation, int _iterations) {
		vectorField = _vectorField;
		gridSize = vectorField.gridSize;
		cellRadius = vectorField.cellRadius;
		cellDiameter = vectorField.cellDiameter;
		
		gravitationalConstant = _gravitationalConstant;
		fluidDensity = _fluidDensity;
		overrelaxation = _overrelaxation;
		iterations = _iterations;
	}
	
	public void UpdateFluidSimulation() {
		IntegrateVelocityField();
		SolveIncompressibility();
		IntegrateVelocityAdvection();
	}
	
	private void IntegrateVelocityField() {
		foreach (Cell cell in vectorField.grid) {
			if (cell.CheckIfBorderCell() || cell.CheckIfImpassible()) continue;
			cell.velocity = Vector2.down * Time.deltaTime * gravitationalConstant;
		}
	}
	
	private void SolveIncompressibility() {
		foreach (Cell cell in vectorField.grid) {	
			if (cell.CheckIfBorderCell() || cell.CheckIfImpassible()) continue;
			
			// calculates the divergence for each non border/ non impassible cell
			cell.divergence = -0.5f * (
				(cell.eastCell.velocity.x - cell.westCell.velocity.x) / cellDiameter +
				(cell.northCell.velocity.y - cell.southCell.velocity.y) / cellDiameter
			) * overrelaxation;
		}
		
		// using the gaus sidel method to find a pressure value that converges to the true pressure value
		for (int i = 0; i < iterations; i++) {
			foreach (Cell cell in vectorField.grid) {	
				if (cell.CheckIfBorderCell() || cell.CheckIfImpassible()) continue;

				// calculating the pressure
				cell.pressure = (cell.divergence + 
					cell.eastCell.pressure + cell.westCell.pressure +
					cell.northCell.pressure + cell.southCell.pressure) / 4f;
			}
		}
		
		foreach (Cell cell in vectorField.grid) {	
			if (cell.CheckIfBorderCell() || cell.CheckIfImpassible()) continue;
			
			// calculating the pressure gradient
			Vector2 pressureGradient = new Vector2(
				(cell.eastCell.pressure - cell.westCell.pressure) * 0.5f / cellDiameter,
				(cell.northCell.pressure - cell.southCell.pressure) * 0.5f / cellDiameter
			);
			cell.velocity += pressureGradient; // applying the pressure gradient
			
			// clearing the divergence
			cell.westCell.velocity.x -= pressureGradient.x;
			cell.eastCell.velocity.x += pressureGradient.x;
			cell.southCell.velocity.y -= pressureGradient.y;
			cell.northCell.velocity.y += pressureGradient.y;
		}
	}
	
	private void IntegrateVelocityAdvection() {
		foreach (Cell cell in vectorField.grid) {
			if (cell.CheckIfBorderCell() || cell.CheckIfImpassible()) continue;

			// advecting the velocity using semi-Lagrangian method
			Vector3 previousPosition = cell.worldPosition - Time.deltaTime * cell.GetVector3Velocity();
			Cell previousCell = vectorField.GetCellFromWorldPosition(previousPosition);

			if (previousCell.CheckIfBorderCell() || previousCell.CheckIfImpassible()) continue;

			// interpolating the velocity using bilinear interpolation
			float xRatio = (previousPosition.x - previousCell.worldPosition.x) / cellDiameter;
			float yRatio = (previousPosition.y - previousCell.worldPosition.y) / cellDiameter;

			Vector2 topLeftVelocity = previousCell.northWestCell.velocity;
			Vector2 topRightVelocity = previousCell.northEastCell.velocity;
			Vector2 bottomLeftVelocity = previousCell.southWestCell.velocity;
			Vector2 bottomRightVelocity = previousCell.southEastCell.velocity;

			Vector2 topInterpolatedVelocity = Vector2.Lerp(topLeftVelocity, topRightVelocity, xRatio);
			Vector2 bottomInterpolatedVelocity = Vector2.Lerp(bottomLeftVelocity, bottomRightVelocity, xRatio);
			Vector2 interpolatedVelocity = Vector2.Lerp(topInterpolatedVelocity, bottomInterpolatedVelocity, yRatio);

			// setting the new velocity
			cell.velocity += interpolatedVelocity;
		}
	}
}