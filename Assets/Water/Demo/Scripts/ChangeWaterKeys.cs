using UnityEngine;
using System.Collections;
using System;

public class ChangeWaterKeys : MonoBehaviour {

	public Material singleTexturedWater;
	public Material doubleTexturedWater;
	public Material tripleTexturedWater;
	public Material mobileWater;
	public GameObject waterplane;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		if (Input.GetKeyDown ("l")) {
			waterplane.GetComponent<Renderer> ().material = singleTexturedWater;
		}

		if (Input.GetKeyDown ("m")) {
			waterplane.GetComponent<Renderer> ().material = doubleTexturedWater;
		}

		if (Input.GetKeyDown ("h")) {
			waterplane.GetComponent<Renderer> ().material = tripleTexturedWater;
		}

		if (Input.GetKeyDown ("j")) {
			waterplane.GetComponent<Renderer> ().material = mobileWater;
		}

	}

	void OnGUI()
	{
		int w = Screen.width, h = Screen.height;

		GUIStyle style = new GUIStyle();

		Rect rect = new Rect(0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 3 / 100;
		style.normal.textColor = new Color (1f, 1f, 1f, 1.0f);
		string text = "Move: W, S, A, D \n To change water press: \n 'L' for single textured \n 'M' for double textured \n 'H' for triple textured \n 'J' for mobile (no underwater support)";
		GUI.Label(rect, text, style);
	}
}
