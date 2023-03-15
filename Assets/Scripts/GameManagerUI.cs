using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerUI : MonoBehaviour
{
	[SerializeField] private GameManager mGM;
	
	// FPS
    [SerializeField] private Text mFPSText;
	private string fpsIntro;
	private int lastFrameIndex;
	private float[] frameDeltaTimeArray = new float[50];
	
	// Unit Count
    [SerializeField] private Text mUnitCountText;
	
	private string unitCountIntro;
	
    private void Start() { 
		fpsIntro = mFPSText.text; 
		unitCountIntro = mUnitCountText.text; 
		StartCoroutine(UpdateGameUI());
	}
	
	private IEnumerator UpdateGameUI()
    {
        while (true)
        {
			// FPS
			frameDeltaTimeArray[lastFrameIndex] = Time.deltaTime;
			lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;
            mFPSText.text = fpsIntro + Mathf.RoundToInt(GetFPS()).ToString();
			
			// Unit Count
			mUnitCountText.text = unitCountIntro + mGM.GetUnitListSize();
			
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
