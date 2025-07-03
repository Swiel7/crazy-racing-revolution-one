using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Events;

public class GarageBackup : MonoBehaviour 
{
	public static GarageBackup instance;
	private ControlsMenu controls;

	[System.Serializable]
	public class Car
	{
		[Header("Details")]
		public string name;
		public Transform carModel;
		public Sprite logo;
		public int price;
		public GameObject carBody;
		public GameObject carWheels;
		public GameObject carNeon;
		public Texture carMainTexture;
		public Texture carBumpTexture;
		public int carLevel;
		public bool unlocked;

		[Header("Parameters")]
		[Range(0, 100)] public float topSpeed;
		[Range(0, 100)] public float acceleration;
		[Range(0, 100)] public float steering;
		[Range(0, 100)] public float braking;
		public CarDriveTrainTypes carDrivetrain;
		[Header("")] public Wheels[] allWheels;
		[Header("")] public UpgradeStatus[] upgradeStatus;
	}
		
	[System.Serializable]
	public class CarBody
	{
		public string name; 
		public Material n_material;
		public int price;
		public bool unlocked;
	}

	[System.Serializable]
	public class Wheels
	{
		public string name; 
		public Material n_material;
		public int price;
		public bool unlocked;
	}

	[System.Serializable]
	public class DecalInfo
	{
		public Text decalPriceText;
		public Image lockedDecal;
		public GameObject decalTick;
	}

	[System.Serializable]
	public class UpgradeInfo
	{
		public Text upgradePriceText;
		public Text upgradeName;
		public Image lockedUpgrade;
		public GameObject upgradeTick;
	}

	[System.Serializable]
	public class WheelInfo
	{
		public Text wheelPriceText;
		public Image lockedWheel;
		public GameObject wheelTick;
	}

	[System.Serializable]
	public class Nitro
	{
		public string name;
		public int price;
		[Range(0, 8f)] public float strength;
		[Range(0, 25f)] public float addedSpeed;
		[Range(0, 0.1f)] public float regenerationRate;
		[Range(0, 0.5f)]public float depletionRate;
		public bool unlocked;
		public Text upgradePriceText;
		public Image lockedNitro;
		public GameObject nitroTick;
	}

	[System.Serializable]
	public class Neon
	{
		public string name;
		public Color neonColor;
		public int price;
		public bool unlocked;
	}

	[System.Serializable]
	public class CarPerformance
	{
		public string name;
		public int price;
		public float topSpeed;
		public float acceleration;
		public float steering;
		public float braking;
	}

	[System.Serializable]
	public class UpgradeStatus
	{
		public string name;
		public bool unlocked;
	}

	[System.Serializable]
	public class Tracks
	{
		public string name;
		public string sceneName;
		public string length;
		public string trackInfo;
		public Sprite image;
		public RaceManager.RaceType raceType = RaceManager.RaceType.Circuit;
		public AI_Control.AiDifficulty aiDifficulty = AI_Control.AiDifficulty.Hard;
		public int laps;
		public int aiCount;
		public bool unlocked;
	}

	[Header("Garage Panels")]
	public GameObject carSelectPanel;
	public GameObject carTuningPanel;
	public GameObject carColorPanel;
	public GameObject carWheelsPanel;
	public GameObject carNeonPanel;
	public GameObject carDecalsPanel;
	public GameObject carPerformancePanel;
	public GameObject carNitroPanel;
	public GameObject trackSelectCanvas;
	public GameObject saveGameCanvas;
	public Transform saveGameContent;
	public GameObject loadingPanel;
	public GameObject notificationCanvas; 
	public GameObject notificationPanel; 
	public GameObject alertPanel;
	public GameObject quitPanel;

	[Header("Cars")]
	public Car[] allCars;

	[Header("Car tuning")]
	public CarBody[] allBodyColors;
	public Neon[] allNeons;
	public Nitro[] allNitros;
	public CarPerformance[] upgradeLevels;

	[Header("Tracks")]
	public Tracks[] allTracks;

	[Header("UI Car Select")]
	public Text UICarName;
	public Image UICarLogo;
	public Slider UITopSpeed;
	public Slider UIAcceleration;
	public Slider UISteering;
	public Slider UIBraking;
	public Text UIDrivetrain;
	public Text UICarPrice;
	public Image UILockedCar;
	public GameObject[] UICarLevels;

	[Header("UI Player")]
	public Text UIPlayerName;
	public Text UIPlayerMoney;
	public Text UIPlayerCar;
	public Image UIPlayerImage; 
	public Sprite MaleIcon;
	public Sprite FemaleIcon;

	[Header("UI Tuning")]
	public Image UILockedColor;
	public Text UICarColorPrice;
	public Image UILockedNeon;
	public Text UINeonPrice;
	public DecalInfo[] UIDecal;
	public UpgradeInfo[] UIUpgrade;
	public WheelInfo[] UIWheels;
	public Slider TopSpeedSlider;
	public Slider AccelerationSlider;
	public Slider SteeringSlider;
	public Slider BrakingSlider;
	public Slider TopSpeedBonusSlider;
	public Slider AccelerationBonusSlider;
	public Slider SteeringBonusSlider;
	public Slider BrakingBonusSlider;
	public Slider NitroStrengthSlider;
	public Slider NitroAddedSpeedSlider;
	public Slider NitroRegenerationRateSlider;
	public Slider NitroDepletionRateSlider;

	[Header("UI Track Select")]
	public Text UITrackName;
	public Text UITrackLength;
	public Text UITrackIndex;
	public Text UIRaceType;
	public Text UI_AICount;
	public Text UILapCount;
	public Text UI_AiDifficulty;
	public Image UITrackImage;
	public Image UILockedTrack;
	public Button UIRaceButton;

	[Header("UI Buttons")]
	public Button UISelectButton;
	public Button UIBuyButton;

	[Header("UI Loading")]
	public Image UILoadingImage;
	public Image UILoadingBar;
	public Text UILoadingProgress;
	public Text UILoadingTrackName;
	public Text UILoadingTrackInfo;

	[Header("UI Info")]
	public Text UIAlertText;
	public Text UINotificationText;
	public Button UIDelete;
	public Button UIAccept;
	public InputField UIFileName;

	[Header("Variables")]
	[Range(1,5)] public int maxOpponents = 5;
	public int maxLaps = 20;
	private int aiLevelIndex = 3;
	public enum CarDriveTrainTypes {RWD, FWD, AWD}
	private int carIndex;
	private int trackIndex = 0;
	private int raceTypeIndex = 1;
	private bool raycastTarget;
	private float rotateDirection = 1;
	private Color neonColor;
	public Button[] buttonsArray;
	[HideInInspector] public bool rotate = true;
	private int carUpgradeLevel;
	private float currentAmount;

