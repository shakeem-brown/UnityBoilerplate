using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Vector2Int gridSize;
    public Vector2Int gridOffset;
    public float cellRadius;
    public FlowField currentFlowField { get; private set; }
	
	public void InitalizeFlowField()
	{
		currentFlowField = new FlowField(gridSize, gridOffset, cellRadius);
		currentFlowField.GenerateGrid();
		GetComponent<GridManagerDebug>().DrawGrid(gridSize, gridOffset, cellRadius, cellRadius * 2);
	}
	
	public void UpdateVectorField()
	{
		foreach (Cell cell in currentFlowField.grid)
		{
			
		}
	}

	//private void GeneratePath(Vector3 startPosition, Vector3 goalPosition)
	//{
	//	currentFlowField.GeneratePath(startPosition, goalPosition);
	//}
	
	/// My stuff below
	// edit this function
	public void UpdateFlowField(Vector3 goalPosition)
	{
		//InitalizeFlowField();
		currentFlowField.GenerateCostField();
		SetDestinationCell(goalPosition);
		currentFlowField.GenerateFlowField();
	}
	
	private void SetDestinationCell(Vector3 goalPosition)
	{
		Cell destinationCell = currentFlowField.GetCellFromWorldPosition(goalPosition);
		currentFlowField.GenerateIntergrationField(destinationCell);
	}
}
