using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Car_Controller : MonoBehaviour 
{
	public enum CarDriveType {FWD, RWD, AWD}
	public enum SpeedUnit {MPH, KPH}
	public CarDriveType carDriveType;
	public SpeedUnit speedUnit;
	public WheelCollider[] wheelColliders = new WheelCollider[4];
	public Transform[] wheelTransforms = new Transform[4];
	public Wheels[] wheelEffects = new Wheels[4];

	[Header("Engine settings")]
	public float engineTorque = 800f;
	public float brakeTorque = 2000f;
	public float handbrakeTorque = 6000f;
	public float maxSteerAngle = 30f;		
	public float maxReverseSpeed = 75f;	
	public float topSpeed = 150f;
	public float acceleration = 100f;
	public float currentSpeed;
	private float currentSteerAngle;
	public float brakeForce = 15000f;

	[Header("Stability settings")]
	private Rigidbody rigid;
	public Transform COM;
	public float antiRoll = 8000f;
	public float downforce = 50f;
	[Range(0, 1)] public float steerHelper = 0.8f;
	[Range(0, 1)] public float tractionControl = 0.5f;

	[Header("Gears settings")]
	public int numberOfGears = 6;
	public int currentGear;
	private float gearFactor;
	private float revRangeBoundary = 1f;
	public float Revs { get; private set; }

	[Header("Wheel Slip Settings")]
	public float sidewaysSlipLimit = 0.35f;
	public float forwardSlipLimit = 0.4f;

	[Header("Slipstream Settings")]
	public float slipstreamRayHeight = 0f;
	public float slipstreamRayLength = 20f;
	[Range(0.1f, 5)] public float slipstreamStrength = 2.5f;

	[Header("Nitro Settings")]
	public GameObject nitroObject;
	[Range(0, 10)] public float nitroStrength = 5f;
	[Range(0, 50)] public float nitroAddedSpeed = 20f;
	public float nitroRegenerationRate = 0.1f;
	public float nitroDepletionRate = 0.25f;
	public float nitroCapacity = 1f;
	private float nitroTopSpeed;
	public AudioSource nitroAudioSource;
	public AudioClip nitroSound;

	[Header("Misc Settings")]
	public GameObject headLightsObject;
	public GameObject brakeLightsObject;
	public GameObject reverseLightsObject;
	public GameObject neonObject;
	public GameObject exhaustObject;
	public Transform steeringWheel;
	public GameObject driver;
	private float speedLimit;
	private float topSpeedCache;
	private Vector3 velocityDir; 
	private float currentRotation;
	public List<AudioClip> crashSounds = new List<AudioClip> ();

	[HideInInspector] public float steerInput;
	[HideInInspector] public float motorInput;
	[HideInInspector] public float brakeInput;
	[HideInInspector] public float handbrakeInput;
	[HideInInspector] public bool useNitro;
	[HideInInspector] public bool useHeadLights;
	[HideInInspector] public bool useNeon;

	[Header("Other Settings")]
	public bool controllable;
	private bool reversing;
	public bool enableSlipstream;
	public bool haveNitro = true;
	public bool haveNeon = true;
	[HideInInspector] public float TopSpeedMPH;
	[HideInInspector] public float TopSpeedKPH;
	[HideInInspector] public bool changeTopSpeed = true;

	void Start(){
		SetRigidbody ();
		SetNitroSound ();
		SetMisc ();
		SetWheels ();
	}

	void OnLevelWasLoaded(){
		SettingsMenu.instance.ChangeSpeedUnit ();
	}

	void SetRigidbody(){
		rigid = GetComponent<Rigidbody> ();
		rigid.centerOfMass = transform.InverseTransformPoint (COM.transform.position);
		rigid.drag = 0.01f;
		rigid.angularDrag = 0f;
	}
		
	void SetNitroSound(){
		nitroAudioSource.clip = nitroSound;
		nitroAudioSource.loop = true;
		nitroAudioSource.spatialBlend = 1.0f;
		nitroAudioSource.minDistance = 3.0f;
		nitroAudioSource.outputAudioMixerGroup = SoundManager.instance.raceMixerGroup;
		nitroAudioSource.Stop ();
	}

	void SetMisc(){
		TopSpeedMPH = topSpeed;
		TopSpeedKPH = TopSpeedMPH * 1.609344f;
		exhaustObject.SetActive (true);

		if (PlayerProfile.instance) {
			if (PlayerProfile.instance.player.m_AllCarNeon [PlayerProfile.instance.player.m_currentCar] > 0 && this.gameObject.tag != "Opponent") {
				haveNeon = true;
				useNeon = true;
			} else {
				haveNeon = false;
				useNeon = false;
			}
			if (PlayerProfile.instance.player.m_AllCarNitro [PlayerProfile.instance.player.m_currentCar] > 0 || this.gameObject.tag == "Opponent") {
				haveNitro = true;
			} else {
				haveNitro = false;
			}
		}
		if (haveNitro) {
			nitroObject.SetActive (true);
			foreach (Transform p in nitroObject.transform) {
				if (p.GetComponent<ParticleSystem> ()) {
					var em = p.GetComponent<ParticleSystem> ().emission;
					em.enabled = false;
				}                       
			}
		}
		if (controllable && RaceManager.instance) {
			controllable = false;
		}
	}

	void SetWheels(){
		for (int i = 0; i < wheelEffects.Length; i++) {
			wheelEffects [i] = wheelColliders [i].GetComponent<Wheels> ();
		}
	}

	void Update(){
		WheelPosition ();
		WheelSpin ();
		CalculateSpeed ();
		if (changeTopSpeed) {
			SetTopSpeed ();
		}
	}

	void FixedUpdate(){
		Move ();
		SpeedLimiter ();
		StabilizerBars ();
		ApplyDownforce ();
		velocityDir = CalculateVelocityDirection ();
	}
		
	void SetTopSpeed(){
		Car_Controller[] cars = FindObjectsOfType<Car_Controller> ();
		foreach (Car_Controller car in cars) {
			if (car.speedUnit == SpeedUnit.MPH) {
				car.topSpeed = car.TopSpeedMPH;
				car.GetComponent<RacerProgress> ().speedRecord /= 1.609344f;
			} else {
				car.topSpeed = car.TopSpeedKPH;
				car.GetComponent<RacerProgress> ().speedRecord *= 1.609344f;
			}
			car.topSpeedCache = car.topSpeed;
			car.nitroTopSpeed = car.topSpeed + nitroAddedSpeed;
			car.speedLimit = car.topSpeedCache;
		}
		changeTopSpeed = false;
	}

	float CalculateSpeed(){        
		if (speedUnit == SpeedUnit.MPH) {
			currentSpeed = rigid.velocity.magnitude * 2.237f;
		} else {
			currentSpeed = rigid.velocity.magnitude * 3.6f;
		}
		currentSpeed = Mathf.Round (currentSpeed);
		return Mathf.Abs(currentSpeed);
	}

	void SpeedLimiter(){
		if (currentSpeed > speedLimit) {
			switch (carDriveType) {
			case CarDriveType.AWD:
				if (speedUnit == SpeedUnit.MPH) {
					rigid.velocity = speedLimit / 2.237f * rigid.velocity.normalized;
				} else {
					rigid.velocity = speedLimit / 3.6f * rigid.velocity.normalized;
				}
				wheelColliders [0].motorTorque = wheelColliders [1].motorTorque = 0;
				wheelColliders [2].motorTorque = wheelColliders [3].motorTorque = 0;
				break;

			case CarDriveType.FWD:
				if (speedUnit == SpeedUnit.MPH) {
					rigid.velocity = speedLimit / 2.237f * rigid.velocity.normalized;
				} else {
					rigid.velocity = speedLimit / 3.6f * rigid.velocity.normalized;
				}
				wheelColliders [0].motorTorque = wheelColliders [1].motorTorque = 0;
				break;

			case CarDriveType.RWD:
				if (speedUnit == SpeedUnit.MPH) {
					rigid.velocity = speedLimit / 2.237f * rigid.velocity.normalized;
				} else {
					rigid.velocity = speedLimit / 3.6f * rigid.velocity.normalized;
				}
				wheelColliders [2].motorTorque = wheelColliders [3].motorTorque = 0;
				break;
			}
		}
	}

	void CalculateGearFactor(){
		float f = (1 / (float)numberOfGears);
		var targetGearFactor = Mathf.InverseLerp (f * currentGear, f * (currentGear + 1), Mathf.Abs (currentSpeed / topSpeed));
		gearFactor = Mathf.Lerp (gearFactor, targetGearFactor, Time.deltaTime * 5f);
	}

	void CalculateRevs(){
		CalculateGearFactor ();
		var gearNumFactor = currentGear / (float)numberOfGears;
		var revsRangeMin = ULerp (0f, revRangeBoundary, CurveFactor (gearNumFactor));
		var revsRangeMax = ULerp (revRangeBoundary, 1f, gearNumFactor);
		Revs = ULerp (revsRangeMin, revsRangeMax, gearFactor);
	}

	static float CurveFactor(float factor){
		return 1 - (1 - factor) * (1 - factor);
	}

	static float ULerp(float from, float to, float value){
		return (1.0f - value) * from + value * to;
	}

	void GearChanging(){
		float f = Mathf.Abs (currentSpeed / topSpeed);
		float upgearlimit = (1 / (float)numberOfGears) * (currentGear + 1);
		float downgearlimit = (1 / (float)numberOfGears) * currentGear;

		if (currentGear > 0 && f < downgearlimit) {
			currentGear--;
		}
		if (f > upgearlimit && (currentGear < (numberOfGears - 1))) {
			currentGear++;
		}
	}

	void Move(){
		if (!controllable)
			return;

		if (currentSpeed <= speedLimit) {
			switch (carDriveType) {

			case CarDriveType.AWD:
				for (int i = 0; i < wheelColliders.Length; i++) {
					if (!reversing) {
						wheelColliders [i].motorTorque = engineTorque * motorInput;
					} else {
						wheelColliders [i].motorTorque = engineTorque * -brakeInput;
					}
				}
				break;

			case CarDriveType.RWD:
				if (!reversing) {
					wheelColliders [2].motorTorque = wheelColliders [3].motorTorque = engineTorque * motorInput;
				} else {
					wheelColliders [2].motorTorque = wheelColliders [3].motorTorque = engineTorque * -brakeInput;
				}
				break;

			case CarDriveType.FWD:
				if (!reversing) {
					wheelColliders [0].motorTorque = wheelColliders [1].motorTorque = engineTorque * motorInput;
				} else {
					wheelColliders [0].motorTorque = wheelColliders [1].motorTorque = engineTorque * -brakeInput;
				}
				break;
			}
		}

		Brake ();
		TractionControl (); 
		SteerHelper ();

		//Steering
		currentSteerAngle = Mathf.Lerp (maxSteerAngle, (maxSteerAngle / 2f), (currentSpeed / (topSpeed * 2f)));
		wheelColliders [0].steerAngle = wheelColliders [1].steerAngle = Mathf.Clamp ((currentSteerAngle * steerInput), -maxSteerAngle, maxSteerAngle);

		//Reverse
		if (brakeInput > 0 && velocityDir.z <= 0.01f) {
			reversing = true;
			speedLimit = maxReverseSpeed;
			reverseLightsObject.SetActive (true);
		} else {
			reversing = false;
			speedLimit = topSpeed;
			reverseLightsObject.SetActive (false);
		}

		//Steering wheel rotation
		Quaternion currentRotation = steeringWheel.rotation;
		Quaternion targetRotation = transform.rotation * Quaternion.Euler (10, 0, (wheelColliders [0].steerAngle) * -2f);
		steeringWheel.rotation = Quaternion.Lerp (currentRotation, targetRotation, Time.deltaTime * 5f);

		//Acceleration
		if (currentSpeed < speedLimit && motorInput > 0) {
			rigid.AddForce (transform.forward * acceleration);
		}

		CalculateRevs (); 
		GearChanging ();

		//Slipstream
		if (enableSlipstream) {               
			Slipstream ();
		}

		//Nitro 
		if (haveNitro) {
			Nitro ();
		}

		//Head lights
		if (useHeadLights) {
			headLightsObject.SetActive (true);
		} else {
			headLightsObject.SetActive (false);
		}

		//Neon
		if (useNeon && haveNeon) {
			neonObject.SetActive (true);
		} else {
			neonObject.SetActive (false);
		}
	}

	void Brake(){
		//Footbrake
		if (!reversing && brakeInput > 0f && handbrakeInput < 0.1f) {
			rigid.AddForce (-transform.forward * brakeForce);

			if (carDriveType == CarDriveType.RWD) {
				wheelColliders [2].brakeTorque = wheelColliders [3].brakeTorque = brakeTorque * brakeInput;
				wheelColliders [2].motorTorque = wheelColliders [3].motorTorque = 0;
			} else if (carDriveType == CarDriveType.FWD) {
				wheelColliders [0].brakeTorque = wheelColliders [1].brakeTorque = brakeTorque * brakeInput;
				wheelColliders [0].motorTorque = wheelColliders [1].motorTorque = 0;
			} else {
				for (int i = 0; i < wheelColliders.Length; i++) {
					wheelColliders [i].brakeTorque = brakeTorque * brakeInput;
					wheelColliders [i].motorTorque = 0;
				}
			}
		} else {
			for (int i = 0; i < wheelColliders.Length; i++) {
				wheelColliders [i].brakeTorque = 0;
			}
		}

		//Handbrake
		if (handbrakeInput > 0) {
			wheelColliders [2].brakeTorque = wheelColliders [3].brakeTorque = handbrakeTorque * handbrakeInput;
		}

		//Decelerate
		if (motorInput == 0 && brakeInput == 0 && rigid.velocity.magnitude > 1f) {
			if (velocityDir.z >= 0.01f)
				rigid.AddForce (-transform.forward * 250);
			else
				rigid.AddForce (transform.forward * 250);
		}

		//Brake lights
		if (brakeInput > 0.3f || handbrakeInput > 0) {
			brakeLightsObject.SetActive (true);
		} else {
			brakeLightsObject.SetActive (false);
		}
	}

	void WheelPosition(){
		for (int i = 0; i < wheelColliders.Length; i++) {
			Quaternion rotation;
			Vector3 position;
			wheelColliders [i].GetWorldPose (out position, out rotation);
			wheelTransforms [i].rotation = rotation;
			wheelTransforms [i].position = position;
		}
	}

	void WheelSpin(){
		for (int i = 0; i < wheelColliders.Length; i++) {
			WheelHit wheelHit;
			wheelColliders [i].GetGroundHit (out wheelHit);

			if (Mathf.Abs (wheelHit.forwardSlip) >= forwardSlipLimit || Mathf.Abs (wheelHit.sidewaysSlip) >= sidewaysSlipLimit) {
				wheelEffects [i].shouldEmit = true;
			} else {
				wheelEffects [i].shouldEmit = false;
			}
		}
	}

	public void StabilizerBars(){
		WheelHit FrontWheelHit;
		WheelHit RearWheelHit;
		float travelFL = 1f;
		float travelFR = 1f;
		float travelRL = 1f;
		float travelRR = 1f;

		bool groundedFL = wheelColliders [0].GetGroundHit (out FrontWheelHit);
		travelFL = (-wheelColliders [0].transform.InverseTransformPoint (FrontWheelHit.point).y - wheelColliders [0].radius) / wheelColliders [0].suspensionDistance;
		bool groundedFR = wheelColliders [1].GetGroundHit (out FrontWheelHit);
		travelFR = (-wheelColliders [1].transform.InverseTransformPoint (FrontWheelHit.point).y - wheelColliders [1].radius) / wheelColliders [1].suspensionDistance;
		float antiRollForceFront = (travelFL - travelFR) * antiRoll;
		rigid.AddForceAtPosition (wheelColliders [0].transform.up * -antiRollForceFront, wheelColliders [0].transform.position);
		rigid.AddForceAtPosition (wheelColliders [1].transform.up * antiRollForceFront, wheelColliders [1].transform.position);

		bool groundedRL = wheelColliders [2].GetGroundHit (out RearWheelHit);
		travelRL = (-wheelColliders [2].transform.InverseTransformPoint (RearWheelHit.point).y - wheelColliders [2].radius) / wheelColliders [2].suspensionDistance;
		bool groundedRR = wheelColliders [3].GetGroundHit (out RearWheelHit);
		travelRR = (-wheelColliders [3].transform.InverseTransformPoint (RearWheelHit.point).y - wheelColliders [3].radius) / wheelColliders [3].suspensionDistance;
		float antiRollForceRear = (travelRL - travelRR) * antiRoll;
		rigid.AddForceAtPosition (wheelColliders [2].transform.up * -antiRollForceRear, wheelColliders [2].transform.position);
		rigid.AddForceAtPosition (wheelColliders [3].transform.up * antiRollForceRear, wheelColliders [3].transform.position);

		if (groundedRR && groundedRL && currentSpeed > 5f)
			rigid.AddRelativeTorque ((Vector3.up * (steerInput * motorInput)) * 5000f);
	}

	void ApplyDownforce(){
		rigid.AddForce (-transform.up * downforce * rigid.velocity.magnitude);
	}		
		
	void TractionControl(){
		WheelFrictionCurve frontWheelStiffness = wheelColliders [0].sidewaysFriction;
		WheelFrictionCurve rearWheelStiffness = wheelColliders [2].sidewaysFriction;
		rearWheelStiffness.stiffness = tractionControl + 1f;
		frontWheelStiffness.stiffness = tractionControl + 1f;

		switch (carDriveType) {
		case CarDriveType.AWD:
			wheelColliders [0].sidewaysFriction = wheelColliders [1].sidewaysFriction = frontWheelStiffness;
			wheelColliders [2].sidewaysFriction = wheelColliders [3].sidewaysFriction = rearWheelStiffness;
			break;

		case CarDriveType.RWD:
			wheelColliders [2].sidewaysFriction = wheelColliders [3].sidewaysFriction = rearWheelStiffness;
			break;

		case CarDriveType.FWD:
			wheelColliders [0].sidewaysFriction = wheelColliders [1].sidewaysFriction = frontWheelStiffness;
			break;
		}
	}

	void SteerHelper(){
		for (int i = 0; i < wheelColliders.Length; i++) {
			WheelHit wheelhit;
			wheelColliders [i].GetGroundHit (out wheelhit);
			if (wheelhit.normal == Vector3.zero)
				return;
		}
		if (Mathf.Abs (currentRotation - transform.eulerAngles.y) < 10f) {
			float turnAdjust = (transform.eulerAngles.y - currentRotation) * (steerHelper / 2);
			Quaternion velRotation = Quaternion.AngleAxis (turnAdjust, Vector3.up);
			rigid.velocity = velRotation * rigid.velocity;
		}
		currentRotation = transform.eulerAngles.y;
	}

	void Slipstream(){
		Vector3 fwd = new Vector3 (transform.position.x, transform.position.y + slipstreamRayHeight, transform.position.z);
		RaycastHit hit;

		if (Physics.Raycast (fwd, transform.forward, out hit, slipstreamRayLength)) {
			if (hit.collider.gameObject.layer != LayerMask.NameToLayer ("IgnoreCollision") && hit.transform.root.GetComponent<RacerProgress> ()) {
				if (currentSpeed > 40 && motorInput > 0)
					rigid.AddForce (transform.forward * slipstreamStrength, ForceMode.Acceleration);
			}
		}
	}

	void Nitro(){
		if (useNitro && nitroCapacity > 0f && motorInput > 0f) {
			topSpeed = nitroTopSpeed;
			nitroCapacity = Mathf.MoveTowards (nitroCapacity, 0, nitroDepletionRate * Time.deltaTime);
			rigid.AddForce (transform.forward * nitroStrength, ForceMode.Acceleration);

			if (!nitroAudioSource.isPlaying) {
				nitroAudioSource.Play ();
			}
			nitroAudioSource.volume = Mathf.Lerp (nitroAudioSource.volume, 1f, Time.deltaTime * 2f);
			nitroAudioSource.pitch = Mathf.Lerp (nitroAudioSource.pitch, 1.5f, Time.deltaTime * 2f);

			foreach (Transform p in nitroObject.transform) {
				if (p.GetComponent<ParticleSystem> ()) {
					p.GetComponent<ParticleSystem> ().Emit (1);
				}
			}
		} else {
			nitroAudioSource.volume = Mathf.Lerp (nitroAudioSource.volume, 0f, Time.deltaTime * 2f);
			nitroAudioSource.pitch = Mathf.Lerp (nitroAudioSource.pitch, 1f, Time.deltaTime * 2f);
			topSpeed = topSpeedCache;
			if (!useNitro && nitroRegenerationRate > 0)
				nitroCapacity = Mathf.MoveTowards (nitroCapacity, 1, nitroRegenerationRate * Time.deltaTime);
		}
	}

	void OnCollisionEnter(Collision col){
		if (col.relativeVelocity.magnitude < 5f)
			return;

		float hitVol = Mathf.InverseLerp (5, 100, currentSpeed);
		foreach (ContactPoint contact in col.contacts) {     
			if (crashSounds.Count > 0) {
				SoundManager.instance.PlayClip (crashSounds [Random.Range (0, crashSounds.Count)], contact.point, hitVol, 1.0f);
			}
		}
	}
		
	Vector3 CalculateVelocityDirection(){
		return transform.InverseTransformDirection (rigid.velocity);
	}
}