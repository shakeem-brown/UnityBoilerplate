using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

public class GridManagerDebug : MonoBehaviour
{
	[SerializeField] private GameObject mLinePrefab;
	[SerializeField] private Transform mCellHolder;
	[SerializeField] private Transform mGridHolder;
	
	public FlowField currentFlowField;
	private GridManager mGridManager;
	private bool mIsDebugActivated;
	
    private void Start() {
        mGridManager = GetComponent<GridManager>();
		mIsDebugActivated = true;
    }

	private void Update() {
		ToogleFlowFieldDisplay(); 
		FlowFieldDisplay();
	}
	
	private void ToogleFlowFieldDisplay() { 
		if (Input.GetKey(KeyCode.Space)) {
			mIsDebugActivated = !mIsDebugActivated; 
			if (!mIsDebugActivated) ClearCellDisplay();
		}
	}
	
	private void FlowFieldDisplay() {	
		if (mIsDebugActivated) {
			if (mGridManager == null || currentFlowField == null) return;
			if (currentFlowField != mGridManager.currentFlowField) return;
			DisplayAllCells();
		}
	}
	
	public void ClearCellDisplay() {
		foreach (Transform cell in mCellHolder) {
			Destroy(cell.gameObject);
			mCellList.Clear();
		}
	}

	private List<Cell> mCellList = new List<Cell>();
	private void DisplayAllCells() {
		foreach (Cell currentCell in currentFlowField.grid) {
			if (!mCellList.Contains(currentCell)) {
				mCellList.Add(currentCell);
				DisplayCell(currentCell);
			}
			else {
				int index = mCellList.IndexOf(currentCell);
				if (mCellList[index].velocity != currentCell.velocity) {
					mCellList[index] = currentCell;
					DisplayCell(currentCell);
				}
			}
		}
	}
	
	private void DisplayCell(Cell cell)  {
		GameObject line = CreateLine("vector", cell.worldPosition, 0.2f, cell.vectorColor, cell.worldPosition, cell.worldPosition + cell.GetVector3Velocity());
		line.transform.parent = mCellHolder;
	}
	
	private GameObject CreateLine(string lineName, Vector3 linePos, float lineWidth, Color lineColor, Vector3 lineStartPos, Vector3 lineEndPos) {
		GameObject line = Instantiate(mLinePrefab);
		line.name = lineName;
		line.transform.position = linePos;
		LineRenderer lineRend = line.GetComponent<LineRenderer>();
		lineRend.startWidth = lineWidth;
		lineRend.endWidth = lineWidth;
		lineRend.positionCount = 2;
		lineRend.material.SetColor("_Color", lineColor);
		lineRend.SetPositions(new Vector3[] { lineStartPos, lineEndPos });
		return line;
	}
	
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
