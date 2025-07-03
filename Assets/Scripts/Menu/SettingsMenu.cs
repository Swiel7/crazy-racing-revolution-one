using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour 
{
	public static SettingsMenu instance;

	[Header("Panels")]
	public GameObject settingsPanel; 
	public GameObject generalSettingsPanel;
	public GameObject graphicsSettingsPanel;
	public GameObject soundSettingsPanel;
	public GameObject controlsSettingsPanel;
	public GameObject notificationPanel;
	public GameObject alertPanel;
	public GameObject quitPanel;

	[Header("General Settings")]
	public Dropdown UIDefaultCameraDropdown;
	public Slider UICameraHeightSlider;
	public Slider UICameraDistanceSlider;
	public Dropdown UISpeedUnitDropdown;
	public Dropdown UIStartPositionDropdown;
	public Toggle UIMinimapToggle;
	public Toggle UIRacerName3DToggle;
	public Toggle UIRacerPointerToggle;
	public Toggle UIWelcomePanelToggle;

	[Header("Graphics Settings")]
	public Dropdown UIGraphicsQualityDropdown;
	public Dropdown UIScreenResolutionDropdown;
	public Dropdown UIAntialiasingDropdown;
	public Dropdown UITextureQualityDropdown;
	public Dropdown UIAnisotropicTexturesDropdown;
	public Dropdown UI_VSyncDropdown;
	public Dropdown UIShadowDropdown;
	public Slider UIBrightnessSlider;
	public Slider UIShadowDistanceSlider;
	public Toggle UIFullscreenToogle;

	[Header("Sound Settings")]
	public Toggle UISoundToggle;
	public Slider UISfxVolumeSlider;
	public Slider UIMusicVolumeSlider;
	public Slider UIButtonVolumeSlider;

	[Header("Controls Settings")]
	public Dropdown UIInputDevice;

	[Header("Other")]
	private GameObject minimap;
	public Button UIApplySettingsButton;
	public Button UICancelSettingsButton;
	public Text UIAlertText;
	public Text UINotificationText;
	[HideInInspector] public List<GameObject> names3D = new List<GameObject> ();
	[HideInInspector] public List<GameObject> aiPointers = new List<GameObject> (); 

	void Awake(){
		instance = this;
		DontDestroyOnLoad (gameObject);
	}

	void OnLevelWasLoaded(){
		if (PlayerProfile.instance) { 
			ApplyGraphicsSettings ();
			ApplyGeneralSettings ();
			LoadSettings ();
		}
		if (RaceManager.instance) {
			minimap = FindObjectOfType<MiniMap> ().transform.parent.gameObject;
			Minimap ();
		}
	}
		
	void OnEnable(){
		foreach (Resolution resolution in PlayerProfile.instance.resolutions) {
			UIScreenResolutionDropdown.options.Add (new Dropdown.OptionData (resolution.ToString ()));
		}
	}

	void Update(){
		if (!PlayerCamera.instance && FindObjectOfType<PlayerCamera> ()) {
			PlayerCamera.instance = FindObjectOfType<PlayerCamera> ();
			SetDefaultCamera ();
			SetCameraDistance ();
			SetCameraHeight ();
			RacerPointer (); 
		}
	}

	public void GeneralSettingsMenu(){
		if (GarageBackup.instance) {
			GarageBackup.instance.rotate = false;
		}
		settingsPanel.SetActive (true);
		generalSettingsPanel.SetActive (true);
		graphicsSettingsPanel.SetActive (false);
		soundSettingsPanel.SetActive (false);
		controlsSettingsPanel.SetActive (false);
	}
	public void GraphicsSettingsMenu(){
		generalSettingsPanel.SetActive (false);
		graphicsSettingsPanel.SetActive (true);
		soundSettingsPanel.SetActive (false);
		controlsSettingsPanel.SetActive (false);
	}
	public void SoundSettingsMenu(){
		generalSettingsPanel.SetActive (false);
		graphicsSettingsPanel.SetActive (false);
		soundSettingsPanel.SetActive (true);
		controlsSettingsPanel.SetActive (false);
	}
	public void ControlsSettingsMenu(){
		generalSettingsPanel.SetActive (false);
		graphicsSettingsPanel.SetActive (false);
		soundSettingsPanel.SetActive (false);
		controlsSettingsPanel.SetActive (true);
	}
		
	public void ChangeGraphicsQuality(){
		QualitySettings.SetQualityLevel (UIGraphicsQualityDropdown.value);
	}

	public void ChangeScreenResolution(){
		Screen.SetResolution (PlayerProfile.instance.resolutions [UIScreenResolutionDropdown.value].width, PlayerProfile.instance.resolutions [UIScreenResolutionDropdown.value].height, Screen.fullScreen);
	}

	public void ChangeAntialiasing(){
		QualitySettings.antiAliasing = (int)Mathf.Pow (2f, UIAntialiasingDropdown.value);
	}

	public void ChangeTextureQuality(){
		QualitySettings.masterTextureLimit = UITextureQualityDropdown.value;
	}

	public void ChangeAnisotropicTextures(){
		switch(UIAnisotropicTexturesDropdown.value)
		{
		case 0:
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
			break;
		case 1:
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
			break;
		case 2:
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
			break;
		}
	}

	public void ChangeVSync(){
		QualitySettings.vSyncCount = UI_VSyncDropdown.value;
	}

	public void ChangeBrightness (){
		if (GameObject.Find ("Directional Light")) {
			Light dLight = GameObject.Find ("Directional Light").GetComponent<Light> ();
			dLight.intensity = UIBrightnessSlider.value; 
		}
	}

	public void ChangeShadowType(){
		Light[] lights = GameObject.FindObjectsOfType<Light> ();
		if (lights.Length == 0)
			return;
		foreach (Light light in lights) {
			switch (UIShadowDropdown.value) 
			{
			case 0: 
				light.shadows = LightShadows.None;
				break;
			case 1:
				light.shadows = LightShadows.Hard;
				break;
			case 2:
				light.shadows = LightShadows.Soft;
				break;
			}
		}
	}

	public void ChangeShadowsDistance(){
		QualitySettings.shadowDistance = UIShadowDistanceSlider.value;
	}

	public void SetFullscreen(){
		Screen.fullScreen = UIFullscreenToogle.isOn;
	}
		
	public void SetCameraHeight(){
		if (PlayerCamera.instance) {
			PlayerCamera.instance.height = UICameraHeightSlider.value;
		}
	}

	public void SetCameraDistance(){
		if (PlayerCamera.instance) {
			PlayerCamera.instance.distance = UICameraDistanceSlider.value;
		}
	}

	public void ChangeSpeedUnit(){
		Car_Controller playerCar = GameObject.FindGameObjectWithTag ("Player").GetComponent<Car_Controller> ();
		if (UISpeedUnitDropdown.value == 0) {
			playerCar.speedUnit = Car_Controller.SpeedUnit.MPH;
			playerCar.changeTopSpeed = true;
			if (FindObjectOfType<AI_Control> ()) {
				GameObject[] opponentCars = GameObject.FindGameObjectsWithTag ("Opponent");
				foreach (GameObject car in opponentCars) {
					car.GetComponent<Car_Controller>().speedUnit = Car_Controller.SpeedUnit.MPH;
				}
			}
		} else {
			playerCar.speedUnit = Car_Controller.SpeedUnit.KPH;
			playerCar.changeTopSpeed = true;
			if (FindObjectOfType<AI_Control> ()) {
				GameObject[] opponentCars = GameObject.FindGameObjectsWithTag ("Opponent");
				foreach (GameObject car in opponentCars) {
					car.GetComponent<Car_Controller>().speedUnit = Car_Controller.SpeedUnit.KPH;
				}
			}
		}
	}

	public void SetDefaultCamera(){
		if (PlayerCamera.instance) {
			if (UIDefaultCameraDropdown.value == 0) {
				PlayerCamera.instance.cameraMode = PlayerCamera.CameraMode.ThirdPerson;
			} else {
				PlayerCamera.instance.cameraMode = PlayerCamera.CameraMode.FirstPerson;
			}
		}
	}
		
	public void Minimap(){ 
		if (RaceManager.instance) {
			if (UIMinimapToggle.isOn) {
				minimap.SetActive (true);
			} else {
				minimap.SetActive (false);
			}
		}
	}

	public void Racer3DName(){
		if (RaceManager.instance) {
			foreach (GameObject r_name in names3D) {
				if (UIRacerName3DToggle.isOn) {
					r_name.SetActive (true);
				} else {
					r_name.SetActive (false);
				}
			}
		}
	}
	public void RacerPointer(){
		if (RaceManager.instance) {
			foreach (GameObject r_pointer in aiPointers) {
				if (UIRacerPointerToggle.isOn) {
					r_pointer.SetActive (true);
				} else {
					r_pointer.SetActive (false);
				}
			}
		}
	}
		
	public void ApplyGeneralSettings(){
		UIDefaultCameraDropdown.value = PlayerProfile.instance.player.m_DefaultCamera;
		UICameraHeightSlider.value = PlayerProfile.instance.player.m_CameraHeight;
		UICameraDistanceSlider.value = PlayerProfile.instance.player.m_CameraDistance;
		UISpeedUnitDropdown.value = PlayerProfile.instance.player.m_SpeedUnit;
		UIStartPositionDropdown.value = PlayerProfile.instance.player.m_PlayerStartPosition;
		UIMinimapToggle.isOn = PlayerProfile.instance.player.m_ShowMinimap;
		UIRacerName3DToggle.isOn = PlayerProfile.instance.player.m_ShowRacerName3D;
		UIRacerPointerToggle.isOn = PlayerProfile.instance.player.m_ShowRacerPointer;
		UIWelcomePanelToggle.isOn = PlayerProfile.instance.player.m_ShowWelcomePanel;
	}

	public void SaveGeneralSettings(){
		PlayerProfile.instance.player.m_DefaultCamera = UIDefaultCameraDropdown.value;
		PlayerProfile.instance.player.m_CameraHeight = UICameraHeightSlider.value;
		PlayerProfile.instance.player.m_CameraDistance = UICameraDistanceSlider.value;
		PlayerProfile.instance.player.m_SpeedUnit = UISpeedUnitDropdown.value;
		PlayerProfile.instance.player.m_PlayerStartPosition = UIStartPositionDropdown.value;
		PlayerProfile.instance.player.m_ShowMinimap = UIMinimapToggle.isOn;
		PlayerProfile.instance.player.m_ShowRacerName3D = UIRacerName3DToggle.isOn;
		PlayerProfile.instance.player.m_ShowRacerPointer = UIRacerPointerToggle.isOn;
		PlayerProfile.instance.player.m_ShowWelcomePanel = UIWelcomePanelToggle.isOn;
	}

	public void ApplyGraphicsSettings(){
		UIGraphicsQualityDropdown.value = PlayerProfile.instance.player.m_GraphicsQuality;
		UIScreenResolutionDropdown.value = PlayerProfile.instance.player.m_ResolutionIndex;
		UIAntialiasingDropdown.value = PlayerProfile.instance.player.m_Antialiasing;
		UITextureQualityDropdown.value = PlayerProfile.instance.player.m_TextureQuality;
		UI_VSyncDropdown.value = PlayerProfile.instance.player.m_vSync;
		UIBrightnessSlider.value = PlayerProfile.instance.player.m_Brightness;
		UIShadowDistanceSlider.value = PlayerProfile.instance.player.m_ShadowsDistance;
		UIFullscreenToogle.enabled = PlayerProfile.instance.player.m_Fullscreen; 
		UIAnisotropicTexturesDropdown.value = PlayerProfile.instance.player.m_AnisotropicTextures;
		UIShadowDropdown.value = PlayerProfile.instance.player.m_ShadowsType;
		UIScreenResolutionDropdown.RefreshShownValue ();
	}

	public void SaveGraphicsSettings(){
		PlayerProfile.instance.player.m_GraphicsQuality = UIGraphicsQualityDropdown.value;
		PlayerProfile.instance.player.m_ResolutionIndex = UIScreenResolutionDropdown.value;
		PlayerProfile.instance.player.m_Antialiasing = UIAntialiasingDropdown.value;
		PlayerProfile.instance.player.m_TextureQuality = UITextureQualityDropdown.value;
		PlayerProfile.instance.player.m_vSync = UI_VSyncDropdown.value;
		PlayerProfile.instance.player.m_Brightness = UIBrightnessSlider.value;
		PlayerProfile.instance.player.m_ShadowsDistance = UIShadowDistanceSlider.value;
		PlayerProfile.instance.player.m_Fullscreen = UIFullscreenToogle.enabled;
		PlayerProfile.instance.player.m_AnisotropicTextures = UIAnisotropicTexturesDropdown.value;
		PlayerProfile.instance.player.m_ShadowsType = UIShadowDropdown.value;
	}

	void LoadSettings(){
		ChangeGraphicsQuality ();
		ChangeScreenResolution ();
		ChangeAntialiasing ();
		ChangeTextureQuality ();
		ChangeAnisotropicTextures ();
		ChangeVSync ();
		ChangeBrightness ();
		ChangeShadowType ();
		ChangeShadowsDistance ();
		SetFullscreen ();
	}

	public void ResetToDefaults(){
		if (graphicsSettingsPanel.activeSelf) {
			PlayerProfile.instance.DefaultGraphicsSettings (PlayerProfile.instance.player);
			ApplyGraphicsSettings ();
		}
		if (controlsSettingsPanel.activeSelf) {
			ControlsMenu.instance.ResetAllAxes ();
			ControlsMenu.instance.SaveAllAxis ();
		}
		if (generalSettingsPanel.activeSelf) {
			PlayerProfile.instance.DefaultGeneralSettings (PlayerProfile.instance.player);
			ApplyGeneralSettings ();
		}
		if (soundSettingsPanel.activeSelf) {
			SoundManager.instance.DefaultSoundSettings ();
			SoundManager.instance.ApplySoundSettings ();
		}
	}

	public void PreviousSettings(){
		ApplyGraphicsSettings ();
		ControlsMenu.instance.LoadAllAxes ();
		ApplyGeneralSettings ();
		SoundManager.instance.ApplySoundSettings ();
	}

	public void SaveSettings(){
		SaveGraphicsSettings ();
		ControlsMenu.instance.SaveAllAxis ();
		SaveGeneralSettings ();
		SoundManager.instance.SaveSoundSettings ();
	}

	public void CreateNotificationPanel(string info){
		if (GarageBackup.instance) {
			FindObjectOfType<GarageBackup> ().GetComponentInChildren<CanvasGroup> ().blocksRaycasts = false;
		}
		notificationPanel.SetActive (true);
		alertPanel.SetActive (false);
		quitPanel.SetActive (false);
		UINotificationText.text = info;
	}

	public void CreateAlertPanel(string info){
		if (GarageBackup.instance) {
			FindObjectOfType<GarageBackup> ().GetComponentInChildren<CanvasGroup> ().blocksRaycasts = false;
		}
		notificationPanel.SetActive (false);
		alertPanel.SetActive (true);
		quitPanel.SetActive (false);
		UIAlertText.text = info;
	}
}