	void Awake(){
		instance = this;
		controls = FindObjectOfType<ControlsMenu> ();
	}

	void Start(){
		if (FindObjectOfType<PlayerProfile> ()) {
			controls.Init ();
			ApplyPlayerDetails ();
			ApplyCarSettings ();
			ApplyTuningSettings ();
			ApplyTrackSettings ();
			CarSelectMenu ();
		} 
	}

	void Update(){
		if (rotate) {
			RotationAndZoom ();
		}
	}
		
	public void CarSelectMenu(){
		rotate = true;
		UISelectButton.gameObject.SetActive(false);
		UIBuyButton.gameObject.SetActive(false);
		carSelectPanel.SetActive (true);
		carTuningPanel.SetActive (false);
		trackSelectCanvas.SetActive (false);
		saveGameCanvas.SetActive (false);
		if (FindObjectOfType<PlayerProfile> ()) {
			ShowCar (PlayerProfile.instance.player.m_currentCar);
		}
	}
	public void CarColorMenu(){
		rotate = true;
		UISelectButton.gameObject.SetActive(false);
		UIBuyButton.gameObject.SetActive(false);
		carSelectPanel.SetActive (false);
		carTuningPanel.SetActive (true);
		carColorPanel.SetActive (true);
		carWheelsPanel.SetActive (false);
		carNeonPanel.SetActive (false);
		carDecalsPanel.SetActive (false);
		carPerformancePanel.SetActive (false);
		carNitroPanel.SetActive (false);
		trackSelectCanvas.SetActive (false);
	}
	public void CarWheelsMenu(){
		UISelectButton.gameObject.SetActive(false);
		UIBuyButton.gameObject.SetActive(false);
		carColorPanel.SetActive (false);
		carWheelsPanel.SetActive (true);
		carNeonPanel.SetActive (false);
		carDecalsPanel.SetActive (false);
		carPerformancePanel.SetActive (false);
		carNitroPanel.SetActive (false);
		CheckWheelStatus ();
		UIWheels [PlayerProfile.instance.player.m_AllCarWheels [PlayerProfile.instance.player.m_currentCar]].wheelTick.SetActive (true);
	}
	public void CarNeonMenu(){
		UISelectButton.gameObject.SetActive(false);
		UIBuyButton.gameObject.SetActive(false);
		carColorPanel.SetActive (false);
		carWheelsPanel.SetActive (false);
		carNeonPanel.SetActive (true);
		carDecalsPanel.SetActive (false);
		carPerformancePanel.SetActive (false);
		carNitroPanel.SetActive (false);
	}
	public void CarDecalsMenu(){
		UISelectButton.gameObject.SetActive(false);
		UIBuyButton.gameObject.SetActive(false);
		carColorPanel.SetActive (false);
		carWheelsPanel.SetActive (false);
		carNeonPanel.SetActive (false);
		carDecalsPanel.SetActive (true);
		carPerformancePanel.SetActive (false);
		carNitroPanel.SetActive (false);
		CheckDecalStatus ();
		if (PlayerProfile.instance.player.m_AllCarBody [PlayerProfile.instance.player.m_currentCar] >= 20) {
			UIDecal [PlayerProfile.instance.player.m_AllCarBody [PlayerProfile.instance.player.m_currentCar] - 20].decalTick.SetActive (true);
		}
	}
	public void CarPerformanceMenu(){
		UISelectButton.gameObject.SetActive(false);
		UIBuyButton.gameObject.SetActive(false);
		carColorPanel.SetActive (false);
		carWheelsPanel.SetActive (false);
		carNeonPanel.SetActive (false);
		carDecalsPanel.SetActive (false);
		carPerformancePanel.SetActive (true);
		carNitroPanel.SetActive (false);
		UISelectButton.gameObject.SetActive (false);
		CheckPerformanceStatus ();
		AdjustCarStats (PlayerProfile.instance.player.m_currentCar);
	}
	public void CarNitroMenu(){
		UISelectButton.gameObject.SetActive(false);
		UIBuyButton.gameObject.SetActive(false);
		carColorPanel.SetActive (false);
		carWheelsPanel.SetActive (false);
		carNeonPanel.SetActive (false);
		carDecalsPanel.SetActive (false);
		carPerformancePanel.SetActive (false);
		carNitroPanel.SetActive (true);
		CheckNitroStatus ();
		allNitros [PlayerProfile.instance.player.m_AllCarNitro [PlayerProfile.instance.player.m_currentCar]].nitroTick.SetActive (true);
		if (PlayerProfile.instance.player.m_AllCarNitro [PlayerProfile.instance.player.m_currentCar] != 0) {
			ShowNitroValue (PlayerProfile.instance.player.m_AllCarNitro [PlayerProfile.instance.player.m_currentCar]);
		} else {
			ShowNitroValue (1);
		}
	}
	public void TrackSelectMenu(){
		rotate = false;
		trackSelectCanvas.SetActive (true);
		UISelectButton.gameObject.SetActive(false);
		UIBuyButton.gameObject.SetActive(false);
		UITrackName.text = allTracks [trackIndex].name;
		UITrackLength.text = allTracks [trackIndex].length;
		UITrackImage.sprite = allTracks [trackIndex].image;
		UITrackIndex.text = (trackIndex + 1).ToString ();
		UIRaceType.text = allTracks [trackIndex].raceType.ToString ();
		UI_AiDifficulty.text = allTracks [trackIndex].aiDifficulty.ToString ();
		UILapCount.text = allTracks[trackIndex].laps.ToString ();
		UI_AICount.text = allTracks [trackIndex].aiCount.ToString ();
		UILockedTrack.enabled = !allTracks [trackIndex].unlocked;
		UIRaceButton.interactable = allTracks [trackIndex].unlocked;
	}

	public void SaveGameMenu(){
		rotate = false;
		saveGameCanvas.SetActive (true);
		UISelectButton.gameObject.SetActive(false);
		UIBuyButton.gameObject.SetActive(false);
	}

	void RemoveButtonsArrayListeners(){
		for (int i = 0; i < buttonsArray.Length; i++) {
			buttonsArray [i].onClick.RemoveAllListeners ();
		}
		UIBuyButton.onClick.RemoveAllListeners ();
		UISelectButton.onClick.RemoveAllListeners ();
	}
		
