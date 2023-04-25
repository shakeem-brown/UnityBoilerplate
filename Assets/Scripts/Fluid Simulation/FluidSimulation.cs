using System.Collections;
using System.Linq;
using UnityEngine;

public class FluidSimulation
{
	public VectorField vectorField { get; private set; }
	public Vector2Int gridSize { get; private set; }
	public float cellRadius { get; private set; }
	public float cellDiameter { get; private set; }
	
	public float dt { get; private set; }
	public float diffision { get; private set; }
	public float viscosity { get; private set; }
	public int iterations = 20;
	
	public FluidSimulation(VectorField _vectorField, float _dt, float _diffision, float _viscosity) {
		vectorField = _vectorField;
		gridSize = vectorField.gridSize;
		cellRadius = vectorField.cellRadius;
		cellDiameter = vectorField.cellDiameter;
		
		dt = _dt;
		diffision = _diffision;
		viscosity = _viscosity;
	}
	
	public void Step() {
		DiffusePreviousVelocity(); // diffuse previous velocity
		ProjectPreviousVelocity(); // project previous velocity
		AdvectPreviousVelocity(); // advect previous velocity
        ProjectVelocity(); // project velocity
        DiffusePreviousDensity(); // diffuse previous density
		AdvectPreviousDensity(); // advect previous density
    }

    private void DiffusePreviousVelocity () {
		float cRecip = 1 / (dt * diffision * (gridSize.x - 2) * (gridSize.y - 2));
		for (int i = 0; i < iterations; i++) {
			foreach (Cell cell in vectorField.grid) {
				if (cell.CheckIfCardinalBorderCell()) continue;
				
				cell.previousVelocity = (
					cell.velocity + viscosity * 
					(cell.eastCell.velocity + cell.westCell.velocity +
					cell.northCell.velocity + cell.southCell.velocity) * 
					cRecip
				);
			}
		}
	}
	
	private void DiffusePreviousDensity () {
		float cRecip = 1 / (dt * diffision * (gridSize.x - 2) * (gridSize.y - 2));
		for (int i = 0; i < iterations; i++) {
			foreach (Cell cell in vectorField.grid) {
				if (cell.CheckIfCardinalBorderCell()) continue;
				
				cell.previousDensity = (
					cell.density + diffision * 
					(cell.eastCell.density + cell.westCell.density +
					cell.northCell.density + cell.southCell.density) * 
					cRecip
				);
			}
		}
	}
	
	private void ProjectPreviousVelocity() {
		foreach (Cell cell in vectorField.grid) {
			if (cell.CheckIfCardinalBorderCell()) {
				SetDivergenceBoundaries(cell);
				SetPressureBoundaries(cell);				
				continue;
			}
			
			cell.divergence = -0.5f * (
				((cell.eastCell.velocity.x - cell.westCell.velocity.x) / gridSize.x) +
				((cell.northCell.velocity.y - cell.southCell.velocity.y) / gridSize.y)
			);
			cell.pressure = 0;
		}
		
		
		float cRecip = 1 / (dt * diffision * (gridSize.x - 2) * (gridSize.y - 2));
		for (int i = 0; i < iterations; i++) {
			foreach (Cell cell in vectorField.grid) {
				if (cell.CheckIfCardinalBorderCell()) { 
					SetPressureBoundaries(cell);
					continue;
				}
				
				cell.pressure = (
					cell.divergence + 1 * 
					(cell.eastCell.divergence + cell.westCell.divergence +
					cell.northCell.divergence + cell.southCell.divergence) * 
					cRecip
				);
			}
		}
		
		foreach (Cell cell in vectorField.grid) {
			if (cell.CheckIfCardinalBorderCell()) {
				SetVelocityBoundaries(cell);
				continue;
			}
			
			cell.velocity.x -= 0.5f * (cell.eastCell.pressure - cell.westCell.pressure) * gridSize.x;
			cell.velocity.y -= 0.5f * (cell.northCell.pressure - cell.southCell.pressure) * gridSize.y;
		}	
	}
	
