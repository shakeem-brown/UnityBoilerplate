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
		// define helpful/ important variables
		mGridManager = GetComponent<GridManager>();
        unitList = new List<Unit>();
    }

    private void Start() {
		// set camera properties based on the grid manager's properties
		Vector3 cameraPos = Camera.main.transform.position;
		cameraPos.x = mGridManager.gridSize.x * mGridManager.cellRadius;
		cameraPos.z = mGridManager.gridSize.y * mGridManager.cellRadius;
		Camera.main.transform.position = cameraPos;
		Camera.main.orthographicSize = mGridManager.gridSize.y * mGridManager.cellRadius;
		
		// init the flow field
		mGridManager.InitalizeFlowField();
	}

    private void Update() { 
		// manipulates the field if the right mouse button is pressed
		if (Input.GetMouseButton(0)) UpdateGoalDestinationMouseClick(); 
		
		// activates the current field to simulate fluid if the bool: isFluidSimulationActive is true
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
				Cell nextCell = mGridManager.currentFlowField.GetCellFromWorldPosition(currentCell.worldPosition + currentCell.GetVector3Velocity());
				
				// do nove to next cell if the next cell is occuiped
				if (nextCell.unit != null && nextCell.unit != unit) continue;
				unit.SetCurrentCell(currentCell);
			
				Vector3 newPosition = currentCell.GetVector3Velocity() * Time.fixedDeltaTime * unit.speed;
				// check if the cell is a border cell and prevent the cell from leaving the grid
				if (currentCell.CheckIfBorderCell()) {
					if (currentCell.northCell == null) newPosition.z += -0.1f;
					if (currentCell.eastCell == null) newPosition.x += -0.1f;
					if (currentCell.southCell == null) newPosition.z += 0.1f;
					if (currentCell.westCell == null) newPosition.x += 0.1f;
				}
				unit.transform.position += newPosition;
				
				// checks if a unit reaches the goal cell then start a timer to change the destinationCell
				if (!isFluidSimulationActive) {
					if (currentCell == mGridManager.currentFlowField.destinationCell) UpdateGoalDestinationTimer(); 
				}
			}
		}
	}
	
	// changes the goal node to a random non border cell position: GetRandomPositionWithinTheGrid()
	private void UpdateGoalDestinationTimer() {
		if (timer.x > 0) timer.x -= Time.deltaTime;
		else {
			timer.x = timer.y;
			mGridManager.UpdateFlowField(GetRandomPositionWithinTheGrid());
		}	
	}
	
	// changes the goal node to the nearest cell at the mouse position
	private void UpdateGoalDestinationMouseClick() {
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = Camera.main.nearClipPlane;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos);
		Cell nearestCellAtMousePosition = mGridManager.currentFlowField.GetCellFromWorldPosition(mousePos);
		
		// check if the mouse clicked an area outside the grid or a border cell
		if (nearestCellAtMousePosition.gridIndex.x - 1 < 0 ||
			nearestCellAtMousePosition.gridIndex.y - 1 < 0 ||
			nearestCellAtMousePosition.gridIndex.x + 1 > mGridManager.gridSize.x - 1 ||
			nearestCellAtMousePosition.gridIndex.y + 1 > mGridManager.gridSize.y - 1) {
				
			float squareDist = (new Vector3(mousePos.x, 0, mousePos.z) - nearestCellAtMousePosition.worldPosition).sqrMagnitude;
			float squareCellRadius = mGridManager.cellRadius * mGridManager.cellRadius;
			
			// check if the distance between the mousePos and the border cell position to confirm if the border cell was clicked
			if (squareDist > squareCellRadius) return;
		}
		// sets the goal cell to the nearest cell at the mouse position
		mGridManager.UpdateFlowField(nearestCellAtMousePosition.worldPosition);
	}
	
	// gets a random non border cell via gridIndex and returns that cell's world position
	private Vector3 GetRandomPositionWithinTheGrid() {
		int randomXIndex = Random.Range(1, mGridManager.gridSize.x - 1);
		int randomYIndex = Random.Range(1, mGridManager.gridSize.y - 1);
		return mGridManager.currentFlowField.grid[randomXIndex, randomYIndex].worldPosition;
	}
	
	/// Accessors
	// spawns "numOfUnitsToSpawn" amount of units at a random non border cell on the grid: GetRandomPositionWithinTheGrid()
	// it to the grid's cell radius, when a unit is Instantiated it is already added to the game manager's unitList
	public void SpawnUnit(int numOfUnitsToSpawn) {
		if (unitList.Count < unitSpawnAmount.y) {
			for (int i = 0; i < numOfUnitsToSpawn; i++) {
				if (unitList.Count >= unitSpawnAmount.y) return; // safety check
				GameObject unit = Instantiate(mUnitPrefab);
				unit.transform.position = GetRandomPositionWithinTheGrid();
				unit.transform.localScale = Vector3.one * mGridManager.cellRadius;
			}
		}
	}
	
	// removes "numOfUnitsToRemove" amount of units from a randomly genergated gridIndex
	// the unit is destroyed and removed from the game manager's unitList
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
