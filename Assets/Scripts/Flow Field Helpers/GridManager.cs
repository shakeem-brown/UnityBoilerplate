using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Vector2Int gridSize;
    public Vector2Int gridOffset;
    public float cellRadius;
    public FlowField currentFlowField { get; private set; }
	public Cell lastDestinationCell { get; private set; }
	private GridManagerDebug mGridManagerDebug;
	
	public void InitalizeFlowField() {
		currentFlowField = new FlowField(gridSize, gridOffset, cellRadius);
		currentFlowField.GenerateGrid();
		
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
			mGridManagerDebug.ClearCellDisplay();
			mGridManagerDebug.currentFlowField = currentFlowField;
        }
    }
}
