using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButtonScript : MonoBehaviour
{
    public AudioSource blipSelect;
    public GameObject leaderBoardPanel;
    public GameObject achievementPanel;
    public GameObject areYouSurePanel;
    public GameObject leaderboardClose;
    public GameObject leaderboardClear;
    public Text highscoreText;
    public GameData gameData;
    public GameObject customPanel;
    private int tempHighScore;
    private void Awake() {
        gameData = SaveSystem.Load();
        tempHighScore = gameData.highScore;
        highscoreText.text = "Highscore: " + tempHighScore;
    }

    public void StartGame()
    {
        blipSelect.Play();
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            if (PlayerPrefs.GetFloat("FirstGame") == 0)
            {
                if (SystemInfo.deviceType == DeviceType.Handheld)
                {
                    StartCoroutine(changeScene("TutorialMobile", blipSelect.clip.length));
                }
                else
                {
                    StartCoroutine(changeScene("TutorialPC", blipSelect.clip.length));
                }
            }
            else
            {
                StartCoroutine(changeScene("infiniteLevel", blipSelect.clip.length));
            }
        }
        
    }

    IEnumerator changeScene(string scene, float time)
    {
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(scene);
    }

    public void ExitGame()
    {
        blipSelect.Play();
        StartCoroutine(exitGame(blipSelect.clip.length));
    }

    IEnumerator exitGame(float time)
    {
        yield return new WaitForSeconds(time);
        Application.Quit();
    }
    private void Update() {
        //highscoreText.text = "Highscore: " + tempHighScore;
    }
    public void leaderBoards(){
        blipSelect.Play();
        leaderBoardPanel.SetActive(true);
    }
    public void closeLeaderboards(){
        blipSelect.Play();
        leaderBoardPanel.SetActive(false);
    }
    public void resetHighScore(){
        blipSelect.Play();
        gameData.highScore = 0;
        tempHighScore = 0;
        highscoreText.text = "Highscore: " + tempHighScore;
        SaveSystem.Save(gameData);
    }

    public void dataAreYouSure()
    {
        blipSelect.Play();
        leaderboardClose.SetActive(false);
        leaderboardClear.SetActive(false);
        areYouSurePanel.SetActive(true);
    }

    public void yesClear()
    {
        blipSelect.Play();
        resetHighScore();
        leaderboardClose.SetActive(true);
        leaderboardClear.SetActive(true);
        areYouSurePanel.SetActive(false);
        gameData = SaveSystem.Load();
        tempHighScore = 0;
        highscoreText.text = "Highscore: " + tempHighScore;
    }

    public void noClear()
    {
        blipSelect.Play();
        leaderboardClose.SetActive(true);
        leaderboardClear.SetActive(true);
        areYouSurePanel.SetActive(false);
    }

    public void openAchievementMenu()
    {
        blipSelect.Play();
        achievementPanel.SetActive(true);
    }

    public void closeAchievementMenu()
    {
        blipSelect.Play();
        achievementPanel.SetActive(false);
    }

    public void openCustom()
    {
        blipSelect.Play();
        customPanel.SetActive(true);
    }

    public void closeCustom()
    {
        blipSelect.Play();
        customPanel.SetActive(false);
    }
}
