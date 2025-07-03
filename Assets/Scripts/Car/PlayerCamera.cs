using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerCamera : MonoBehaviour 
{
	public static PlayerCamera instance;
	public CameraMode cameraMode = CameraMode.FirstPerson;
	public enum CameraMode {ThirdPerson, FirstPerson}
	private Rigidbody rigid;
	public Transform target;
	private Transform driverCamera;
	private int index = 0;

	[Header("Third Person Camera Settings")]
	public float distance = 6.0f;
	public float height = 2.5f;
	public float lookAtHeight = 0.5f;
	public float rotationDamping = 2.5f;
	public float heightDamping = 5.0f;
	[Range(1, 10f)] public float distanceZoomSpeed = 5.0f;
	[Range(0.01f, 0.1f)] public float distanceMultiplier = 0.05f;
	public bool distanceBasedOnVelocity = true;
	private float currentDistance;
	private float wantedRotationAngle;
	private float wantedHeight;
	private float currentRotationAngle = 45.0f;
	private float currentHeight;
	private Quaternion currentRotation;
	private Vector3 lookAtVector;
	private Vector3 wantedPosition;
	[HideInInspector] public Vector3 velocityDir;
	[HideInInspector] public bool lookLeft, lookRight, lookBack;

	[Header("First Person Camera Settings")]
	public float xSpeed = 50.0f;
	public float ySpeed = 50.0f;
	public float yMinLimit = 0.0f;
	public float yMaxLimit = 30.0f;
	public float xMinLimit = -40.0f;
	public float xMaxLimit = 40.0f;
	private float x;
	private float y;
	private bool canOrbit;
	public bool allowOrbit = true;
	public bool autoSnapRotation = true;
	[HideInInspector] public float orbitX, orbitY;
	private float autoSnapTimer;
	private FlipCamera[] mirrors;

	void Start(){
		GetComponent<Camera> ().depth = -1;
	}

	void Update(){
		GetCameraInput ();
		if (!target && GameObject.FindGameObjectWithTag ("Player")) {
			target = GameObject.FindGameObjectWithTag ("Player").transform;
			driverCamera = GameObject.FindGameObjectWithTag ("DriverCam").transform;
			rigid = target.GetComponent<Rigidbody> ();
			mirrors = FindObjectsOfType (typeof(FlipCamera)) as FlipCamera[];
		}
	}  

	void LateUpdate(){
		switch (cameraMode) {
		case CameraMode.ThirdPerson:
			ThirdPersonCamera ();
			Mirrors (false);
			break;

		case CameraMode.FirstPerson:
			FirstPersonCamera ();
			Mirrors (true);
			break;
		}
	}
		
	void ThirdPersonCamera(){
		wantedHeight = target.position.y + height;
		currentHeight = transform.position.y;
		currentRotationAngle = transform.eulerAngles.y;

		if (!lookLeft && !lookRight && !lookBack) {
			wantedRotationAngle = target.eulerAngles.y;
		}
		currentRotationAngle = Mathf.LerpAngle (currentRotationAngle, wantedRotationAngle, Time.deltaTime * rotationDamping);
		currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping * Time.deltaTime);
		if (distanceBasedOnVelocity) {
			currentDistance = Mathf.Lerp (currentDistance, distance + (rigid.velocity.magnitude * distanceMultiplier), distanceZoomSpeed * Time.deltaTime); 
		} else {
			Mathf.Lerp (currentDistance, distance, distanceZoomSpeed * Time.deltaTime);
		}
		currentRotation = Quaternion.Euler (0, currentRotationAngle, 0);
		velocityDir = target.InverseTransformDirection (rigid.velocity);

		if (lookLeft)
			wantedRotationAngle = target.eulerAngles.y - 90;
		if (lookRight)
			wantedRotationAngle = target.eulerAngles.y + 90;
		if (lookBack)
			wantedRotationAngle = target.eulerAngles.y + 180;

		wantedPosition = target.position;
		wantedPosition.y = currentHeight;
		transform.position = wantedPosition;
		transform.position -= currentRotation * Vector3.forward * currentDistance;
		transform.LookAt (target.position + new Vector3 (0, lookAtHeight, 0));
	}
		
	void FirstPersonCamera(){
		if (allowOrbit) {
			x += (orbitX * xSpeed * Time.deltaTime);
			y -= (orbitY * ySpeed * Time.deltaTime);
		}
		if (orbitX == 0 && orbitY == 0) {
			autoSnapTimer += Time.deltaTime;
			if (autoSnapTimer >= 1 && autoSnapRotation) {
				x = Mathf.Lerp (x, 0, Time.deltaTime * 2.5f);
				y = Mathf.Lerp (y, 0, Time.deltaTime * 2.5f);
			}
		} else {
			autoSnapTimer = 0f;
		}
		y = ClampAngle (y, yMinLimit, yMaxLimit);
		x = ClampAngle (x, xMinLimit, xMaxLimit);
		Quaternion rotation = Quaternion.Euler (y, x, 0);
		transform.position = driverCamera.position;
		transform.rotation = driverCamera.rotation * rotation;
	}
		
	void GetCameraInput(){
		switch (ControlsMenu.instance.inputDevice) {
		case ControlsMenu.InputDevice.Keyboard:
			if (ControlsMenu.instance.GetButtonDown ("ChangeCamera")) {
				SwitchCameras ();
			}
			lookLeft = ControlsMenu.instance.GetButton ("LookLeft");
			lookRight = ControlsMenu.instance.GetButton ("LookRight");
			lookBack = lookRight && lookLeft || ControlsMenu.instance.GetButton ("LookBack") || velocityDir.z <= -2.0f;
			orbitX = Input.GetAxis ("Mouse X");
			orbitY = Input.GetAxis ("Mouse Y");
			break;

		case ControlsMenu.InputDevice.Xbox360Controller:
			if (ControlsMenu.instance.GetButtonDown ("ChangeCameraXBOX")) {
				SwitchCameras ();
			}
			lookLeft = ControlsMenu.instance.GetButton ("LookLeftXBOX");
			lookRight = ControlsMenu.instance.GetButton ("LookRightXBOX");
			lookBack = lookRight && lookLeft || ControlsMenu.instance.GetButton ("LookBackXBOX") || velocityDir.z <= -2.0f;
			orbitX = Input.GetAxis ("RightAnalogHorizontal");
			orbitY = Input.GetAxis ("RightAnalogVertical");
			break;
		}
	}

	public void SwitchCameras(){
		index++;
		if (index > 1) {
			index = 0;
		}
		if (index == 0) {
			cameraMode = CameraMode.FirstPerson;
		} else {
			cameraMode = CameraMode.ThirdPerson;
		}
	}
		
	float ClampAngle(float angle, float min, float max){
		if (angle < -360) {
			angle += 360;
		}
		if (angle > 360) {
			angle -= 360;
		}
		return Mathf.Clamp (angle, min, max);
	}

	void Mirrors(bool status){
		foreach (FlipCamera mirror in mirrors) {
			mirror.gameObject.GetComponent<Camera> ().enabled = status;
		}
	}
}