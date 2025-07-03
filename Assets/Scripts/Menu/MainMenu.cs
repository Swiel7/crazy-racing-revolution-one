using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour 
{
	[Header("Panels")]
	public GameObject notificationCanvas;
	public GameObject notificationPanel;
	public GameObject alertPanel;
	public GameObject loadGameCanvas;
	public GameObject newGameCanvas;
	public GameObject quitPanel;

	[Header("Save Game Components")] 
	public InputField UIFileName;
	public Transform saveGameContent;
	public Button UIDelete;
	public Button UIAccept;

	[Header("Player Components")]
	public Text UINotificationText;
	public Text UIAlertText;
	public InputField playerName;
	public Dropdown playerGender;
	public Image playerImage;
	public Sprite MaleIcon;
	public Sprite FemaleIcon;

	void Awake(){
		notificationCanvas.SetActive (false);
		loadGameCanvas.SetActive (false);
		newGameCanvas.SetActive (false);
	}

	public void QuitRequest(){
		notificationCanvas.SetActive (true);
		notificationPanel.SetActive (false);
		alertPanel.SetActive (false);
		quitPanel.SetActive (true);
	}

	public void ConfirmQuit(){
		Application.Quit ();
	}
}