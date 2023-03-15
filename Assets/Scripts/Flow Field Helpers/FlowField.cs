using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlowField
{
	public Cell[,] grid { get; private set; }
	public Vector2Int gridSize { get; private set; }
	public Vector2Int gridOffset { get; private set; }
	public float cellRadius { get; private set; }
	public float cellDiameter { get; private set; }
	public Cell destinationCell { get; private set; }
	
	public FlowField(Vector2Int size, Vector2Int offset, float radius) {
		gridSize = size;
		gridOffset = offset;
		cellRadius = radius;
		cellDiameter = cellRadius + cellRadius;
	}
	
	// generates the grid where the 3 fields are being generated on
	public void GenerateGrid() {
		grid = new Cell[gridSize.x, gridSize.y];
		for (int x = 0; x < gridSize.x; x++) {
			for (int y = 0; y < gridSize.y; y++) {
				float worldXPos = cellDiameter * (x + gridOffset.x) + cellRadius;
				float worldZPos = cellDiameter * (y + gridOffset.y) + cellRadius;
				Vector3 worldPosition = new Vector3(worldXPos, 0, worldZPos);
				grid[x, y] = new Cell(worldPosition, new Vector2Int(x, y));
			}
		}
		
		// add neighborCells to neighborCells list for each cell
		foreach (Cell cell in grid) {
			for (int x = -1; x <= 1; x++) {
				for (int y = -1; y <= 1; y++) {
					// Calculate the grid index of the adjacent cell
					Vector2Int neighborIndex = new Vector2Int(cell.gridIndex.x + x, cell.gridIndex.y + y);
					neighborIndex.x = Mathf.Clamp(neighborIndex.x, 0, gridSize.x - 1);
					neighborIndex.y = Mathf.Clamp(neighborIndex.y, 0, gridSize.y - 1);
					
					if (x == 0 && y == 0) continue; // skip yourself (the center cell)
					if (neighborIndex == cell.gridIndex) continue; // skip yourself
					cell.neighborCells.Add(grid[neighborIndex.x, neighborIndex.y]);
				}
			}
		}
	}
	
	// sets the inital cost for the terrain. Different terrain textures have more cost
	// compared to smoother ones. The max cost is 255, and the minium cost for terrain.
	// terrain cannot have 0 cost as it will be mistaken as the destination cell
	public void GenerateCostField() {
		Vector3 cellHalfExtents = Vector3.one * cellRadius;
		int terrainMask = LayerMask.GetMask("Impassible", "Rough");
		foreach (Cell currentCell in grid) {
			Collider[] terrain = Physics.OverlapBox(currentCell.worldPosition, cellHalfExtents, Quaternion.identity, terrainMask);
			bool isIncreaseCost = false;
			foreach (Collider collider in terrain) {
				if (collider.gameObject.layer == 6) {
					currentCell.IncreaseCost(byte.MaxValue); // set cost to max byte val == 255
					continue;
				}
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
		// Reset all cells' costs and best costs
		foreach (Cell cell in grid) {
			cell.cost = 1;
			cell.bestCost = ushort.MaxValue;
		}
		
		destinationCell = goalCell;
		destinationCell.cost = 0;
		destinationCell.bestCost = 0;
		
		Queue<Cell> cellsToCheck = new Queue<Cell>();
		cellsToCheck.Enqueue(destinationCell);
		
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
		foreach (Cell currentCell in grid) {
			int bestCost = currentCell.bestCost;
			
			foreach (Cell currentNeighbor in currentCell.neighborCells) {
				if (currentNeighbor.bestCost < bestCost) {
					bestCost = currentNeighbor.bestCost;
					currentCell.velocity = CalculateCellVelocity(currentCell.gridIndex, currentNeighbor.gridIndex);
				}
			}
		}
	}
	
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
	
	public Cell GetCellFromWorldPosition(Vector3 worldPosition) {
		float xRatio = worldPosition.x / cellDiameter;
		float yRatio = worldPosition.z / cellDiameter;
		xRatio -= gridOffset.x;
		yRatio -= gridOffset.y;
		int x = Mathf.Clamp(Mathf.FloorToInt(xRatio), 0, gridSize.x - 1);
		int y = Mathf.Clamp(Mathf.FloorToInt(yRatio), 0, gridSize.y - 1);

		return grid[x, y];
	}
	
	// returns the neighbor cell the current cell will move to
	public Cell GetNeighborCell(Cell currentCell) { return GetCellFromWorldPosition(currentCell.worldPosition + currentCell.GetVector3Velocity()); }
}
