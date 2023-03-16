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
	
	
    private void Start() { 
		fpsIntro = fpsText.text; 
		unitCountIntro = unitCountText.text; 
		
		gridButton.onClick.AddListener(OnGridButtonClick);
		flowFieldButton.onClick.AddListener(OnFlowFieldButtonClick);
		fluidSimulationButton.onClick.AddListener(OnFluidSimulationButtonClick);
		
		StartCoroutine(UpdateGameUI());
	}
	
	private void OnGridButtonClick() {
		mGridManagerDebug.ToogleGridVisibility();
	}
	
	private void OnFlowFieldButtonClick() {
		mGridManagerDebug.ToogleFlowFieldDisplay();
	}
	
	private void OnFluidSimulationButtonClick() {
		mGM.isFluidSimulationActive = !mGM.isFluidSimulationActive;
	}
	
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