	private void ProjectVelocity() {
		foreach (Cell cell in vectorField.grid) {
			if (cell.CheckIfCardinalBorderCell()) {
				SetDivergenceBoundaries(cell);
				SetPressureBoundaries(cell);				
				continue;
			}
			
			cell.divergence = -0.5f * (
				((cell.eastCell.velocity.x - cell.westCell.velocity.x) / gridSize.x) +
				((cell.northCell.velocity.y - cell.southCell.velocity.y) / gridSize.y)
			);
			cell.pressure = 0;
		}
		
		float cRecip = 1 / (dt * diffision * (gridSize.x - 2) * (gridSize.y - 2));
		for (int i = 0; i < iterations; i++) {
			foreach (Cell cell in vectorField.grid) {
				if (cell.CheckIfCardinalBorderCell()) { 
					SetPressureBoundaries(cell);
					continue;
				}
				
				cell.pressure = (
					cell.divergence + 1 * 
					(cell.eastCell.divergence + cell.westCell.divergence +
					cell.northCell.divergence + cell.southCell.divergence) * 
					cRecip
				);
			}
		}
		
		foreach (Cell cell in vectorField.grid) {
			if (cell.CheckIfCardinalBorderCell()) { 
				SetVelocityBoundaries(cell);
				continue;
			}
			
			cell.velocity.x -= 0.5f * (cell.eastCell.pressure - cell.westCell.pressure) * gridSize.x;
			cell.velocity.y -= 0.5f * (cell.northCell.pressure - cell.southCell.pressure) * gridSize.y;
		}
	}
	
	private void SetVelocityBoundaries(Cell borderCell) {
		// handling cardinal cases
		if (borderCell.eastCell == null) borderCell.velocity.x = -borderCell.westCell.velocity.x;
		if (borderCell.westCell == null) borderCell.velocity.x = -borderCell.eastCell.velocity.x;
		if (borderCell.northCell == null) borderCell.velocity.y = -borderCell.southCell.velocity.y;
		if (borderCell.southCell == null) borderCell.velocity.y = -borderCell.northCell.velocity.y;
		// handling diagnol cases
		if (borderCell.northEastCell == null) borderCell.velocity = 0.5f * (borderCell.southCell.velocity + borderCell.westCell.velocity);
		if (borderCell.northWestCell == null) borderCell.velocity = 0.5f * (borderCell.southCell.velocity + borderCell.eastCell.velocity);
		if (borderCell.southEastCell == null) borderCell.velocity = 0.5f * (borderCell.northCell.velocity + borderCell.westCell.velocity);
		if (borderCell.southWestCell == null) borderCell.velocity = 0.5f * (borderCell.northCell.velocity + borderCell.eastCell.velocity);
	}
	
	private void SetDivergenceBoundaries(Cell borderCell) {
		// handling cardinal cases
		if (borderCell.eastCell == null) borderCell.divergence = -borderCell.westCell.divergence;
		if (borderCell.westCell == null) borderCell.divergence = -borderCell.eastCell.divergence;
		if (borderCell.northCell == null) borderCell.divergence = -borderCell.southCell.divergence;
		if (borderCell.southCell == null) borderCell.divergence = -borderCell.northCell.divergence;
		// handling diagnol cases
		if (borderCell.northEastCell == null) borderCell.divergence = 0.5f * (borderCell.southCell.divergence + borderCell.westCell.divergence);
		if (borderCell.northWestCell == null) borderCell.divergence = 0.5f * (borderCell.southCell.divergence + borderCell.eastCell.divergence);
		if (borderCell.southEastCell == null) borderCell.divergence = 0.5f * (borderCell.northCell.divergence + borderCell.westCell.divergence);
		if (borderCell.southWestCell == null) borderCell.divergence = 0.5f * (borderCell.northCell.divergence + borderCell.eastCell.divergence);
	}
	
	private void SetPressureBoundaries(Cell borderCell) {
		// handling cardinal cases
		if (borderCell.eastCell == null) borderCell.pressure = -borderCell.westCell.pressure;
		if (borderCell.westCell == null) borderCell.pressure = -borderCell.eastCell.pressure;
		if (borderCell.northCell == null) borderCell.pressure = -borderCell.southCell.pressure;
		if (borderCell.southCell == null) borderCell.pressure = -borderCell.northCell.pressure;
		// handling diagnol cases
		if (borderCell.northEastCell == null) borderCell.pressure = 0.5f * (borderCell.southCell.pressure + borderCell.westCell.pressure);
		if (borderCell.northWestCell == null) borderCell.pressure = 0.5f * (borderCell.southCell.pressure + borderCell.eastCell.pressure);
		if (borderCell.southEastCell == null) borderCell.pressure = 0.5f * (borderCell.northCell.pressure + borderCell.westCell.pressure);
		if (borderCell.southWestCell == null) borderCell.pressure = 0.5f * (borderCell.northCell.pressure + borderCell.eastCell.pressure);
	}
	
