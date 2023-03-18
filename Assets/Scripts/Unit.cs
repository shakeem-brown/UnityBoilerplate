using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
	public Cell cell   { get; private set; }
	public Color color { get; private set; }
	public float speed { get; private set; }
	
	private void Start() {
		FindObjectOfType<GameManager>().unitList.Add(this);
		color = new Color(Random.value, Random.value, Random.value);
		GetComponent<Renderer>().material.SetColor("_Color", color);
		speed = Random.Range(5f, 10f);
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
