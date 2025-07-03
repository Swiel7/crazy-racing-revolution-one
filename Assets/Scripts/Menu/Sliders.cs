using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Sliders : MonoBehaviour 
{
	public Text sliderValue;
	public Slider defaultSlider;

	void Start (){
		sliderValue = GetComponent<Text> ();
		defaultSlider = GetComponentInParent<Slider> ();
		ShowSliderValue ();
	}
		
	public void ShowSliderValue (){
		if (defaultSlider.gameObject.activeSelf) {     
			sliderValue.text = defaultSlider.value + "";
		} 
	}
}