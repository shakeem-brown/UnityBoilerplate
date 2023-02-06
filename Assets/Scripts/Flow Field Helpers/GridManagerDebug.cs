using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

public class GridManagerDebug : MonoBehaviour
{
	public Sprite[] FlowFieldIcons;
	
	private GridManager mGridManager;
	private bool mIsDisplayGrid;
	private int mDebugHandlerTracker1;
	private int mDebugHandlerTracker2;
	
	private Vector2Int mGridSize;
	private float mCellRadius;
	private FlowField mCurrentFlowField;
	private Vector3 mIconPositionOffset;
	private Quaternion mIconRotation;
	private FlowField mPreviousFlowField;
	
    private void Start()
    {
        mGridManager = GetComponent<GridManager>();
		mIsDisplayGrid = true;
		mDebugHandlerTracker1 = 0;
		mDebugHandlerTracker2 = 1;
		mIconPositionOffset = new Vector3(0.0f, 0.2f, 0.0f);
		mIconRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
		mPreviousFlowField = null;
    }

	private void Update()
	{
		//ToogleGridDisplay();
		//ToogleDebugNumbers();
		ToogleFlowFieldDisplay();
	}
	
	private void ToogleGridDisplay()
	{
		if (Input.GetKeyUp(KeyCode.DownArrow))
		{
			if (mIsDisplayGrid) mIsDisplayGrid = false;
			else mIsDisplayGrid = true;
		}
	}
	
	private void ToogleDebugNumbers()
	{
		if (Input.GetKeyUp(KeyCode.UpArrow)) mDebugHandlerTracker1++;
		if (mDebugHandlerTracker1 > 1) mDebugHandlerTracker1 = 0;
	}
	
	private void ToogleFlowFieldDisplay()
	{
		if (Input.GetKeyUp(KeyCode.Space)) mDebugHandlerTracker2++;
		if (mDebugHandlerTracker2 > 1) mDebugHandlerTracker2 = 0;
		
		if (mDebugHandlerTracker2 == 0)
		{
			mPreviousFlowField = null;
			ClearCellDisplay();
		}
		else if (mDebugHandlerTracker2 == 1)
		{
			if (mGridManager == null || mGridManager.CurrentFlowField == null) return;
			
			if (mPreviousFlowField != mGridManager.CurrentFlowField)
			{
				mPreviousFlowField = mGridManager.CurrentFlowField;
				SetFlowField(mGridManager.CurrentFlowField);
				DrawFlowField();
			}
		}
	}
	/*
    private void OnDrawGizmos()
    {
		if (mGridManager == null) return;
		
        if (mIsDisplayGrid == true)
		{
			if (mGridManager.CurrentFlowField == null) DrawGrid(mGridManager.GridSize, mGridManager.CellRadius, Color.red);
			else DrawGrid(mGridManager.GridSize, mGridManager.CellRadius, Color.green);
		}
		
		if (mGridManager.CurrentFlowField == null) return;
		
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.alignment = TextAnchor.MiddleCenter;
		
		if (mDebugHandlerTracker1 == 0)
		{
			foreach (Cell currentCell in mGridManager.CurrentFlowField.mGrid)
			{
				Handles.Label(currentCell.mWorldPosition, currentCell.mCost.ToString(), style);
			}
		}
		else if (mDebugHandlerTracker1 == 1)
		{
			foreach (Cell currentCell in mGridManager.CurrentFlowField.mGrid)
			{
				Handles.Label(currentCell.mWorldPosition, currentCell.mBestCost.ToString(), style);
			}
		}
    }
	*/
	private void DrawGrid(Vector2Int gridSize, float cellRadius, Color lineColor)
	{
		float cellDiameter = cellRadius * 2.0f;
		Gizmos.color = lineColor;
		for (int x = 0; x < gridSize.x; x++)
		{
			for (int y = 0; y < gridSize.y; y++)
			{
				Vector3 cellCenter = new Vector3(cellDiameter * x + cellRadius, 0, cellDiameter * y + cellRadius);
				Vector3 cellSize = Vector3.one * cellDiameter;
				Gizmos.DrawWireCube(cellCenter, cellSize);
			}
		}
	}
	
	private void SetFlowField(FlowField flowField)
	{
		mCurrentFlowField = flowField;
		mCellRadius = flowField.mCellRadius;
		mGridSize = flowField.mGridSize;
	}
	
	private void DrawFlowField()
	{
		ClearCellDisplay();
		
		if (mDebugHandlerTracker1 == 0) DisplayAllCells();
		else if (mDebugHandlerTracker1 == 1) DisplayDestinationCell();
	}
	
	private void ClearCellDisplay()
	{
		foreach (Transform iconTransforms in transform)
		{
			GameObject.Destroy(iconTransforms.gameObject);
		}
	}
	
	private void DisplayAllCells()
	{
		if (mCurrentFlowField == null) return;
		foreach (Cell currentCell in mCurrentFlowField.mGrid)
		{
			DisplayCell(currentCell);
		}
	}
	
	private void DisplayDestinationCell()
	{
		if (mCurrentFlowField == null) return;
		DisplayCell(mCurrentFlowField.mDestinationCell);
	}
	
	private void DisplayCell(Cell cell)
	{
		GameObject iconObj = new GameObject();
		SpriteRenderer iconSR = iconObj.AddComponent<SpriteRenderer>();
		iconObj.transform.parent = transform;
		iconObj.transform.position = cell.mWorldPosition + mIconPositionOffset;
		iconObj.transform.rotation = mIconRotation;
		
		if (cell.mCost == 0)										iconSR.sprite = FlowFieldIcons[9];
		else if (cell.mCost == byte.MaxValue)						iconSR.sprite = FlowFieldIcons[0];
		else if (cell.mBestDirection == GridDirection.North)		iconSR.sprite = FlowFieldIcons[1];
		else if (cell.mBestDirection == GridDirection.NorthEast)	iconSR.sprite = FlowFieldIcons[2];
		else if (cell.mBestDirection == GridDirection.East)			iconSR.sprite = FlowFieldIcons[3];
		else if (cell.mBestDirection == GridDirection.SouthEast)	iconSR.sprite = FlowFieldIcons[4];
		else if (cell.mBestDirection == GridDirection.South)		iconSR.sprite = FlowFieldIcons[5];
		else if (cell.mBestDirection == GridDirection.SouthWest)	iconSR.sprite = FlowFieldIcons[6];
		else if (cell.mBestDirection == GridDirection.West)			iconSR.sprite = FlowFieldIcons[7];
		else if (cell.mBestDirection == GridDirection.NorthWest)	iconSR.sprite = FlowFieldIcons[8];
	}
}
