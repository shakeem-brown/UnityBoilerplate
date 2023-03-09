using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	[SerializeField] private GameObject mUnitPrefab;
	public GridManager mGridManager { get; private set; }
	public List<Unit> mUnitList { get; private set; }

	private float mTimer;
	private const float MAX_TIME = 10;
	
	private float screenWidth;
	private float screenHeight;
	
	private const int UNIT_SPAWN_AMOUNT = 250;
	[SerializeField] private int MAX_UNIT_NUMBER = 6000;
	
    private void Awake()
    {
		mGridManager = GetComponent<GridManager>();
        mUnitList = new List<Unit>();
    }

	
    private void Start() 
	{
		screenWidth = mGridManager.gridSize.x * (mGridManager.cellRadius * 2f);
		screenHeight = mGridManager.gridSize.y * (mGridManager.cellRadius * 2f);
		
		mGridManager.InitalizeFlowField();
		SpawnUnits(true); // Spawning the units at the start of the game
	}

    private void Update()
    {
		UpdateUnitGoalDestination();
		GetComponent<GameManagerUI>().UpdateUnitCount(); // UI updates
	}

    private void FixedUpdate() { GamePlayControls(); }
	
	private void GamePlayControls()
	{
		// game modifications
		SpawnUnits(Input.GetKeyUp(KeyCode.P));
		UpdateUnitPosition();
		
		// key inputs
		MoveCamera();
		
		// scene management
		if (Input.GetKey(KeyCode.Escape)) Application.Quit();
		else if (Input.GetKey(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
	
	private void UpdateUnitPosition()
	{
		if (mGridManager.currentFlowField != null) 
		{
			foreach (Unit unit in mUnitList)
			{
				// Get the current cell and set it as the previous cell
				Cell currentCell = mGridManager.currentFlowField.GetCellFromWorldPosition(unit.transform.position);
				Cell previousCell = currentCell;
				Cell nextCell = mGridManager.currentFlowField.GetNeighborCell(currentCell);
				
				previousCell.unit = null; // Set the unit of the previous cell to null			
				currentCell.unit = unit; // Set the unit of the current cell
				currentCell.UpdateColor(unit.color);
				
				float unitSpeed = unit.speed;
				
				if (nextCell.unit != null) unitSpeed *= 0.5f; // slow down
				
				unit.transform.position += nextCell.GetVector3Velocity() * Time.fixedDeltaTime * unitSpeed;
			}
		}
	}

	private void UpdateUnitGoalDestination()
	{
		if (mUnitList.Count > 0)
		{
			if (mTimer <= 0)
			{
				mTimer = MAX_TIME;
				mGridManager.UpdateFlowField(GetRandomPositionWithinTheGrid());
			}
			else
				mTimer -= Time.deltaTime;
		}
	}
	
	private void SpawnUnits(bool isSpawnUnits)
	{
		if (isSpawnUnits && mUnitList.Count < MAX_UNIT_NUMBER)
        {
			for (int i = 0; i < UNIT_SPAWN_AMOUNT; i++)
			{
				if (mUnitList.Count >= MAX_UNIT_NUMBER) return;
				GameObject unit = Instantiate(mUnitPrefab);
				unit.transform.position = GetRandomPositionWithinTheGrid();
			}
		}
	}

	private void MoveCamera()
	{
		Vector3 cameraPos = Camera.main.transform.position;
		if (Input.GetKey(KeyCode.W))	  cameraPos.z += 1.0f;
		else if (Input.GetKey(KeyCode.A)) cameraPos.x -= 1.0f;
		else if (Input.GetKey(KeyCode.S)) cameraPos.z -= 1.0f;
		else if (Input.GetKey(KeyCode.D)) cameraPos.x += 1.0f;
		
		cameraPos.x = Mathf.Clamp(cameraPos.x, -screenWidth, screenWidth);
		cameraPos.z = Mathf.Clamp(cameraPos.z, -screenHeight, screenHeight);
		Camera.main.transform.position = cameraPos;
	}
	
	// GETTERS
	public List<Unit> GetUnitList(){return mUnitList;} 
	public int GetUnitListSize(){return mUnitList.Count;}
	public Vector3 GetRandomPositionWithinTheGrid()
	{
		Vector3 spawnLoc = new Vector3(
		Random.Range(mGridManager.gridOffset.x, mGridManager.gridSize.x + mGridManager.gridOffset.x), 0f,
		Random.Range(mGridManager.gridOffset.y, mGridManager.gridSize.y + mGridManager.gridOffset.y));
		return spawnLoc;
	}
	
	// SETTERS
	public void AddUnitToUnitList(Unit unit){mUnitList.Add(unit);}
	public void RemoveUnitFromUnitList(Unit unit){mUnitList.Remove(unit);}
}
