using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI_Control : MonoBehaviour
{
	public enum AiDifficulty { Easy, Medium, Hard }
	public enum AiState { FollowingPath, Overtaking }
	public AiDifficulty difficulty = AiDifficulty.Hard;
	private AiState aiState;
	private RacerProgress stats;
	private Car_Controller car_controller;

	[Header("Behaviour")]
	[Range(0, 1f)] public float accelerationSensitivity = 1f;
	[Range(0, 1f)] public float accelerationWander = 0.1f; 
	[Range(0, 1f)] private float accelerationFluctationRate = 0.5f;
	[Range(0, 1f)] public float brakeSensitivity = 1f;
	[Range(0, 0.1f)] public float steerSensitivity = 0.01f;
	[Range(0, 0.2f)] public float avoidanceSensitivity = 0.2f;
	[Range(0, 25)] public float maxWanderDistance = 2f;
	[Range(0, 0.25f)] public float wanderRate = 0.1f;
	[Range(0, 1)] public float cautionSpeed = 0.3f;
	[Range(0, 5)] public float overtakeStrength = 1.5f;
	[Range(0, 1)] public float nitroProbability = 0.5f;
	[Range(0, 180)] public float cautionAngle = 45f;
	public float cautionAmount = 30f;
	public float maxOvertakeTime = 5f; 
	public bool randomBehaviour = true;

	[Header("Sensors")]
	public float forwardDistance = 18f;
	public float forwardLateralDistance = 1.5f;
	public float sidewayDistance = 2.5f;
	public float angleDistance = 8f;
	public float sensorHeight = 0f;

	[Header("Misc")]
	public float respawnTime = 10f;

	private Transform target;
	private Transform overtakeTarget;
	private Vector3 offsetTargetPos;
	private Vector3 steerVector;
	private Vector3 lastStuckPos;
	private float newSteer;
	private float avoidanceSteer;
	private float avoidOffset;
	private float randomValue;
	private float requiredSpeed;
	private float aproachingAngle;
	private float spinningAngle;
	private float cautionRequired;
	private float throttleSensitvity;
	private float throttle;
	private float targetAngle;
	private float stuckTimer;
	private float reverseTimer;
	private float topSpeed;
	private float currentSpeed;
	private float brakeSpeed = 50f; 
	private float overtakeCounter;
	private bool overtakeNow;
	private bool reverse;
	private bool somethingFront;
	private bool somethingLeft;
	private bool somethingRight;
	private bool brakezone;
	private float lineFollow;

	void Awake(){
		stats = GetComponent<RacerProgress> ();
		car_controller = GetComponent<Car_Controller> ();
		randomValue = Random.value * 100;
	}

	void Start(){
		SetDifficulty (difficulty);
		RandomizeAIBehaviour ();
		InvokeRepeating ("Sensors", 1, 0.1f);
	}

	void Update(){
		CheckIfStuck ();
	}

	void FixedUpdate(){
		currentSpeed = car_controller.currentSpeed;
		if (!brakezone && !stats.knockedOut) { 
			topSpeed = car_controller.topSpeed;
		} else {
			topSpeed = brakeSpeed;
		}
		NavigateAI ();
	}

	void RandomizeAIBehaviour(){
		if (!randomBehaviour)
			return;
		wanderRate = Random.Range (0.01f, 0.25f);
		accelerationFluctationRate = Random.Range (0.1f, 1f);
	}

	public void SetDifficulty(AiDifficulty _difficulty){
		difficulty = _difficulty;

		switch (_difficulty) {
		case AiDifficulty.Easy:
			accelerationSensitivity = 0.5f;
			brakeSensitivity = 0.5f;
			accelerationWander = Random.Range (0.5f, 0.75f);
			nitroProbability = Random.Range (0.0f, 0.25f);
			break;

		case AiDifficulty.Medium:
			accelerationSensitivity = 0.8f;
			brakeSensitivity = 0.8f;
			accelerationWander = Random.Range (0.1f, 0.25f);
			nitroProbability = Random.Range (0.5f, 0.75f);
			break;

		case AiDifficulty.Hard:
			accelerationSensitivity = Random.Range (0.9f, 1f);
			brakeSensitivity = 1f;
			accelerationWander = 0;
			nitroProbability = Random.Range (0.85f, 1f);
			break;
		}
	}
		
	void NavigateAI(){
		target = stats.target;
		Vector3 fwd = transform.forward;
		requiredSpeed = topSpeed;
		aproachingAngle = Vector3.Angle (target.forward, fwd);
		spinningAngle = GetComponent<Rigidbody> ().angularVelocity.magnitude * cautionAmount;
		cautionRequired = Mathf.InverseLerp (0, cautionAngle, Mathf.Max (spinningAngle, aproachingAngle));
		requiredSpeed = Mathf.Lerp (topSpeed, topSpeed * (cautionSpeed), cautionRequired);
		offsetTargetPos = target.position;
	
		if (aiState == AiState.FollowingPath) {
			lineFollow = (Mathf.PerlinNoise (Time.time * wanderRate, randomValue) * 2 - 1) * maxWanderDistance;
		} else {
			lineFollow = avoidOffset;
		}
		offsetTargetPos += target.right * lineFollow;
		if (requiredSpeed < currentSpeed) {
			throttleSensitvity = brakeSensitivity;
		} else {
			throttleSensitvity = accelerationSensitivity;
		}
		throttle = Mathf.Clamp ((requiredSpeed - currentSpeed) * throttleSensitvity, -1, 1);
		throttle *= (1 - accelerationWander) + (Mathf.PerlinNoise (Time.time * accelerationFluctationRate, randomValue) * accelerationWander);
		steerVector = transform.InverseTransformPoint (offsetTargetPos);
		targetAngle = Mathf.Atan2 (steerVector.x, steerVector.z) * Mathf.Rad2Deg;
		newSteer = Mathf.Clamp (targetAngle * steerSensitivity, -1, 1) * Mathf.Sign (currentSpeed);
		FeedInput (throttle, throttle, newSteer);
	}
		
	void FeedInput(float motor, float brake, float steer){
		float m, b, s;
		if (!reverse) {
			m = motor = Mathf.Clamp (motor, 0, 1);
			b = brake = -1 * Mathf.Clamp (brake, -1, 0);
			s = steer;
		} else {
			m = 0;
			b = 1;
			s = -steer;
		}
		s += avoidanceSteer;
		car_controller.motorInput = m;
		car_controller.brakeInput = b;
		car_controller.handbrakeInput = 0;
		car_controller.steerInput = s + avoidanceSteer;
	}
		
	void Sensors(){
		Vector3 fwd;
		RaycastHit hit;
		avoidanceSteer = 0f;
		somethingFront = false;
		somethingLeft = false;
		somethingRight = false;

		fwd = new Vector3 (transform.position.x, transform.position.y + sensorHeight, transform.position.z);
		if (Physics.Raycast (fwd, transform.forward, out hit, forwardDistance)) {
			if (hit.collider.gameObject.layer != LayerMask.NameToLayer ("IgnoreCollision") && hit.transform.root.GetComponent<RacerProgress> ()) {
				overtakeTarget = hit.transform;
				if (aiState != AiState.Overtaking && !somethingLeft && !somethingRight) {
					StartCoroutine (AttemptOvertake ());
				}
				if (hit.distance <= 5f) {
					requiredSpeed *= 0.5f;
				}
				somethingFront = true;
			} else {
				somethingFront = true;
			}
		}

		fwd += transform.right * forwardLateralDistance;
		if (Physics.Raycast (fwd, transform.forward, out hit, forwardDistance)) {
			if (hit.collider.gameObject.layer != LayerMask.NameToLayer ("IgnoreCollision") && hit.transform.root.GetComponent<RacerProgress> ()) {
				if (hit.distance <= 5f) {
					requiredSpeed *= 0.5f;
				}
				somethingFront = true;
			} else {
				somethingFront = true;
			}
		}
			
		fwd = new Vector3 (transform.position.x, transform.position.y + sensorHeight, transform.position.z);
		fwd -= transform.right * forwardLateralDistance;
		if (Physics.Raycast (fwd, transform.forward, out hit, forwardDistance)) {
			if (hit.collider.gameObject.layer != LayerMask.NameToLayer ("IgnoreCollision") && hit.transform.root.GetComponent<RacerProgress> ()) {
				if (hit.distance <= 5f) {
					requiredSpeed *= 0.5f;
				}
				somethingFront = true;
			} else {
				somethingFront = true;
			}
		}
			
		fwd = new Vector3 (transform.position.x, transform.position.y + sensorHeight, transform.position.z);
		if (Physics.Raycast (fwd, Quaternion.AngleAxis (30, transform.up) * transform.forward, out hit, angleDistance)) {
			if (hit.collider.gameObject.layer != LayerMask.NameToLayer ("IgnoreCollision") && hit.transform.root.GetComponent<RacerProgress> ()) {
				avoidanceSteer = -0.5f * avoidanceSensitivity;
				somethingRight = true;
			} else {
				somethingRight = true;
			}
		}
			
		if (Physics.Raycast (fwd, Quaternion.AngleAxis (-30, transform.up) * transform.forward, out hit, angleDistance)) {
			if (hit.collider.gameObject.layer != LayerMask.NameToLayer ("IgnoreCollision") && hit.transform.root.GetComponent<RacerProgress> ()) {
				avoidanceSteer = 0.5f * avoidanceSensitivity;
				somethingLeft = true;
			} else {
				somethingLeft = true;
			}
		}
			
		if (Physics.Raycast (fwd, Quaternion.AngleAxis (90, transform.up) * transform.forward, out hit, sidewayDistance)) {
			if (hit.collider.gameObject.layer != LayerMask.NameToLayer ("IgnoreCollision") && hit.transform.root.GetComponent<RacerProgress> ()) {
				avoidanceSteer = -0.5f * avoidanceSensitivity;
				somethingRight = true;
			} else {
				somethingRight = true;
			}
		}
			
		if (Physics.Raycast (fwd, Quaternion.AngleAxis (-90, transform.up) * transform.forward, out hit, sidewayDistance)) {
			if (hit.collider.gameObject.layer != LayerMask.NameToLayer ("IgnoreCollision") && hit.transform.root.GetComponent<RacerProgress> ()) {
				avoidanceSteer = 0.5f * avoidanceSensitivity;
				somethingLeft = true;
			} else {
				somethingLeft = true;
			}
		}
	}

	IEnumerator AttemptOvertake(){
		aiState = AiState.Overtaking;
		overtakeNow = true;

		while (overtakeNow) {
			overtakeCounter += Time.deltaTime;
			if (overtakeCounter <= maxOvertakeTime) {
				var otherCarLocalDelta = transform.InverseTransformPoint (overtakeTarget.transform.position);
				float otherCarAngle = Mathf.Atan2 (otherCarLocalDelta.x, otherCarLocalDelta.z);
				avoidOffset = overtakeStrength * -Mathf.Sign (otherCarAngle);
			} else {
				overtakeNow = false;
				EndOvertake ();
			}
				
			if (overtakeTarget) {
				if (Vector3.Angle (transform.forward, overtakeTarget.transform.position - transform.position) > 90 && PositionDifference (overtakeTarget.position, transform.position) > 5f) {
					overtakeNow = false;
					EndOvertake ();
				}
					
				if (PositionDifference (overtakeTarget.position, transform.position) > 20f) {
					overtakeNow = false;
					EndOvertake ();
				}
			}
			yield return null;
		}
	}

	void EndOvertake(){
		aiState = AiState.FollowingPath;
		overtakeCounter = 0;
	}

	void CheckIfStuck(){
		if (RaceManager.instance.raceStarted && !stats.finishedRace && !stats.knockedOut) {
			if (currentSpeed <= 3f) {
				stuckTimer += Time.deltaTime;
			} else {
				stuckTimer = 0f;
			}
			if (stuckTimer >= 3f && !reverse) {
				if (somethingFront) {
					StartCoroutine (Reverse ());
				}
			}
		}
	}

	IEnumerator Reverse(){
		reverse = true;
		while (reverse) {
			if (reverseTimer < 3f) {
				reverseTimer += Time.deltaTime;
			} else {
				reverse = false;
				reverseTimer = 0;
			}
			yield return null;
		}
	}

	float PositionDifference(Vector3 lastPos, Vector3 currPos){
		return Vector3.Distance (lastPos, currPos);
	}

	void OnCollisionEnter(Collision other){
		if (other.transform.root.GetComponent<RacerProgress> ()) {
			if (Vector3.Angle (transform.forward, other.transform.position - transform.position) < 90) {
				overtakeTarget = other.transform;
				if (aiState != AiState.Overtaking) {
					StartCoroutine (AttemptOvertake ());
				}
			}
		}
	}

	void OnTriggerEnter(Collider other){
		if (other.tag == "NitroTrigger") {
			float random = Random.Range ((float)0, (float)1);
			if (random <= nitroProbability) {
				StartCoroutine (UseNitro (5f));
			}
		}
	}

	void OnTriggerStay(Collider other){
		if (other.GetComponent<Brakezone> ()) {
			brakezone = true;
			brakeSpeed = other.GetComponent<Brakezone> ().targetSpeed;
		}
	}

	void OnTriggerExit(Collider other){
		if (other.GetComponent<Brakezone> ()) {
			brakezone = false;
		}
	}

	IEnumerator UseNitro(float time){
		if (!enabled || GetComponent<RacerProgress> ().knockedOut)
			yield break;

		if (car_controller.useNitro)
			yield break;

		car_controller.useNitro = !somethingFront;
		yield return new WaitForSeconds (time);
		car_controller.useNitro = false;
	}
}