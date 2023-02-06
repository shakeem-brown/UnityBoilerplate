using UnityEngine;

public class Cell
{
	public Vector3 mWorldPosition;
	public Vector2Int mGridIndex;
	public byte mCost; // int value from 1 to 255
	public ushort mBestCost;
	public GridDirection mBestDirection;
	public bool mIsOccupied;
	
	public Cell(Vector3 worldPosition, Vector2Int gridIndex)
	{
		mWorldPosition = worldPosition;
		mGridIndex = gridIndex;
		mCost = 1;
		mBestCost = ushort.MaxValue;
		mBestDirection = GridDirection.None;
		mIsOccupied = false;
	}
	
	public void IncreaseCost(int val)
	{
		if (mCost == byte.MaxValue) return;
		if (val + (int)mCost >= 255) mCost = byte.MaxValue;
		else mCost += (byte)val;
	}
}