	public void SelectCarColor(int colorIndex){
		RemoveButtonsArrayListeners ();
		Renderer currentColorMaterial = allCars [PlayerProfile.instance.player.m_currentCar].carBody.GetComponent<Renderer> ();
		currentColorMaterial.material = allBodyColors [colorIndex].n_material;
		currentColorMaterial.material.SetTexture ("_MainTex", allCars [PlayerProfile.instance.player.m_currentCar].carMainTexture);
		currentColorMaterial.material.SetTexture ("_BumpMap", allCars [PlayerProfile.instance.player.m_currentCar].carBumpTexture);
		CheckColorStatus (colorIndex);

		UISelectButton.onClick.AddListener (delegate {
			PlayerProfile.instance.player.m_AllCarBody [PlayerProfile.instance.player.m_currentCar] = colorIndex;
			for(int a = 0; a < UIDecal.Length; a++){
				if(UIDecal[a].decalTick.activeSelf){
					UIDecal[a].decalTick.SetActive(false);
				}
			}	return;
		});
			
		UIBuyButton.onClick.AddListener (delegate {
			if(PlayerProfile.instance.player.m_money >= allBodyColors[colorIndex].price){
				CreateNotificationPanel("Czy na pewno chcesz zakupić wybrany kolor dla samochodu ?");
				UIAccept.onClick.RemoveAllListeners();

				UIAccept.onClick.AddListener(delegate {
					allBodyColors [colorIndex].unlocked = true;
					CheckColorStatus (colorIndex);
					PlayerProfile.instance.player.m_CarBody[colorIndex] = true;
					PlayerProfile.instance.DecreaseMoney(allBodyColors[colorIndex].price);
				});
			}else{
				CreateAlertPanel("Nie masz wystarczającej sumy pieniędzy aby zakupić wybrany kolor dla samochodu !");
			}
			return;
		});
			
		for (int i = 0; i < buttonsArray.Length; i++) {
			buttonsArray [i].onClick.AddListener (delegate {
				currentColorMaterial.material = allBodyColors [PlayerProfile.instance.player.m_AllCarBody [PlayerProfile.instance.player.m_currentCar]].n_material;
				if(PlayerProfile.instance.player.m_AllCarBody [PlayerProfile.instance.player.m_currentCar] < 20){
					currentColorMaterial.material.SetTexture ("_MainTex", allCars [PlayerProfile.instance.player.m_currentCar].carMainTexture);
				}
				currentColorMaterial.material.SetTexture ("_BumpMap", allCars [PlayerProfile.instance.player.m_currentCar].carBumpTexture);
				return;
			});
		}
	}

	void CheckColorStatus(int index){
		if (allBodyColors [index].unlocked) {
			UICarColorPrice.text = "ZAKUPIONY";
			UILockedColor.enabled = false;
			UISelectButton.gameObject.SetActive (true);
			UIBuyButton.gameObject.SetActive (false);
		} else {
			UICarColorPrice.text = allBodyColors [index].price.ToString () + " C";
			UILockedColor.enabled = true;
			UISelectButton.gameObject.SetActive (false);
			UIBuyButton.gameObject.SetActive (true);
		}
	}

	public void SelectCarNeon(int colorIndex){
		RemoveButtonsArrayListeners ();
		ParticleSystem neon = allCars [PlayerProfile.instance.player.m_currentCar].carNeon.GetComponent<ParticleSystem> ();
		neonColor.r = allNeons [colorIndex].neonColor.r;
		neonColor.g = allNeons [colorIndex].neonColor.g;
		neonColor.b = allNeons [colorIndex].neonColor.b;
		neonColor.a = allNeons [colorIndex].neonColor.a;
		neon.startColor = neonColor;

		if (colorIndex != 0) {
			allCars [PlayerProfile.instance.player.m_currentCar].carNeon.gameObject.SetActive (false);
			allCars [PlayerProfile.instance.player.m_currentCar].carNeon.gameObject.SetActive (true);
		} else {
			allCars [PlayerProfile.instance.player.m_currentCar].carNeon.gameObject.SetActive (false);
		}
		CheckNeonStatus (colorIndex);

		UISelectButton.onClick.AddListener (delegate {
			PlayerProfile.instance.player.m_AllCarNeon [PlayerProfile.instance.player.m_currentCar] = colorIndex;
			return;
		});
			
		UIBuyButton.onClick.AddListener (delegate {
			if(PlayerProfile.instance.player.m_money >= allNeons[colorIndex].price){
				CreateNotificationPanel("Czy na pewno chcesz zakupić neon wybranego koloru ?");
				UIAccept.onClick.RemoveAllListeners();

				UIAccept.onClick.AddListener(delegate {
					allNeons [colorIndex].unlocked = true;
					CheckNeonStatus (colorIndex);
					PlayerProfile.instance.player.m_Neon[colorIndex] = true;
					PlayerProfile.instance.DecreaseMoney(allNeons[colorIndex].price);
				});
			}else{
				CreateAlertPanel("Nie masz wystarczającej sumy pieniędzy aby zakupić neon wybranego koloru !");
			}
			return;
		});
			
		for (int i = 0; i < buttonsArray.Length; i++) {
			buttonsArray [i].onClick.AddListener (delegate {
				if(PlayerProfile.instance.player.m_AllCarNeon [PlayerProfile.instance.player.m_currentCar] != 0) {
					allCars [PlayerProfile.instance.player.m_currentCar].carNeon.gameObject.SetActive (false);
					allCars [PlayerProfile.instance.player.m_currentCar].carNeon.gameObject.SetActive (true);
				}else{
					allCars [PlayerProfile.instance.player.m_currentCar].carNeon.gameObject.SetActive (false);
				}
				neon.startColor = allNeons [PlayerProfile.instance.player.m_AllCarNeon [PlayerProfile.instance.player.m_currentCar]].neonColor;
				return;
			});
		}
	}

	void CheckNeonStatus(int index){
		if (allNeons [index].unlocked) {
			UINeonPrice.text = "ZAKUPIONY";
			UILockedNeon.enabled = false;
			UISelectButton.gameObject.SetActive (true);
			UIBuyButton.gameObject.SetActive (false);
		} else {
			UINeonPrice.text = allNeons [index].price.ToString () + " C";
			UILockedNeon.enabled = true;
			UISelectButton.gameObject.SetActive (false);
			UIBuyButton.gameObject.SetActive (true);
		}
	}

