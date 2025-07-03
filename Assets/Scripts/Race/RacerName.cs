using UnityEngine;
using System.Collections;

public class RacerName : MonoBehaviour 
{
	[HideInInspector] public Transform target;
	private RacerProgress target_stats;

	[Header("3D Texts")]
	public TextMesh racerPosition;
	public TextMesh racerName;
	public TextMesh racerDistance;

	[Header("Other")]
	public Vector3 positionOffset = new Vector3 (0, 1.5f, 0.5f);
	public float visibleDistance = 30f;
	private GameObject player;

	public void Initialize(){
		Vector3 rot;
		transform.position = target.localPosition + positionOffset;
		rot = target.transform.eulerAngles;
		transform.eulerAngles = new Vector3 (transform.eulerAngles.x, rot.y, transform.eulerAngles.z);
		transform.parent = target;
		target_stats = target.GetComponent<RacerProgress> ();
		player = GameObject.FindGameObjectWithTag ("Player");
	}

	void Update(){
		if (!player || !target)
			return;
		Display ();
	}

	void Display(){
		if (GetDistanceFromPlayer () <= visibleDistance && !IsPlayerAhead ()) {
			racerPosition.text = target_stats.currentRank.ToString ();
			racerName.text = target_stats.racerDetails.racerName;
			racerDistance.text = (int)GetDistanceFromPlayer () + "M";
		}
		foreach (Transform t in transform) {
			if (RaceManager.instance.raceState == RaceManager.RaceState.InRace) {
				t.gameObject.SetActive (GetDistanceFromPlayer () <= visibleDistance && !IsPlayerAhead ());
			} else {
				t.gameObject.SetActive (false);
			}
		}
	}
		
	bool IsPlayerAhead(){
		Vector3 targetPos = player.transform.InverseTransformPoint (target.position);
		if (targetPos.z < 0) {
			return true;
		} else {
			return false;
		}
	}

	float GetDistanceFromPlayer(){
		return Vector3.Distance (transform.position, player.transform.position);
	}
}