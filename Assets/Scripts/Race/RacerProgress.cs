using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class RacerProgress : MonoBehaviour
{
	[System.Serializable]
	public class RacerDetails
	{
		public string racerName;
		public string carName;
	}
		
	public RacerDetails racerDetails;
	private WaypointCircuit circuit;
	public WaypointCircuit.RoutePoint progressPoint { get; private set; } 
	private float lookAheadForTargetOffset = 20f;
	private float lookAheadForTargetFactor = 0.1f; 
	private float lookAheadForSpeedOffset = 20f;
	private float lookAheadForSpeedFactor = 0.5f;
	public float progressDistance;
	public float raceCompletion;
	private Vector3 lastPosition;
	private float speed;
	public int currentRank;
	public int currentLap;
	public int currentCheckpoint;
	public string currentLapTime; 
	public string prevLapTime; 
	public string totalRaceTime; 
	public string bestLapTime; 
	private float lapTimeCounter; 
	private float totalTimeCounter; 
	private float prevLapCounter;
	private float bestLapCounter;
	private float registerDistance = 10f;
	private float wrongwayDelayTimer;

	[HideInInspector] public Transform target;
	[HideInInspector] public int currentNodeNumber; 
	[HideInInspector] public List<Transform> path = new List<Transform> ();
	[HideInInspector] public List<bool> passednodes = new List<bool> ();
	[HideInInspector] public List<Transform> checkpoints = new List<Transform> ();
	[HideInInspector] public List<bool> passedcheckpoints = new List<bool> ();
	[HideInInspector] public bool finishedRace;
	[HideInInspector] public bool knockedOut;
	[HideInInspector] public bool goingWrongway;
	[HideInInspector] public bool passedAllNodes;
	[HideInInspector] public bool infiniteLaps;
	[HideInInspector] public float speedRecord;
	private GameObject finishLine;

	void Awake(){
		target = new GameObject ("pt").transform;
		finishLine = GameObject.FindGameObjectWithTag ("Finish");
		circuit = GameObject.FindObjectOfType (typeof(WaypointCircuit)) as WaypointCircuit;
	}

	void Start(){
		currentLap = 1;
		target.name = racerDetails.racerName + " Progress";
		infiniteLaps = RaceManager.instance.raceType == RaceManager.RaceType.TimeTrial; 
		if (RaceManager.instance.raceType == RaceManager.RaceType.Checkpoints) {
			lapTimeCounter = RaceManager.instance.initialCheckpointTime;
		}
		FindPath ();
		FindCheckpoints ();
	}

	void Update(){
		GetPath ();
		CalculateRaceTimes ();
		ProgressTracker ();
		CheckForWrongway ();
	}

	void FindPath(){
		Transform[] nodes = RaceManager.instance.pathContainer.GetComponentsInChildren<Transform> ();
		foreach (Transform p in nodes) {
			if (p != RaceManager.instance.pathContainer) {
				path.Add (p);
			}
		}
		passednodes = new List<bool> (new bool[path.Count]);
		finishLine.GetComponent<MiniMapItem> ().HideItem ();
	}
		
	void FindCheckpoints(){
		Checkpoint[] _checkpoint = RaceManager.instance.checkpointContainer.GetComponentsInChildren<Checkpoint> ();
		foreach (Checkpoint c in _checkpoint) {
			c.gameObject.GetComponent<MeshRenderer> ().enabled = false;
			c.gameObject.GetComponent<MiniMapItem> ().HideItem ();
			if (RaceManager.instance.raceType == RaceManager.RaceType.SpeedTrap) {
				if (c.checkpointType == Checkpoint.CheckpointType.Speedtrap) {
					checkpoints.Add (c.transform);
					if ((checkpoints.IndexOf (c.transform) == 0)) {
						c.gameObject.GetComponent<MeshRenderer> ().enabled = true;
						c.gameObject.GetComponent<MiniMapItem> ().ShowItem ();
					}
				}
			}
			if (RaceManager.instance.raceType == RaceManager.RaceType.Checkpoints) {
				if (c.checkpointType == Checkpoint.CheckpointType.TimeCheckpoint) {
					checkpoints.Add (c.transform);
					if ((checkpoints.IndexOf (c.transform) == 0)) {
						c.gameObject.GetComponent<MeshRenderer> ().enabled = true;
						c.gameObject.GetComponent<MiniMapItem> ().ShowItem ();
					}
				}
			} 
		}
		passedcheckpoints = new List<bool> (new bool[checkpoints.Count]);
	}
		
	void GetPath(){
		int n = currentNodeNumber;
		Transform node = path [n] as Transform;
		Vector3 nodeVector = target.InverseTransformPoint (node.position);
		if (currentLap == RaceManager.instance.totalLaps && RaceManager.instance.raceType != RaceManager.RaceType.Checkpoints && RaceManager.instance.raceType != RaceManager.RaceType.SpeedTrap) {
			finishLine.GetComponent<MiniMapItem> ().ShowItem ();
		}

		if (nodeVector.magnitude <= registerDistance) {
			currentNodeNumber++;
			passednodes [n] = true;
		}
		foreach (bool pass in passednodes) {
			if (pass == true) {
				passedAllNodes = true;
			} else {
				passedAllNodes = false;
			}
		}
		if (currentNodeNumber >= path.Count) {
			currentNodeNumber = 0;
		}
	}

	void ProgressTracker(){
		if (Time.deltaTime > 0) {
			speed = Mathf.Lerp (speed, (lastPosition - transform.position).magnitude / Time.deltaTime, Time.deltaTime);
		}
		target.position = circuit.GetRoutePoint (progressDistance + lookAheadForTargetOffset + lookAheadForTargetFactor * speed).position;
		target.rotation = Quaternion.LookRotation (circuit.GetRoutePoint (progressDistance + lookAheadForSpeedOffset + lookAheadForSpeedFactor * speed).direction);
		progressPoint = circuit.GetRoutePoint (progressDistance);
		Vector3 progressDelta = progressPoint.position - transform.position;

		if (Vector3.Dot (progressDelta, progressPoint.direction) < 0) {
			progressDistance += progressDelta.magnitude * 0.5f;
		}
		if (Vector3.Dot (progressDelta, progressPoint.direction) > 5.0f) {
			progressDistance -= progressDelta.magnitude * 0.5f;
		}
		lastPosition = transform.position;

		if (!finishedRace) {
			if (!knockedOut) {
				raceCompletion = (progressDistance / (circuit.Length * RaceManager.instance.totalLaps)) * 100;
			}
		} else {
			raceCompletion = 100;
		}
		raceCompletion = Mathf.Round (raceCompletion * 100) / 100;
	}
		
	void CalculateRaceTimes(){
		if (RaceManager.instance.raceStarted && !knockedOut && !finishedRace) {
			if (RaceManager.instance.timerType == RaceManager.TimerType.CountUp) {
				lapTimeCounter += Time.deltaTime;
			}
			if (RaceManager.instance.timerType == RaceManager.TimerType.CountDown) {
				lapTimeCounter -= Time.deltaTime;

				if (lapTimeCounter <= 0) {
					if (RaceManager.instance.raceType == RaceManager.RaceType.Checkpoints) {
						knockedOut = true;
						RaceManager.instance.KnockoutRacer (this);
					}
				}
			}
			totalTimeCounter += Time.deltaTime;
		}
		currentLapTime = RaceManager.instance.FormatTime (lapTimeCounter);
		totalRaceTime = RaceManager.instance.FormatTime (totalTimeCounter);
	}
		
	public void NewLap(){
		if (finishedRace || knockedOut)
			return;
		prevLapTime = currentLapTime;
		prevLapCounter = lapTimeCounter;
		CheckForBestTime ();

		for (int i = 0; i < passednodes.Count; i++) {
			passednodes [i] = false;
		}
		for (int i = 0; i < passedcheckpoints.Count; i++) {
			passedcheckpoints [i] = false;
		}
		currentLap++;

		if (!infiniteLaps) {
			if (currentLap == RaceManager.instance.totalLaps && gameObject.tag == "Player") {
				RaceUI.instance.ShowRaceInfo ("Ostatnie okrążenie !", 2.0f, Color.white);
			}
			if (currentLap > RaceManager.instance.totalLaps) {
				if (!knockedOut && !finishedRace) {
					FinishRace ();
				}
			}
		}
		if (RaceManager.instance.timerType != RaceManager.TimerType.CountDown) {
			lapTimeCounter = 0f;
		}
		if (RaceManager.instance.raceType == RaceManager.RaceType.LapKnockout) {  
			if (this.currentRank == RankManager.instance.currentRacers - 1)
				RaceManager.instance.KnockoutRacer (RankManager.instance.GetLastPlace ());
		}
	}

	public void FinishRace(){
		finishedRace = true;
		if (gameObject.tag == "Player") {
			RaceManager.instance.EndRace (currentRank);
		}
		AIMode ();
	}

	public void AIMode(){
		if (GetComponent<PlayerControl> ()) {
			GetComponent<PlayerControl> ().enabled = false;
			gameObject.AddComponent<AI_Control> ();
		}
	}
		
	void RegisterCheckpoint(Checkpoint.CheckpointType type, float timeAdd){
		if (finishedRace || knockedOut)
			return;

		switch (type) {
		case Checkpoint.CheckpointType.Speedtrap:
			if (RaceManager.instance.raceType != RaceManager.RaceType.SpeedTrap)
				return;
			float speed = GetComponent<Car_Controller> ().currentSpeed;
			var speedUnit = GetComponent<Car_Controller> ().speedUnit;
			speedRecord += speed;

			if (gameObject.tag == "Player") {
				SoundManager.instance.PlayDefaultSound (SoundManager.instance.raceSounds.speedTrapSound);
				RaceUI.instance.ShowRaceInfo ("+ " + speed + " " + speedUnit, 1.0f, Color.white);
			}
			break;

		case Checkpoint.CheckpointType.TimeCheckpoint:
			currentCheckpoint++;
			lapTimeCounter += timeAdd;

			if (gameObject.tag == "Player") {
				SoundManager.instance.PlayDefaultSound (SoundManager.instance.raceSounds.checkpointSound);
				RaceUI.instance.ShowRaceInfo ("+ " + RaceManager.instance.FormatTime (timeAdd), 1.0f, Color.white);
			}
			break;
		}
	}
		
	void CheckForBestTime(){  
		if (bestLapCounter == 0) {
			bestLapCounter = lapTimeCounter;
			bestLapTime = RaceManager.instance.FormatTime (bestLapCounter);
		} else if (prevLapCounter < bestLapCounter) {
			bestLapCounter = prevLapCounter;
			bestLapTime = RaceManager.instance.FormatTime (bestLapCounter);
		}
		if (gameObject.tag == "Player") {
			if (bestLapTime == "" || bestLapCounter > lapTimeCounter) {
				RaceUI.instance.ShowRaceInfo ("Najszybsze okrążenie !", 2.0f, Color.white);
			}
		}
	}

	void CheckForWrongway(){
		float nodeAngle = target.transform.eulerAngles.y;
		float transformAngle = transform.eulerAngles.y;
		float angleDifference = nodeAngle - transformAngle;
		goingWrongway = (wrongwayDelayTimer >= 1f);

		if (Mathf.Abs (angleDifference) <= 230f && Mathf.Abs (angleDifference) >= 120) {
			if (GetComponent<Rigidbody> ().velocity.magnitude >= 5.0f) {
				wrongwayDelayTimer += Time.deltaTime;
			} else {
				wrongwayDelayTimer = 0.0f;
			}
		} else {
			wrongwayDelayTimer = 0.0f;
		}
	}
		
	void OnTriggerEnter(Collider other){
		if (other.tag == "FinishLine" || other.tag == "Finish") {
			if (passedAllNodes) {
				NewLap ();
			}
		}
		if (other.GetComponent<Checkpoint> ()) {
			for (int i = 0; i < checkpoints.Count; i++) {
				if (checkpoints [i] == other.transform && !passedcheckpoints [i]) {
					passedcheckpoints [i] = true;
					if (this.tag == "Player") {
						checkpoints [i].gameObject.GetComponent<MeshRenderer> ().enabled = false;
						checkpoints [i].gameObject.GetComponent<MiniMapItem> ().HideItem ();
						if (i != checkpoints.Count - 1) {
							checkpoints [i + 1].gameObject.GetComponent<MeshRenderer> ().enabled = true;
							checkpoints [i + 1].gameObject.GetComponent<MiniMapItem> ().ShowItem ();
						} else {
							checkpoints [0].gameObject.GetComponent<MeshRenderer> ().enabled = true;
							checkpoints [0].gameObject.GetComponent<MiniMapItem> ().ShowItem ();
						}
					}
					RegisterCheckpoint (checkpoints [i].GetComponent<Checkpoint> ().checkpointType, checkpoints [i].GetComponent<Checkpoint> ().timeToAdd);
				}
			}
		}
	}

	void OnDestroy(){
		if (target)
			Destroy (target.gameObject);
	}
}