	public void SelectCarWheel(int wheelIndex){
		RemoveButtonsArrayListeners ();
		if (allCars[PlayerProfile.instance.player.m_currentCar].allWheels [wheelIndex].unlocked) {
			UISelectButton.gameObject.SetActive (true);
			UIBuyButton.gameObject.SetActive (false);	
		} else {
			UISelectButton.gameObject.SetActive (false);
			UIBuyButton.gameObject.SetActive (true);	
		}

		Renderer[] carWheelsArray = allCars [PlayerProfile.instance.player.m_currentCar].carWheels.GetComponentsInChildren<Renderer> ();
		for (int i = 0; i < carWheelsArray.Length; i++) {
			carWheelsArray [i].material = allCars[PlayerProfile.instance.player.m_currentCar].allWheels [wheelIndex].n_material;
		}
			
		UISelectButton.onClick.AddListener (delegate {
			UIWheels [PlayerProfile.instance.player.m_AllCarWheels [PlayerProfile.instance.player.m_currentCar]].wheelTick.SetActive(false);
			PlayerProfile.instance.player.m_AllCarWheels [PlayerProfile.instance.player.m_currentCar] = wheelIndex;
			UIWheels[wheelIndex].wheelTick.SetActive(true);
			return;
		});
			
		UIBuyButton.onClick.AddListener (delegate {
			if(PlayerProfile.instance.player.m_money >= allCars[PlayerProfile.instance.player.m_currentCar].allWheels[wheelIndex].price){
				CreateNotificationPanel("Czy na pewno chcesz zakupić wybrane koło ?");
				UIAccept.onClick.RemoveAllListeners();

				UIAccept.onClick.AddListener(delegate {
					allCars[PlayerProfile.instance.player.m_currentCar].allWheels [wheelIndex].unlocked = true;
					UISelectButton.gameObject.SetActive (true);
					UIBuyButton.gameObject.SetActive (false);
					CheckWheelStatus();
					PlayerProfile.instance.DecreaseMoney(allCars[PlayerProfile.instance.player.m_currentCar].allWheels[wheelIndex].price);

					switch(PlayerProfile.instance.player.m_currentCar){
					case 0:
						PlayerProfile.instance.player.m_PorscheWheels [wheelIndex] = true;
						break;
					case 1:
						PlayerProfile.instance.player.m_AstonWheels [wheelIndex] = true;
						break;
					case 2:
						PlayerProfile.instance.player.m_AudiWheels [wheelIndex] = true;
						break;
					case 3:
						PlayerProfile.instance.player.m_Ferrari599Wheels [wheelIndex] = true;
						break;
					case 4:
						PlayerProfile.instance.player.m_Ferrari488Wheels [wheelIndex] = true;
						break;
					case 5:
						PlayerProfile.instance.player.m_LamborghiniWheels [wheelIndex] = true;
						break;
					}
				});
			}else{
				CreateAlertPanel("Nie masz wystarczającej sumy pieniędzy aby zakupić wybrane koło !");
			}
			return;
		});
			
		for (int i = 0; i < buttonsArray.Length; i++) {
			buttonsArray [i].onClick.AddListener (delegate {
				for (int j = 0; j < carWheelsArray.Length; j++) {
					carWheelsArray [j].material = allCars[PlayerProfile.instance.player.m_currentCar].allWheels [PlayerProfile.instance.player.m_AllCarWheels [PlayerProfile.instance.player.m_currentCar]].n_material;
				} return;
			});
		}
	}

	void CheckWheelStatus(){
		for (int i = 0; i < allCars [PlayerProfile.instance.player.m_currentCar].allWheels.Length; i++) {
			if (allCars [PlayerProfile.instance.player.m_currentCar].allWheels [i].unlocked) {
				UIWheels [i].wheelPriceText.text = "";
				UIWheels [i].lockedWheel.enabled = false;
			} else {
				UIWheels [i].wheelPriceText.text = allCars [PlayerProfile.instance.player.m_currentCar].allWheels [i].price.ToString () + " C";
				UIWheels [i].lockedWheel.enabled = true;
			}
		} 
	}

	public void SelectCarDecal(int colorIndex){
		RemoveButtonsArrayListeners ();
		if (allBodyColors [colorIndex].unlocked) {
			UISelectButton.gameObject.SetActive (true);
			UIBuyButton.gameObject.SetActive (false);	
		} else {
			UISelectButton.gameObject.SetActive (false);
			UIBuyButton.gameObject.SetActive (true);	
		}
		Renderer currentColorMaterial = allCars [PlayerProfile.instance.player.m_currentCar].carBody.GetComponent<Renderer> ();
		currentColorMaterial.material = allBodyColors [colorIndex].n_material;
		currentColorMaterial.material.SetTexture ("_BumpMap", allCars [PlayerProfile.instance.player.m_currentCar].carBumpTexture);
	
		UISelectButton.onClick.AddListener (delegate {
			if(PlayerProfile.instance.player.m_AllCarBody [PlayerProfile.instance.player.m_currentCar] >= 20){
				UIDecal[PlayerProfile.instance.player.m_AllCarBody [PlayerProfile.instance.player.m_currentCar]-20].decalTick.SetActive(false);
			}
			PlayerProfile.instance.player.m_AllCarBody [PlayerProfile.instance.player.m_currentCar] = colorIndex;
			UIDecal[colorIndex-20].decalTick.SetActive(true);
			return;
		});
			
		UIBuyButton.onClick.AddListener (delegate {
			if(PlayerProfile.instance.player.m_money >= allBodyColors[colorIndex].price){
				CreateNotificationPanel("Czy na pewno chcesz zakupić wybrany winyl ?");
				UIAccept.onClick.RemoveAllListeners();

				UIAccept.onClick.AddListener(delegate {
					allBodyColors [colorIndex].unlocked = true;
					PlayerProfile.instance.player.m_CarBody [colorIndex] = true;
					UISelectButton.gameObject.SetActive (true);
					UIBuyButton.gameObject.SetActive (false);
					CheckDecalStatus ();
					PlayerProfile.instance.DecreaseMoney(allBodyColors[colorIndex].price);
				});
			}else{
				CreateAlertPanel("Nie masz wystarczającej sumy pieniędzy aby zakupić wybrany winyl !");
			}
			return;
		});
			
		for (int i = 0; i < buttonsArray.Length; i++) {
			buttonsArray [i].onClick.AddListener (delegate {
				currentColorMaterial.material = allBodyColors [PlayerProfile.instance.player.m_AllCarBody [PlayerProfile.instance.player.m_currentCar]].n_material;
				currentColorMaterial.material.SetTexture ("_BumpMap", allCars [PlayerProfile.instance.player.m_currentCar].carBumpTexture);
				if(PlayerProfile.instance.player.m_AllCarBody [PlayerProfile.instance.player.m_currentCar] < 20){
					currentColorMaterial.material.SetTexture ("_MainTex", allCars [PlayerProfile.instance.player.m_currentCar].carMainTexture);
				}
				return;
			});
		}
	}

