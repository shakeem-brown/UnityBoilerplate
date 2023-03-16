using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	[HideInInspector] [SerializeField] private GameObject mUnitPrefab;
	[SerializeField] private Vector2Int unitSpawnAmount; // x = current unit number, y = max unit number
	[SerializeField] private Vector2 timer; // x = current time, y = max time
	
	public GridManager mGridManager { get; private set; }
	public List<Unit> unitList { get; private set; }
	[HideInInspector] public bool isFluidSimulationActive;


    private void Awake() {
		mGridManager = GetComponent<GridManager>();
        unitList = new List<Unit>();
    }

    private void Start() {
		Vector3 cameraPos = Camera.main.transform.position;
		cameraPos.x = mGridManager.gridSize.x * mGridManager.cellRadius;
		cameraPos.z = mGridManager.gridSize.y * mGridManager.cellRadius;
		Camera.main.transform.position = cameraPos;
		Camera.main.orthographicSize = mGridManager.gridSize.y * mGridManager.cellRadius;
		
		mGridManager.InitalizeFlowField();
		SpawnUnits(true); // Spawning the units at the start of the game
	}

    private void Update() { 
		if (Input.GetMouseButton(0)) UpdateGoalDestinationMouseClick(); 
		UpdateFluidSimulation();
	}

    private void FixedUpdate() { GameLoop(); }
	
	private void UpdateFluidSimulation() {
		if (isFluidSimulationActive) {
			if (mGridManager.currentFluidSimulation != null) mGridManager.currentFluidSimulation.UpdateFluidSimulation();
		}
	}
	
	private void GameLoop() {
		// game modifications
		SpawnUnits(Input.GetKey(KeyCode.P));
		UpdateUnitPosition();
		
		// scene management
		if (Input.GetKey(KeyCode.Escape)) Application.Quit();
		else if (Input.GetKey(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
	
	private void UpdateUnitPosition() {
		if (mGridManager.currentFlowField != null) {
			foreach (Unit unit in unitList) {
				Cell currentCell = mGridManager.currentFlowField.GetCellFromWorldPosition(unit.transform.position);
				Cell nextCell = mGridManager.currentFlowField.GetNeighborCell(currentCell);
				
				unit.SetCurrentCell(currentCell);
				
				float unitSpeed = unit.speed;
				if (nextCell.unit != null) unitSpeed *= 0.5f; // slow down
				unit.transform.position += currentCell.GetVector3Velocity() * Time.fixedDeltaTime * unitSpeed;
				
				// if a unit reaches the goal cell then start a timer to change the destinationCell
				if (currentCell == mGridManager.currentFlowField.destinationCell) UpdateGoalDestinationTimer(); 
			}
		}
	}

	private void UpdateGoalDestinationTimer() {
		if (timer.x > 0) timer.x -= Time.deltaTime;
		else {
			timer.x = timer.y;
			mGridManager.UpdateFlowField(GetRandomPositionWithinTheGrid());
		}	
	}
	
	private void UpdateGoalDestinationMouseClick() {
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = Camera.main.nearClipPlane;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos);
		
		Vector3 nearestCellAtMousePosition = mGridManager.currentFlowField.GetCellFromWorldPosition(mousePos).worldPosition;
		mGridManager.UpdateFlowField(nearestCellAtMousePosition);
	}
	
	private void SpawnUnits(bool isSpawnUnits) {
		if (isSpawnUnits && unitList.Count < unitSpawnAmount.y) {
			for (int i = 0; i < unitSpawnAmount.x; i++) {
				if (unitList.Count >= unitSpawnAmount.y) return;
				GameObject unit = Instantiate(mUnitPrefab);
				unit.transform.position = mGridManager.currentFlowField.GetCellFromWorldPosition(GetRandomPositionWithinTheGrid()).worldPosition;
				unit.transform.localScale = Vector3.one * mGridManager.cellRadius;
			}
		}
	}
	
	private Vector3 GetRandomPositionWithinTheGrid() {
		int randomXIndex = Random.Range(1, mGridManager.gridSize.x - 1);
		int randomYIndex = Random.Range(1, mGridManager.gridSize.y - 1);
		return mGridManager.currentFlowField.grid[randomXIndex, randomYIndex].worldPosition;
	}
}
