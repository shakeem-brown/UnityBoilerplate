using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

public class GridManagerDebug : MonoBehaviour
{
	[HideInInspector] [SerializeField] private GameObject mLinePrefab;
	[HideInInspector] [SerializeField] private GameObject mCubePrefab;
	[HideInInspector] [SerializeField] private Transform mGridHolder;
	[HideInInspector] [SerializeField] private Transform mVectorHolder;
    [HideInInspector] [SerializeField] private Transform mCubeHolder;
	[HideInInspector] private GridManager mGridManager;
	
	public VectorField vectorField;
	public FlowField flowField;
	public FluidSimulation fluidSimulation;
	public bool isFlowFieldActivated;
	public bool isFluidSimulationActive;
	
    private void Start() { mGridManager = GetComponent<GridManager>(); }

	private void Update() { 
		FlowFieldDisplay(); 
		FluidSimulationDisplay();
	}
	
	private void FlowFieldDisplay() {	
		if (isFlowFieldActivated) {
			if (mGridManager == null || vectorField == null || flowField == null) return;
			if (flowField != mGridManager.flowField) return;
			
			// Display all the flow field vectors
			foreach (Cell currentCell in vectorField.grid) {
				if (mVectorHolder.childCount < vectorField.grid.Length) DisplayLine(currentCell);
				else currentCell.vector.GetComponent<LineRenderer>().SetPositions(new Vector3[] { currentCell.worldPosition, currentCell.worldPosition + currentCell.GetVector3Velocity() });
			}
		}
	}
	
	private void FluidSimulationDisplay() {
		if (isFluidSimulationActive) {
			if (mGridManager == null || vectorField == null || fluidSimulation == null) return;
			if (fluidSimulation != mGridManager.fluidSimulation) return;
			
			foreach (Cell currentCell in vectorField.grid) {
				if (mCubeHolder.childCount < vectorField.grid.Length) DisplayCube(currentCell);
				else VisualizeFluidSimulation(currentCell);
			}
		}
	}
	
	private void VisualizeFluidSimulation(Cell cell) {
		MeshRenderer meshRenderer = cell.cube.GetComponent<MeshRenderer>();
		
		float pressure = cell.pressure;
		float r, g, b;
		if (pressure < 0.25f) {
			r = 4f * pressure;
			g = 0f;
			b = 1f;
		} else if (pressure < 0.5f) {
			r = 1f;
			g = 4f * (pressure - 0.25f);
			b = 1f - 4f * (pressure - 0.25f);
		} else if (pressure < 0.75f) {
			r = 1f - 4f * (pressure - 0.5f);
			g = 1f;
			b = 0f;
		} else {
			r = 0f;
			g = 1f - 4f * (pressure - 0.75f);
			b = 0f;
		}

		// sets the mesh renderer's color
		meshRenderer.material.color = new Color(r, g, b);
	}
	
	private void DisplayLine(Cell cell) {
		GameObject vector = CreateLine("vector", cell.worldPosition, 0.2f, Color.white, cell.worldPosition, cell.worldPosition + cell.GetVector3Velocity());
		vector.transform.parent = mVectorHolder;
		cell.vector = vector;
	}
	
	private void DisplayCube(Cell cell) {
		GameObject cube = CreateCube("cube", cell.worldPosition, Color.black);
		cube.transform.parent = mCubeHolder;
		cell.cube = cube;
	}
	
	private GameObject CreateCube(string cubeName, Vector3 cubePos, Color cubeColor) {
		GameObject cube = Instantiate(mCubePrefab);
		cube.name = cubeName;
		cube.transform.position = new Vector3(cubePos.x, -mGridManager.cellRadius, cubePos.z);
		cube.transform.localScale = new Vector3(mGridManager.cellRadius * 2, cube.transform.localScale.y, mGridManager.cellRadius * 2);
		cube.GetComponent<MeshRenderer>().material.SetColor("_Color", cubeColor);
		return cube;
	}
	
	private GameObject CreateLine(string lineName, Vector3 linePos, float lineWidth, Color lineColor, Vector3 lineStartPos, Vector3 lineEndPos) {
		GameObject line = Instantiate(mLinePrefab);
		line.name = lineName;
		line.transform.position = linePos;
		line.transform.localScale = new Vector3(mGridManager.cellRadius * 2, line.transform.localScale.y, mGridManager.cellRadius * 2);
		LineRenderer lineRend = line.GetComponent<LineRenderer>();
		lineRend.startWidth = lineWidth;
		lineRend.endWidth = lineWidth;
		lineRend.positionCount = 2;
		lineRend.material.SetColor("_Color", lineColor);
		lineRend.SetPositions(new Vector3[] { lineStartPos, lineEndPos });
		return line;
	}
	
	// accessors
	public void ToogleFlowFieldDisplay() { 
		isFlowFieldActivated = !isFlowFieldActivated; 
		if (!isFlowFieldActivated) foreach (Transform cell in mVectorHolder) Destroy(cell.gameObject); 
	}
	
	public void ToogleFluidSimulationDisplay() {
		isFluidSimulationActive = !isFluidSimulationActive;
		if (!isFluidSimulationActive) foreach (Transform cell in mCubeHolder) Destroy(cell.gameObject); 
	}
	
	public void ToogleGridVisibility() { foreach (Transform cell in mGridHolder) cell.gameObject.SetActive(!cell.gameObject.activeSelf); }
	
	public void DrawGrid(Vector2Int gridSize, Vector2Int gridOffset, float cellRadius, float cellDiameter) {
		float gridWidth = gridSize.x * cellDiameter;
		float gridHeight = gridSize.y * cellDiameter;
		
		for (int y = 0; y <= gridSize.y; y++) {
			Vector3 startPos = new Vector3(gridOffset.x * cellDiameter, 0, y * cellDiameter + gridOffset.y * cellDiameter);
			Vector3 endPos = new Vector3(gridOffset.x * cellDiameter + gridWidth, 0, y * cellDiameter + gridOffset.y * cellDiameter);	
			GameObject line = CreateLine("row", Vector3.zero, 0.1f, Color.black, startPos, endPos);
			line.transform.parent = mGridHolder;
		}
		
		for (int x = 0; x <= gridSize.x; x++) {
			Vector3 startPos = new Vector3(x * cellDiameter + gridOffset.x * cellDiameter, 0, gridOffset.y * cellDiameter);
			Vector3 endPos = new Vector3(x * cellDiameter + gridOffset.x * cellDiameter, 0, gridOffset.y * cellDiameter + gridHeight);
			GameObject line = CreateLine("column", Vector3.zero, 0.1f, Color.black, startPos, endPos);
			line.transform.parent = mGridHolder;
		}
	}
}
