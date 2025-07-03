using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RankManager : MonoBehaviour 
{
	[System.Serializable]
	public class Ranker : IComparer<Ranker>
	{
		public GameObject racer;
		public float raceCompletion;
		public float speedRecord;

		public int Compare(Ranker x, Ranker y){
			if (RaceManager.instance.raceType != RaceManager.RaceType.SpeedTrap) {
				return x.raceCompletion.CompareTo (y.raceCompletion);
			} else {
				return x.speedRecord.CompareTo (y.speedRecord);
			}
		}
	}

	public static RankManager instance;
	[HideInInspector] public List<Ranker> racerRanks = new List<Ranker> (new Ranker[30]);
	[HideInInspector] public List<RacerProgress> racerStats = new List<RacerProgress> ();
	[HideInInspector] public int totalRacers;
	[HideInInspector] public int currentRacers;

	void Awake(){
		instance = this;
	}
		
	void Start(){
		InvokeRepeating ("SetCarRank", 0.4f, 0.5f);
	}

	void Update(){
		FillList ();
	}

	public void RefreshRacerCount(){
		RacerProgress[] m_racers = GameObject.FindObjectsOfType (typeof(RacerProgress)) as RacerProgress[];
		totalRacers = m_racers.Length;

		for (int i = 0; i < m_racers.Length; i++) {
			if (m_racers [i].knockedOut == false) {
				if (!racerStats.Contains (m_racers [i].GetComponent<RacerProgress> ())) {
					racerStats.Add (m_racers [i].GetComponent<RacerProgress> ());
				}
			} else {
				racerStats.Remove (m_racers [i].GetComponent<RacerProgress> ());
			}
		}
		racerRanks.RemoveRange (totalRacers, racerRanks.Count - totalRacers);
		currentRacers = racerStats.Count;
	}
		
	void FillList(){
		for (int i = 0; i < currentRacers; i++) {
			if (racerRanks [i] != null && racerStats [i] != null) {
				racerRanks [i].racer = racerStats [i].gameObject;
				racerRanks [i].raceCompletion = racerStats [i].raceCompletion - ((float)racerStats [i].currentRank / 10000);
				racerRanks [i].speedRecord = racerRanks [i].racer.GetComponent<RacerProgress> ().speedRecord;
			}
		}
		Ranker m_ranker = new Ranker ();
		racerRanks.Sort (m_ranker);
		racerRanks.Reverse ();
	}

	public RacerProgress GetLastPlace(){
		return racerRanks [currentRacers - 1].racer.GetComponent<RacerProgress> ();
	}

	void SetCarRank(){
		for (int r = 0; r < currentRacers; r++) {
			racerRanks [r].racer.GetComponent<RacerProgress> ().currentRank = r + 1;
		}
	}
}