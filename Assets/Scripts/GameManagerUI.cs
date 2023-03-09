using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerUI : MonoBehaviour
{
    [SerializeField] private Text mNumberOfUnitsText;
	private const string UNIT_COUNT_INTRO = "Unit Count: ";
	
    private void Start() { mNumberOfUnitsText.text = UNIT_COUNT_INTRO; }

    public void UpdateUnitCount() { mNumberOfUnitsText.text = UNIT_COUNT_INTRO + GetComponent<GameManager>().GetUnitListSize(); }
}
