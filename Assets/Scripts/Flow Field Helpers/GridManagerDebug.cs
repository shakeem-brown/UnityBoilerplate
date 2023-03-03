using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

public class GridManagerDebug : MonoBehaviour
{
	public Sprite[] FlowFieldIcons;
	public Material ArrowMaterial;
	
	private GridManager mGridManager;
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
		mDebugHandlerTracker1 = 0;
		mDebugHandlerTracker2 = 1;
		mIconPositionOffset = new Vector3(0.0f, 0.2f, 0.0f);
		mIconRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
		mPreviousFlowField = null;
    }

	private void Update() { ToogleFlowFieldDisplay(); }
	
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
		mCellRadius = flowField.cellRadius;
		mGridSize = flowField.gridSize;
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
		foreach (Cell currentCell in mCurrentFlowField.grid)
		{
			DisplayCell(currentCell);
		}
	}
	
	private void DisplayDestinationCell()
	{
		if (mCurrentFlowField == null) return;
		DisplayCell(mCurrentFlowField.destinationCell);
	}
	
	// I need to edit this
	private void DisplayCell(Cell cell)
	{
		// Create a new game object to represent the cell's velocity
		GameObject arrow = new GameObject("Arrow");
		arrow.transform.parent = transform;

		// Add a mesh renderer component to the game object
		MeshRenderer renderer = arrow.AddComponent<MeshRenderer>();
		renderer.material = ArrowMaterial;

		// Create a new mesh for the arrow
		Mesh mesh = new Mesh();
		mesh.vertices = new Vector3[] {
			new Vector3(0, 0, 0),
			new Vector3(0, 1, 0),
			new Vector3(0.25f, 0.75f, 0),
			new Vector3(-0.25f, 0.75f, 0),
			new Vector3(0, 1, 0),
			new Vector3(0, 0, 0)
		};
		mesh.triangles = new int[] { 0, 1, 2, 0, 3, 1, 4, 5, 1 };
		mesh.RecalculateNormals();

		// Set the mesh of the renderer to the new mesh
		MeshFilter filter = arrow.AddComponent<MeshFilter>();
		filter.mesh = mesh;
		
		arrow.transform.position = cell.worldPosition;
		Quaternion rotation = Quaternion.LookRotation(Vector3.forward, cell.velocity);
		rotation.eulerAngles = new Vector3(90f, rotation.eulerAngles.y, rotation.eulerAngles.z);
		arrow.transform.rotation = rotation;
		
		/*
		GameObject iconObj = new GameObject();
		SpriteRenderer iconSR = iconObj.AddComponent<SpriteRenderer>();
		iconObj.transform.parent = transform;
		iconObj.transform.position = cell.worldPosition + mIconPositionOffset;
		iconObj.transform.rotation = mIconRotation;
		
		if (cell.cost == 0)								iconSR.sprite = FlowFieldIcons[9]; // goal
		else if (cell.cost == byte.MaxValue)			iconSR.sprite = FlowFieldIcons[0]; // non-passable
		else if (cell.velocity == new Vector2(0, 1))	iconSR.sprite = FlowFieldIcons[1]; // north
		else if (cell.velocity == new Vector2(1, 1))	iconSR.sprite = FlowFieldIcons[2]; // north east
		else if (cell.velocity == new Vector2(1, 0))	iconSR.sprite = FlowFieldIcons[3]; // east
		else if (cell.velocity == new Vector2(1, -1))	iconSR.sprite = FlowFieldIcons[4]; // south east
		else if (cell.velocity == new Vector2(0, -1))	iconSR.sprite = FlowFieldIcons[5]; // south
		else if (cell.velocity == new Vector2(-1, -1))	iconSR.sprite = FlowFieldIcons[6]; // south west
		else if (cell.velocity == new Vector2(-1, 0))	iconSR.sprite = FlowFieldIcons[7]; // west
		else if (cell.velocity == new Vector2(-1, 1))	iconSR.sprite = FlowFieldIcons[8]; // north west
		*/
	}
}
