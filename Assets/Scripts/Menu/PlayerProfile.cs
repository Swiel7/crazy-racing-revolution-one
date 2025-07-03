using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

[System.Serializable]
public class PlayerProfile : MonoBehaviour
{
	public static PlayerProfile instance; 
	private MainMenu mm;
	public PlayerData player = new PlayerData ();
	private bool showAlert = true;
	private string controller = "";
	private Resolution defaultResolution;
	public Resolution[] resolutions;

	void Awake(){
		mm = GameObject.FindObjectOfType<MainMenu> ();
		instance = this;
		DontDestroyOnLoad (gameObject);
		defaultResolution = Screen.currentResolution;
		resolutions = Screen.resolutions;
	}

	void Start(){
		InvokeRepeating ("DetectController", 1.0f, 2.0f);
	}

	void OnEnable(){
		mm.playerGender.onValueChanged.AddListener (delegate {
			PlayerImage ();
		});
	}
		
	void DetectController(){
		string[] joyNames = Input.GetJoystickNames ();
		if (joyNames.Length > 0) {
			for (int i = 0; i < joyNames.Length; ++i) {
				if (!string.IsNullOrEmpty (joyNames [i])) {
					controller = joyNames [i];
					if (showAlert) {
						if (SettingsMenu.instance) {
							SettingsMenu.instance.CreateAlertPanel ("Podłączono " + controller + " !"); 
						} else {
							CreateAlertPanel ("Podłączono " + controller + " !");
						}
						showAlert = !showAlert;
					}
				} else {
					if (!showAlert) {
						if (SettingsMenu.instance) {
							SettingsMenu.instance.CreateAlertPanel ("Odłączono " + controller + " !");
						} else {
							CreateAlertPanel ("Odłączono " + controller + " !");
						}
						showAlert = !showAlert;
					}
				}
			}
		}
	}

	public void CreatePlayer(){
		if (mm.playerName.text != "") {
			player.m_name = mm.playerName.text;
			player.m_gender = mm.playerGender.GetComponent<Dropdown> ().GetComponentInChildren<Text> ().text;
			player.m_image = mm.playerGender.value;
			player.m_money = 10000;
			player.m_currentCar = 0;
			DefaultGraphicsSettings (player);
			SceneManager.LoadScene ("Garage");
		} else {
			CreateAlertPanel ("Proszę wpisać nazwę dla gracza !");
		}
	}

	void PlayerImage(){
		if (mm.playerGender.value == 0) {
			mm.playerImage.sprite = mm.MaleIcon;
		} else {
			mm.playerImage.sprite = mm.FemaleIcon;
		}
	}
		
	public void LoadGame(){
		string fileName = mm.UIFileName.text;
		string filePath = System.IO.Path.Combine (Application.persistentDataPath, "Savegames");
		filePath = System.IO.Path.Combine (filePath, fileName + ".sav");

		if (fileName == "") {
			CreateAlertPanel ("Nie wybrano żadnego zapisu stanu gry !");
			return;
		}
		if (File.Exists (filePath) == false) {
			CreateAlertPanel ("Na dysku twardym nie znaleziono wybranego zapisu gry !");
			return;
		}
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (filePath, FileMode.Open);
		PlayerData player = (PlayerData)bf.Deserialize (file);

		LoadPlayerDetails (player);
		LoadCarSettings (player);
		LoadTuningSettings (player);
		LoadTrackSettings (player);
		LoadGraphicsSettings (player);
		LoadControlsSettings (player);
		LoadGeneralSettings (player);
		LoadSoundSettings (player);

		file.Close ();
		SceneManager.LoadScene ("Garage");
	}
		
	public void SaveGame(){
		string fileName = GarageBackup.instance.UIFileName.text;
		string filePath = System.IO.Path.Combine (Application.persistentDataPath, "Savegames");
		filePath = System.IO.Path.Combine (filePath, fileName + ".sav");
		Debug.Log (filePath);

		if (fileName == "") {
			GarageBackup.instance.CreateAlertPanel ("Proszę podać nazwę dla zapisu gry !");
			return;
		}
		if (File.Exists (filePath) == true) {
			GarageBackup.instance.CreateAlertPanel ("Plik o podanej nazwie już istnieje. Proszę wpisać unikalną nazwę dla zapisu stanu gry !");
			return;
		}
		if (Directory.Exists (System.IO.Path.Combine (Application.persistentDataPath, "Savegames")) == false) {
			Directory.CreateDirectory (System.IO.Path.Combine (Application.persistentDataPath, "Savegames"));
		}
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (filePath);
		bf.Serialize (file, PlayerProfile.instance.player);
		file.Close ();
		CloseSaveGamePanel ();
	}
		
