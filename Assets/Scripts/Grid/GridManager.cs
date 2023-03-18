using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
	[Header("Generic Grid Properties")]
    public Vector2Int gridSize;
    public Vector2Int gridOffset;
    public float cellRadius;
	[Space]
	[Header("Fluid Simulation Properties")]
	[Min(1)] public float maxSpeed;
	[Min(0)] public float speedDamping;
	[Min(1)] public int pressureIterations;
	public Vector2 fluidDensity;
	[HideInInspector] public bool isFluidSimulationActive;
	
    public FlowField currentFlowField { get; private set; }
    public FluidSimulation currentFluidSimulation { get; private set; }
	public Cell lastDestinationCell { get; private set; }
	private GridManagerDebug mGridManagerDebug;
	
	public void InitalizeFlowField() {
		// initalizing the flow field and generating the grid
		currentFlowField = new FlowField(gridSize, gridOffset, cellRadius);
		currentFlowField.GenerateGrid();
		
		// initalizing the fluid simulation
		currentFluidSimulation = new FluidSimulation(currentFlowField, maxSpeed, speedDamping, pressureIterations, fluidDensity);
		
		// DEBUG
		mGridManagerDebug = GetComponent<GridManagerDebug>();
		mGridManagerDebug.DrawGrid(gridSize, gridOffset, cellRadius, cellRadius * 2);
	}
	
	public void UpdateFlowField(Vector3 goalPosition) {
		Cell destinationCell = currentFlowField.GetCellFromWorldPosition(goalPosition);
		bool isUpdateFlowField = destinationCell != lastDestinationCell || lastDestinationCell == null;
		
        if (isUpdateFlowField) {
			currentFlowField.GenerateCostField();
			currentFlowField.GenerateIntergrationField(destinationCell);
            currentFlowField.GenerateFlowField();
            lastDestinationCell = destinationCell;
			
			// DEBUG
			mGridManagerDebug.currentFlowField = currentFlowField;
        }
    }
	
	public void UpdateFluidSimulation() {
		currentFluidSimulation.UpdateFluidSimulation();
		mGridManagerDebug.UpdateFluidSimulationColorDiffusion();
	}
}
