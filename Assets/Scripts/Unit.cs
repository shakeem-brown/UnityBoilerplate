using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
	public Color color { get; private set; }
	public float speed { get; private set; }
	
	private void Start()
	{
		FindObjectOfType<GameManager>().AddUnitToUnitList(this);
		color = new Color(Random.value, Random.value, Random.value);
		GetComponent<Renderer>().material.SetColor("_Color", color);
		speed = Random.Range(1f, 10f);
	}
}