	public void OpenSaveGamePanel(GameObject savePrefab){
		string filePath = System.IO.Path.Combine (Application.persistentDataPath, "Savegames");
		string[] allSavegames = Directory.GetFiles (filePath, "*.sav");
		InputField inputField = GarageBackup.instance.UIFileName;
		GarageBackup.instance.UIDelete.onClick.RemoveAllListeners ();

		foreach (string file in allSavegames) {
			GameObject save = (GameObject)GameObject.Instantiate (savePrefab);
			save.transform.SetParent (GarageBackup.instance.saveGameContent, false);
			save.GetComponentInChildren<Text> ().text = Path.GetFileNameWithoutExtension (file);
			save.transform.GetChild (1).GetComponent<Text> ().text = File.GetCreationTime (file).ToString ();
			save.GetComponent<InputFieldName> ().inputField = inputField;
		}

		GarageBackup.instance.UIDelete.onClick.AddListener (delegate {
			filePath = System.IO.Path.Combine (filePath, inputField.text + ".sav");
			if (inputField.text == "") {
				GarageBackup.instance.CreateAlertPanel ("Nie wybrano żadnego zapisu stanu gry !");
				return;
			} else if (File.Exists (filePath) == false) {
				GarageBackup.instance.CreateAlertPanel ("Na dysku twardym nie znaleziono wybranego zapisu gry !");
				return;
			} else {
				GarageBackup.instance.CreateNotificationPanel ("Czy na pewno chcesz usunąć wybrany zapis gry ?");
			}
			GarageBackup.instance.UIAccept.onClick.RemoveAllListeners ();
			GarageBackup.instance.UIAccept.onClick.AddListener (delegate {
				GarageBackup.instance.notificationCanvas.SetActive(false);
				File.Delete (filePath);
				foreach (Transform save in GarageBackup.instance.saveGameContent) {
					if (save.GetComponentInChildren<Text> ().text == inputField.text) {
						Destroy (save.gameObject);
					}
				}
			});
		});
	}

	public void OpenLoadGamePanel(GameObject savePrefab){
		string filePath = System.IO.Path.Combine (Application.persistentDataPath, "Savegames");
		string[] allSavegames = Directory.GetFiles (filePath, "*.sav");
		InputField inputField = mm.UIFileName;
		mm.UIDelete.onClick.RemoveAllListeners ();

		foreach (string file in allSavegames) {
			GameObject save = (GameObject)GameObject.Instantiate (savePrefab);
			save.transform.SetParent (mm.saveGameContent, false);
			save.GetComponentInChildren<Text> ().text = Path.GetFileNameWithoutExtension (file);
			save.transform.GetChild (1).GetComponent<Text> ().text = File.GetCreationTime (file).ToString ();
			save.GetComponent<InputFieldName> ().inputField = inputField;
		}
			
		mm.UIDelete.onClick.AddListener (delegate {
			filePath = System.IO.Path.Combine (filePath, inputField.text + ".sav");
			if (inputField.text == "") {
				CreateAlertPanel ("Nie wybrano żadnego zapisu stanu gry !");
				return;
			} else if (File.Exists (filePath) == false) {
				CreateAlertPanel ("Na dysku twardym nie znaleziono wybranego zapisu gry !");
				return;
			} else {
				CreateNotificationPanel ("Czy na pewno chcesz usunąć wybrany zapis gry ?");
			}
			mm.UIAccept.onClick.RemoveAllListeners ();
			mm.UIAccept.onClick.AddListener (delegate {
				mm.notificationCanvas.SetActive(false);
				File.Delete (filePath);
				foreach (Transform save in mm.saveGameContent) {
					if (save.GetComponentInChildren<Text> ().text == inputField.text) {
						Destroy (save.gameObject);
					}
				}
			});
		});
	}
		
	public void CloseSaveGamePanel(){
		GarageBackup.instance.saveGameCanvas.SetActive (false);
		while (GarageBackup.instance.saveGameContent.childCount > 0) {
			Transform s = GarageBackup.instance.saveGameContent.GetChild (0);
			s.SetParent (null);
			Destroy (s.gameObject);
		}
	}

	public void CloseLoadGamePanel(){
		mm.loadGameCanvas.SetActive (false);
		while (mm.saveGameContent.childCount > 0) {
			Transform s = mm.saveGameContent.GetChild (0);
			s.SetParent (null);
			Destroy (s.gameObject);
		}
	}

	void LoadGeneralSettings(PlayerData player){
		PlayerProfile.instance.player.m_DefaultCamera = player.m_DefaultCamera;
		PlayerProfile.instance.player.m_CameraHeight = player.m_CameraHeight;
		PlayerProfile.instance.player.m_CameraDistance = player.m_CameraDistance;
		PlayerProfile.instance.player.m_SpeedUnit = player.m_SpeedUnit;
		PlayerProfile.instance.player.m_PlayerStartPosition = player.m_PlayerStartPosition;
		PlayerProfile.instance.player.m_ShowMinimap = player.m_ShowMinimap;
		PlayerProfile.instance.player.m_ShowRacerName3D = player.m_ShowRacerName3D;
		PlayerProfile.instance.player.m_ShowRacerPointer = player.m_ShowRacerPointer;
		PlayerProfile.instance.player.m_ShowWelcomePanel = player.m_ShowWelcomePanel;
	}