	void CheckDecalStatus(){
		for (int i = 20; i < allBodyColors.Length; i++) {
			if (allBodyColors [i].unlocked) {
				UIDecal [i-20].decalPriceText.text = "";
				UIDecal [i-20].lockedDecal.enabled = false;
			} else {
				UIDecal [i-20].decalPriceText.text = allBodyColors [i].price.ToString () + " C";
				UIDecal [i-20].lockedDecal.enabled = true;
			}
		} 
	}

	public void SelectCarNitro(int nitroIndex){ 
		RemoveButtonsArrayListeners ();
		if (allNitros [nitroIndex].unlocked) {
			UISelectButton.gameObject.SetActive (true);
			UIBuyButton.gameObject.SetActive (false);	
		} else {
			UISelectButton.gameObject.SetActive (false);
			UIBuyButton.gameObject.SetActive (true);	
		}
		ShowNitroValue (nitroIndex);
			
		UISelectButton.onClick.AddListener (delegate {
			allNitros [PlayerProfile.instance.player.m_AllCarNitro [PlayerProfile.instance.player.m_currentCar]].nitroTick.SetActive (false);
			PlayerProfile.instance.player.m_AllCarNitro [PlayerProfile.instance.player.m_currentCar] = nitroIndex;
			allNitros [nitroIndex].nitroTick.SetActive (true);
			ApplyNitroValue(PlayerProfile.instance.player.m_currentCar);
			return;
		});
			
		UIBuyButton.onClick.AddListener (delegate {
			if(PlayerProfile.instance.player.m_money >= allNitros[nitroIndex].price){
				CreateNotificationPanel("Czy na pewno chcesz zakupić wybrane nitro ?");
				UIAccept.onClick.RemoveAllListeners();

				UIAccept.onClick.AddListener(delegate {
					allNitros [nitroIndex].unlocked = true;
					PlayerProfile.instance.player.m_Nitro [nitroIndex] = true;
					UISelectButton.gameObject.SetActive (true);
					UIBuyButton.gameObject.SetActive (false);
					CheckNitroStatus ();
					PlayerProfile.instance.DecreaseMoney(allNitros[nitroIndex].price);
				});
			}else{
				CreateAlertPanel("Nie masz wystarczającej sumy pieniędzy aby zakupić wybrane nitro !");
			}
			return;
		});
	}

	void CheckNitroStatus(){
		for (int i = 0; i < allNitros.Length; i++) {
			if (allNitros [i].unlocked) {
				allNitros [i].upgradePriceText.text = "";
				allNitros [i].lockedNitro.enabled = false;
			} else {
				allNitros [i].upgradePriceText.text = allNitros [i].price.ToString () + " C";
				allNitros [i].lockedNitro.enabled = true;
			}
		} 
	}

	void ShowNitroValue(int index){
		NitroStrengthSlider.value = allNitros[index].strength;
		NitroAddedSpeedSlider.value = allNitros[index].addedSpeed;
		NitroRegenerationRateSlider.value = allNitros[index].regenerationRate;
		NitroDepletionRateSlider.value = allNitros[index].depletionRate;
	}

	public void ApplyNitroValue(int carIndex){
		Car_Controller car = allCars [carIndex].carModel.gameObject.GetComponent<Car_Controller> ();
		car.nitroStrength = allNitros [PlayerProfile.instance.player.m_AllCarNitro [carIndex]].strength;
		car.nitroAddedSpeed = allNitros [PlayerProfile.instance.player.m_AllCarNitro [carIndex]].addedSpeed;
		car.nitroRegenerationRate = allNitros [PlayerProfile.instance.player.m_AllCarNitro [carIndex]].regenerationRate;
		car.nitroDepletionRate = allNitros [PlayerProfile.instance.player.m_AllCarNitro [carIndex]].depletionRate;
	}
		
	public void SelectCarPerformance(int upgradeIndex){
		RemoveButtonsArrayListeners ();
		if (allCars[PlayerProfile.instance.player.m_currentCar].upgradeStatus [upgradeIndex].unlocked) {
			UIBuyButton.gameObject.SetActive (false);	
		} else {
			UIBuyButton.gameObject.SetActive (true);	
		}
		if (upgradeIndex > PlayerProfile.instance.player.m_AllCarPerformance [PlayerProfile.instance.player.m_currentCar]) {
			ShowBonusSliders (true);
			Car_Controller car = allCars [PlayerProfile.instance.player.m_currentCar].carModel.GetComponent<Car_Controller> ();
			float speed = upgradeLevels [upgradeIndex].topSpeed + car.topSpeed;
			float acceleration = upgradeLevels [upgradeIndex].acceleration + car.acceleration;
			float steering = upgradeLevels [upgradeIndex].steering + car.steerHelper;
			float braking = upgradeLevels [upgradeIndex].braking + car.brakeForce;
			TopSpeedBonusSlider.value = Mathf.RoundToInt (100 * Mathf.InverseLerp (0f, 250f, speed));
			AccelerationBonusSlider.value = Mathf.RoundToInt (100 * Mathf.InverseLerp (0f, 1000f, acceleration));
			SteeringBonusSlider.value = Mathf.RoundToInt (100 * Mathf.InverseLerp (0f, 0.55f, steering));
			BrakingBonusSlider.value = Mathf.RoundToInt (100 * Mathf.InverseLerp (0f, 29000f, braking));
		} else {
			ShowBonusSliders (false);
		}

		UIBuyButton.onClick.AddListener (delegate {
			if(PlayerProfile.instance.player.m_money >= upgradeLevels[upgradeIndex].price){
				CreateNotificationPanel("Czy na pewno chcesz zakupić wybrane ulepszenie parametrów technicznych samochodu ?");
				UIAccept.onClick.RemoveAllListeners();

				UIAccept.onClick.AddListener(delegate {
					allCars[PlayerProfile.instance.player.m_currentCar].upgradeStatus [upgradeIndex].unlocked = true;
					ShowBonusSliders (false);
					UIBuyButton.gameObject.SetActive (false);
					PlayerProfile.instance.DecreaseMoney(upgradeLevels[upgradeIndex].price);
					PlayerProfile.instance.player.m_AllCarPerformance [PlayerProfile.instance.player.m_currentCar] = upgradeIndex;
					CheckPerformanceStatus ();
					UpgradeCar(PlayerProfile.instance.player.m_currentCar, upgradeIndex);
					LerpStats(PlayerProfile.instance.player.m_currentCar);
					AdjustCarStats(PlayerProfile.instance.player.m_currentCar);

					switch(PlayerProfile.instance.player.m_currentCar){
					case 0:
						PlayerProfile.instance.player.m_PorschePerformance [upgradeIndex] = true;
						break;
					case 1:
						PlayerProfile.instance.player.m_AstonPerformance [upgradeIndex] = true;
						break;
					case 2:
						PlayerProfile.instance.player.m_AudiPerformance [upgradeIndex] = true;
						break;
					case 3:
						PlayerProfile.instance.player.m_Ferrari599Performance [upgradeIndex] = true;
						break;
					case 4:
						PlayerProfile.instance.player.m_Ferrari488Performance [upgradeIndex] = true;
						break;
					case 5:
						PlayerProfile.instance.player.m_LamborghiniPerformance [upgradeIndex] = true;
						break;
					}
				});
			}else{
				CreateAlertPanel("Nie masz wystarczającej sumy pieniędzy aby dokonać zakupu wybranego ulepszenia parametrów technicznych samochodu !");
			}
			return;
		});

		for (int i = 0; i < buttonsArray.Length; i++) {
			buttonsArray [i].onClick.AddListener (delegate {
				ShowBonusSliders(false);
				return;
			});
		}
	}

