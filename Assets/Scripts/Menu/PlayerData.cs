using UnityEngine;

[System.Serializable]
public class PlayerData
{
	[Header("Player Details")]
	public string m_name;
	public string m_gender;
	public int m_image;
	public int m_money;
	public int m_currentCar;

	[Header("Car Status")]
	public int[] m_AllCarBody = new int[6];
	public int[] m_AllCarWheels = new int[6];
	public int[] m_AllCarNeon = new int[6];
	public int[] m_AllCarPerformance = new int[6];
	public int[] m_AllCarNitro = new int[6];
	public bool[] m_AllCarStatus = new bool[6];

	[Header("Track Status")]
	public bool[] m_TrackStatus = new bool[3];

	[Header("Tuning Parts Status")]
	public bool[] m_CarBody = new bool[26];
	public bool[] m_PorscheWheels = new bool[6];
	public bool[] m_AstonWheels = new bool[6];
	public bool[] m_AudiWheels = new bool[6];
	public bool[] m_Ferrari599Wheels = new bool[6];
	public bool[] m_Ferrari488Wheels = new bool[6];
	public bool[] m_LamborghiniWheels = new bool[6];
	public bool[] m_Neon = new bool[20];
	public bool[] m_Nitro = new bool[4];
	public bool[] m_PorschePerformance = new bool[4];
	public bool[] m_AstonPerformance = new bool[4];
	public bool[] m_AudiPerformance = new bool[4];
	public bool[] m_Ferrari599Performance = new bool[4];
	public bool[] m_Ferrari488Performance = new bool[4];
	public bool[] m_LamborghiniPerformance = new bool[4];

	[Header("General Settings")]
	public int m_DefaultCamera;
	public float m_CameraHeight;
	public float m_CameraDistance;
	public int m_SpeedUnit;
	public int m_PlayerStartPosition;
	public bool m_ShowMinimap;
	public bool m_ShowRacerName3D;
	public bool m_ShowRacerPointer;
	public bool m_ShowWelcomePanel;

	[Header("Graphics Settings")]
	public int m_GraphicsQuality;
	public int m_ResolutionIndex;
	public int m_Antialiasing;
	public int m_TextureQuality;
	public int m_AnisotropicTextures;
	public int m_vSync;
	public float m_Brightness;
	public int m_ShadowsType;
	public float m_ShadowsDistance;
	public bool m_Fullscreen;

	[Header("Sound Settings")]
	public bool m_SoundON;
	public float m_SfxVolume;
	public float m_MusicVolume;
	public float m_ButtonVolume;

	[Header("Controls Settings")]
	public int[] m_Controls = new int[30];
}