	void LoadSoundSettings(PlayerData player){
		PlayerProfile.instance.player.m_SoundON = player.m_SoundON;
		PlayerProfile.instance.player.m_SfxVolume = player.m_SfxVolume;
		PlayerProfile.instance.player.m_MusicVolume = player.m_MusicVolume;
		PlayerProfile.instance.player.m_ButtonVolume = player.m_ButtonVolume;
	}

	void LoadGraphicsSettings(PlayerData player){
		PlayerProfile.instance.player.m_GraphicsQuality = player.m_GraphicsQuality;
		PlayerProfile.instance.player.m_ResolutionIndex = player.m_ResolutionIndex;
		PlayerProfile.instance.player.m_Antialiasing = player.m_Antialiasing;
		PlayerProfile.instance.player.m_TextureQuality = player.m_TextureQuality;
		PlayerProfile.instance.player.m_AnisotropicTextures = player.m_AnisotropicTextures;
		PlayerProfile.instance.player.m_vSync = player.m_vSync;
		PlayerProfile.instance.player.m_Brightness = player.m_Brightness;
		PlayerProfile.instance.player.m_ShadowsType = player.m_ShadowsType;
		PlayerProfile.instance.player.m_ShadowsDistance = player.m_ShadowsDistance;
		PlayerProfile.instance.player.m_Fullscreen = player.m_Fullscreen;
	}

	void LoadPlayerDetails(PlayerData player){
		PlayerProfile.instance.player.m_name = player.m_name;
		PlayerProfile.instance.player.m_gender = player.m_gender;
		PlayerProfile.instance.player.m_image = player.m_image;
		PlayerProfile.instance.player.m_money = player.m_money;
		PlayerProfile.instance.player.m_currentCar = player.m_currentCar;
	}

	void LoadCarSettings(PlayerData player){
		for (int i = 0; i < player.m_AllCarBody.Length; i++) {
			PlayerProfile.instance.player.m_AllCarBody [i] = player.m_AllCarBody [i];
		}
		for (int i = 0; i < player.m_AllCarWheels.Length; i++) {
			PlayerProfile.instance.player.m_AllCarWheels [i] = player.m_AllCarWheels [i];
		}
		for (int i = 0; i < player.m_AllCarNeon.Length; i++) {
			PlayerProfile.instance.player.m_AllCarNeon [i] = player.m_AllCarNeon [i];
		}
		for (int i = 0; i < player.m_AllCarPerformance.Length; i++) {
			PlayerProfile.instance.player.m_AllCarPerformance [i] = player.m_AllCarPerformance [i];
		}
		for (int i = 0; i < player.m_AllCarNitro.Length; i++) {
			PlayerProfile.instance.player.m_AllCarNitro [i] = player.m_AllCarNitro [i];
		}
		for (int i = 0; i < player.m_AllCarStatus.Length; i++) {
			PlayerProfile.instance.player.m_AllCarStatus [i] = player.m_AllCarStatus [i];
		}
	}
		
