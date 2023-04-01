using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorField
{
	public Cell[,] grid { get; private set; }
	public Vector2Int gridSize { get; private set; }
	public Vector2Int gridOffset { get; private set; }
	public float cellRadius { get; private set; }
	public float cellDiameter { get; private set; }
	
	public VectorField(Vector2Int size, Vector2Int offset, float radius) {
		gridSize = size;
		gridOffset = offset;
		cellRadius = radius;
		cellDiameter = cellRadius + cellRadius;
		GenerateGrid();
	}
	
	// takes the world position and converts that position to the nearest cell
	public Cell GetCellFromWorldPosition(Vector3 worldPosition) {
		float xRatio = worldPosition.x / cellDiameter;
		float yRatio = worldPosition.z / cellDiameter;
		xRatio -= gridOffset.x;
		yRatio -= gridOffset.y;
		int x = Mathf.Clamp(Mathf.FloorToInt(xRatio), 0, gridSize.x - 1);
		int y = Mathf.Clamp(Mathf.FloorToInt(yRatio), 0, gridSize.y - 1);
		return grid[x, y]; // return that cell at that index
	}
	
	// changes the goal node to the nearest cell at the mouse position
	public Cell GetCellAtMouseClickPosition() {
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = Camera.main.nearClipPlane;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos);
		Cell nearestCellAtMousePosition = GetCellFromWorldPosition(mousePos);
		
		// check if the mouse clicked an area outside the grid or a border cell
		if (nearestCellAtMousePosition.gridIndex.x - 1 < 0 ||
			nearestCellAtMousePosition.gridIndex.y - 1 < 0 ||
			nearestCellAtMousePosition.gridIndex.x + 1 > gridSize.x - 1 ||
			nearestCellAtMousePosition.gridIndex.y + 1 > gridSize.y - 1) {
				
			float squareDist = (new Vector3(mousePos.x, 0, mousePos.z) - nearestCellAtMousePosition.worldPosition).sqrMagnitude;
			
			// check if the distance between the mousePos and the border cell position to confirm if the border cell was clicked
			if (squareDist > cellRadius * cellRadius) return null;
		}
		return nearestCellAtMousePosition;
	}
	
	// gets a random non border cell via gridIndex and returns that cell's world position
	public Cell GetRandomCellWithinTheGrid() {
		int randomXIndex = Random.Range(1, gridSize.x - 1);
		int randomYIndex = Random.Range(1, gridSize.y - 1);
		return grid[randomXIndex, randomYIndex];
	}
	
	// generates the grid where the 3 fields are being generated on
	private void GenerateGrid() {
		grid = new Cell[gridSize.x, gridSize.y];
		for (int x = 0; x < gridSize.x; x++) {
			for (int y = 0; y < gridSize.y; y++) {
				float worldXPos = cellDiameter * (x + gridOffset.x) + cellRadius;
				float worldZPos = cellDiameter * (y + gridOffset.y) + cellRadius;
				Vector3 worldPosition = new Vector3(worldXPos, 0, worldZPos);
				grid[x, y] = new Cell(worldPosition, new Vector2Int(x, y));
			}
		}
		GenerateNeighbors();
	}
	
	// adds neighborCells to each cell's neighborCells list
	private void GenerateNeighbors() {
		foreach (Cell cell in grid) {
			for (int x = -1; x <= 1; x++) {
				for (int y = -1; y <= 1; y++) {
					// calculates the grid index of the adjacent cell
					Vector2Int neighborIndex = new Vector2Int(cell.gridIndex.x + x, cell.gridIndex.y + y);
					neighborIndex.x = Mathf.Clamp(neighborIndex.x, 0, gridSize.x - 1);
					neighborIndex.y = Mathf.Clamp(neighborIndex.y, 0, gridSize.y - 1);
					
					if (x == 0 && y == 0) continue; // skip yourself (the center cell)
					if (neighborIndex == cell.gridIndex) continue; // skip yourself
					Cell neighborCell = grid[neighborIndex.x, neighborIndex.y];
					
					// sets the cardinal neighbor cells if they exist
					if (x == 0 && y == 1) cell.northCell = neighborCell;
					else if (x == 1 && y == 0) cell.eastCell = neighborCell;
					else if (x == 0 && y == -1) cell.southCell = neighborCell;
					else if (x == -1 && y == 0) cell.westCell = neighborCell;
					// sets the diagnol neighbor cells if they exist
					else if (x == 1 && y == 1) cell.northEastCell = neighborCell;
					else if (x == -1 && y == 1) cell.northWestCell = neighborCell;
					else if (x == 1 && y == -1) cell.southEastCell = neighborCell;
					else if (x == -1 && y == -1) cell.southWestCell = neighborCell;
					
					cell.neighborCells.Add(neighborCell);
				}
			}
		}
	}
}
