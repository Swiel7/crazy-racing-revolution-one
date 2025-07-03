using UnityEngine;
using System.Collections;

public class OrbitCamera : MonoBehaviour 
{
	public Transform target;
	public float distance = 7f;
	public float height = 1f;
	public float rotateSpeed = 5f;
	private float x;

	void Update(){
		if (!target && GameObject.FindGameObjectWithTag ("Player")) {
			target = GameObject.FindGameObjectWithTag ("Player").transform;
		}
	}
		
	void LateUpdate(){
		x += Time.unscaledDeltaTime * rotateSpeed;
		Quaternion rotation = Quaternion.Euler (0, x, 0);
		Vector3 position = rotation * (new Vector3 (0.0f, height, -distance)) + target.position;
		transform.rotation = rotation;
		transform.position = position;
		transform.LookAt (target);
	}
}