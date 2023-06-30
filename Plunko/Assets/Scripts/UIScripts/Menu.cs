using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public GameObject menuProj;
    [SerializeField] Image imageLevelsCleared, imagePegsDestroyed, imageShotsFired, imageProjScore, imageTutorial;
    [SerializeField] Image imageLevelsClearedChild, imagePegsDestroyedChild, imageShotsFiredChild, imageProjScoreChild, imageTutorialChild;
    [SerializeField] Text levelsClearedText, pegsDestroyedText, shotsFiredText, projScoreText, tutorialText;
    public GameData gameData;
    public GameObject leaderboardClose, leaderboardClear, areYouSurePanel;
    public AudioSource blipSelect;
    private void Awake() {
        gameData = SaveSystem.Load();
    } 
    // Start is called before the first frame update
    void Start()
    {
        GameObject menu_meteor = Instantiate(menuProj, new Vector3(0 , 0 , 0), Quaternion.identity);
        menu_meteor.GetComponent<Rigidbody2D>().gravityScale = 0;
        Rigidbody2D rb = menu_meteor.GetComponent<Rigidbody2D>();
        rb.AddForce(new Vector2(Random.Range(-5, 5), Random.Range(-5, 5)), ForceMode2D.Impulse);
        
        levelsClearedChecker();
        pegsDestroyedChecker();
        shotFiredChecker();
        projScoreChecker();

        tutorialBeatenChecker();

        levelsClearedText.text = "Levels Cleared: " + gameData.totalLevels;
        pegsDestroyedText.text = "Pegs destroyed: " + gameData.totalPegs;
        shotsFiredText.text = "Shots Fired: " + gameData.shotsFired;
        projScoreText.text = "Highest Projectile score: " + gameData.highestProjScore;

    }
    private void Update() {
        levelsClearedChecker();
        pegsDestroyedChecker();
        shotFiredChecker();
        projScoreChecker();

        tutorialBeatenChecker();
    }
    void levelsClearedChecker(){
        if(gameData.totalLevels >= 5){
            imageLevelsCleared.color = new Color32(255,255,255,255);
            imageLevelsClearedChild.color = new Color32(255,255,255,255);
            levelsClearedText.enabled = true;
            if(gameData.totalLevels >= 5 && gameData.totalLevels < 25){
                //first color
                imageLevelsCleared.color = new Color32(205, 127, 50, 255);
            }
            else if(gameData.totalLevels >= 25 && gameData.totalLevels < 50){
                //second color
                imageLevelsCleared.color = new Color32(192, 192, 192, 255);
            }
            else if(gameData.totalLevels >= 50){
                //third color
                imageLevelsCleared.color = new Color32(255, 215, 0, 255);
            }
        }
        else{
            imageLevelsCleared.color = new Color32(0,20,85,255);
            imageLevelsClearedChild.color = new Color32(0,0,0,255);
            levelsClearedText.enabled = false;
        }
    }

    void pegsDestroyedChecker(){
        if(gameData.totalPegs >= 100){
            imagePegsDestroyed.color = new Color32(255,255,255,255);
            imagePegsDestroyedChild.color = new Color32(255,255,255,255);
            pegsDestroyedText.enabled = true;
            if(gameData.totalPegs >= 100 && gameData.totalPegs < 500){
                //first color
                imagePegsDestroyed.color = new Color32(205, 127, 50, 255);
            }
            else if(gameData.totalPegs>= 500 && gameData.totalPegs < 1000){
                //second color
                imagePegsDestroyed.color = new Color32(192, 192, 192, 255);
            }
            else if(gameData.totalPegs >= 1000){
                //third color
                imagePegsDestroyed.color = new Color32(255, 215, 0, 255);
            }
        }
        else{
            imagePegsDestroyed.color = new Color32(0,20,85,255);
            imagePegsDestroyedChild.color = new Color32(0,0,0,255);
            pegsDestroyedText.enabled = false;
        }
    }

    void shotFiredChecker(){
        if(gameData.shotsFired >= 10){
            imageShotsFired.color = new Color32(255,255,255,255);
            imageShotsFiredChild.color = new Color32(255,255,255,255);
            shotsFiredText.enabled = true;
            if(gameData.shotsFired >= 10 && gameData.shotsFired < 100){
                //first color
                imageShotsFired.color = new Color32(205, 127, 50, 255);
            }
            else if(gameData.shotsFired >= 100 && gameData.shotsFired < 250){
                //second color
                imageShotsFired.color = new Color32(192, 192, 192, 255);
            }
            else if(gameData.shotsFired >= 250){
                //third color
                imageShotsFired.color = new Color32(255, 215, 0, 255);
            }
        }
        else{
            imageShotsFired.color = new Color32(0,20,85,255);
            imageShotsFiredChild.color = new Color32(0,0,0,255);
            shotsFiredText.enabled = false;
        }
    }

    void projScoreChecker(){
        if(PlayerPrefs.GetInt("projectileScore1") != 0 || PlayerPrefs.GetInt("projectileScore2") != 0 || PlayerPrefs.GetInt("projectileScore3") != 0 ){
            imageProjScore.color = new Color32(255,255,255,255);
            imageProjScoreChild.color = new Color32(255,255,255,255);
            projScoreText.enabled = true;
            if(PlayerPrefs.GetInt("projectileScore1") != 0  && PlayerPrefs.GetInt("projectileScore2") == 0 ){
                //first color
                imageProjScore.color = new Color32(205, 127, 50, 255);
            }
            else if(PlayerPrefs.GetInt("projectileScore2") != 0 && PlayerPrefs.GetInt("projectileScore3") == 0 ){
                //second color
                imageProjScore.color = new Color32(192, 192, 192, 255);
            }
            else if(PlayerPrefs.GetInt("projectileScore3") != 0 ){
                //third color
                imageProjScore.color = new Color32(255, 215, 0, 255);
            }
        }
        else{
            imageProjScore.color = new Color32(0,20,85,255);
            imageProjScoreChild.color = new Color32(0,0,0,255);
            projScoreText.enabled = false;
        }
    }

    void tutorialBeatenChecker(){
        //check if tutorial has been beaten
        if(PlayerPrefs.GetFloat("FirstGame") == 1){
            imageTutorial.color = new Color32(255, 215, 0, 255);
            imageTutorialChild.color = new Color32(255,255,255,255);
            tutorialText.enabled = true;
        }else{
            imageTutorial.color = new Color32(0,20,85,255);
            imageTutorialChild.color = new Color32(0, 0, 0, 255);
            tutorialText.enabled = false;
        }
    }

    public void ClearSave(){
        //PlayerPrefs.SetFloat("FirstGame", 0);
        gameData.shotsFired = 0;
        gameData.totalLevels = 0;
        gameData.totalPegs = 0;
        gameData.highestProjScore = 0;
        PlayerPrefs.SetInt("pegsDestroyed1", 0);
        PlayerPrefs.SetInt("pegsDestroyed2", 0);
        PlayerPrefs.SetInt("pegsDestroyed3", 0);
        PlayerPrefs.SetInt("shotsDestroyed1", 0);
        PlayerPrefs.SetInt("shotsDestroyed2", 0);
        PlayerPrefs.SetInt("shotsDestroyed3", 0);
        PlayerPrefs.SetInt("levelsCleared1", 0);
        PlayerPrefs.SetInt("levelsCleared2", 0);
        PlayerPrefs.SetInt("levelsCleared3", 0);
        PlayerPrefs.SetInt("projectileScore1", 0);
        PlayerPrefs.SetInt("projectileScore2", 0);
        PlayerPrefs.SetInt("projectileScore3", 0);
        PlayerPrefs.SetFloat("FirstGame", 0);
        PlayerPrefs.SetInt("customShooter", 0);
        PlayerPrefs.SetInt("customProj", 0);
        PlayerPrefs.SetInt("customBucket", 0);
        PlayerPrefs.SetInt("customSlide", 0);
        PlayerPrefs.SetInt("customTrail", 0);
        gameData.highScore = 0;
        SaveSystem.Save(gameData);
        customizationButtons.resetStuff = true;
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
        ClearSave();
        leaderboardClose.SetActive(true);
        leaderboardClear.SetActive(true);
        areYouSurePanel.SetActive(false);
    }

    public void noClear()
    {
        blipSelect.Play();
        leaderboardClose.SetActive(true);
        leaderboardClear.SetActive(true);
        areYouSurePanel.SetActive(false);
    }
}