	void LoadTuningSettings(PlayerData player){
		for (int i = 0; i < player.m_CarBody.Length; i++) {
			PlayerProfile.instance.player.m_CarBody [i] = player.m_CarBody [i];
		}
		for (int i = 0; i < player.m_PorscheWheels.Length; i++) {
			PlayerProfile.instance.player.m_PorscheWheels [i] = player.m_PorscheWheels [i];
		}
		for (int i = 0; i < player.m_AstonWheels.Length; i++) {
			PlayerProfile.instance.player.m_AstonWheels [i] = player.m_AstonWheels [i];
		}
		for (int i = 0; i < player.m_AudiWheels.Length; i++) {
			PlayerProfile.instance.player.m_AudiWheels [i] = player.m_AudiWheels [i];
		}
		for (int i = 0; i < player.m_Ferrari599Wheels.Length; i++) {
			PlayerProfile.instance.player.m_Ferrari599Wheels [i] = player.m_Ferrari599Wheels [i];
		}
		for (int i = 0; i < player.m_Ferrari488Wheels.Length; i++) {
			PlayerProfile.instance.player.m_Ferrari599Wheels [i] = player.m_Ferrari599Wheels [i];
		}
		for (int i = 0; i < player.m_LamborghiniWheels.Length; i++) {
			PlayerProfile.instance.player.m_LamborghiniWheels [i] = player.m_LamborghiniWheels [i];
		}
		for (int i = 0; i < player.m_Neon.Length; i++) {
			PlayerProfile.instance.player.m_Neon [i] = player.m_Neon [i];
		}
		for (int i = 0; i < player.m_PorschePerformance.Length; i++) {
			PlayerProfile.instance.player.m_PorschePerformance [i] = player.m_PorschePerformance [i];
		}
		for (int i = 0; i < player.m_AstonPerformance.Length; i++) {
			PlayerProfile.instance.player.m_AstonPerformance [i] = player.m_AstonPerformance [i];
		}
		for (int i = 0; i < player.m_AudiPerformance.Length; i++) {
			PlayerProfile.instance.player.m_AudiPerformance [i] = player.m_AudiPerformance [i];
		}
		for (int i = 0; i < player.m_Ferrari599Performance.Length; i++) {
			PlayerProfile.instance.player.m_Ferrari599Performance [i] = player.m_Ferrari599Performance [i];
		}
		for (int i = 0; i < player.m_Ferrari488Performance.Length; i++) {
			PlayerProfile.instance.player.m_Ferrari488Performance [i] = player.m_Ferrari488Performance [i];
		}
		for (int i = 0; i < player.m_LamborghiniPerformance.Length; i++) {
			PlayerProfile.instance.player.m_LamborghiniPerformance [i] = player.m_LamborghiniPerformance [i];
		}
		for (int i = 0; i < player.m_Nitro.Length; i++) {
			PlayerProfile.instance.player.m_Nitro [i] = player.m_Nitro [i];
		}
	}

	void LoadTrackSettings(PlayerData player){
		for (int i = 0; i < player.m_TrackStatus.Length; i++) {
			PlayerProfile.instance.player.m_TrackStatus [i] = player.m_TrackStatus [i];
		}
	}

	void LoadControlsSettings(PlayerData player){
		for (int i = 0; i < player.m_Controls.Length; i++) {
			PlayerProfile.instance.player.m_Controls [i] = player.m_Controls [i];
		}
	}

	public void DefaultGraphicsSettings(PlayerData player){
		PlayerProfile.instance.player.m_GraphicsQuality = 3;
		PlayerProfile.instance.player.m_Antialiasing = 1;
		PlayerProfile.instance.player.m_TextureQuality = 0;
		PlayerProfile.instance.player.m_AnisotropicTextures = 2;
		PlayerProfile.instance.player.m_vSync = 2;
		PlayerProfile.instance.player.m_Brightness = 0.8f;
		PlayerProfile.instance.player.m_ShadowsType = 2;
		PlayerProfile.instance.player.m_ShadowsDistance = 20f;
		PlayerProfile.instance.player.m_Fullscreen = true;
		ApplyDefaultResolution ();
	}

	public void DefaultGeneralSettings(PlayerData player){
		PlayerProfile.instance.player.m_DefaultCamera = 1;
		PlayerProfile.instance.player.m_CameraHeight = 2.5f;
		PlayerProfile.instance.player.m_CameraDistance = 6.0f;
		PlayerProfile.instance.player.m_SpeedUnit = 1;
		PlayerProfile.instance.player.m_PlayerStartPosition = 1; 
		PlayerProfile.instance.player.m_ShowMinimap = true;
		PlayerProfile.instance.player.m_ShowRacerName3D = true;
		PlayerProfile.instance.player.m_ShowRacerPointer = true;
		PlayerProfile.instance.player.m_ShowWelcomePanel = true;
	}

	void ApplyDefaultResolution(){
		for (int i = 0; i < PlayerProfile.instance.resolutions.Length; i++) {
			if (PlayerProfile.instance.resolutions [i].ToString() == defaultResolution.ToString()) {
				PlayerProfile.instance.player.m_ResolutionIndex = i;
			}
		}
	}

	public void CreateAlertPanel(string info){
		mm.notificationCanvas.SetActive (true);
		mm.notificationPanel.SetActive (false);
		mm.alertPanel.SetActive (true);
		mm.quitPanel.SetActive (false);
		mm.UIAlertText.text = info;
	}

	public void CreateNotificationPanel(string info){
		mm.notificationCanvas.SetActive (true);
		mm.notificationPanel.SetActive (true);
		mm.alertPanel.SetActive (false);
		mm.quitPanel.SetActive (false);
		mm.UINotificationText.text = info;
	}

	public void DecreaseMoney(int amount) {
		if (PlayerProfile.instance.player.m_money > 0) {
			PlayerProfile.instance.player.m_money -= amount;
			if (GarageBackup.instance) {
				GarageBackup.instance.UIPlayerMoney.text = PlayerProfile.instance.player.m_money + " C";
			}
		}
	}

	public void IncreaseMoney(int amount) {
		PlayerProfile.instance.player.m_money += amount;
	}
}