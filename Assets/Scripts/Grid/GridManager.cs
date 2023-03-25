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
	public float gravitationalConstant;
	[Min(0)] public float fluidDensity;
	[Range(1,2)] public float overrelaxation;
	[Min(1)] public int iterations;
	
    public VectorField vectorField { get; private set; }
    public FlowField flowField { get; private set; }
    public FluidSimulation fluidSimulation { get; private set; }
	public Cell lastDestinationCell { get; private set; }
	private GridManagerDebug mGridManagerDebug;
	
	private void Start() {
		vectorField = new VectorField(gridSize, gridOffset, cellRadius); // initalizing the vector field
		flowField = new FlowField(vectorField); // initalizing the flow field
		fluidSimulation = new FluidSimulation(vectorField, gravitationalConstant, fluidDensity, overrelaxation, iterations); // initalizing the fluid simulation

		// DEBUG
		mGridManagerDebug = GetComponent<GridManagerDebug>();
		mGridManagerDebug.vectorField = vectorField;
		mGridManagerDebug.DrawGrid(gridSize, gridOffset, cellRadius, cellRadius * 2);
	}
	
    private void Update() { 
		if (Input.GetMouseButton(0)) { 
			UpdateFlowField(vectorField.GetCellAtMouseClickPosition());
		}
		UpdateFluidSimulation();
	}
	
	// accessors
	public void UpdateFlowField(Cell destinationCell) {
		if (destinationCell == null || !mGridManagerDebug.isFlowFieldActivated) return;
		
		bool isUpdateFlowField = destinationCell != lastDestinationCell || lastDestinationCell == null;
		
        if (isUpdateFlowField) {
			flowField.GenerateCostField();
			flowField.GenerateIntergrationField(destinationCell);
            flowField.GenerateFlowField();
            lastDestinationCell = destinationCell;
			mGridManagerDebug.flowField = flowField; // for DEBUG
        }
    }
	
	public void UpdateFluidSimulation() {
		if (!mGridManagerDebug.isFluidSimulationActive) return;
		
		fluidSimulation.UpdateFluidSimulation();
		mGridManagerDebug.fluidSimulation = fluidSimulation; // for DEBUG
	}
}
