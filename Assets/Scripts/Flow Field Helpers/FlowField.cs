using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlowField
{
	public Cell[,] mGrid {get; private set;}
	public Vector2Int mGridSize {get; private set;}
	public Vector2Int mGridOffset { get; private set; }
	public float mCellRadius {get; private set;}
	public Cell mDestinationCell; // the goal cell
	private float mCellDiameter;
	
	public FlowField(Vector2Int gridSize, Vector2Int gridOffset, float cellRadius)
	{
		mGridSize = gridSize;
		mGridOffset = gridOffset;
		mCellRadius = cellRadius;
		mCellDiameter = mCellRadius * 2.0f;
	}
	
	// generates the grid where the 3 fields are being generated on
	public void GenerateGrid()
	{
		mGrid = new Cell[mGridSize.x, mGridSize.y];
		for (int x = 0; x < mGridSize.x; x++)
		{
			for (int y = 0; y < mGridSize.y; y++)
			{
				float worldXPos = mCellDiameter * (x + mGridOffset.x) + mCellRadius;
				float worldZPos = mCellDiameter * (y + mGridOffset.y) + mCellRadius;
				Vector3 worldPosition = new Vector3(worldXPos, 0, worldZPos);
				mGrid[x, y] = new Cell(worldPosition, new Vector2Int(x, y));
			}
		}
	}
	
	// sets the inital cost for the terrain. Different terrain textures have more cost
	// compared to smoother ones. The max cost is 255, and the minium cost for terrain.
	// terrain cannot have 0 cost as it will be mistaken as the destination cell
	public void GenerateCostField()
	{
		//Vector3 cellHalfExtents = Vector3.one * mCellRadius;
		//int terrainMask = LayerMask.GetMask("Impassible", "Rough");
		//foreach (Cell currentCell in mGrid)
		//{
		//	Collider[] terrain = Physics.OverlapBox(currentCell.mWorldPosition, cellHalfExtents, Quaternion.identity, terrainMask);
		//	bool isIncreaseCost = false;
		//	foreach (Collider collider in terrain)
		//	{
		//		if (collider.gameObject.layer == 6)
		//		{
		//			currentCell.IncreaseCost(byte.MaxValue); // set cost to max byte val == 255
		//			continue;
		//		}
		//		else if (!isIncreaseCost && collider.gameObject.layer == 7)
		//		{
		//			currentCell.IncreaseCost(3);
		//			isIncreaseCost = true;
		//		}
		//	}
		//}
	}
	
	// generates the cost for each cell towards the destination cell where cells that are closer
	// to the destination are lower then others further away from the destination
	public void GenerateIntergrationField(Cell destinationCell)
	{
		mDestinationCell = destinationCell;
		mDestinationCell.mCost = 0;
		mDestinationCell.mBestCost = 0;
		
		Queue<Cell> cellsToCheck = new Queue<Cell>();
		cellsToCheck.Enqueue(mDestinationCell);
		
		while (cellsToCheck.Count > 0)
		{
			Cell currentCell = cellsToCheck.Dequeue();
			List<Cell> currentNeighbors = GetNeighborCells(currentCell.mGridIndex, GridDirection.CardinalDirections);
			foreach (Cell currentNeighbor in currentNeighbors)
			{
				if (currentNeighbor.mCost == byte.MaxValue) continue;
				if (currentNeighbor.mCost + currentCell.mBestCost < currentNeighbor.mBestCost)
				{
					currentNeighbor.mBestCost = (ushort)(currentNeighbor.mCost + currentCell.mBestCost);
					cellsToCheck.Enqueue(currentNeighbor);
				}
			}
		}
	}
	
	// generates the best direction to the destination based of the intergration field cost calculations
	public void GenerateFlowField() 
	{
		foreach (Cell currentCell in mGrid)
		{
			List<Cell> currentNeighbors = GetNeighborCells(currentCell.mGridIndex, GridDirection.AllDirections);
			int bestCost = currentCell.mBestCost;
			
			foreach (Cell currentNeighbor in currentNeighbors)
			{
				if (currentNeighbor.mBestCost < bestCost)
				{
					bestCost = currentNeighbor.mBestCost;
					currentCell.mBestDirection = GridDirection.GetDirectionFromVector2Int(currentNeighbor.mGridIndex - currentCell.mGridIndex);
				}
			}
		}
	}
	
	public Cell GetCellFromWorldPosition(Vector3 worldPosition)
	{
		float xRatio = worldPosition.x / (mCellDiameter);
		float yRatio = worldPosition.z / (mCellDiameter);
		xRatio -= mGridOffset.x;
		yRatio -= mGridOffset.y;
		int x = Mathf.Clamp(Mathf.FloorToInt(xRatio), 0, mGridSize.x - 1);
		int y = Mathf.Clamp(Mathf.FloorToInt(yRatio), 0, mGridSize.y - 1);

		return mGrid[x, y];
	}
	
	public List<Cell> GetNeighborCells(Vector2Int cellIndex, List<GridDirection> directions)
	{
		List<Cell> neighborCells = new List<Cell>();
		foreach (Vector2Int currentDirection in directions)
		{
			Cell newNeighbor = GetCellAtRelativePosition(cellIndex, currentDirection);
			if (newNeighbor != null) neighborCells.Add(newNeighbor);
		}
		return neighborCells;
	}
	
	private Cell GetCellAtRelativePosition(Vector2Int cellOrigin, Vector2Int relativePosition)
	{
		Vector2Int finalPosition = cellOrigin + relativePosition;
		if (finalPosition.x < 0 || finalPosition.x >= mGridSize.x || finalPosition.y < 0 || finalPosition.y >= mGridSize.y) return null;
		else return mGrid[finalPosition.x, finalPosition.y];
	}
}
