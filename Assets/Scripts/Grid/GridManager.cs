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
	[Min(0.001f)] public float timeStep;
	[Min(0.001f)] public float diffision;
	[Min(0.001f)] public float viscosity;
	
    public VectorField vectorField { get; private set; }
    public FlowField flowField { get; private set; }
    public FluidSimulation fluidSimulation { get; private set; }
	public Cell lastDestinationCell { get; private set; }
	private GridManagerDebug mGridManagerDebug;
	
	private Vector3 previousMousePos;
	
	private void Start() {
		vectorField = new VectorField(gridSize, gridOffset, cellRadius); // initalizing the vector field
		flowField = new FlowField(vectorField); // initalizing the flow field
		fluidSimulation = new FluidSimulation(vectorField, timeStep, diffision, viscosity); // initalizing the fluid simulation

		// DEBUG
		mGridManagerDebug = GetComponent<GridManagerDebug>();
		mGridManagerDebug.vectorField = vectorField;
		mGridManagerDebug.DrawGrid(gridSize, gridOffset, cellRadius, cellRadius * 2);
	}
	
    private void Update() { 
		if (Input.GetMouseButton(0)) { 
			UpdateFlowField(vectorField.GetCellAtMouseClickPosition());
			FluidSimulationMouseDrag(vectorField.GetCellAtMouseClickPosition());
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
		if (mGridManagerDebug.isFluidSimulationActive) {
			fluidSimulation.Step(); 
			mGridManagerDebug.fluidSimulation = fluidSimulation; // for DEBUG
		}
	}
	
	private void FluidSimulationMouseDrag(Cell mouseCell) {
		if (previousMousePos == Vector3.zero) previousMousePos = mouseCell.worldPosition;
		if (mouseCell == null || !mGridManagerDebug.isFluidSimulationActive) return;
		mouseCell.density += 100f;
		mouseCell.pressure += 100f;
		mouseCell.velocity.x += (mouseCell.worldPosition.x - previousMousePos.x) * 5;
		mouseCell.velocity.y += (mouseCell.worldPosition.z - previousMousePos.z) * 5;
		foreach (Cell neighbor in mouseCell.neighborCells) {
			neighbor.density += 100f;
			neighbor.pressure += 100f;
			neighbor.velocity.x += (mouseCell.worldPosition.x - previousMousePos.x) * 5;
			neighbor.velocity.y += (mouseCell.worldPosition.z - previousMousePos.z) * 5;
		}
		previousMousePos = mouseCell.worldPosition;
	}
}
