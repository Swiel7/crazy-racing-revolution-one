using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ControlsMenu : MonoBehaviour 
{
	public static ControlsMenu instance;
	public enum InputDevice {Keyboard, Xbox360Controller}
	public InputDevice inputDevice = InputDevice.Keyboard;
	[SerializeField] public List <AxisBase> Axes = new List<AxisBase> ();
	Dictionary<string, int> defaultAxes = new Dictionary<string, int> ();
	public GameObject axisPrefab;
	public Transform axesGrid;
	public bool rebinding;
	private bool negativeKey;
	private AxisBase targetAxis;
	private bool initOnce;
	[HideInInspector] public bool clicked = false;

	void Awake(){
		instance = this;
	}

	public void Init(){
		if (!initOnce) {
			if (FindObjectOfType<PlayerProfile> ()) {
				SaveAllAxis ();
			}
			SaveAxesDefaults ();
			initOnce = true;
		}
		LoadAllAxes ();
		CreateAxisButtons ();
	}
		
	void FixedUpdate(){
		for (int i = 0; i < Axes.Count; i++) {
			AxisBase a = Axes [i];
			a.negative = (Input.GetKey (a.nKey));
			a.positive = (Input.GetKey (a.pKey));
			a.targetAxis = (a.negative) ? -1 : (a.positive) ? 1 : 0;
			a.axis = Mathf.MoveTowards (a.axis, a.targetAxis, Time.deltaTime * a.sensitivity);
		}
	}

	public void CheckInput(){
		if (inputDevice != InputDevice.Xbox360Controller && SettingsMenu.instance.UIInputDevice.value == 1) {
			inputDevice = InputDevice.Xbox360Controller;
			CreateAxisButtons ();
		}
		if (inputDevice != InputDevice.Keyboard && SettingsMenu.instance.UIInputDevice.value == 0) {
			inputDevice = InputDevice.Keyboard;
			CreateAxisButtons ();
		}
	}

	void OnGUI(){
		if (rebinding) {
			Event e = Event.current;

			if (e != null) {
				if (e.isKey && !e.isMouse && inputDevice == InputDevice.Keyboard) {
					if (e.keyCode != KeyCode.None) {
						ChangeInputKey (targetAxis.axisName, e.keyCode, negativeKey);
						clicked = !clicked;
						rebinding = false;
					}
				} else if (e.isMouse && inputDevice == InputDevice.Keyboard) {
					KeyCode targetKey = KeyCode.None;
					switch (e.button) {
					case 0:
						targetKey = KeyCode.Mouse0;
						break;
					case 1:
						targetKey = KeyCode.Mouse1;
						break; 
					case 2:
						targetKey = KeyCode.Mouse2;
						break;
					}
					if (targetKey != KeyCode.None) {
						ChangeInputKey (targetAxis.axisName, targetKey, negativeKey);
						rebinding = false;
					}
				} else if (Input.GetKey (KeyCode.JoystickButton0) && inputDevice == InputDevice.Xbox360Controller) {
					ChangeInputKey (targetAxis.axisName, KeyCode.JoystickButton0, negativeKey);
					rebinding = false;
					clicked = !clicked;
				} else if (Input.GetKey (KeyCode.JoystickButton1) && inputDevice == InputDevice.Xbox360Controller) {
					ChangeInputKey (targetAxis.axisName, KeyCode.JoystickButton1, negativeKey);
					rebinding = false;
					clicked = !clicked;
				} else if (Input.GetKey (KeyCode.JoystickButton2) && inputDevice == InputDevice.Xbox360Controller) {
					ChangeInputKey (targetAxis.axisName, KeyCode.JoystickButton2, negativeKey);
					rebinding = false;
					clicked = !clicked;
				} else if (Input.GetKey (KeyCode.JoystickButton3) && inputDevice == InputDevice.Xbox360Controller) {
					ChangeInputKey (targetAxis.axisName, KeyCode.JoystickButton3, negativeKey);
					rebinding = false;
					clicked = !clicked;
				} else if (Input.GetKey (KeyCode.JoystickButton4) && inputDevice == InputDevice.Xbox360Controller) {
					ChangeInputKey (targetAxis.axisName, KeyCode.JoystickButton4, negativeKey);
					rebinding = false;
					clicked = !clicked;
				} else if (Input.GetKey (KeyCode.JoystickButton5) && inputDevice == InputDevice.Xbox360Controller) {
					ChangeInputKey (targetAxis.axisName, KeyCode.JoystickButton5, negativeKey);
					rebinding = false;
					clicked = !clicked;
				} else if (Input.GetKey (KeyCode.JoystickButton6) && inputDevice == InputDevice.Xbox360Controller) {
					ChangeInputKey (targetAxis.axisName, KeyCode.JoystickButton6, negativeKey);
					rebinding = false;
					clicked = !clicked;
				} else if (Input.GetKey (KeyCode.JoystickButton8) && inputDevice == InputDevice.Xbox360Controller) {
					ChangeInputKey (targetAxis.axisName, KeyCode.JoystickButton8, negativeKey);
					rebinding = false;
					clicked = !clicked;
				} else if (Input.GetKey (KeyCode.JoystickButton9) && inputDevice == InputDevice.Xbox360Controller) {
					ChangeInputKey (targetAxis.axisName, KeyCode.JoystickButton9, negativeKey);
					rebinding = false;
					clicked = !clicked;
				} else if (inputDevice == InputDevice.Xbox360Controller && (e.isKey || e.isMouse)) {
					ChangeInputKey (targetAxis.axisName, KeyCode.None, negativeKey);
					rebinding = false;
					return;
				}
			}
		}
	}

	public void ChangeInputKey(string name, KeyCode newKey, bool negative = false){
		AxisBase a = ReturnAxis (name);

		for (int i = 0; i < Axes.Count; i++) {
			if (Axes [i].pKey == newKey) {
				Axes [i].pKey = KeyCode.None;
				if (Axes [i].pUIButton) {
					Axes [i].pUIButton.ChangeKeyText (KeyCode.None.ToString ());
				}
			} else if (Axes [i].nKey == newKey) {
				Axes [i].nKey = KeyCode.None;
				if (Axes [i].nUIButton) {
					Axes [i].nUIButton.ChangeKeyText (KeyCode.None.ToString ());
				}
			}
		}
		if (a == null) {
			return;
		}
		if (negative) {
			a.nKey = newKey;
			a.nUIButton.ChangeKeyText (a.nKey.ToString ());
		} else {
			a.pKey = newKey;
			a.pUIButton.ChangeKeyText (a.pKey.ToString ());
		}
	}

	AxisBase ReturnAxis(string name){
		AxisBase retVal = null;

		for (int i = 0; i < Axes.Count; i++) {
			if (string.Equals (name, Axes [i].axisName)) {
				retVal = Axes [i];
			}
		}
		return retVal;
	}

	public void SaveAxesDefaults(){
		for (int i = 0; i < Axes.Count; i++) {
			AxisBase a = Axes [i];
			defaultAxes.Add (a.axisName + "pKey", (int)a.pKey);
			defaultAxes.Add (a.axisName + "nKey", (int)a.nKey);
		}
	}

	public void SaveAllAxis(){
		for (int i = 0; i < Axes.Count; i++) {
			AxisBase a = Axes [i];
			int pKeyIndex = (Axes.IndexOf (a)) * 2;
			int nKeyIndex = pKeyIndex + 1;
			PlayerProfile.instance.player.m_Controls [pKeyIndex] = (int)a.pKey;
			PlayerProfile.instance.player.m_Controls [nKeyIndex] = (int)a.nKey;
		}
	}

	public void LoadAllAxes(){
		for (int i = 0; i < Axes.Count; i++) {
			AxisBase a = Axes [i];
			int pKeyIndex = (Axes.IndexOf (a)) * 2;
			int nKeyIndex = pKeyIndex + 1;
			int p = PlayerProfile.instance.player.m_Controls [pKeyIndex];
			int n = PlayerProfile.instance.player.m_Controls [nKeyIndex];
			a.pKey = (KeyCode)p;
			a.nKey = (KeyCode)n;
		}
	}

	public void ResetAllAxes(){
		foreach (AxisBase a in Axes) {
			int pKeyValue = 0;
			defaultAxes.TryGetValue (a.axisName + "pKey", out pKeyValue);
			a.pKey = (KeyCode)pKeyValue;
			a.pUIButton.ChangeKeyText (a.pKey.ToString ());

			int nKeyValue = 0;
			defaultAxes.TryGetValue (a.axisName + "nKey", out nKeyValue);
			a.nKey = (KeyCode)nKeyValue;
			if (a.nUIButton) {
				a.nUIButton.ChangeKeyText (a.nKey.ToString ());
			}
		}
	}

	public float GetAxis(string name){
		float v = 0;
		for (int i = 0; i < Axes.Count; i++) {
			if (string.Equals (Axes [i].axisName, name)) {
				v = Axes [i].axis;
			}
		}
		return v;
	}
		
	public bool GetButton(string name){
		bool retVal = false;
		for (int i = 0; i < Axes.Count; i++) {
			if (string.Equals (Axes [i].axisName, name)) {
				retVal = Axes [i].positive;
			}
		}
		return retVal;
	}

	public bool GetButtonDown(string name){
		AxisBase a = ReturnAxis (name);
		return Input.GetKeyDown (a.pKey);
	}

	void CreateAxisButtons(){
		ClearAxisList ();
		foreach (AxisBase a in Axes) {
			GameObject p = Instantiate (axisPrefab);
			p.transform.SetParent (axesGrid, false);
			AxisButton pB = p.GetComponent<AxisButton> ();
			pB.Init (a.axisName, a.pKey_description, a.pKey.ToString ());
			a.pUIButton = pB;
			a.pUIButton.ChangeKeyText (a.pKey.ToString ());
			if ((inputDevice == InputDevice.Keyboard && (Axes.IndexOf (a)) >= 12) || (inputDevice == InputDevice.Xbox360Controller && (Axes.IndexOf (a)) < 12)) {
				p.SetActive (false);  
			}

			if (a.nKey != KeyCode.None) {
				GameObject n = Instantiate (axisPrefab);
				n.transform.SetParent (axesGrid, false);
				AxisButton nB = n.GetComponent<AxisButton> ();
				nB.Init (a.axisName, a.nKey_description, a.nKey.ToString (), true);
				a.nUIButton = nB;
				a.nUIButton.ChangeKeyText (a.nKey.ToString ());
				if ((inputDevice == InputDevice.Keyboard && (Axes.IndexOf (a)) >= 12) || (inputDevice == InputDevice.Xbox360Controller && (Axes.IndexOf (a)) < 12)) {
					n.SetActive (false);
				}
			}
		}
	}

	public void ClearAxisList(){
		while (axesGrid.childCount > 0) {
			Transform s = axesGrid.GetChild (0);
			s.SetParent (null);
			Destroy (s.gameObject);
		}
	}
		
	public void ClearButton(string axisName, bool negative){
		targetAxis = ReturnAxis (axisName);
		rebinding = true;
		negativeKey = negative;
		ChangeInputKey (targetAxis.axisName, KeyCode.None, negativeKey);
	}
}
	
[System.Serializable]
public class AxisBase
{
	public string axisName;
	public KeyCode pKey;
	public KeyCode nKey;
	[HideInInspector] public bool positive;
	[HideInInspector] public bool negative;
	[HideInInspector] public float axis;
	public float targetAxis;
	public float sensitivity = 3;
	public string pKey_description;
	public string nKey_description;
	[HideInInspector] public AxisButton pUIButton;
	[HideInInspector] public AxisButton nUIButton;
}