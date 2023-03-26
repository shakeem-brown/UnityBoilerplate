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
	
	public int iterations = 10;
	private Vector3 previousMousePos;
	
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
		DiffuseVelocity(); // diffuse previous velocity
		Project(); // project previous velocity
		AdvectVelocity(); // advect previous velocity
		Project(); // project velocity
		DiffuseDensity(); // diffuse density
		AdvectDensity(); // advect density
	}
	
	private void DiffuseVelocity () {
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
	
	private void DiffuseDensity () {
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
			if (cell.CheckIfCardinalBorderCell()) continue;
			
			cell.divergence = -0.5f * (
				((cell.eastCell.velocity.x - cell.westCell.velocity.x) / gridSize.x) +
				((cell.northCell.velocity.y - cell.southCell.velocity.y) / gridSize.y)
			);
			cell.pressure = 0;
		}
		SetDivergenceBoundaries();
		SetPressureBoundaries();
		
		float cRecip = 1 / 6;
		for (int i = 0; i < iterations; i++) {
			foreach (Cell cell in vectorField.grid) {
				if (cell.CheckIfCardinalBorderCell()) continue;
				
				cell.pressure = (
					cell.divergence + 1 * 
					(cell.eastCell.divergence + cell.westCell.divergence +
					cell.northCell.divergence + cell.southCell.divergence) * 
					cRecip
				);
			}
		}
		
		foreach (Cell cell in vectorField.grid) {
			if (cell.CheckIfCardinalBorderCell()) continue;
			
			cell.velocity.x -= 0.5f * (cell.eastCell.pressure - cell.westCell.pressure) * gridSize.x;
			cell.velocity.y -= 0.5f * (cell.northCell.pressure - cell.southCell.pressure) * gridSize.y;
		}
		SetVelocityBoundaries();
	}
	
	private void Project() {
		foreach (Cell cell in vectorField.grid) {
			if (cell.CheckIfCardinalBorderCell()) continue;
			
			cell.divergence = -0.5f * (
				((cell.eastCell.velocity.x - cell.westCell.velocity.x) / gridSize.x) +
				((cell.northCell.velocity.y - cell.southCell.velocity.y) / gridSize.y)
			);
			cell.pressure = 0;
		}
		SetDivergenceBoundaries();
		SetPressureBoundaries();
		
		float cRecip = 1 / 6;
		for (int i = 0; i < iterations; i++) {
			foreach (Cell cell in vectorField.grid) {
				if (cell.CheckIfCardinalBorderCell()) continue;
				
				cell.pressure = (
					cell.divergence + 1 * 
					(cell.eastCell.divergence + cell.westCell.divergence +
					cell.northCell.divergence + cell.southCell.divergence) * 
					cRecip
				);
			}
		}
		
		foreach (Cell cell in vectorField.grid) {
			if (cell.CheckIfCardinalBorderCell()) continue;
			
			cell.velocity.x -= 0.5f * (cell.eastCell.pressure - cell.westCell.pressure) * gridSize.x;
			cell.velocity.y -= 0.5f * (cell.northCell.pressure - cell.southCell.pressure) * gridSize.y;
		}
		SetVelocityBoundaries();
	}
	
	private void SetVelocityBoundaries() {
		foreach (Cell cell in vectorField.grid) {
			// handling cardinal cases
			if (cell.eastCell == null) cell.velocity.x = -cell.westCell.velocity.x;
			if (cell.westCell == null) cell.velocity.x = -cell.eastCell.velocity.x;
			if (cell.northCell == null) cell.velocity.y = -cell.southCell.velocity.y;
			if (cell.southCell == null) cell.velocity.y = -cell.northCell.velocity.y;
			// handling diagnol cases
			if (cell.northEastCell == null) cell.velocity = 0.5f * (cell.southCell.velocity + cell.westCell.velocity);
			if (cell.northWestCell == null) cell.velocity = 0.5f * (cell.southCell.velocity + cell.eastCell.velocity);
			if (cell.southEastCell == null) cell.velocity = 0.5f * (cell.northCell.velocity + cell.westCell.velocity);
			if (cell.southWestCell == null) cell.velocity = 0.5f * (cell.northCell.velocity + cell.eastCell.velocity);
		}
	}
	
	private void SetDivergenceBoundaries() {
		foreach (Cell cell in vectorField.grid) {
			// handling cardinal cases
			if (cell.eastCell == null) cell.divergence = -cell.westCell.divergence;
			if (cell.westCell == null) cell.divergence = -cell.eastCell.divergence;
			if (cell.northCell == null) cell.divergence = -cell.southCell.divergence;
			if (cell.southCell == null) cell.divergence = -cell.northCell.divergence;
			// handling diagnol cases
			if (cell.northEastCell == null) cell.divergence = 0.5f * (cell.southCell.divergence + cell.westCell.divergence);
			if (cell.northWestCell == null) cell.divergence = 0.5f * (cell.southCell.divergence + cell.eastCell.divergence);
			if (cell.southEastCell == null) cell.divergence = 0.5f * (cell.northCell.divergence + cell.westCell.divergence);
			if (cell.southWestCell == null) cell.divergence = 0.5f * (cell.northCell.divergence + cell.eastCell.divergence);
		}
	}
	
	private void SetPressureBoundaries() {
		foreach (Cell cell in vectorField.grid) {
			// handling cardinal cases
			if (cell.eastCell == null) cell.pressure = -cell.westCell.pressure;
			if (cell.westCell == null) cell.pressure = -cell.eastCell.pressure;
			if (cell.northCell == null) cell.pressure = -cell.southCell.pressure;
			if (cell.southCell == null) cell.pressure = -cell.northCell.pressure;
			// handling diagnol cases
			if (cell.northEastCell == null) cell.pressure = 0.5f * (cell.southCell.pressure + cell.westCell.pressure);
			if (cell.northWestCell == null) cell.pressure = 0.5f * (cell.southCell.pressure + cell.eastCell.pressure);
			if (cell.southEastCell == null) cell.pressure = 0.5f * (cell.northCell.pressure + cell.westCell.pressure);
			if (cell.southWestCell == null) cell.pressure = 0.5f * (cell.northCell.pressure + cell.eastCell.pressure);
		}
	}
	
	private void AdvectVelocity() {
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
			
			if (i0i > gridSize.x - 1) continue;
			if (i1i > gridSize.x - 1) continue;
			if (j0i > gridSize.x - 1) continue;
			if (j1i > gridSize.x - 1) continue;
			
			cell.velocity = (
	        s.x * (t.x * vectorField.grid[i0i, j0i].previousVelocity)
				+ (t.y * vectorField.grid[i0i, j1i].previousVelocity)
				+ s.y * (t.x * vectorField.grid[i1i, j0i].previousVelocity) 
				+ (t.y * vectorField.grid[i1i, j1i].previousVelocity)
			);
		}
		SetVelocityBoundaries();
	}
	
	private void AdvectDensity() {
		Vector2 i, j;
		Vector2 delta = new Vector2(dt * (gridSize.x - 2), dt * (gridSize.y - 2));
		
		Vector2 s, t;
		float tmp1, tmp2, x, y;
		float Nfloat = gridSize.x;
		
		foreach (Cell cell in vectorField.grid) {
			if (cell.CheckIfCardinalBorderCell()) continue;
			
			tmp1 = delta.x * cell.velocity.x;
			tmp2 = delta.y * cell.velocity.y;
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
			
			if (i0i > gridSize.x - 1) continue;
			if (i1i > gridSize.x - 1) continue;
			if (j0i > gridSize.x - 1) continue;
			if (j1i > gridSize.x - 1) continue;
			
			cell.density = (
	        s.x * (t.x * vectorField.grid[i0i, j0i].previousDensity)
				+ (t.y * vectorField.grid[i0i, j1i].previousDensity)
				+ s.y * (t.x * vectorField.grid[i1i, j0i].previousDensity) 
				+ (t.y * vectorField.grid[i1i, j1i].previousDensity)
			);
		}
		SetVelocityBoundaries();
	}
}