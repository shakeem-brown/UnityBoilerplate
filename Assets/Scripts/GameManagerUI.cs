using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerUI : MonoBehaviour
{
	[HideInInspector] [SerializeField] private GameManager mGM;
	[HideInInspector] [SerializeField] private GridManagerDebug mGridManagerDebug;
	
	// FPS
    [HideInInspector] [SerializeField] private Text fpsText;
	private string fpsIntro;
	private int lastFrameIndex;
	private float[] frameDeltaTimeArray = new float[50];
	
	// Unit Count
    [HideInInspector] [SerializeField] private Text unitCountText;
	private string unitCountIntro;
	
	// Buttons
	[HideInInspector] [SerializeField] private Button gridButton;
	[HideInInspector] [SerializeField] private Button flowFieldButton;
	[HideInInspector] [SerializeField] private Button fluidSimulationButton;
	
	[HideInInspector] [SerializeField] private Button spawn1UnitButton;
	[HideInInspector] [SerializeField] private Button remove1UnitButton;
	[HideInInspector] [SerializeField] private Button spawn50UnitButton;
	[HideInInspector] [SerializeField] private Button remove50UnitButton;
	
	
    private void Start() { 
		fpsIntro = fpsText.text; 
		unitCountIntro = unitCountText.text; 
		
		gridButton.onClick.AddListener(OnGridButtonClick);
		flowFieldButton.onClick.AddListener(OnFlowFieldButtonClick);
		fluidSimulationButton.onClick.AddListener(OnFluidSimulationButtonClick);
		
		spawn1UnitButton.onClick.AddListener(OnSpawnOneUnitButtonClick);
		remove1UnitButton.onClick.AddListener(OnRemoveOneUnitButtonClick);
		spawn50UnitButton.onClick.AddListener(OnSpawnFiftyUnitButtonClick);
		remove50UnitButton.onClick.AddListener(OnRemoveFiftyUnitButtonClick);
		
		StartCoroutine(UpdateGameUI());
	}
	
	private void OnGridButtonClick() { mGridManagerDebug.ToogleGridVisibility(); }
	private void OnFlowFieldButtonClick() { mGridManagerDebug.ToogleFlowFieldDisplay(); }
	private void OnFluidSimulationButtonClick() { mGridManagerDebug.ToogleFluidSimulationDisplay(); }
	private void OnSpawnOneUnitButtonClick () { mGM.SpawnUnit(1); }
	private void OnRemoveOneUnitButtonClick () { mGM.RemoveUnit(1); }
	private void OnSpawnFiftyUnitButtonClick () { mGM.SpawnUnit(50); }
	private void OnRemoveFiftyUnitButtonClick () { mGM.RemoveUnit(50); }
	
	private IEnumerator UpdateGameUI() {
        while (true) {
			// FPS
			frameDeltaTimeArray[lastFrameIndex] = Time.deltaTime;
			lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;
            fpsText.text = fpsIntro + Mathf.RoundToInt(GetFPS()).ToString();
			
			// Unit Count
			unitCountText.text = unitCountIntro + mGM.unitList.Count;
			
            yield return new WaitForSeconds(0.2f);
        }
    }
	
	private float GetFPS() {
		float totalFPS = 0f;
		foreach (float deltaTime in frameDeltaTimeArray) {
			totalFPS += deltaTime;
		}
		return frameDeltaTimeArray.Length / totalFPS;
	}
}
