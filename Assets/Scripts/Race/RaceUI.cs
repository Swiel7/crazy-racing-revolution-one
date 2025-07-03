using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RaceUI : MonoBehaviour 
{
	[System.Serializable]
	public class RacerInfo
	{
		public Text position;
		public Text name;
		public Text carName;
		public Text bestLapTime;
		public Text totalTime;
	}

	[System.Serializable]
	public class RacingUI
	{
		public Text rank;
		public Text lap;
		public Text currentLapTime;
		public Text previousLapTime;
		public Text bestLapTime;
		public Text totalTime;
		public Text countdown;
		public Text raceInfo;
		public Text finishedText;
	
		[Header("In Race Standings")]
		public List<RacerInfo> inRaceStandings = new List<RacerInfo> ();
		public Color playerColor = Color.green;
		public Color normalColor = Color.white;

		[Header("Wrongway Indication")]
		public Text wrongwayText;
		public Image wrongwayImage;
	}

	[System.Serializable]
	public class CarUI
	{
		public Text currentSpeed;
		public Text currentGear;
		public Image nitro;
		private string speedUnit;

		[Header("Speedometer")]
		public RectTransform needle;
		public float minNeedleAngle = -20.0f;
		public float maxNeedleAngle = 220.0f;
		public float rotationMultiplier = 0.85f;
		[HideInInspector] public float needleRotation;
	}

	[System.Serializable]
	public class Rewards
	{
		public Text rewardCurrency;
		public Text rewardTrack;
		public Image trophyImage;
	}

	public static RaceUI instance;
	private RacerProgress player;
	private Car_Controller playerCar;

	[Header("Welcome UI")]
	public GameObject welcomePanel;

	[Header("Racing UI")]
	public GameObject HUD;
	public GameObject pausePanel;
	public RacingUI racingUI;
	public CarUI carUI;

	[Header("Fail Race UI")]
	public GameObject failRacePanel;
	public Text failTitle;
	public Text failReason;

	[Header("Race Finished UI")]
	public GameObject raceResultsPanel;
	public List<RacerInfo> raceResults = new List<RacerInfo> ();

	[Header("Rewards UI")]
	public GameObject rewardPanel;
	public Rewards rewardTexts;
	[HideInInspector] public List<string> raceInfos = new List<string> ();

	void Awake(){
		instance = this;
	}

	void Start(){
		UpdateUIPanels ();
	}
		
	void Update(){
		if (!player && GameObject.FindGameObjectWithTag ("Player")) {
			player = GameObject.FindGameObjectWithTag ("Player").GetComponent<RacerProgress> ();
			playerCar = GameObject.FindGameObjectWithTag ("Player").GetComponent<Car_Controller> ();
		}
		UpdateUI ();
		CarGUI ();
	}
		
	void UpdateUI(){
		switch (RaceManager.instance.raceType) {
		case RaceManager.RaceType.Circuit:
			DefaultUI ();
			break;

		case RaceManager.RaceType.LapKnockout:
			DefaultUI ();
			break;

		case RaceManager.RaceType.TimeTrial:
			TimeTrialUI ();
			break;

		case RaceManager.RaceType.SpeedTrap:
			DefaultUI ();
			break;

		case RaceManager.RaceType.Checkpoints:
			CheckpointRaceUI ();
			break;
		}

		switch (RaceManager.instance.raceState) {
		case RaceManager.RaceState.OnStart:
			break;

		case RaceManager.RaceState.InRace:
			Invoke("ShowInRaceStandings", 0.000000001f);
			WrongwayUI ();
			break;

		case RaceManager.RaceState.Complete:
			ShowRaceResults ();
			break;
		}
	}
		
	void DefaultUI(){
		racingUI.rank.text = "Pozycja: " + player.currentRank + "/" + RankManager.instance.currentRacers;
		racingUI.lap.text = "Okrążenie: " + player.currentLap + "/" + RaceManager.instance.totalLaps;
		racingUI.currentLapTime.text = "Aktualny czas: " + player.currentLapTime;
		racingUI.totalTime.text = "Łączny czas: " + player.totalRaceTime;
		racingUI.previousLapTime.text = GetPrevLapTime ();
		racingUI.bestLapTime.text = GetBestLapTime ();
	}
		
	void TimeTrialUI(){
		racingUI.rank.text = "Pozycja: " + player.GetComponent<RacerProgress> ().currentRank + "/" + RankManager.instance.currentRacers;
		racingUI.lap.text = "Okrążenie: " + player.currentLap;
		racingUI.currentLapTime.text = "Aktualny czas: " + player.currentLapTime;
		racingUI.totalTime.text = "Łączny czas: " + player.totalRaceTime;
		racingUI.previousLapTime.text = GetPrevLapTime ();
		racingUI.bestLapTime.text = GetBestLapTime ();
	}

	void CheckpointRaceUI(){
		racingUI.rank.text = "Pozycja: " + player.GetComponent<RacerProgress> ().currentRank + "/" + RankManager.instance.currentRacers;
		racingUI.lap.text = "Punkt kontrolny: " + player.currentCheckpoint + "/" + player.checkpoints.Count * RaceManager.instance.totalLaps;
		racingUI.currentLapTime.text = "Czas: " + player.currentLapTime;
		racingUI.bestLapTime.text = GetBestLapTime ();
		racingUI.previousLapTime.text = "";
		racingUI.totalTime.text = "";
	}
		
	void CarGUI(){
		carUI.currentSpeed.text = playerCar.currentSpeed + playerCar.speedUnit.ToString ();
		carUI.currentGear.text = (playerCar.currentGear + 1).ToString ();
		carUI.nitro.fillAmount = playerCar.nitroCapacity;

		//Speedometer
		float fraction = playerCar.currentSpeed / carUI.maxNeedleAngle;
		carUI.needleRotation = Mathf.Lerp(carUI.minNeedleAngle, carUI.maxNeedleAngle, (fraction * carUI.rotationMultiplier));
		carUI.needle.transform.eulerAngles = new Vector3(carUI.needle.transform.eulerAngles.x, carUI.needle.transform.eulerAngles.y, -carUI.needleRotation);
	}

	public void UpdateUIPanels(){
		switch (RaceManager.instance.raceState) {
		case RaceManager.RaceState.OnStart:
			welcomePanel.SetActive (true);
			HUD.SetActive (false);
			pausePanel.SetActive (false);
			failRacePanel.SetActive (false);
			raceResultsPanel.SetActive (false);
			break;

		case RaceManager.RaceState.InRace:
			welcomePanel.SetActive (false);
			HUD.SetActive (true);
			pausePanel.SetActive (false);
			failRacePanel.SetActive (false);
			raceResultsPanel.SetActive (false);
			break;

		case RaceManager.RaceState.PausedRace:
			welcomePanel.SetActive (false);
			HUD.SetActive (false);
			pausePanel.SetActive (true);
			failRacePanel.SetActive (false);
			raceResultsPanel.SetActive (false);
			break;

		case RaceManager.RaceState.Complete:
			welcomePanel.SetActive (false);
			HUD.SetActive (false);
			pausePanel.SetActive (false);
			failRacePanel.SetActive (false);
			raceResultsPanel.SetActive (true);
			break;

		case RaceManager.RaceState.KnockedOut:
			welcomePanel.SetActive (false);
			HUD.SetActive (false);
			pausePanel.SetActive (false);
			failRacePanel.SetActive (true);
			raceResultsPanel.SetActive (false);
			break;
		}
	}
		
	void ShowInRaceStandings(){
		if (racingUI.inRaceStandings.Count <= 0 || RankManager.instance.totalRacers <= 1)
			return;

		for (int i = 0; i < RankManager.instance.totalRacers; i++) {
			if (i < racingUI.inRaceStandings.Count) {
				RacerProgress _statistics = RankManager.instance.racerRanks [i].racer.GetComponent<RacerProgress> ();

				if (_statistics == null)
					return;

				if (RaceManager.instance.raceType != RaceManager.RaceType.SpeedTrap) {
					racingUI.inRaceStandings [i].name.text = _statistics.racerDetails.racerName;
				} else {
					racingUI.inRaceStandings [i].name.text = _statistics.racerDetails.racerName + " [" + Mathf.Round(RankManager.instance.racerRanks [i].speedRecord) + " " + playerCar.speedUnit + "]";
				}
					
				if (player == _statistics) {
					racingUI.inRaceStandings [i].position.color = racingUI.playerColor;
					racingUI.inRaceStandings [i].name.color = racingUI.playerColor;
				} else {
					racingUI.inRaceStandings [i].position.color = racingUI.normalColor;
					racingUI.inRaceStandings [i].name.color = racingUI.normalColor;
				}
			}
		}
	}

	public void RefreshInRaceStandings(){
		if (RankManager.instance.totalRacers <= 1)
			return;
		
		for (int i = 0; i < racingUI.inRaceStandings.Count; i++) {
			if (i < RankManager.instance.totalRacers) {
				racingUI.inRaceStandings [i].position.text = (i + 1).ToString ();
				racingUI.inRaceStandings [i].position.transform.parent.parent.gameObject.SetActive (true);
			}
		}
	}
		
	void ShowRaceResults(){
		if (raceResults.Count > 0) {
			for (int i = 0; i < RankManager.instance.totalRacers; i++) {
				RacerProgress _statistics = RankManager.instance.racerRanks [i].racer.GetComponent<RacerProgress> ();

				if (_statistics == null)
					return;
				raceResults [i].position.text = _statistics.currentRank.ToString ();

				if (RaceManager.instance.raceType != RaceManager.RaceType.SpeedTrap) {
					raceResults [i].name.text = _statistics.racerDetails.racerName;
				} else {
					raceResults [i].name.text = _statistics.racerDetails.racerName + " [" + Mathf.Round(RankManager.instance.racerRanks [i].speedRecord) + " " + playerCar.speedUnit + "]";
				}
					
				if (_statistics.finishedRace && !_statistics.knockedOut) {
					raceResults [i].totalTime.text = _statistics.totalRaceTime;
				} else if (_statistics.knockedOut) {
					raceResults [i].totalTime.text = "Wyeliminowany";
				} else {
					raceResults [i].totalTime.text = "W wyścigu...";
				}
				raceResults [i].bestLapTime.text = (_statistics.bestLapTime == string.Empty) ? "--:--:--" : _statistics.bestLapTime;
				raceResults [i].carName.text = _statistics.racerDetails.carName;
			}
		}
	}
		
	public void ShowRaceInfo(string info, float time, Color c){
		StartCoroutine (RaceInfo (info, time, c));
	}

	IEnumerator RaceInfo(string info, float time, Color c){
		if (!racingUI.raceInfo)
			yield break;

		if (racingUI.raceInfo.text == "") {
			racingUI.raceInfo.text = info;

			Color col = c;
			col.a = 1.0f;
			racingUI.raceInfo.color = col;
			yield return new WaitForSeconds (time);

			while (col.a > 0.0f) {
				col.a -= Time.deltaTime * 2.0f;
				racingUI.raceInfo.color = col;
				yield return null;
			}

			if (col.a <= 0.01f) {
				racingUI.raceInfo.text = string.Empty;
			}
				
			CheckRaceInfoList ();
		} else {
			raceInfos.Add (info);
		}
	}
		
	public void CheckRaceInfoList(){
		if (raceInfos.Count > 0) {
			ShowRaceInfo (raceInfos [raceInfos.Count - 1], 2.0f, Color.white);
			raceInfos.RemoveAt (raceInfos.Count - 1);
		}
	}

	void WrongwayUI(){
		if (player.GetComponent<RacerProgress> ().goingWrongway) {
			racingUI.wrongwayText.text = "Podążasz w złym kierunku !";
		} else {
			racingUI.wrongwayText.text = string.Empty;
		}

		if (player.GetComponent<RacerProgress> ().goingWrongway) {
			racingUI.wrongwayImage.enabled = true;
		} else {
			racingUI.wrongwayImage.enabled = false;
		}
	}

	string GetPrevLapTime(){
		if (player.prevLapTime != "") {
			return "Poprzedni czas: " + player.prevLapTime;
		} else {
			return "Poprzedni czas: --:--:--";
		}
	}

	string GetBestLapTime(){
		if (player.bestLapTime != "") {
			return "Najlepszy czas: " + player.bestLapTime;
		} else {
			return "Najlepszy czas: --:--:--";
		}
	}

	public void SetCountDownText(string value){
		racingUI.countdown.text = value;
	}

	public void SetFailRace(string title, string reason){
		failTitle.text = title;
		failReason.text = reason;
	}
		
	public void DisableRacePanelChildren(){
		RectTransform[] rectTransforms = HUD.GetComponentsInChildren<RectTransform> ();
		foreach (RectTransform t in rectTransforms) {
			if (t != HUD.GetComponent<RectTransform> () && t != racingUI.finishedText.GetComponent<RectTransform> ()) {
				t.gameObject.SetActive (false);
			}
		}
	}

	public void SetFinishedText(string word){
		racingUI.finishedText.text = word;
	}

	public void SetRewardText(string currency, string vehicleUnlock, string trackUnlock, Sprite trophy){
		if (currency != "") {
			rewardPanel.SetActive (true);
			rewardTexts.rewardCurrency.text = "Wygrałeś: " + currency + " C";
			rewardTexts.trophyImage.sprite = trophy;
		}
		if (trackUnlock != "") {
			rewardTexts.rewardTrack.text = "Odblokowałeś tor: " + trackUnlock;
		}
	}
		
	public void StartCountDown(float time){
		StartCoroutine(RaceManager.instance.Countdown(time));
	}

	public void PauseResume(){
		RaceManager.instance.PauseRace ();
	}

	public void Restart(){
		if (RaceManager.instance.raceState == RaceManager.RaceState.PausedRace) {
			PauseResume ();
		}
		RaceManager.instance.playerCar.SetActive (true);
		SettingsMenu.instance.aiPointers.Clear ();
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}

	public void Exit(){
		if (RaceManager.instance.raceState == RaceManager.RaceState.PausedRace) {
			PauseResume ();
		}
		Destroy (GameObject.Find ("Settings Manager"));
		Destroy (GameObject.Find ("Cars"));
		PlayerPrefs.DeleteAll ();
		SceneManager.LoadScene ("Garage");
	}

	public void OpenSettingsMenu(){
		SettingsMenu.instance.GeneralSettingsMenu ();
	}
}