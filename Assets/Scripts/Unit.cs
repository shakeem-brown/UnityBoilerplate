using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
	public Cell cell   { get; private set; }
	public Color color { get; private set; }
	public float speed { get; private set; }
	
	private const float OFFSET = 0.01f;
	
	private void Start() {
		FindObjectOfType<GameManager>().unitList.Add(this);
		//color = new Color(Random.value, Random.value, Random.value);
		color = Color.black;
		GetComponent<Renderer>().material.SetColor("_Color", color);
		speed = Random.Range(5f, 10f);
	}
	
	public Vector3 GetBorderOffset() {
		Vector3 offset = Vector3.zero;
		
		// check if the cell is a border cell and prevent the cell from leaving the grid by applying the offset
		if (cell != null) {
			if (cell.CheckIfBorderCell()) {
				if (cell.northCell == null) offset.z += -OFFSET;
				if (cell.eastCell == null) offset.x += -OFFSET;
				if (cell.southCell == null) offset.z += OFFSET;
				if (cell.westCell == null) offset.x += OFFSET;
			}
		}
		return offset;
	}
	
	// calculates the separation offset so that the unit can smoothly move away from the unit on the nextCell if that cell is occuiped
	public Vector3 GetSeparationOffset(Cell nextCell) {
		
		Vector3 offset = Vector3.zero;
		
		if (cell != null) {
			// handles what to do if the next cell is occupied by another unit
			if (nextCell.unit != null && nextCell.unit != this) { 
			
				foreach (Cell neighbor in nextCell.neighborCells) {
					Vector2Int neighborGridIndex = neighbor.gridIndex;
					
					if (neighborGridIndex.y + 1 == cell.gridIndex.y) {
						offset.z += OFFSET; // North
						if (neighborGridIndex.x + 1 == cell.gridIndex.x) offset.x += OFFSET; // North East	
						else if (neighborGridIndex.x - 1 == cell.gridIndex.x) offset.x += -OFFSET; // North West
					}
					else if (neighborGridIndex.y - 1 == cell.gridIndex.y) {
						offset.z += -OFFSET; // South
						if (neighborGridIndex.x + 1 == cell.gridIndex.x) offset.x += OFFSET; // South East	
						else if (neighborGridIndex.x - 1 == cell.gridIndex.x) offset.x += -OFFSET; // South West
					}
					else if (neighborGridIndex.x + 1 == cell.gridIndex.x) offset.x += OFFSET; // East	
					else if (neighborGridIndex.x - 1 == cell.gridIndex.x) offset.x += -OFFSET; // West
				}
			}
		}
		return offset;
	}
	
	public void SetCurrentCell(Cell newCurrentCell) {
		// the cell is empty/ not occupied
		if (newCurrentCell.unit == null) {
			if (cell != null) cell.unit = null;
			cell = newCurrentCell;
			cell.unit = this;
		}
		// the cell is occupied but by me
		else if (newCurrentCell.unit == this) return;
		// the cell is occupied but by not me
		else {
			if (cell != null) {
				// loop through a list of neighborCells
				foreach (Cell neighbor in newCurrentCell.neighborCells) {
					if (cell != null && neighbor == cell) continue; // skip you because you are already occuiped
				
					// the neighbor cell is empty/ not occuiped
					if (neighbor.unit == null) {
						if (cell != null) cell.unit = null;
						cell = neighbor;
						cell.unit = this;
						return; // break loop
					}
				}
			}
		}
	}
}
