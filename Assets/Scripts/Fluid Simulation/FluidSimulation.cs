using System.Collections;
using System.Linq;
using UnityEngine;

public class FluidSimulation
{
	public FlowField flowField { get; private set; }
	public Vector2Int gridSize { get; private set; }
	public float cellRadius { get; private set; }
	public float cellDiameter { get; private set; }
	
	public float maxSpeed { get; private set; }
	public float damping { get; private set; }
	public int densityIterations { get; private set; }
	public Vector2 fluidDensity { get; private set; }
	
	public FluidSimulation(FlowField _flowField, float _maxSpeed, float _damping, int _densityIterations, Vector2 _fluidDensity) {
		flowField = _flowField;
		gridSize = flowField.gridSize;
		cellRadius = flowField.cellRadius;
		cellDiameter = flowField.cellDiameter;
		
		maxSpeed = _maxSpeed;
		damping = _damping;
		densityIterations = _densityIterations;
		fluidDensity = _fluidDensity;
	}
	
	public void UpdateFluidSimulation() {
		CalculateVelocityField();
        CalculateDensityField();
        UpdateVelocityField();
	}
	
	private void CalculateVelocityField() {
		foreach (Cell cell in flowField.grid)
		{
			// calculating the cell's velocity using Navier-Stokes equations to simulate a fluid like cell
			cell.densityGradient = CalculateDensityGradient(cell);
			Vector2 viscousForce = CalculateViscousForce(cell);
			Vector2 acceleration = (cell.densityGradient + viscousForce) / cell.density;
			
			// update the velocity
			cell.velocity += acceleration * Time.deltaTime;

			// clamp velocity
			if (cell.velocity.magnitude > maxSpeed) cell.velocity = cell.velocity.normalized * maxSpeed;

			// damping the velocity
			cell.velocity *= 1f - damping * Time.deltaTime;
		}
	}

	// Calculates the Pressure Gradient by calculating the sum of the density differences between the current cell & all of its neighbors
	private Vector2 CalculateDensityGradient(Cell cell) {
		float densityDiffSum = 0f;
		foreach (Cell neighborCell in cell.neighborCells) {
			float densityDiff = cell.density - neighborCell.density;
			densityDiffSum += densityDiff;
		}
		return -densityDiffSum * (1f / cell.neighborCells.Count) * cell.densityGradient;
	}

	// Calculates the Viscous Force by calculating the sum of the viscous forces between the current cell & all of its neighbors
	private Vector2 CalculateViscousForce(Cell cell) {
		Vector2 viscousForce = Vector2.zero;
		foreach (Cell neighborCell in cell.neighborCells) {
			Vector2 velocityDiff = neighborCell.velocity - cell.velocity;
			float dist = Vector3.Distance(cell.worldPosition, neighborCell.worldPosition);

			viscousForce += cell.viscosity * velocityDiff / dist;
		}
		return viscousForce;
	}
	
	private void CalculateDensityField() {
		// calculating the cell's divergence
		foreach (Cell cell in flowField.grid) {	
			// the cell is not a border cell
			if (!cell.CheckIfBorderCell())
			{
				cell.divergence = -0.5f * (
					(cell.eastCell.velocity.x - cell.westCell.velocity.x) / cellDiameter +
					(cell.northCell.velocity.y - cell.southCell.velocity.y) / cellDiameter
				);
			}
		}
		
		// calculating the cell's new density 
		for (int i = 0; i < densityIterations; i++) {
			foreach (Cell cell in flowField.grid) {	
				// the cell is not a border cell
				if (!cell.CheckIfBorderCell()) {
					cell.density = (cell.divergence + 
						cell.eastCell.density + cell.westCell.density +
						cell.northCell.density + cell.southCell.density) / 4f;
				}
			}
		}

		// updating the cell's velocity with the calculated density
		foreach (Cell cell in flowField.grid) {	
			// the cell is not a border cell
			if (!cell.CheckIfBorderCell()) {
				cell.velocity -= new Vector2(
					(cell.eastCell.density - cell.westCell.density) * 0.5f / cellDiameter,
					(cell.northCell.density - cell.southCell.density) * 0.5f / cellDiameter);
			}
		}
	}

	private void UpdateVelocityField() {
		foreach (Cell cell in flowField.grid) {	
			// the cell is not a border cell
			if (!cell.CheckIfBorderCell()) {
				// defining velocity divergence based of the velocity diff
				float velocityDivergence = ((cell.eastCell.velocity.x - cell.westCell.velocity.x) +
					(cell.northCell.velocity.y - cell.southCell.velocity.y)) / (2f * cellDiameter);
				
				// updating the cell's velocity according to the calculated velocity divergence
				cell.velocity += (velocityDivergence * cellRadius * fluidDensity * Time.deltaTime);
			}
		}
	}
}