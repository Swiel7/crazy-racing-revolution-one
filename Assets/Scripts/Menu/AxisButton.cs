using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AxisButton : MonoBehaviour 
{
	public Text axisText;
	public Text keyText;
	public string axisName;
	public bool negativeKey;

	public void Init(string axisName, string buttonDescription, string key, bool nkey = false) {
		this.axisName = axisName;
		axisText.text = buttonDescription;
		keyText.text = key;
		negativeKey = nkey;
	}
	
	public void ChangeKeyText(string key){
		if (key == "JoystickButton0") {
			keyText.text = "A";
			return;
		} else if (key == "JoystickButton1") {
			keyText.text = "B";
			return;
		} else if (key == "JoystickButton2") {
			keyText.text = "X";
			return;
		} else if (key == "JoystickButton3") {
			keyText.text = "Y";
			return;
		} else if (key == "JoystickButton4") {
			keyText.text = "LB";
			return;
		} else if (key == "JoystickButton5") {
			keyText.text = "RB";
			return;
		} else if (key == "JoystickButton6") {
			keyText.text = "Back";
			return;
		}else if (key == "JoystickButton8") {
			keyText.text = "LS Click";
			return;
		} else if (key == "JoystickButton9") {
			keyText.text = "RS Click";
			return;
		} else if (key == "JoystickButton10") {
			keyText.text = "LT";
			this.GetComponent<Button> ().interactable = false;
			return;
		} else if (key == "JoystickButton11") {
			keyText.text = "RT";
			this.GetComponent<Button> ().interactable = false;
			return;
		} else if (key == "JoystickButton12") {
			keyText.text = "LS Right";
			this.GetComponent<Button> ().interactable = false;
			return;
		}else if (key == "JoystickButton13") {
			keyText.text = "LS Left";
			this.GetComponent<Button> ().interactable = false;
			return;
		}else if (key == "JoystickButton14") {
			keyText.text = "RS Vertical";
			this.GetComponent<Button> ().interactable = false;
			return;
		}else if (key == "JoystickButton15") {
			keyText.text = "RS Horizontal";
			this.GetComponent<Button> ().interactable = false;
			return;
		}else if (key == "Mouse5") {
			keyText.text = "Mouse Y";
			this.GetComponent<Button> ().interactable = false;
			return;
		} else if (key == "Mouse6") {
			keyText.text = "Mouse X";
			this.GetComponent<Button> ().interactable = false;
			return;
		} else if (key != "None") {
			keyText.text = key;
			return;
		} else {
			keyText.text = "";
			return;
		}
	}

	public void RebindAxis(){
		ControlsMenu.instance.clicked = !ControlsMenu.instance.clicked;
		if (ControlsMenu.instance.clicked == true) {
			ControlsMenu.instance.ClearButton (axisName, negativeKey);
		}
	}
}