	void CheckPerformanceStatus(){
		carUpgradeLevel = PlayerProfile.instance.player.m_AllCarPerformance [PlayerProfile.instance.player.m_currentCar];
		for (int i = 0; i < allCars [PlayerProfile.instance.player.m_currentCar].upgradeStatus.Length; i++) {
			if (allCars [PlayerProfile.instance.player.m_currentCar].upgradeStatus [i].unlocked) {
				UIUpgrade [i].upgradePriceText.text = "";
				UIUpgrade [i].lockedUpgrade.enabled = false;
				UIUpgrade [i].upgradeTick.SetActive (true);
			} else {
				UIUpgrade [i].upgradePriceText.text = allNitros [i].price.ToString () + " C";
				UIUpgrade [i].lockedUpgrade.enabled = true;
				UIUpgrade [i].upgradeTick.SetActive (false);
			}
		}
		for (int a = carUpgradeLevel; a < allCars [PlayerProfile.instance.player.m_currentCar].upgradeStatus.Length; a++) {
			if (a > carUpgradeLevel + 1 && a != 0) {
				UIUpgrade [a].upgradeTick.GetComponentInParent<Button> ().interactable = false;
				UIUpgrade [a].lockedUpgrade.color = new Color32 (255, 255, 255, 40);
				UIUpgrade [a].upgradeName.color = new Color32 (255, 255, 255, 40);
				UIUpgrade [a].upgradePriceText.color = new Color32 (255, 255, 255, 40);
				UIUpgrade [a].upgradeName.fontStyle = FontStyle.Italic;
				UIUpgrade [a].upgradePriceText.fontStyle = FontStyle.Italic;
				UIBuyButton.gameObject.SetActive (false);
			} else if (a <= carUpgradeLevel + 1 && a != 0) {
				UIUpgrade [a].upgradeTick.GetComponentInParent<Button> ().interactable = true;
				UIUpgrade [a].lockedUpgrade.color = new Color32 (255, 255, 255, 255);
				UIUpgrade [a].upgradeName.color = new Color32 (255, 255, 255, 255);
				UIUpgrade [a].upgradePriceText.color = new Color32 (255, 255, 255, 255);
				UIUpgrade [a].upgradeName.fontStyle = FontStyle.Bold;
				UIUpgrade [a].upgradePriceText.fontStyle = FontStyle.Bold;
			}
		}
	}

	void UpgradeCar(int carIndex, int level){
		Car_Controller car = allCars [carIndex].carModel.gameObject.GetComponent<Car_Controller> ();
		car.topSpeed += upgradeLevels [level].topSpeed;
		car.acceleration += upgradeLevels [level].acceleration;
		car.steerHelper += upgradeLevels [level].steering;
		car.brakeForce += upgradeLevels [level].braking;
	}

	void LerpStats(int carIndex){
		Car_Controller car = allCars [carIndex].carModel.gameObject.GetComponent<Car_Controller> ();
		allCars [carIndex].topSpeed = Mathf.RoundToInt (100 * Mathf.InverseLerp (0f, 250f, car.topSpeed));
		allCars [carIndex].acceleration = Mathf.RoundToInt (100 * Mathf.InverseLerp (0f, 1000f, car.acceleration));
		allCars [carIndex].steering = Mathf.RoundToInt (100 * Mathf.InverseLerp (0f, 0.55f, car.steerHelper));
		allCars [carIndex].braking = Mathf.RoundToInt (100 * Mathf.InverseLerp (0f, 29000f, car.brakeForce));
	}
		
	void ShowBonusSliders(bool status){
		TopSpeedBonusSlider.gameObject.SetActive (status);
		AccelerationBonusSlider.gameObject.SetActive (status);
		SteeringBonusSlider.gameObject.SetActive (status);
		BrakingBonusSlider.gameObject.SetActive (status);
	}
		
	public void CreateNotificationPanel(string info){  
		notificationCanvas.SetActive (true);
		notificationPanel.SetActive (true);
		alertPanel.SetActive (false);
		quitPanel.SetActive (false);
		UINotificationText.text = info;
	}

	public void CreateAlertPanel(string info){
		notificationCanvas.SetActive (true);
		notificationPanel.SetActive (false);  
		alertPanel.SetActive (true);
		quitPanel.SetActive (false);
		UIAlertText.text = info;
	}
		
	public void SelectTrack(int a){
		trackIndex += a;
		trackIndex = Mathf.Clamp (trackIndex, 0, 1);
		TrackSelectMenu ();
	}

	public void SelectRaceType(int a){
		raceTypeIndex += a;
		raceTypeIndex = Mathf.Clamp (raceTypeIndex, 1, 5);

		switch (raceTypeIndex) {
		case 1:
			allTracks [trackIndex].raceType = RaceManager.RaceType.Circuit;
			break;

		case 2:
			allTracks [trackIndex].raceType = RaceManager.RaceType.TimeTrial;
			break;

		case 3:
			allTracks [trackIndex].raceType = RaceManager.RaceType.LapKnockout;
			break;

		case 4:
			allTracks [trackIndex].raceType = RaceManager.RaceType.SpeedTrap;
			break;

		case 5:
			allTracks [trackIndex].raceType = RaceManager.RaceType.Checkpoints;
			break;
		}
		TrackSelectMenu ();
	}

	public void SelectLapsNumber(int a){
		allTracks [trackIndex].laps += a;
		allTracks [trackIndex].laps = Mathf.Clamp (allTracks [trackIndex].laps, 1, maxLaps);
		TrackSelectMenu ();
	}

	public void SelectAICount(int a){
		allTracks [trackIndex].aiCount += a;
		allTracks [trackIndex].aiCount = Mathf.Clamp (allTracks [trackIndex].aiCount, 0, maxOpponents);
		TrackSelectMenu ();
	}

