using System.Collections;
using System.Linq;
using UnityEngine;

public class FluidSimulation
{
	public VectorField vectorField { get; private set; }
	public Vector2Int gridSize { get; private set; }
	public float cellRadius { get; private set; }
	public float cellDiameter { get; private set; }
	
	public float fluidDensity { get; private set; }
	public int iterations { get; private set; }
	
	public FluidSimulation(VectorField _vectorField, float _fluidDensity, int _iterations) {
		vectorField = _vectorField;
		gridSize = vectorField.gridSize;
		cellRadius = vectorField.cellRadius;
		cellDiameter = vectorField.cellDiameter;
		
		fluidDensity = _fluidDensity;
		iterations = _iterations;
	}
	
	public void UpdateFluidSimulation() {
		ApplyNavierStokesEquations();
		IntegrateVelocityField();
		SolveIncompressibility();
		IntegrateVelocityAdvection();
	}
	
	private void ApplyNavierStokesEquations() {
		foreach (Cell cell in vectorField.grid) {
			Vector2 acceleration = (PressureGradient(cell) + CalculateViscousForce(cell)) / fluidDensity;

			// update the velocity
			cell.velocity += acceleration * Time.deltaTime;

			// clamp velocity
			if (cell.velocity.magnitude > 5) cell.velocity = cell.velocity.normalized * 5;

			// damping the velocity
			cell.velocity *= 1f - .2f * Time.deltaTime;
		}
	}
	
	// Calculates the Pressure Gradient by calculating the sum of the pressure differences between the current cell & all of its neighbors
	private Vector2 PressureGradient(Cell cell) {
		float pressureDiffSum = 0f;
		foreach (Cell neighborCell in cell.neighborCells) {
			pressureDiffSum += (cell.pressure - neighborCell.pressure);
		}
		Vector2 projectionVector = Vector2.down * fluidDensity;
		return -pressureDiffSum * (1f / cell.neighborCells.Count) * projectionVector;
	}
	
	// Calculates the Viscous Force by calculating the sum of the viscous forces between the current cell & all of its neighbors
	private Vector2 CalculateViscousForce(Cell cell) {
		Vector2 viscousForce = Vector2.zero;
		foreach (Cell neighborCell in cell.neighborCells) {
			Vector2 velocityDiff = neighborCell.velocity - cell.velocity;
			float dist = Vector3.Distance(cell.worldPosition, neighborCell.worldPosition);
			viscousForce += cell.pressure * velocityDiff / dist;
		}
		return viscousForce;
	}
	
	private void IntegrateVelocityField() {
		foreach (Cell cell in vectorField.grid) {
			if (cell.CheckIfBorderCell() || cell.CheckIfImpassible()) continue;
			
			// clearing divergence
			float velocityDivergence = ((cell.eastCell.velocity.x - cell.westCell.velocity.x) +
					                   (cell.northCell.velocity.y - cell.southCell.velocity.y)) /
									   (2f * cellDiameter);
			Vector2 projectionVector = Vector2.down * fluidDensity;
			cell.velocity += (velocityDivergence * cellRadius * projectionVector * Time.deltaTime);
		}
	}
	
	private void SolveIncompressibility() {	

		foreach (Cell cell in vectorField.grid) {	
			if (cell.CheckIfBorderCell() || cell.CheckIfImpassible()) continue;
			
			cell.divergence = -0.5f * (
				(cell.eastCell.velocity.x - cell.westCell.velocity.x) / cellDiameter +
				(cell.northCell.velocity.y - cell.southCell.velocity.y) / cellDiameter
			);
		}
	
		// using the Gauss-Seidel method to find a pressure value that converges to the true pressure value
		for (int i = 0; i < iterations; i++) {
			foreach (Cell cell in vectorField.grid) {	
				if (cell.CheckIfBorderCell() || cell.CheckIfImpassible()) continue;
				
				cell.pressure = ((cell.divergence + 
					cell.eastCell.pressure + cell.westCell.pressure +
					cell.northCell.pressure + cell.southCell.pressure) / 4f
				);
			}
		}
		
		foreach (Cell cell in vectorField.grid) {	
			if (cell.CheckIfBorderCell() || cell.CheckIfImpassible()) continue;
			
			// velocity correction according to pressure
			cell.velocity -= new Vector2(
				(cell.eastCell.pressure - cell.westCell.pressure) * 0.5f / cellDiameter,
				(cell.northCell.pressure - cell.southCell.pressure) * 0.5f / cellDiameter
			);
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