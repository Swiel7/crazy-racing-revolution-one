using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class InputFieldName : MonoBehaviour, IPointerClickHandler 
{
	public InputField inputField;

	public void OnPointerClick(PointerEventData eventData){
		inputField.text = transform.GetChild(0).GetComponent<Text> ().text;
	}
}