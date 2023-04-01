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
	public Cell northEastCell;
	public Cell northWestCell;
	public Cell southEastCell;
	public Cell southWestCell;
	
	// visualization
	public GameObject vector;
	public GameObject cube;
	public Color vectorColor;
	
	// Pathfinding Values
	public Unit unit;
	public byte cost; // int value from 1 to 255
	public ushort bestCost;
	
	// Fluid Simulation Values
	public Vector2 previousVelocity;
	public float previousDensity;
	public float density;
	public float pressure;
	public float divergence;
	
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
		northEastCell = null;
		northWestCell = null;
		southEastCell = null;
		southWestCell = null;
		
		// visualization values init
		vector = null;
		cube = null;
		vectorColor = Color.white;
		
		// Pathfinding values init
		unit = null;
		cost = 1;
		bestCost = ushort.MaxValue;
		
		// Fluid Simulation values init
		previousVelocity = Vector2.zero;
		previousDensity = 0;
		density = 0;
		pressure = 0;
		divergence = 0;
	}
	
	public Vector3 GetVector3Velocity() { return new Vector3(velocity.x, 0, velocity.y).normalized; }

	public bool CheckIfCardinalBorderCell() { return (northCell == null || eastCell == null || southCell == null || westCell == null); }	
	public bool CheckIfDiagnolBorderCell() { return (northEastCell == null || northWestCell == null || southEastCell == null || southWestCell == null); }
	public bool CheckIfImpassible() { return (cost == byte.MaxValue); }
	
	public void IncreaseCost(int val) {
		if (cost == byte.MaxValue) return;
		if (val + (int)cost >= 255) cost = byte.MaxValue;
		else cost += (byte)val;
	}
}
