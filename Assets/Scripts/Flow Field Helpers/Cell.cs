using System.Collections.Generic;
using UnityEngine;

public class Cell
{
	public Vector3 worldPosition;
	public Vector2Int gridIndex;
	public List<Cell> neighborCells;
	
	// Pathfinding Values
	public Unit unit;
	public byte cost; // int value from 1 to 255
	public ushort bestCost;
	public Color vectorColor;
	
	// Fluid Simulation Values
	public Vector2 velocity;
	public float density;
	
	// Constructor
	public Cell(Vector3 worldPos, Vector2Int index) {
		worldPosition = worldPos;
		gridIndex = index;
		neighborCells = new List<Cell>();
		
		unit = null;
		cost = 1;
		bestCost = ushort.MaxValue;
		vectorColor = Color.white;
	
		velocity = Vector2.zero;
		density = 0f;
	}
	
	public Vector3 GetVector3Velocity() { return new Vector3(velocity.x, 0, velocity.y); }
	
	public void IncreaseCost(int val) {
		if (cost == byte.MaxValue) return;
		if (val + (int)cost >= 255) cost = byte.MaxValue;
		else cost += (byte)val;
	}
}
