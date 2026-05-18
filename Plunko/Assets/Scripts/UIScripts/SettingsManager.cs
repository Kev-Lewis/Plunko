using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    [Header("Settings UI")]
    [SerializeField] private GameObject SettingsCanvas;
    [SerializeField] private GameObject TutorialCanvas;
    [SerializeField] private GameObject CloseSettingsButton;
    [SerializeField] private GameObject SettingsGroup;
    [SerializeField] private GameObject AreYouSureGroup;

    [Header("Audio Groups")]
    [SerializeField] private GameObject Audio_Player;
    [SerializeField] private GameObject SFX_Player;
    [SerializeField] private AudioSource blipSelect;

    [Header("Sliders")]
    [SerializeField] private Slider VolumeSlider;
    [SerializeField] private Slider EffectsSlider;

    [Header("Optional References")]
    [SerializeField] private buttonScript bs;

    private const string FirstPlayKey = "FirstPlay";
    private const string VolumeValueKey = "VolumeValue";
    private const string EffectsValueKey = "EffectsValue";

    private AudioSource[] audioPlayers;
    private AudioSource[] sfxPlayers;

    private bool settingsOpen;
    private bool tutorialOpen;

    private void Start() {
        CacheAudioSources();
        LoadOrCreateAudioSettings();
        UpdateAudioLevels();
    }

    private void CacheAudioSources() {
        audioPlayers = Audio_Player != null ? Audio_Player.GetComponentsInChildren<AudioSource>() : new AudioSource[0];
        sfxPlayers = SFX_Player != null ? SFX_Player.GetComponentsInChildren<AudioSource>() : new AudioSource[0];
    }

    private void LoadOrCreateAudioSettings() {
        if (PlayerPrefs.GetFloat(FirstPlayKey) == 0) {
            PlayerPrefs.SetFloat(FirstPlayKey, 1);
            LoadNewDefaults();
            return;
        }

        LoadValues();
    }

    public void openTutorial() {
        SetPanelActive(TutorialCanvas, true);
        SetPanelActive(CloseSettingsButton, false);

        tutorialOpen = true;
        PlaySelectSound();
    }

    public void closeTutorial() {
        SetPanelActive(TutorialCanvas, false);

        PlaySelectSound();
        StartCoroutine(readyToPlay());
    }

    public void areYouSureOpen() {
        PlaySelectSound();

        SetPanelActive(SettingsGroup, false);
        SetPanelActive(AreYouSureGroup, true);
    }

    public void areYouSureClose() {
        PlaySelectSound();

        SetPanelActive(SettingsGroup, true);
        SetPanelActive(AreYouSureGroup, false);
    }

    private IEnumerator readyToPlay() {
        yield return new WaitForSeconds(0.5f);

        tutorialOpen = false;
        SetPanelActive(CloseSettingsButton, true);
    }

    public bool getTutorialOpen() {
        return tutorialOpen;
    }

    public void openSettings() {
        DestroyActiveParticles();

        settingsOpen = !settingsOpen;

        UpdateAudioLevels();
        PlaySelectSound();
        SetPanelActive(SettingsCanvas, settingsOpen);
    }

    public bool getSettingsOpen() {
        return settingsOpen;
    }

    public void openMenuSettings() {
        settingsOpen = true;

        UpdateAudioLevels();
        PlaySelectSound();
        SetPanelActive(SettingsCanvas, true);
    }

    public void closeSettings() {
        settingsOpen = false;

        UpdateAudioLevels();
        PlaySelectSound();
        SetPanelActive(SettingsCanvas, false);
    }

    public void Volume_Slider(float volume) {
        float multiplier = SystemInfo.deviceType == DeviceType.Handheld ? 0.04f : 0.01f;
        PlayerPrefs.SetFloat(VolumeValueKey, volume * multiplier);
        PlayerPrefs.Save();

        UpdateAudioLevels();
    }

    public void Effects_Slider(float volume) {
        float multiplier = SystemInfo.deviceType == DeviceType.Handheld ? 0.4f : 0.2f;
        PlayerPrefs.SetFloat(EffectsValueKey, volume * multiplier);
        PlayerPrefs.Save();

        UpdateAudioLevels();
    }

    private void UpdateAudioLevels() {
        float musicVolume = PlayerPrefs.GetFloat(VolumeValueKey);
        float sfxVolume = PlayerPrefs.GetFloat(EffectsValueKey);

        for (int i = 0; i < audioPlayers.Length; i++) {
            if (audioPlayers[i] != null) {
                audioPlayers[i].volume = musicVolume;
            }
        }

        for (int i = 0; i < sfxPlayers.Length; i++) {
            if (sfxPlayers[i] != null) {
                sfxPlayers[i].volume = sfxVolume;
            }
        }
    }

    private void LoadValues() {
        float musicMultiplier = SystemInfo.deviceType == DeviceType.Handheld ? 0.04f : 0.01f;
        float sfxMultiplier = SystemInfo.deviceType == DeviceType.Handheld ? 0.4f : 0.2f;

        float savedMusicVolume = PlayerPrefs.GetFloat(VolumeValueKey);
        float savedSfxVolume = PlayerPrefs.GetFloat(EffectsValueKey);

        if (VolumeSlider != null) {
            VolumeSlider.value = savedMusicVolume / musicMultiplier;
        }

        if (EffectsSlider != null) {
            EffectsSlider.value = savedSfxVolume / sfxMultiplier;
        }

        UpdateAudioLevels();
    }

    private void LoadNewDefaults() {
        float defaultSliderValue = 0.25f;

        if (VolumeSlider != null) {
            VolumeSlider.value = defaultSliderValue;
        }

        if (EffectsSlider != null) {
            EffectsSlider.value = defaultSliderValue;
        }

        float musicMultiplier = SystemInfo.deviceType == DeviceType.Handheld ? 0.04f : 0.01f;
        float sfxMultiplier = SystemInfo.deviceType == DeviceType.Handheld ? 0.4f : 0.2f;

        PlayerPrefs.SetFloat(VolumeValueKey, defaultSliderValue * musicMultiplier);
        PlayerPrefs.SetFloat(EffectsValueKey, defaultSliderValue * sfxMultiplier);
        PlayerPrefs.Save();

        UpdateAudioLevels();
    }

    public void MainMenu() {
        UpdateAudioLevels();

        if (SceneManager.GetActiveScene().name == "infiniteLevel") {
            GameSpeedManager.EndShotSpeed();
            SaveRunBeforeLeavingLevel();
        }

        PlaySelectSound();
        StartCoroutine(changeScene("Menu", GetSelectSoundLength()));
    }

    private void SaveRunBeforeLeavingLevel() {
        if (RunManager.Instance == null) {
            return;
        }

        if (Shooter.gameOver) {
            RunManager.Instance.MarkRunFinished();
            return;
        }

        if (!Shooter.shooting) {
            RunManager.Instance.SaveRunCheckpoint();
        }
    }

    private void DestroyActiveParticles() {
        ParticleSystem[] particleObjects = FindObjectsOfType<ParticleSystem>();

        foreach (ParticleSystem particleObject in particleObjects) {
            if (particleObject != null) {
                Destroy(particleObject.gameObject);
            }
        }
    }

    private void SetPanelActive(GameObject panel, bool active) {
        if (panel != null) {
            panel.SetActive(active);
        }
    }

    private void PlaySelectSound() {
        if (blipSelect != null) {
            blipSelect.Play();
        }
    }

    private float GetSelectSoundLength() {
        if (blipSelect != null && blipSelect.clip != null) {
            return blipSelect.clip.length;
        }

        return 0f;
    }

    private IEnumerator changeScene(string scene, float time) {
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(scene);
    }
}