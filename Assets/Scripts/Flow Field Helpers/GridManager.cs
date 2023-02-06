using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Vector2Int GridSize;
    public Vector2Int GridOffset;
    public float CellRadius;
    public FlowField CurrentFlowField;
	
	public void UpdateFlowField(Vector3 goalPosition)
	{
		InitalizeFlowField();
		CurrentFlowField.GenerateCostField();
		SetDestinationCell(goalPosition);
		CurrentFlowField.GenerateFlowField();
	}
	
	private void SetDestinationCell(Vector3 goalPosition)
	{
		Cell destinationCell = CurrentFlowField.GetCellFromWorldPosition(goalPosition);
		CurrentFlowField.GenerateIntergrationField(destinationCell);
	}
	
	private void InitalizeFlowField()
	{
		CurrentFlowField = new FlowField(GridSize, GridOffset, CellRadius);
		CurrentFlowField.GenerateGrid();
	}
}
