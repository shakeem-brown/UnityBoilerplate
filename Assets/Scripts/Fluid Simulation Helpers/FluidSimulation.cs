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
	public int pressureIterations { get; private set; }
	public Vector2 fluidDensity { get; private set; }
	
	public FluidSimulation(FlowField _flowField, float _maxSpeed, float _damping, int _pressureIterations, Vector2 _fluidDensity) {
		flowField = _flowField;
		gridSize = flowField.gridSize;
		cellRadius = flowField.cellRadius;
		cellDiameter = flowField.cellDiameter;
		
		maxSpeed = _maxSpeed;
		damping = _damping;
		pressureIterations = _pressureIterations;
		fluidDensity = _fluidDensity;
	}
	
	public void UpdateFluidSimulation() {
		CalculateVelocityField();
        CalculatePressureField();
        UpdateVelocityField();
	}
	
	private void CalculateVelocityField() {
		foreach (Cell cell in flowField.grid)
		{
			// calculating the cell's velocity using Navier-Stokes equations to simulate a fluid like cell
			Vector2 pressureGradient = CalculatePressureGradient(cell);
			Vector2 viscousForce = CalculateViscousForce(cell);
			Vector2 acceleration = (pressureGradient + viscousForce) / cell.density;
			
			// update the velocity
			cell.velocity += acceleration * Time.deltaTime;

			// clamp velocity
			if (cell.velocity.magnitude > maxSpeed) cell.velocity = cell.velocity.normalized * maxSpeed;

			// damping the velocity
			cell.velocity *= 1f - damping * Time.deltaTime;
		}
	}

	// Calculates the Pressure Gradient by calculating the sum of the pressure differences between the current cell & all of its neighbors
	private Vector2 CalculatePressureGradient(Cell cell) {
		float pressureDiffSum = 0f;
		foreach (Cell neighborCell in cell.neighborCells) {
			float pressureDiff = cell.pressure - neighborCell.pressure;
			pressureDiffSum += pressureDiff;
		}
		return -pressureDiffSum * (1f / cell.neighborCells.Count) * cell.pressureGradient;
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
	
	private void CalculatePressureField()
	{
		// initialize the pressure & divergence
		float[,] pressure = new float[gridSize.x, gridSize.y];
		float[,] divergence = new float[gridSize.x, gridSize.y];
		
		// calculating the cell's divergence
		foreach (Cell cell in flowField.grid)
		{	
			// the cell is not a border cell
			if (!cell.CheckIfBorderCell())
			{
				divergence[cell.gridIndex.x, cell.gridIndex.y] = -0.5f * (
					(cell.eastCell.velocity.x - cell.westCell.velocity.x) / cellDiameter +
					(cell.northCell.velocity.y - cell.southCell.velocity.y) / cellDiameter
				);
			}
		}
		
		// calculating the cell's pressure
		for (int i = 0; i < pressureIterations; i++)
		{
			foreach (Cell cell in flowField.grid)
			{	
				// the cell is not a border cell
				if (!cell.CheckIfBorderCell())
				{
					pressure[cell.gridIndex.x, cell.gridIndex.y] = (
						divergence[cell.gridIndex.x, cell.gridIndex.y] +
						pressure[cell.eastCell.gridIndex.x, cell.eastCell.gridIndex.y] +
						pressure[cell.westCell.gridIndex.x, cell.westCell.gridIndex.y] +
						pressure[cell.northCell.gridIndex.x, cell.northCell.gridIndex.y] +
						pressure[cell.southCell.gridIndex.x, cell.southCell.gridIndex.y]
					) / 4f;
				}
			}
		}

		// updating the cell's velocity with the calculated pressure
		foreach (Cell cell in flowField.grid)
		{	
			// the cell is not a border cell
			if (!cell.CheckIfBorderCell())
			{
				cell.velocity -= new Vector2(
					(pressure[cell.eastCell.gridIndex.x, cell.eastCell.gridIndex.y] -
					 pressure[cell.westCell.gridIndex.x, cell.westCell.gridIndex.y]) * 0.5f / cellDiameter,
					(pressure[cell.northCell.gridIndex.x, cell.northCell.gridIndex.y] -
					 pressure[cell.southCell.gridIndex.x, cell.southCell.gridIndex.y]) * 0.5f / cellDiameter
				);
			}
		}
	}

	private void UpdateVelocityField()
	{
		// calculating the cell's velocity divergence
		float[,] velocityDivergence = new float[gridSize.x, gridSize.y];
		foreach (Cell cell in flowField.grid)
		{	
			// the cell is not a border cell
			if (!cell.CheckIfBorderCell())
			{
				velocityDivergence[cell.gridIndex.x, cell.gridIndex.y] = (
					cell.eastCell.velocity.x - cell.westCell.velocity.x +
					cell.northCell.velocity.y - cell.southCell.velocity.y
				) / (2f * cellDiameter);
			}
		}
		
		// updating the cell's velocity according to the calculated velocity divergence
		Vector2 volumeDisplacment = cellRadius * fluidDensity * Time.deltaTime;
		foreach (Cell cell in flowField.grid)
		{	
			// the cell is not a border cell
			if (!cell.CheckIfBorderCell())
				cell.velocity += (velocityDivergence[cell.gridIndex.x, cell.gridIndex.y] * volumeDisplacment);
		}
	}
}