	public void SelectAiDifficulty(int a){
		aiLevelIndex += a;
		aiLevelIndex = Mathf.Clamp (aiLevelIndex, 1, 3);

		switch (aiLevelIndex) {
		case 1:
			allTracks [trackIndex].aiDifficulty = AI_Control.AiDifficulty.Easy;
			break;

		case 2:
			allTracks [trackIndex].aiDifficulty = AI_Control.AiDifficulty.Medium;
			break;

		case 3:
			allTracks [trackIndex].aiDifficulty = AI_Control.AiDifficulty.Hard;
			break;
		}
		TrackSelectMenu ();
	}
		
	public void ShowCar(int a){
		RemoveButtonsArrayListeners ();
		foreach (WheelInfo wheel in UIWheels) {
			wheel.wheelTick.SetActive (false);
		}
		UIUpgrade [PlayerProfile.instance.player.m_AllCarPerformance [PlayerProfile.instance.player.m_currentCar]].upgradeTick.SetActive (false);
		allNitros [PlayerProfile.instance.player.m_AllCarNitro [PlayerProfile.instance.player.m_currentCar]].nitroTick.SetActive (false);
		if (PlayerProfile.instance.player.m_AllCarBody [PlayerProfile.instance.player.m_currentCar] >= 20) {
			UIDecal [PlayerProfile.instance.player.m_AllCarBody [PlayerProfile.instance.player.m_currentCar] - 20].decalTick.SetActive (false);
		}
		for (int i = 0; i < allCars.Length; i++) {
			if (i == a) {
				allCars [i].carModel.rotation = allCars [carIndex].carModel.rotation;
				carIndex = a;
				allCars [a].carModel.gameObject.SetActive (true);
				Renderer carTexture = allCars [a].carBody.GetComponent<Renderer> ();
				if (PlayerProfile.instance.player.m_AllCarBody [a] < 20) {
					carTexture.material.SetTexture ("_MainTex", allCars [a].carMainTexture);
				}
				carTexture.material.SetTexture ("_BumpMap", allCars [a].carBumpTexture);
				UICarName.text = allCars [a].name;
				UICarLogo.sprite = allCars [a].logo;
				UIDrivetrain.text = allCars [a].carDrivetrain.ToString ();
				if (allCars [a].unlocked) {
					UILockedCar.gameObject.SetActive (false);
				} else {
					UILockedCar.gameObject.SetActive (true);
				}

				UISelectButton.onClick.AddListener (delegate {
					PlayerProfile.instance.player.m_currentCar = a;
					UIPlayerCar.text = allCars [PlayerProfile.instance.player.m_currentCar].name;
					return;
				});

				UIBuyButton.onClick.AddListener (delegate {
					if (PlayerProfile.instance.player.m_money >= allCars [a].price) {
						CreateNotificationPanel ("Czy na pewno chcesz zakupić wybrany samochód ?");
						UIAccept.onClick.RemoveAllListeners ();

						UIAccept.onClick.AddListener (delegate {
							allCars [a].unlocked = true;
							PlayerProfile.instance.player.m_AllCarStatus [a] = true;
							CheckCarStatus (a);
							PlayerProfile.instance.DecreaseMoney (allCars [a].price);
						});
					} else {
						CreateAlertPanel ("Nie masz wystarczającej sumy pieniędzy aby dokonać zakupu wybranego samochodu !");
					}
					return;
				});

				for (int n = 0; n < buttonsArray.Length; n++) {
					buttonsArray [n].onClick.AddListener (delegate {
						allCars [a].carModel.gameObject.SetActive (false);
						allCars [PlayerProfile.instance.player.m_currentCar].carModel.gameObject.SetActive (true);
						if (PlayerProfile.instance.player.m_AllCarBody [a] < 20) {
							carTexture.material.SetTexture ("_MainTex", allCars [PlayerProfile.instance.player.m_currentCar].carMainTexture);
						}
						carTexture.material.SetTexture ("_BumpMap", allCars [PlayerProfile.instance.player.m_currentCar].carBumpTexture);
						return;
					});
				}
				if (allCars [a].carLevel == 2) {
					UICarLevels [2].SetActive (false);
				} else {
					UICarLevels [2].SetActive (true);
				}
				UITopSpeed.value = allCars[a].topSpeed;
				UIAcceleration.value = allCars[a].acceleration;
				UISteering.value = allCars[a].steering;
				UIBraking.value = allCars[a].braking;
			} else {
				allCars [i].carModel.gameObject.SetActive (false);
			}
		}
	}

	void CheckCarStatus(int index){
		if (allCars [index].unlocked) {
			UICarPrice.text = "ZAKUPIONY";
			UISelectButton.gameObject.SetActive (true);
			UIBuyButton.gameObject.SetActive (false);
			UILockedCar.enabled = false;
		} else {
			UICarPrice.text = allCars [index].price.ToString () + " C";
			UISelectButton.gameObject.SetActive (false);
			UIBuyButton.gameObject.SetActive (true);
			UILockedCar.enabled = true;
		}
	}
		
	void AdjustCarStats(int index){ 
		TopSpeedSlider.value = allCars[index].topSpeed;
		AccelerationSlider.value = allCars[index].acceleration;
		SteeringSlider.value = allCars[index].steering;
		BrakingSlider.value = allCars[index].braking;
	}

	void ApplyCarSettings(){
		for (int i = 0; i < allCars.Length; i++) {
			//Body
			allCars [i].carBody.GetComponent<Renderer> ().material = allBodyColors [PlayerProfile.instance.player.m_AllCarBody [i]].n_material;
			//Status (locked/unlocked)
			allCars [i].unlocked = PlayerProfile.instance.player.m_AllCarStatus [i];
			//Neon
			if (PlayerProfile.instance.player.m_AllCarNeon [i] != 0) {
				allCars [i].carNeon.gameObject.SetActive (true);
				ParticleSystem neon = allCars [i].carNeon.GetComponent<ParticleSystem> ();
				neonColor.r = allNeons [PlayerProfile.instance.player.m_AllCarNeon [i]].neonColor.r;
				neonColor.g = allNeons [PlayerProfile.instance.player.m_AllCarNeon [i]].neonColor.g;
				neonColor.b = allNeons [PlayerProfile.instance.player.m_AllCarNeon [i]].neonColor.b;
				neonColor.a = allNeons [PlayerProfile.instance.player.m_AllCarNeon [i]].neonColor.a;
				neon.startColor = neonColor;
			}
			//Wheels
			Renderer[] carWheelsArray = allCars [i].carWheels.GetComponentsInChildren<Renderer> ();
			for (int j = 0; j < carWheelsArray.Length; j++) {
				carWheelsArray [j].material = allCars[i].allWheels [PlayerProfile.instance.player.m_AllCarWheels [i]].n_material;
			}
			//Performance
			int upgradeLevel = PlayerProfile.instance.player.m_AllCarPerformance [i];
			if (upgradeLevel > 0) {
				for (int a = 1; a <= upgradeLevel; a++) {
					UpgradeCar (i, a);
				}
			}
			LerpStats (i);
			//Nitro
			ApplyNitroValue(i);
		}
	}

