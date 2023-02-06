using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
	private GameManager mGM;
	private Cell mCurrentCell;
	private Cell mPreviousCell;

	private void Start()
	{
		mGM = FindObjectOfType<GameManager>();
		mGM.AddUnitToUnitList(this);
	}

    private void Update()
    {
		if (mPreviousCell != null) 
		{
			// update the unit cell's occupancy
			if (mPreviousCell != mCurrentCell)
			{
				mPreviousCell.mIsOccupied = false;
				mPreviousCell = mCurrentCell;
			}
		}
    }

    // GETTERS
    public Cell GetCurrentCell()  { return mCurrentCell;  }
    public Cell GetPreviousCell() { return mPreviousCell; }

	// SETTERS
	public void SetCurrentCell(Cell cell)  { mCurrentCell  = cell; }
	public void SetPreviousCell(Cell cell) { mPreviousCell = cell; }
}
