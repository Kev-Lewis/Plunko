using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButtonScript : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource blipSelect;

    [Header("Panels")]
    [SerializeField] private GameObject leaderBoardPanel;
    [SerializeField] private GameObject achievementPanel;
    [SerializeField] private GameObject areYouSurePanel;
    [SerializeField] private GameObject customPanel;

    [Header("Leaderboard Buttons")]
    [SerializeField] private GameObject leaderboardClose;
    [SerializeField] private GameObject leaderboardClear;

    [Header("Run Buttons")]
    [SerializeField] private GameObject continueRunButton;
    [SerializeField] private GameObject newRunButton;

    [Header("Text")]
    [SerializeField] private Text highscoreText;

    public GameData gameData;

    private int tempHighScore;

    private void Awake() {
        gameData = SaveSystem.Load();
        tempHighScore = gameData.highScore;

        UpdateHighScoreText();
        UpdateRunButtons();
    }

    private void UpdateRunButtons() {
        bool hasActiveRun = HasActiveRunSave();

        if (continueRunButton != null) {
            continueRunButton.SetActive(hasActiveRun);
        }

        if (newRunButton != null) {
            newRunButton.SetActive(true);
        }
    }

    private bool HasActiveRunSave() {
        RunSaveData runSaveData = SaveSystem.LoadRun();

        if (runSaveData == null) {
            return false;
        }

        return runSaveData.hasActiveRun && !runSaveData.gameOver;
    }

    public void StartGame() {
        PlaySelectSound();

        if (SceneManager.GetActiveScene().name != "Menu") {
            return;
        }

        if (PlayerPrefs.GetFloat("FirstGame") == 0) {
            LoadTutorialSceneAfterSound();
            return;
        }

        StartCoroutine(ChangeScene("infiniteLevel", GetSelectSoundLength()));
    }

    public void ContinueRun() {
        PlaySelectSound();

        if (!HasActiveRunSave()) {
            NewRun();
            return;
        }

        StartCoroutine(ChangeScene("infiniteLevel", GetSelectSoundLength()));
    }

    public void NewRun() {
        PlaySelectSound();
        SaveSystem.DeleteRunSave();
        StartCoroutine(ChangeScene("infiniteLevel", GetSelectSoundLength()));
    }

    private void LoadTutorialSceneAfterSound() {
        string tutorialScene = SystemInfo.deviceType == DeviceType.Handheld ? "TutorialMobile" : "TutorialPC";
        StartCoroutine(ChangeScene(tutorialScene, GetSelectSoundLength()));
    }

    public void ExitGame() {
        PlaySelectSound();
        StartCoroutine(ExitGameAfterDelay(GetSelectSoundLength()));
    }

    private IEnumerator ChangeScene(string scene, float delay) {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(scene);
    }

    private IEnumerator ExitGameAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        Application.Quit();
    }

    public void leaderBoards() {
        PlaySelectSound();
        SetPanelActive(leaderBoardPanel, true);
    }

    public void closeLeaderboards() {
        PlaySelectSound();
        SetPanelActive(leaderBoardPanel, false);
    }

    public void resetHighScore() {
        PlaySelectSound();

        if (gameData == null) {
            gameData = SaveSystem.Load();
        }

        gameData.highScore = 0;
        tempHighScore = 0;

        UpdateHighScoreText();
        SaveSystem.Save(gameData);
    }

    public void dataAreYouSure() {
        PlaySelectSound();
        SetClearConfirmPanel(true);
    }

    public void yesClear() {
        PlaySelectSound();

        resetHighScore();
        SetClearConfirmPanel(false);

        gameData = SaveSystem.Load();
        tempHighScore = 0;
        UpdateHighScoreText();
    }

    public void noClear() {
        PlaySelectSound();
        SetClearConfirmPanel(false);
    }

    public void openAchievementMenu() {
        PlaySelectSound();
        SetPanelActive(achievementPanel, true);
    }

    public void closeAchievementMenu() {
        PlaySelectSound();
        SetPanelActive(achievementPanel, false);
    }

    public void openCustom() {
        PlaySelectSound();
        SetPanelActive(customPanel, true);
    }

    public void closeCustom() {
        PlaySelectSound();
        SetPanelActive(customPanel, false);
    }

    private void SetClearConfirmPanel(bool active) {
        SetPanelActive(leaderboardClose, !active);
        SetPanelActive(leaderboardClear, !active);
        SetPanelActive(areYouSurePanel, active);
    }

    private void SetPanelActive(GameObject panel, bool active) {
        if (panel != null) {
            panel.SetActive(active);
        }
    }

    private void UpdateHighScoreText() {
        if (highscoreText != null) {
            highscoreText.text = "Highscore: " + tempHighScore;
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
}