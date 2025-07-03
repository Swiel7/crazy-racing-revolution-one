using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour
{
	public static CameraManager instance;
	private List<Camera> cameraList = new List<Camera>();
	private AudioListener audioListener;

	[Header("Orbit Camera")]
	public Camera orbitCamera;

	[Header("Player Camera")]
	public Camera playerCamera;

	void Awake(){
		instance = this;
		CreateAudioListener();
		AddCamerasToCameraList();
	}
		
	void SetAudioListerParent(Transform t){
		audioListener.transform.parent = t;
		audioListener.transform.localPosition = Vector3.zero;
		audioListener.transform.localRotation = Quaternion.identity;
	}

	void AddCamerasToCameraList(){
		if (!cameraList.Contains (orbitCamera))
			cameraList.Add (orbitCamera);

		if (!cameraList.Contains (playerCamera))
			cameraList.Add (playerCamera);
	}

	void CreateAudioListener(){
		audioListener = new GameObject ("AudioListener").AddComponent<AudioListener> ();
		AudioListener[] allListeners = GameObject.FindObjectsOfType (typeof(AudioListener)) as AudioListener[];
		foreach (AudioListener a in allListeners) {
			if (a != audioListener)
				Destroy (a);
		}
	}

	public void ActivatePlayerCamera(){
		for (int i = 0; i < cameraList.Count; i++) {
			if (cameraList [i] == playerCamera) {
				cameraList [i].enabled = true;
				SetAudioListerParent (FindObjectOfType<PlayerControl> ().transform);
			} else {
				if (playerCamera != null)
					cameraList [i].enabled = false;
			}
		}
	}

	public void ActivateOrbitCamera(){
		for (int i = 0; i < cameraList.Count; i++) {
			if (cameraList [i] == orbitCamera) {
				cameraList [i].enabled = true;
				SetAudioListerParent (cameraList [i].transform);
			} else {
				if (orbitCamera != null)
					cameraList [i].enabled = false;
			}
		}
	}
}