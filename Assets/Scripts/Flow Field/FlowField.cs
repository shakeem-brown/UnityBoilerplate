using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlowField
{
	public VectorField vectorField { get; private set; }
	public Vector2Int gridSize { get; private set; }
	public float cellRadius { get; private set; }
	public float cellDiameter { get; private set; }
	public Cell destinationCell { get; private set; }
	
	public FlowField(VectorField _vectorField) {
		vectorField = _vectorField;
		gridSize = vectorField.gridSize;
		cellRadius = vectorField.cellRadius;
		cellDiameter = vectorField.cellDiameter;
	}
	
	// sets the inital cost for the terrain, Dev. pref each layer can have different cost values
	// The max cost is 255, and the minium cost for terrain is 1. Terrain cannot have 0 
	// cost as it will be mistaken as the destination cell. By default all cost values are set
	// to 1 until the cost field possibly sets it to  some other value
	public void GenerateCostField() {
		int terrainMask = LayerMask.GetMask("Impassable", "Rough");
		foreach (Cell currentCell in vectorField.grid) {
			Collider[] terrain = Physics.OverlapBox(currentCell.worldPosition, Vector3.one * cellRadius, Quaternion.identity, terrainMask);
			bool isIncreaseCost = false;
			
			// loop through the colliders on the vectorField.grid and assign a cost value to every cell 
			// within the confines of the collider via terrain's physics.overlap box
			foreach (Collider collider in terrain) {
				// this terrain is impassable
				if (collider.gameObject.layer == 6) {
					currentCell.IncreaseCost(byte.MaxValue); // set cost to max byte val == 255
					continue;
				}
				// this terrain is rough
				else if (!isIncreaseCost && collider.gameObject.layer == 7) {
					currentCell.IncreaseCost(3);
					isIncreaseCost = true;
				}
			}
		}
	}
	
	// generates the cost for each cell towards the destination cell where cells that are closer
	// to the destination are lower then others further away from the destination
	public void GenerateIntergrationField(Cell goalCell) {
		// reset all cells' pathfinding properties: unit, cost, & bestCost
		foreach (Cell cell in vectorField.grid) {
			cell.unit = null;
			cell.cost = 1;
			cell.bestCost = ushort.MaxValue;
		}
		// set the destination cell specific properties: cost/ bestCost == 0 & velocity == 0
		destinationCell = goalCell;
		destinationCell.cost = 0;
		destinationCell.bestCost = 0;
		destinationCell.velocity = Vector2.zero;
		
		Queue<Cell> cellsToCheck = new Queue<Cell>();
		cellsToCheck.Enqueue(destinationCell);
		
		// check through all the neighbors in the queue: cellsToCheck to find the best cost
		// to help later generate a path
		while (cellsToCheck.Count > 0) {
			Cell currentCell = cellsToCheck.Dequeue();
			foreach (Cell currentNeighbor in currentCell.neighborCells) {
				if (currentNeighbor.cost == byte.MaxValue) continue;
				if (currentNeighbor.cost + currentCell.bestCost < currentNeighbor.bestCost) {
					currentNeighbor.bestCost = (ushort)(currentNeighbor.cost + currentCell.bestCost);
					cellsToCheck.Enqueue(currentNeighbor);
				}
			}
		}
	}
	
	// generates the best direction to the destination based of the intergration field cost calculations
	public void GenerateFlowField() {
		foreach (Cell currentCell in vectorField.grid) {
			int bestCost = currentCell.bestCost;
			
			foreach (Cell currentNeighbor in currentCell.neighborCells) {
				if (currentNeighbor.bestCost < bestCost) {
					bestCost = currentNeighbor.bestCost;
					currentCell.velocity = CalculateCellVelocity(currentCell.gridIndex, currentNeighbor.gridIndex);
				}
			}
		}
	}
	
	// takes the difference between the cell indexs and converts them to a respective cell velocity that cordinates to a direction
	private Vector2 CalculateCellVelocity(Vector2Int currentCellIndex, Vector2Int neighborCellIndex) {
		Vector2Int diff = neighborCellIndex - currentCellIndex;
		Vector2 cellVelocity = Vector2.zero;
		
		if (diff.x == 0) {
			if (diff.y == 1) cellVelocity = new Vector2(0, 1); // north
			else if (diff.y == -1) cellVelocity = new Vector2(0, -1); // south
		}
		else if (diff.x == 1) {
			if (diff.y == 0) cellVelocity = new Vector2(1, 0); // east
			else if (diff.y == 1) cellVelocity = new Vector2(1, 1); // north east
			else if (diff.y == -1) cellVelocity = new Vector2(1, -1); // south east
		}
		else if (diff.x == -1) {
			if (diff.y == 0) cellVelocity = new Vector2(-1, 0); // west
			else if (diff.y == 1) cellVelocity = new Vector2(-1, 1); // north west
			else if (diff.y == -1) cellVelocity = new Vector2(-1, -1); // south west
		}
		return cellVelocity;
	}
}
