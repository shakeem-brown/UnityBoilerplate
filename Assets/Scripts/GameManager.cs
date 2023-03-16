using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	[SerializeField] private GameObject mUnitPrefab;
	[SerializeField] private Vector2Int unitSpawnAmount; // x = current unit number, y = max unit number
	[SerializeField] private Vector2 timer; // x = current time, y = max time
	
	public GridManager mGridManager { get; private set; }
	public List<Unit> mUnitList { get; private set; }
	
	private float screenWidth;
	private float screenHeight;
	
    private void Awake() {
		mGridManager = GetComponent<GridManager>();
        mUnitList = new List<Unit>();
    }

	
    private void Start() {
		screenWidth = mGridManager.gridSize.x * (mGridManager.cellRadius * 2f);
		screenHeight = mGridManager.gridSize.y * (mGridManager.cellRadius * 2f);
		
		Vector3 cameraPos = Camera.main.transform.position;
		cameraPos.x = mGridManager.gridSize.x * 0.5f;
		cameraPos.z = mGridManager.gridSize.y * 0.5f;
		Camera.main.transform.position = cameraPos;
		Camera.main.orthographicSize = mGridManager.gridSize.y * 0.5f;
		
		mGridManager.InitalizeFlowField();
		SpawnUnits(true); // Spawning the units at the start of the game
	}

    private void Update() { 
		UpdateGoalDestination(); 
		if (mGridManager.currentFluidSimulation != null) mGridManager.currentFluidSimulation.ApplyFluidSimulation();
	}

    private void FixedUpdate() { GamePlayControls(); }
	
	private void GamePlayControls() {
		// game modifications
		SpawnUnits(Input.GetKey(KeyCode.P));
		UpdateUnitPosition();
		
		// key inputs
		MoveCamera();
		
		// scene management
		if (Input.GetKey(KeyCode.Escape)) Application.Quit();
		else if (Input.GetKey(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
	
	private void UpdateUnitPosition() {
		if (mGridManager.currentFlowField != null) {
			foreach (Unit unit in mUnitList) {
				Cell currentCell = mGridManager.currentFlowField.GetCellFromWorldPosition(unit.transform.position);
				Cell nextCell = mGridManager.currentFlowField.GetNeighborCell(currentCell);
				
				unit.SetCurrentCell(currentCell);
				
				float unitSpeed = unit.speed;
				if (nextCell.unit != null) unitSpeed *= 0.5f; // slow down
				unit.transform.position += currentCell.GetVector3Velocity() * Time.fixedDeltaTime * unitSpeed;
			}
		}
	}

	private void UpdateGoalDestination() {
		if (timer.x > 0) timer.x -= Time.deltaTime;
		else {
			timer.x = timer.y;
			mGridManager.UpdateFlowField(GetRandomPositionWithinTheGrid());
		}	
	}
	
	private void SpawnUnits(bool isSpawnUnits) {
		if (isSpawnUnits && mUnitList.Count < unitSpawnAmount.y) {
			for (int i = 0; i < unitSpawnAmount.x; i++) {
				if (mUnitList.Count >= unitSpawnAmount.y) return;
				GameObject unit = Instantiate(mUnitPrefab);
				unit.transform.position = mGridManager.currentFlowField.GetCellFromWorldPosition(GetRandomPositionWithinTheGrid()).worldPosition;
				unit.transform.localScale = Vector3.one * mGridManager.cellRadius;
			}
		}
	}

	private void MoveCamera() {
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
	public Vector3 GetRandomPositionWithinTheGrid() {
		Vector3 spawnLoc = new Vector3(
		Random.Range(mGridManager.gridOffset.x, mGridManager.gridSize.x + mGridManager.gridOffset.x), 0f,
		Random.Range(mGridManager.gridOffset.y, mGridManager.gridSize.y + mGridManager.gridOffset.y));
		return spawnLoc;
	}
	
	// SETTERS
	public void AddUnitToUnitList(Unit unit){mUnitList.Add(unit);}
	public void RemoveUnitFromUnitList(Unit unit){mUnitList.Remove(unit);}
}
