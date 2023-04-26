using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
	[HideInInspector] [SerializeField] private GameObject mUnitPrefab;
    [HideInInspector] [SerializeField] private GameObject mBoatPrefab;
	[SerializeField] private Vector2Int unitSpawnAmount; // x = current unit number, y = max unit number
	[SerializeField] private Vector2 timer; // x = current time, y = max time
	
	public GridManager mGridManager { get; private set; }
	public List<Unit> unitList { get; private set; }
	public int score { get; private set; }
	public int level { get; private set; }

    private void Awake() {
		// define helpful/ important variables
		mGridManager = GetComponent<GridManager>();
        unitList = new List<Unit>();
		score = 0;
		level = 1;
    }

    private void Start() {
		// set camera properties based on the grid manager's properties
		Vector3 cameraPos = Camera.main.transform.position;
		cameraPos.x = mGridManager.gridSize.x * mGridManager.cellRadius;
		cameraPos.z = mGridManager.gridSize.y * mGridManager.cellRadius;
		Camera.main.transform.position = cameraPos;
		Camera.main.orthographicSize = mGridManager.gridSize.y * mGridManager.cellRadius;

		SpawnUnit(unitSpawnAmount.x);
    }

    private void FixedUpdate() { GameLoop(); }
	
	private void GameLoop() {
		// game modifications
		UpdateUnitsSpawned();
        UpdateUnitPosition();
		
		// scene management
		if (Input.GetKey(KeyCode.Escape)) Application.Quit();
		else if (Input.GetKey(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
	
	private void UpdateUnitPosition() {
		List<Unit> unitDeletionList = new List<Unit>();

		// loop that handles unit behavior in regards to the occuiped cell in the vector field
        foreach (Unit unit in unitList) {
			Cell currentCell = mGridManager.vectorField.GetCellFromWorldPosition(unit.transform.position);
			unit.SetCurrentCell(currentCell);
				
			Cell nextCell = mGridManager.vectorField.GetCellFromWorldPosition(currentCell.worldPosition + currentCell.GetVector3Velocity());
				
			Vector3 newPosition = unit.GetSeparationOffset(nextCell); // offset the new position from collision with other units
			newPosition += currentCell.GetVector3Velocity() * Time.fixedDeltaTime * unit.speed; // apply the current cell velocity
			newPosition += unit.GetBorderOffset(); // apply the border offset when colliding with the border
			unit.transform.position += newPosition; // update the unit position

            // checks if a unit reaches the goal cell then start a timer to change the destinationCell
            if (mGridManager.flowField != null)  if (currentCell == mGridManager.flowField.destinationCell) UpdateGoalDestinationTimer();

			// check if the score needs to be updated
			UpdateScore(unitDeletionList, currentCell);
        }

		// loop that controls the deletion of units
		foreach (Unit unit in unitDeletionList) {
			unitList.Remove(unit);
			Destroy(unit.gameObject);
        }
    }

    // update the score depending on the unit's grid index
    private void UpdateScore(List<Unit> unitDeletionList, Cell currentCell)
	{
		// the unit is on the right side of the screen award them points
		if (currentCell.gridIndex.x > mGridManager.gridSize.x - 5) {
			score += 3;
            unitDeletionList.Add(currentCell.unit);
        }
		// the unit is on the "wrong" side of the screen so penalize them and delete them
		else if (currentCell.gridIndex.x < 1) {
			score -= 1;
            unitDeletionList.Add(currentCell.unit);
        }
        else if (currentCell.gridIndex.y == mGridManager.gridSize.y - 1) {
			score -= 1;
            unitDeletionList.Add(currentCell.unit);
        }
        else if (currentCell.gridIndex.y < 1) {
			score -= 1;
            unitDeletionList.Add(currentCell.unit);
        }
        
		score = Mathf.Clamp(score, 0, 999999);
		level = (score % 10) + 1;
    }


	// changes the goal node to a random non border cell: GetRandomCellWithinTheGrid()
	private void UpdateGoalDestinationTimer() {
		if (timer.x > 0) timer.x -= Time.deltaTime;
		else {
			timer.x = timer.y;
			mGridManager.UpdateFlowField(mGridManager.vectorField.GetRandomCellWithinTheGrid());
		}	
	}

	private void UpdateUnitsSpawned()
	{
		if (timer.x > 0) timer.x -= Time.deltaTime;
		else {
			timer.x = timer.y;
			SpawnUnit(unitSpawnAmount.x * level);
        }
	}

	/// Accessors
	// spawns "numOfUnitsToSpawn" amount of units at a random non border cell on the grid: GetRandomCellWithinTheGrid()
	// it to the grid's cell radius, when a unit is Instantiated it is already added to the game manager's unitList
	public void SpawnUnit(int numOfUnitsToSpawn) {
		if (unitList.Count < unitSpawnAmount.y * level) {
			for (int i = 0; i < numOfUnitsToSpawn; i++) {
				if (unitList.Count >= unitSpawnAmount.y * level) return; // safety check
                //GameObject unit = Instantiate(mUnitPrefab);
                //unit.transform.position = mGridManager.vectorField.GetRandomCellWithinTheGrid().worldPosition;
				//unit.transform.localScale = Vector3.one * mGridManager.cellRadius;

                GameObject unit = Instantiate(mBoatPrefab);
				int ranomdXIndex = Random.Range(5, 10);
				int randomYIndex = Random.Range(ranomdXIndex, mGridManager.gridSize.y - 5);
                unit.transform.position = mGridManager.vectorField.grid[5, randomYIndex].worldPosition;
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
