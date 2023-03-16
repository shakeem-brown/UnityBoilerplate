using System.Collections.Generic;
using UnityEngine;

public class Cell
{
	// generic 
	public Vector3 worldPosition;
	public Vector2Int gridIndex;
	public Vector2 velocity;
	
	// neighbor info
	public List<Cell> neighborCells;
	public Cell northCell;
	public Cell eastCell;
	public Cell southCell;
	public Cell westCell;
	
	// visualization
	public Color vectorColor;
	public GameObject gameObject;
	
	// Pathfinding Values
	public Unit unit;
	public byte cost; // int value from 1 to 255
	public ushort bestCost;
	
	// Fluid Simulation Values
	public float density;
	public float pressure;
	public Vector2 pressureGradient;
	public float viscosity;
	
	// Constructor
	public Cell(Vector3 worldPos, Vector2Int index) {
		// generic values init
		worldPosition = worldPos;
		gridIndex = index;
		velocity = Vector2.zero;
		
		// neighbor info init
		neighborCells = new List<Cell>();
		northCell = null;
		eastCell = null;
		southCell = null;
		westCell = null;
		
		// visualization values init
		gameObject = null;
		vectorColor = Color.white;
		
		// Pathfinding values init
		unit = null;
		cost = 1;
		bestCost = ushort.MaxValue;
		
		// Fluid Simulation values init
		density = 1.0f; // cannot == 0
		pressure = 0.8f;
		pressureGradient = new Vector2(1 , 0.2f);
		viscosity = 7.5f;
	}
	
	public Vector3 GetVector3Velocity() { return new Vector3(velocity.x, 0, velocity.y).normalized; }
	
	public bool CheckIfBorderCell() { return (northCell == null || eastCell == null || southCell == null || westCell == null); }
	
	public void IncreaseCost(int val) {
		if (cost == byte.MaxValue) return;
		if (val + (int)cost >= 255) cost = byte.MaxValue;
		else cost += (byte)val;
	}
}
