using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
	public static SoundManager instance;
	public AudioMixer masterMixer;

	[System.Serializable]
	public class RaceSounds
	{
		public AudioClip countdownSound;
		public AudioClip startRaceSound;
		public AudioClip checkpointSound;
		public AudioClip speedTrapSound;
	}
		
	[Header("Audio Mixer Groups")]
	public AudioMixerGroup musicMixerGroup;
	public AudioMixerGroup raceMixerGroup;
	public AudioMixerGroup buttonMixerGroup;

	[Header("Race Sounds")]
	public RaceSounds raceSounds;
	private AudioSource audioSource;

	[Header("Button Sounds")]
	public AudioClip buttonClickedSound;
	private static GameObject button;
	private static AudioSource buttonAudio;

	[Header("Background Music")]
	private static GameObject bgm;
	public List<AudioClip> backgroundMusic = new List<AudioClip> ();
	private static AudioSource bgmAudio;
	private int trackIndex;

	void Awake(){
		if (instance == null) {
			instance = this;
		} else {
			Destroy (this.gameObject);
		}
		DontDestroyOnLoad (gameObject);
		audioSource = GetComponent<AudioSource> ();
	}

	void Start(){
		ButtonSoundManager ();
		StartMusic ();
	}

	void OnLevelWasLoaded(){
		ApplySoundSettings ();
	}

	void Update(){
		if (!bgmAudio.isPlaying) {
			trackIndex++;
			if (trackIndex >= backgroundMusic.Count) { 
				trackIndex = 0;
			}
			PlayMusicTrack (trackIndex);
		}
	}
		
	public void PlayDefaultSound(AudioClip c){
		audioSource.spatialBlend = 0;
		audioSource.PlayOneShot (c);
	}

	public void StartMusic(){
		if (backgroundMusic.Count > 0) {
			if (bgm == null) {
				bgm = new GameObject ("Background Music");
				DontDestroyOnLoad (bgm);
				bgm.AddComponent<AudioSource> ();
				bgmAudio = bgm.GetComponent<AudioSource> ();
				bgmAudio.loop = (backgroundMusic.Count == 1);
				bgmAudio.spatialBlend = 0;
				bgmAudio.outputAudioMixerGroup = musicMixerGroup;
				int trackIndex = 0;
				PlayMusicTrack (trackIndex);
			}
		}
	}

	public void PlayClip(AudioClip clip, Vector3 position, float volume, float minDistance){
		GameObject go = new GameObject("Hit Sound");
		go.transform.position = position;
		AudioSource source = go.AddComponent<AudioSource>() as AudioSource;
		source.outputAudioMixerGroup = raceMixerGroup;
		source.spatialBlend = 1.0f;
		source.clip = clip;
		source.volume = volume;
		source.minDistance = minDistance;
		source.Play();
		Destroy(go, clip.length);
	}

	public void ButtonSoundManager(){
		if (button == null) {
			button = new GameObject ("Button Sound");
			DontDestroyOnLoad (button);
			button.AddComponent<AudioSource> ();
			buttonAudio = button.GetComponent<AudioSource> ();
			buttonAudio.playOnAwake = false;
			buttonAudio.outputAudioMixerGroup = buttonMixerGroup;
			buttonAudio.clip = buttonClickedSound;
		}
	}

	public void PlayButtonSound(){
		buttonAudio.Play ();
	}
		
	void PlayMusicTrack(int index){
		bgmAudio.clip = backgroundMusic [index];
		bgmAudio.Play ();
	}
		
	public void SoundInGame(){
		if (SettingsMenu.instance.UISoundToggle.isOn) {
			masterMixer.ClearFloat ("MasterVolume");
		} else {
			masterMixer.SetFloat ("MasterVolume", -80f);
		}
	}

	public void SetSfxVolume(float sfxValue){
		masterMixer.SetFloat ("SfxVolume", sfxValue);
	}

	public void SetMusicVolume(float musicValue){
		masterMixer.SetFloat ("MusicVolume", musicValue);
	}

	public void SetButtonVolume(float buttonValue){
		masterMixer.SetFloat ("ButtonVolume", buttonValue);
	}

	public void ApplySoundSettings(){
		if (SettingsMenu.instance) {
			SettingsMenu.instance.UISoundToggle.isOn = PlayerProfile.instance.player.m_SoundON;
			SettingsMenu.instance.UISfxVolumeSlider.value = PlayerProfile.instance.player.m_SfxVolume;
			SettingsMenu.instance.UIMusicVolumeSlider.value = PlayerProfile.instance.player.m_MusicVolume;
			SettingsMenu.instance.UIButtonVolumeSlider.value = PlayerProfile.instance.player.m_ButtonVolume;
		}
	}

	public void SaveSoundSettings(){
		float value;
		masterMixer.GetFloat ("MasterVolume", out value);
		if (value > -80f) {
			PlayerProfile.instance.player.m_SoundON = true;
		} else {
			PlayerProfile.instance.player.m_SoundON = false;
		}
		PlayerProfile.instance.player.m_SfxVolume = SettingsMenu.instance.UISfxVolumeSlider.value;
		masterMixer.GetFloat ("MusicVolume", out PlayerProfile.instance.player.m_MusicVolume);
		masterMixer.GetFloat ("ButtonVolume", out PlayerProfile.instance.player.m_ButtonVolume);
	}

	public void DefaultSoundSettings(){
		PlayerProfile.instance.player.m_SoundON = true;
		PlayerProfile.instance.player.m_SfxVolume = 10f;
		PlayerProfile.instance.player.m_MusicVolume = -10f;
		PlayerProfile.instance.player.m_ButtonVolume = 0f;
	}
}