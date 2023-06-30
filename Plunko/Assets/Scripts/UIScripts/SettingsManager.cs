using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    // Settings UI
    public GameObject SettingsCanvas;
    public GameObject TutorialCanvas;
    public GameObject CloseSettingsButton;
    public GameObject SettingsGroup;
    public GameObject AreYouSureGroup;

    // Audio Sources
    public GameObject Audio_Player;
    public GameObject SFX_Player;
    private AudioSource[] audio_players;
    private AudioSource[] sfx_players;
    public AudioSource blipSelect;

    // Sliders
    public Slider VolumeSlider;
    public Slider EffectsSlider;

    // bool
    private bool settingsOpen;
    private bool tutorialOpen;

    // other scripts
    private Shooter shoot;
    public buttonScript bs;

    void Start()
    {
        
        audio_players = Audio_Player.GetComponentsInChildren<AudioSource>();
        sfx_players = SFX_Player.GetComponentsInChildren<AudioSource>();
        updateAudioLevels();

        if (PlayerPrefs.GetFloat("FirstPlay") == 0)
        {
            PlayerPrefs.SetFloat("FirstPlay", 1);
            LoadNewDefaults();
        }
        else
        {
            LoadValues();
        }
    }

    // Open the tutorial menu
    public void openTutorial()
    {
        TutorialCanvas.SetActive(true);
        CloseSettingsButton.SetActive(false);
        tutorialOpen = true;
        blipSelect.Play();
    }

    // Open the tutorial menu
    public void closeTutorial()
    {
        TutorialCanvas.SetActive(false);
        blipSelect.Play();
        StartCoroutine(readyToPlay());
    }

    public void areYouSureOpen()
    {
        blipSelect.Play();
        SettingsGroup.SetActive(false);
        AreYouSureGroup.SetActive(true);
    }

    public void areYouSureClose()
    {
        blipSelect.Play();
        SettingsGroup.SetActive(true);
        AreYouSureGroup.SetActive(false);
    }

    IEnumerator readyToPlay()
    {
        yield return new WaitForSeconds(0.5f);
        tutorialOpen = false;
        CloseSettingsButton.SetActive(true);
    }

    public bool getTutorialOpen()
    {
        return tutorialOpen;
    }

    // Opens the settings menu
    public void openSettings()
    {
        var particleObjects = FindObjectsOfType<ParticleSystem>();
        foreach(var pO in particleObjects){
            Destroy(pO.gameObject);
        }
        
        if (!settingsOpen)
        {
            updateAudioLevels();
            blipSelect.Play();
            SettingsCanvas.SetActive(true);
            settingsOpen = true;
        }
        else
        {
            updateAudioLevels();
            blipSelect.Play();
            SettingsCanvas.SetActive(false);
            settingsOpen = false;
        }
    }

    public bool getSettingsOpen()
    {
        return settingsOpen;
    }

    public void openMenuSettings()
    {
        updateAudioLevels();
        blipSelect.Play();
        SettingsCanvas.SetActive(true);
    }

    public void closeSettings()
    {
        updateAudioLevels();
        blipSelect.Play();
        SettingsCanvas.SetActive(false);
    }

    // Slider Value for general sounds
    public void Volume_Slider(float volume)
    {
        float volumeValue = volume;
        if(SystemInfo.deviceType == DeviceType.Handheld){
            PlayerPrefs.SetFloat("VolumeValue", volumeValue * .04f);
        }
        else{
            PlayerPrefs.SetFloat("VolumeValue", volumeValue * .01f);
        }
        LoadValues();
    }

    // Slider Value for SFX
    public void Effects_Slider(float volume)
    {
        float effectsValue = volume;
        if(SystemInfo.deviceType == DeviceType.Handheld){
            PlayerPrefs.SetFloat("EffectsValue", effectsValue * .4f);
        }
        else{
            PlayerPrefs.SetFloat("EffectsValue", effectsValue * .2f);
        }
        LoadValues();
    }

    // Updates all audio levels
    private void updateAudioLevels()
    {
        for (int i = 0; i < audio_players.Length; i++)
        {
            
            audio_players[i].volume = PlayerPrefs.GetFloat("VolumeValue");
        }
        for (int i = 0; i < sfx_players.Length; i++)
        {
            
            sfx_players[i].volume = PlayerPrefs.GetFloat("EffectsValue");
        }
    }

    // Load the values from PlayerPref
    private void LoadValues()
    {
        float volumeValue = PlayerPrefs.GetFloat("VolumeValue");
        if(SystemInfo.deviceType == DeviceType.Handheld){
            VolumeSlider.value = volumeValue / .04f;
        }
        else{
            VolumeSlider.value = volumeValue / .01f;
        }

        float effectsValue = PlayerPrefs.GetFloat("EffectsValue");
        if(SystemInfo.deviceType == DeviceType.Handheld){
            EffectsSlider.value = effectsValue / .4f;
        }
        else{
            EffectsSlider.value = effectsValue / .2f;
        }

        updateAudioLevels();
    }

    // Load new defaults on first play
    private void LoadNewDefaults()
    {
        float volumeValue = .25f;
        VolumeSlider.value = volumeValue;

        float effectsValue = .25f;
        EffectsSlider.value = effectsValue;

        updateAudioLevels();
    }

    public void MainMenu()
    {
        updateAudioLevels();
        if(SceneManager.GetActiveScene().name == "infiniteLevel")
        {
            bs.cleanGame();
            shoot = GameObject.Find("Shooter").GetComponent<Shooter>();
            shoot.resetLocalScore();
        }
        blipSelect.Play();
        StartCoroutine(changeScene("Menu", blipSelect.clip.length));
    }

    IEnumerator changeScene(string scene, float time)
    {
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(scene);
    }
}
