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
	}

    private void Update() { 
		if (Input.GetMouseButton(0)) UpdateGoalDestinationMouseClick(); 
		if (isFluidSimulationActive) {
			if (mGridManager.currentFluidSimulation != null) mGridManager.currentFluidSimulation.UpdateFluidSimulation();
		}
	}

    private void FixedUpdate() { GameLoop(); }
	
	private void GameLoop() {
		// game modifications
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
				Vector3 newPosition = currentCell.GetVector3Velocity() * Time.fixedDeltaTime * unitSpeed;
				
				// check if the cell is a border cell and prevent the cell from leaving the grid
				if (currentCell.CheckIfBorderCell()) {
					if (currentCell.northCell == null) newPosition.z += -0.1f;
					if (currentCell.eastCell == null) newPosition.x += -0.1f;
					if (currentCell.southCell == null) newPosition.z += 0.1f;
					if (currentCell.westCell == null) newPosition.x += 0.1f;
				}
				
				unit.transform.position += newPosition;
				
				// if a unit reaches the goal cell then start a timer to change the destinationCell
				if (!isFluidSimulationActive) {
					if (currentCell == mGridManager.currentFlowField.destinationCell) UpdateGoalDestinationTimer(); 
				}
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
	
	private Vector3 GetRandomPositionWithinTheGrid() {
		int randomXIndex = Random.Range(1, mGridManager.gridSize.x - 1);
		int randomYIndex = Random.Range(1, mGridManager.gridSize.y - 1);
		return mGridManager.currentFlowField.grid[randomXIndex, randomYIndex].worldPosition;
	}
	
	// Accessors
	public void SpawnUnit(int numOfUnitsToSpawn) {
		if (unitList.Count < unitSpawnAmount.y) {
			for (int i = 0; i < numOfUnitsToSpawn; i++) {
				if (unitList.Count >= unitSpawnAmount.y) return; // safety check
				GameObject unit = Instantiate(mUnitPrefab);
				unit.transform.position = mGridManager.currentFlowField.GetCellFromWorldPosition(GetRandomPositionWithinTheGrid()).worldPosition;
				unit.transform.localScale = Vector3.one * mGridManager.cellRadius;
			}
		}
	}
	
	public void RemoveUnit(int numOfUnitsToRemove) {
		if (unitList.Count > 0) {
			for (int i = 0; i < numOfUnitsToRemove; i++) {
				if (unitList.Count == 0) return; // saftey check
				int randomIndex = Random.Range(0, unitList.Count - 1);
				Unit unit = unitList[randomIndex];
				Destroy(unit.gameObject);
				unitList.Remove(unit);
			}
		}
	}
}