	private void SetDensityBoundaries(Cell borderCell) {
		// handling cardinal cases
		if (borderCell.eastCell == null) borderCell.density = -borderCell.westCell.density;
		if (borderCell.westCell == null) borderCell.density = -borderCell.eastCell.density;
		if (borderCell.northCell == null) borderCell.density = -borderCell.southCell.density;
		if (borderCell.southCell == null) borderCell.density = -borderCell.northCell.density;
		// handling diagnol cases
		if (borderCell.northEastCell == null) borderCell.density = 0.5f * (borderCell.southCell.density + borderCell.westCell.density);
		if (borderCell.northWestCell == null) borderCell.density = 0.5f * (borderCell.southCell.density + borderCell.eastCell.density);
		if (borderCell.southEastCell == null) borderCell.density = 0.5f * (borderCell.northCell.density + borderCell.westCell.density);
		if (borderCell.southWestCell == null) borderCell.density = 0.5f * (borderCell.northCell.density + borderCell.eastCell.density);
	}
	
	private void AdvectPreviousVelocity() {
		Vector2 i, j;
		Vector2 delta = new Vector2(dt * (gridSize.x - 2), dt * (gridSize.y - 2));
		
		Vector2 s, t;
		float tmp1, tmp2, x, y;
		float Nfloat = gridSize.x;
		
		foreach (Cell cell in vectorField.grid) {
			if (cell.CheckIfCardinalBorderCell()) continue;
			
			tmp1 = delta.x * cell.previousVelocity.x;
			tmp2 = delta.y * cell.previousVelocity.y;
			x = cell.gridIndex.x - tmp1; 
			y = cell.gridIndex.y - tmp2;
			
			if(x < 0.5f) x = 0.5f; 
			if(x > Nfloat + 0.5f) x = Nfloat + 0.5f; 
			i.x = Mathf.Floor(x); 
			i.y = i.x + 1.0f;
			
			if(y < 0.5f) y = 0.5f; 
			if(y > Nfloat + 0.5f) y = Nfloat + 0.5f; 
			j.x = Mathf.Floor(y);
			j.y = j.x + 1.0f; 
			
			s.y = x - i.x; 
			s.x = 1.0f - s.y; 
			t.y = y - j.x; 
			t.x = 1.0f - t.y;
			
			int i0i = (int)i.x;
			int i1i = (int)i.y;
			int j0i = (int)j.x;
			int j1i = (int)j.y;
			
			if (i0i > gridSize.x - 1 || i0i < 1) continue;
			if (i1i > gridSize.x - 1 || i1i < 1) continue;
			if (j0i > gridSize.y - 1 || j0i < 1) continue;
			if (j1i > gridSize.y - 1 || j1i < 1) continue;
			
			cell.velocity = (
	        s.x * (t.x * vectorField.grid[i0i, j0i].previousVelocity)
				+ (t.y * vectorField.grid[i0i, j1i].previousVelocity)
				+ s.y * (t.x * vectorField.grid[i1i, j0i].previousVelocity) 
				+ (t.y * vectorField.grid[i1i, j1i].previousVelocity)
			);
		}
	}
	
	private void AdvectPreviousDensity() {
		Vector2 i, j;
		Vector2 delta = new Vector2(dt * (gridSize.x - 2), dt * (gridSize.y - 2));
		
		Vector2 s, t;
		float tmp1, tmp2, x, y;
		float Nfloat = gridSize.x;
		
		foreach (Cell cell in vectorField.grid) {
			if (cell.CheckIfCardinalBorderCell()) continue;
			
			tmp1 = delta.x * cell.previousVelocity.x * cell.previousDensity;
			tmp2 = delta.y * cell.previousVelocity.y * cell.previousDensity;
			x = cell.gridIndex.x - tmp1; 
			y = cell.gridIndex.y - tmp2;
			
			if(x < 0.5f) x = 0.5f; 
			if(x > Nfloat + 0.5f) x = Nfloat + 0.5f; 
			i.x = Mathf.Floor(x); 
			i.y = i.x + 1.0f;
			
			if(y < 0.5f) y = 0.5f; 
			if(y > Nfloat + 0.5f) y = Nfloat + 0.5f; 
			j.x = Mathf.Floor(y);
			j.y = j.x + 1.0f; 
			
			s.y = x - i.x; 
			s.x = 1.0f - s.y; 
			t.y = y - j.x; 
			t.x = 1.0f - t.y;
			
			int i0i = (int)i.x;
			int i1i = (int)i.y;
			int j0i = (int)j.x;
			int j1i = (int)j.y;
			
			if (i0i > gridSize.x - 1 || i0i < 1) continue;
			if (i1i > gridSize.x - 1 || i1i < 1) continue;
			if (j0i > gridSize.y - 1 || j0i < 1) continue;
			if (j1i > gridSize.y - 1 || j1i < 1) continue;
			
			cell.density = (
	        s.x * (t.x * vectorField.grid[i0i, j0i].previousDensity)
				+ (t.y * vectorField.grid[i0i, j1i].previousDensity)
				+ s.y * (t.x * vectorField.grid[i1i, j0i].previousDensity) 
				+ (t.y * vectorField.grid[i1i, j1i].previousDensity)
			);
		}
	}
}