	void ApplyTrackSettings(){
		for (int i = 0; i < allTracks.Length; i++) {
			allTracks [i].unlocked = PlayerProfile.instance.player.m_TrackStatus [i];
		}
	}

	void ApplyTuningSettings(){
		for (int i = 0; i < allBodyColors.Length; i++) {
			allBodyColors [i].unlocked = PlayerProfile.instance.player.m_CarBody [i];
		}
		for (int i = 0; i < allCars.Length; i++) {
			for (int j = 0; j < allCars [i].allWheels.Length; j++) {
				switch (i) {
				case 0:
					allCars [0].allWheels [j].unlocked = PlayerProfile.instance.player.m_PorscheWheels [j];
					break;
				case 1:
					allCars [1].allWheels [j].unlocked = PlayerProfile.instance.player.m_AstonWheels [j];
					break;
				case 2:
					allCars [2].allWheels [j].unlocked = PlayerProfile.instance.player.m_AudiWheels [j];
					break;
				case 3:
					allCars [3].allWheels [j].unlocked = PlayerProfile.instance.player.m_Ferrari599Wheels [j];
					break;
				case 4:
					allCars [4].allWheels [j].unlocked = PlayerProfile.instance.player.m_Ferrari488Wheels [j];
					break;
				case 5:
					allCars [5].allWheels [j].unlocked = PlayerProfile.instance.player.m_LamborghiniWheels [j];
					break;
				}
			}
		}
		for (int i = 0; i < allNeons.Length; i++) {
			allNeons [i].unlocked = PlayerProfile.instance.player.m_Neon [i];
		}
		for (int i = 0; i < allCars.Length; i++) {
			for (int j = 0; j < allCars [i].upgradeStatus.Length; j++) {
				switch (i) {
				case 0:
					allCars [0].upgradeStatus [j].unlocked = PlayerProfile.instance.player.m_PorschePerformance [j];
					break;
				case 1:
					allCars [1].upgradeStatus [j].unlocked = PlayerProfile.instance.player.m_AstonPerformance [j];
					break;
				case 2:
					allCars [2].upgradeStatus [j].unlocked = PlayerProfile.instance.player.m_AudiPerformance [j];
					break;
				case 3:
					allCars [3].upgradeStatus [j].unlocked = PlayerProfile.instance.player.m_Ferrari599Performance [j];
					break;
				case 4:
					allCars [4].upgradeStatus [j].unlocked = PlayerProfile.instance.player.m_Ferrari488Performance [j];
					break;
				case 5:
					allCars [5].upgradeStatus [j].unlocked = PlayerProfile.instance.player.m_LamborghiniPerformance [j];
					break;
				}
			}
		}
		for (int i = 0; i < allNitros.Length; i++) {
			allNitros [i].unlocked = PlayerProfile.instance.player.m_Nitro [i];
		}
	}

	void ApplyPlayerDetails(){
		UIPlayerName.text = PlayerProfile.instance.player.m_name;
		UIPlayerMoney.text = PlayerProfile.instance.player.m_money + " C";
		UIPlayerCar.text = allCars [PlayerProfile.instance.player.m_currentCar].name;

		if (PlayerProfile.instance.player.m_image == 0) {
			UIPlayerImage.sprite = MaleIcon;
		} else {
			UIPlayerImage.sprite = FemaleIcon;
		}
	}
		
	public void RotationAndZoom(){
		float distanceMin = 15f;
		float distanceMax = 70f;
		float zoomSpeed = 20f;
		float FOV = Camera.main.fieldOfView;

		//Zoom
		FOV += Input.GetAxis ("Mouse ScrollWheel") * zoomSpeed;
		FOV = Mathf.Clamp (FOV, distanceMin, distanceMax);
		Camera.main.fieldOfView = FOV;

		//Rotation
		if (Input.GetButtonDown ("Fire2")) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit)) {
				Collider[] childTransforms = allCars [carIndex].carModel.GetComponentsInChildren<MeshCollider> ();
				foreach (Collider t in childTransforms) {
					if (hit.collider == t) {
						raycastTarget = true;
					} else {
						raycastTarget = false;
					}
				}
			}
		}
		if (Input.GetButtonUp ("Fire2")) {
			Vector3 mPos = Camera.main.ScreenToViewportPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0));
			if (raycastTarget) {
				if (mPos.x < 0.5f) {
					rotateDirection = 1;
				} else {
					rotateDirection = -1;
				}
			}
				raycastTarget = false;
		}
			if (!raycastTarget)
				return;
		
			#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL
		allCars [carIndex].carModel.Rotate (0, -Input.GetAxis ("Mouse X"), 0);
			#endif
	}
		
	public void LeaveGarage(){
		Destroy (GameObject.Find ("Player Profile"));
		Destroy (GameObject.Find ("Settings Manager"));
		Destroy (GameObject.Find ("Cars"));
		SceneManager.LoadScene ("MainMenu");
	}
		
	public void Play(){
		PlayerPrefs.SetString ("PlayerCarName", allCars [PlayerProfile.instance.player.m_currentCar].name);
		PlayerPrefs.SetString ("RaceType", allTracks [trackIndex].raceType.ToString ());
		PlayerPrefs.SetString ("AiDifficulty", allTracks [trackIndex].aiDifficulty.ToString ());
		PlayerPrefs.SetInt ("Opponents", allTracks [trackIndex].aiCount);
		PlayerPrefs.SetInt ("Laps", allTracks [trackIndex].laps);
		PlayerPrefs.SetInt ("Track", trackIndex);
		StartCoroutine (LoadScene ());
	}
		
	IEnumerator LoadScene(){
		loadingPanel.SetActive (true);
		UILoadingImage.sprite = allTracks [trackIndex].image;
		UILoadingTrackName.text = allTracks [trackIndex].name;
		UILoadingTrackInfo.text = allTracks [trackIndex].trackInfo;
		while (currentAmount < 100f) {
			currentAmount += 10 * Time.deltaTime;
			UILoadingProgress.text = (int)currentAmount + "%";
			UILoadingBar.fillAmount = currentAmount / 100; 
			yield return null;
		}
		SceneManager.LoadScene (allTracks [trackIndex].sceneName);
	}
}