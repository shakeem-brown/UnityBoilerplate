using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerUI : MonoBehaviour
{
    [SerializeField] private Text mNumberOfUnitsText;
	private GameManager mGM;
	private const string NUMBER_OF_UNITS_INTRO = "Number of Units: ";
	
    private void Awake()
    {
        mGM = GetComponent<GameManager>();
        mNumberOfUnitsText.text = NUMBER_OF_UNITS_INTRO;
    }

    public void UpdateUnitCount() { mNumberOfUnitsText.text = NUMBER_OF_UNITS_INTRO + mGM.GetUnitListSize(); }
}
