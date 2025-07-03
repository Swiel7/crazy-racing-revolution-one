using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
	private Car_Controller car_controller;

	void Awake(){
		car_controller = GetComponent<Car_Controller> ();
	}
		
	void Update(){
		ChangeInputController ();
	}

	public void ChangeInputController(){
		if (ControlsMenu.instance.inputDevice == ControlsMenu.InputDevice.Keyboard) {
			KeyboardControl ();
		} else {
			XboxControl ();
		}
	}
		
	public void KeyboardControl(){
		float acceleration = Mathf.Clamp01 (ControlsMenu.instance.GetAxis ("Vertical"));
		float brake = Mathf.Clamp01 (-ControlsMenu.instance.GetAxis ("Vertical"));
		float steer = Mathf.Clamp (ControlsMenu.instance.GetAxis ("Horizontal"), -1, 1);
		float handbrake = Mathf.Clamp01 (ControlsMenu.instance.GetAxis ("Handbrake"));
		bool nitro = ControlsMenu.instance.GetButton ("Nitro");
		AssignInput (acceleration, brake, steer, handbrake, nitro);

		if (ControlsMenu.instance.GetButtonDown ("HeadLights")) {
			car_controller.useHeadLights = !car_controller.useHeadLights;
		}
		if (ControlsMenu.instance.GetButtonDown ("Neon")) {
			car_controller.useNeon = !car_controller.useNeon;
		}
		if (ControlsMenu.instance.GetButtonDown ("Pause")) {
			RaceManager.instance.PauseRace ();
		}
	}
		
	public void XboxControl(){
		float acceleration = Mathf.Clamp01 (Input.GetAxis ("RT"));
		float brake = Mathf.Clamp01 (Input.GetAxis ("LT"));
		float steer = Mathf.Clamp (Input.GetAxis ("LeftAnalogHorizontal"), -1, 1);
		float handbrake = ControlsMenu.instance.GetButton ("HandbrakeXBOX") ? 1 : 0;
		bool nitro = ControlsMenu.instance.GetButton ("NitroXBOX");
		AssignInput (acceleration, brake, steer, handbrake, nitro);

		if (ControlsMenu.instance.GetButtonDown ("HeadLightsXBOX")) {
			car_controller.useHeadLights = !car_controller.useHeadLights;
		}
		if (ControlsMenu.instance.GetButtonDown ("NeonXBOX")) {
			car_controller.useNeon = !car_controller.useNeon;
		}
		if (ControlsMenu.instance.GetButtonDown ("PauseXBOX")) {
			RaceManager.instance.PauseRace ();
		}
	}
		
	public void AssignInput(float acceleration, float brake, float steer, float handbrake, bool nitro){
		car_controller.motorInput = (brake <= 0) ? acceleration : 0;
		car_controller.brakeInput = brake;
		car_controller.steerInput = steer;
		car_controller.handbrakeInput = handbrake;
		car_controller.useNitro = nitro;
	}
}