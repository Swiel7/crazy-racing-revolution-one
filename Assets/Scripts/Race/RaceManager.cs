using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RaceManager : MonoBehaviour 
{
	public static RaceManager instance;

	[System.Serializable]
	public class RaceRewards
	{
		public int money;
		public string trackUnlock;
		public Sprite trophy;
	}

	private RacerProgress[] racers;
	public enum RaceType {Circuit, TimeTrial, LapKnockout, SpeedTrap, Checkpoints}
	public enum RaceState {OnStart, InRace, PausedRace, Complete, KnockedOut}
	public enum PlayerSpawnPosition {Randomized, Selected}
	public enum TimerType {CountUp, CountDown}
	public enum AISpawnType {Randomized, Order}

	[Header("Race Settings")]
	public RaceType raceType;
	[HideInInspector] public RaceState raceState = RaceState.OnStart;
	[HideInInspector] public TimerType timerType = TimerType.CountUp;
	public int totalLaps = 3;
	public int totalRacers = 4;
	private int spawnIndex;
	private int trackIndex;
	private bool startCountdown;
	private int countdownFrom = 3;
	private int currentCountdownTime;
	private float raceDistance;
	private float countdownDelay = 3f;
	private float countdownTimer = 1f;
	public float initialCheckpointTime;

	[Header("Player Settings")]
	public GameObject playerCar;
	public PlayerSpawnPosition playerSpawnPosition;
	public int playerStartRank = 4;

	[Header("AI Settings")]
	public List<GameObject> opponentCars = new List<GameObject> ();
	public AI_Control.AiDifficulty aiDifficulty = AI_Control.AiDifficulty.Hard;
	public AISpawnType aiSpawnType;

	[Header("Race Containers")]
	public Transform pathContainer;
	public Transform spawnpointContainer;
	public Transform checkpointContainer;
	[HideInInspector] public List<Transform> spawnpoints = new List<Transform> ();

	[Header("Racers Name")]
	public string playerName = "Gracz";
	public GameObject racerName;
	public List<string> opponentNamesList = new List<string> ();

	[Header("Race Rewards")]
	public List<RaceRewards> raceRewards = new List<RaceRewards> ();

	[Header("Misc Settings")]
	public Button continueButton;
	private bool showRacerNames;
	[HideInInspector] public bool raceStarted;
	[HideInInspector] public bool raceCompleted;
	private bool showWelcomePanel;

	void Awake(){
		instance = this;
		if (PlayerProfile.instance) {
			LoadRaceData ();
		}
	}
		
	void Start(){
		AssignRacePreferences ();
		ConfigureNodes ();
		SpawnRacers ();
	}
		
	void LoadRaceData(){
		playerName = PlayerProfile.instance.player.m_name;
		showRacerNames = PlayerProfile.instance.player.m_ShowRacerName3D;
		showWelcomePanel = PlayerProfile.instance.player.m_ShowWelcomePanel;
		totalLaps = PlayerPrefs.GetInt ("Laps");
		totalRacers = PlayerPrefs.GetInt ("Opponents") + 1;
		trackIndex = PlayerPrefs.GetInt ("Track");
	
		switch (PlayerPrefs.GetString ("RaceType")) {
		case "Circuit":
			raceType = RaceManager.RaceType.Circuit;
			break;
		case "LapKnockout":
			raceType = RaceManager.RaceType.LapKnockout;
			break;
		case "TimeTrial":
			raceType = RaceManager.RaceType.TimeTrial;
			break;
		case "SpeedTrap":
			raceType = RaceManager.RaceType.SpeedTrap;
			break;
		case "Checkpoints":
			raceType = RaceManager.RaceType.Checkpoints;
			break;
		}

		switch (PlayerPrefs.GetString ("AiDifficulty")) {
		case "Easy":
			aiDifficulty = AI_Control.AiDifficulty.Easy;
			break;
		case "Medium":
			aiDifficulty = AI_Control.AiDifficulty.Medium;
			break;
		case "Hard":
			aiDifficulty = AI_Control.AiDifficulty.Hard;
			break;
		}
	}

	void AssignRacePreferences(){
		switch (raceType) {
		case RaceType.Circuit:
			timerType = TimerType.CountUp;
			break;

		case RaceType.Checkpoints:
			timerType = TimerType.CountDown;
			break;

		case RaceType.LapKnockout:
			if (totalRacers < 2) {
				totalRacers = 2;
			}
			totalLaps = totalRacers - 1;
			timerType = TimerType.CountUp;
			break;

		case RaceType.TimeTrial:
			totalRacers = 1;
			timerType = TimerType.CountUp;
			break;
		}
	}

	public void SwitchRaceState(RaceState state){
		raceState = state;
		RaceUI.instance.UpdateUIPanels ();
	}

	void ConfigureNodes(){
		Transform[] m_path = pathContainer.GetComponentsInChildren<Transform> ();
		List<Transform> m_pathList = new List<Transform> ();

		foreach (Transform node in m_path) {
			if (node != pathContainer) {
				m_pathList.Add (node);
			}
		}
		for (int i = 0; i < m_pathList.Count; i++) {
			if (i < m_pathList.Count - 1) {
				m_pathList [i].transform.LookAt (m_pathList [i + 1].transform);
			} else {
				m_pathList [i].transform.LookAt (m_pathList [0].transform);
			}
		}
		raceDistance = pathContainer.GetComponent<WaypointCircuit> ().distances [pathContainer.GetComponent<WaypointCircuit> ().distances.Length - 1];
	}
		
	void SetPlayerStartPosition(){
		if (PlayerProfile.instance.player.m_PlayerStartPosition == 0) {
			playerSpawnPosition = PlayerSpawnPosition.Selected;
		} else {
			playerSpawnPosition = PlayerSpawnPosition.Randomized;
		}
	}

	void SpawnRacers(){
		playerCar = GameObject.FindGameObjectWithTag ("Player");
		SetPlayerStartPosition ();
		spawnpoints.Clear ();
		Transform[] sp = spawnpointContainer.GetComponentsInChildren<Transform> ();
		foreach (Transform point in sp) {
			if (point != spawnpointContainer) {
				spawnpoints.Add (point);
			}
		}
		if (playerSpawnPosition == PlayerSpawnPosition.Randomized) {
			playerStartRank = Random.Range (1, totalRacers);
		} else {
			playerStartRank = totalRacers;
		}

		for (int i = 0; i < totalRacers; i++) {
			if (spawnpoints [i] != spawnpoints [playerStartRank - 1] && opponentCars.Count > 0) {
				if (aiSpawnType == AISpawnType.Order) {
					spawnIndex = 0;
				} else {
					spawnIndex = Random.Range (0, opponentCars.Count);
				}
				if (spawnIndex > opponentCars.Count) {
					spawnIndex = opponentCars.Count - 1;
				}
				Instantiate (opponentCars [spawnIndex], spawnpoints [i].position, spawnpoints [i].rotation);
				opponentCars.RemoveAt (spawnIndex);

			} else if (spawnpoints [i] == spawnpoints [playerStartRank - 1] && playerCar) {
				Transform spawnPos = spawnpoints [i];
				playerCar.AddComponent<RacerProgress> ();
				playerCar.GetComponent<Car_Controller> ().enabled = true;
				playerCar.GetComponent<Car_Controller> ().driver.SetActive (true);
				Wheels[] wheels = playerCar.GetComponentsInChildren<Wheels> ();
				foreach (Wheels w in wheels) {
					w.enabled = true;
				}
				GameObject player = (GameObject)Instantiate (playerCar, spawnPos.position, spawnPos.rotation);
				player.GetComponent<CarSound> ().enabled = true;
				Destroy (playerCar.GetComponent<RacerProgress> ());
				playerCar.SetActive (false);
			}
		}
		RankManager.instance.RefreshRacerCount ();
		RaceUI.instance.RefreshInRaceStandings (); 
		SetRacerPreferences ();

		if (!showWelcomePanel) {
			StartCoroutine (Countdown (countdownDelay));
		} else {
			CameraManager.instance.ActivateOrbitCamera ();
		}
	}

	void SetRacerPreferences(){
		racers = GameObject.FindObjectsOfType (typeof(RacerProgress)) as RacerProgress[];
		for (int i = 0; i < racers.Length; i++) {
			racers [i].name = racers [i].name.Replace ("Clone", "");
			if (racers [i].gameObject.tag == "Player") {
				racers [i].racerDetails.racerName = playerName;
				racers [i].racerDetails.carName = PlayerPrefs.GetString ("PlayerCarName");
			} else {
				int nameIndex = Random.Range (0, opponentNamesList.Count);
				if (nameIndex > opponentNamesList.Count) {
					nameIndex = opponentNamesList.Count - 1;
				}
				racers [i].racerDetails.racerName = opponentNamesList [nameIndex].ToString ();
				opponentNamesList.RemoveAt (nameIndex);

				if (racerName) {
					GameObject _name = (GameObject)Instantiate (racerName);
					_name.GetComponent<RacerName> ().target = racers [i].transform;
					_name.GetComponent<RacerName> ().Initialize ();
					SettingsMenu.instance.names3D.Add (_name);
					if (!showRacerNames) {
						_name.SetActive (false);
					}
				}
				racers [i].gameObject.GetComponent<AI_Control> ().SetDifficulty (aiDifficulty);
			}
		}
	}

	public IEnumerator Countdown(float delay){
		SwitchRaceState (RaceState.InRace);
		PlayerProfile.instance.player.m_ShowWelcomePanel = false;
		CameraManager.instance.ActivatePlayerCamera ();
		yield return new WaitForSeconds (delay);
		currentCountdownTime = countdownFrom + 1;
		startCountdown = true;

		while (startCountdown == true) {
			countdownTimer -= Time.deltaTime;
			if (currentCountdownTime >= 1) {
				if (countdownTimer < 0.01f) {
					currentCountdownTime -= 1;
					countdownTimer = 1;
					if (currentCountdownTime > 0) {
						RaceUI.instance.SetCountDownText (currentCountdownTime.ToString ());
						SoundManager.instance.PlayDefaultSound (SoundManager.instance.raceSounds.countdownSound);
					}
				}
			} else {
				startCountdown = false;
				RaceUI.instance.SetCountDownText ("START !");
				SoundManager.instance.PlayDefaultSound (SoundManager.instance.raceSounds.startRaceSound);
				StartRace ();
				yield return new WaitForSeconds (1);
				RaceUI.instance.SetCountDownText (string.Empty);
			}
			yield return null;
		}
	}
		
	public void StartRace(){
		racers = GameObject.FindObjectsOfType (typeof(RacerProgress)) as RacerProgress[];
		foreach (RacerProgress go in racers) {
			go.GetComponent<Car_Controller> ().controllable = true;
		}
		raceStarted = true;
	}

	public void EndRace(int rank){
		StartCoroutine (EndRaceRoutine ());
		raceCompleted = true;
		PlayerCamera.instance.allowOrbit = false;
		continueButton.onClick.AddListener (delegate {
			CalculateRaceRewards (rank);
		});
	}

	IEnumerator EndRaceRoutine(){
		RaceUI.instance.DisableRacePanelChildren ();
		RaceUI.instance.SetFinishedText ("WYŚCIG ZAKOŃCZONY");
		yield return new WaitForSeconds (3.0f);
		SwitchRaceState (RaceState.Complete);
	}
		
	void CalculateRaceRewards(int pos){
		if (raceRewards.Count >= pos) {
			if (PlayerProfile.instance) {
				if (raceRewards [pos - 1].money > 0) {
					PlayerProfile.instance.IncreaseMoney (raceRewards [pos - 1].money);
					RaceUI.instance.SetRewardText (raceRewards [pos - 1].money.ToString ("N0"), "", "", raceRewards [pos - 1].trophy);
				}
				if ((PlayerProfile.instance.player.m_TrackStatus.Length - 1 > trackIndex) && raceRewards [pos - 1].trackUnlock != "") {
					PlayerProfile.instance.player.m_TrackStatus [trackIndex + 1] = true;
					RaceUI.instance.SetRewardText ("", "", raceRewards [pos - 1].trackUnlock, raceRewards [pos - 1].trophy);
				}
			}
		} else {
			RaceUI.instance.Exit ();
		}
	}

	public void PauseRace(){
		if (raceCompleted || raceState == RaceState.OnStart)
			return;

		if (raceState == RaceState.PausedRace) {
			SwitchRaceState (RaceState.InRace);
			Time.timeScale = 1.0f;
			SoundManager.instance.SetSfxVolume (PlayerProfile.instance.player.m_SfxVolume);
		} else {
			SwitchRaceState (RaceState.PausedRace);
			Time.timeScale = 0.0f;
			SoundManager.instance.SetSfxVolume (-80f);
		}
	}
		
	public void KnockoutRacer(RacerProgress racer){
		racer.knockedOut = true;
		racer.gameObject.GetComponent<MiniMapItem> ().HideItem ();
		string reason;
		string keyword;
		if (racer.tag == "Player") {
			SwitchRaceState (RaceState.KnockedOut);
			PlayerCamera.instance.allowOrbit = false;
			racer.AIMode ();

			if (raceType == RaceType.LapKnockout) {
				reason = "Zostałeś wyeliminowany z wyścigu !";
			} else {
				reason = "Skończył się czas !";
			}
			RaceUI.instance.SetFailRace ("PRZEGRYWASZ", reason);
		}

		if (raceType == RaceType.LapKnockout) {
			keyword = " został wyeliminowany !";
		} else {
			keyword = " przekroczył limit czasu !";
		}
		RaceUI.instance.ShowRaceInfo (racer.racerDetails.racerName + keyword, 2.0f, Color.white);
		ChangeLayer (racer.transform, "IgnoreCollision");
		RankManager.instance.RefreshRacerCount ();
	}

	public string FormatTime(float time){
		int minutes = (int)Mathf.Floor (time / 60);
		int seconds = (int)time % 60;
		int milliseconds = (int)(time * 100) % 100;
		return string.Format ("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
	}
		
	public void ChangeLayer(Transform racer, string LayerName){
		for (int i = 0; i < racer.childCount; i++){
			racer.GetChild(i).gameObject.layer = LayerMask.NameToLayer(LayerName);
			ChangeLayer(racer.GetChild(i), LayerName);
		}
	}
}