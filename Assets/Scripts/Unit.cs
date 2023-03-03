using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
	private GameManager mGM;
	
	private void Start()
	{
		mGM = FindObjectOfType<GameManager>();
		mGM.AddUnitToUnitList(this);
		GetComponent<Renderer>().material.SetColor("_Color", new Color(Random.value, Random.value, Random.value));
	}
}
