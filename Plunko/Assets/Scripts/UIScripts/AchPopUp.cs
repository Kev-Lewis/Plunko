using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchPopUp : MonoBehaviour
{
    [SerializeField] private GameObject achPanel;
    [SerializeField] private Image achImage;
    [SerializeField] private Sprite oneSmallStep;
    [SerializeField] private Sprite SpaceCadetT1, SpaceCadetT2, SpaceCadetT3;
    [SerializeField] private Sprite ExplorerT1, ExplorerT2, ExplorerT3;
    [SerializeField] private Sprite DestroyerT1, DestroyerT2, DestroyerT3;
    [SerializeField] private Sprite AchieverT1,AchieverT2, AchieverT3;
    private AudioSource audioClip;
    public GameData gameData;
    private bool popUpPlaying;
    
    private void Start() {
        //gameData = SaveSystem.Load();
        popUpPlaying = false;
        //achPanel.SetActive(true);
        //StartCoroutine(achFadeIn());
    }
    // Update is called once per frame
    public void checkForAchPopUp()
    {
        gameData = SaveSystem.Load(); 
        levelsClearedChecker();
        projScoreChecker();
    }

    void Update()
    {
        if (PlayerPrefs.GetInt("pegsDestroyed3") != 1)
        {
            gameData = SaveSystem.Load();
            pegsDestroyedChecker();
        }
        if (PlayerPrefs.GetInt("shotsDestroyed3") != 1)
        {
            gameData = SaveSystem.Load();
            shotFiredChecker();
        }
        if (PlayerPrefs.GetInt("levelsCleared3") != 1)
        {
            gameData = SaveSystem.Load();
            levelsClearedChecker();
        }
        if (PlayerPrefs.GetInt("projectileScore3") != 1)
        {
            gameData = SaveSystem.Load();
            projScoreChecker();
        }
    }

    IEnumerator achFadeIn()
    {
        //gameData = SaveSystem.Load();
        audioClip = GameObject.Find("level_clear").GetComponent<AudioSource>();
        audioClip.Play();
        achPanel.GetComponent<Image>().color = new Color(achPanel.GetComponent<Image>().color.r, achPanel.GetComponent<Image>().color.g, achPanel.GetComponent<Image>().color.b, 0);
        achImage.color = new Color(achImage.color.r, achImage.color.g, achImage.color.b, 0);
        while (achPanel.GetComponent<Image>().color.a < 1.0f)
        {
            achPanel.GetComponent<Image>().color = new Color(achPanel.GetComponent<Image>().color.r, achPanel.GetComponent<Image>().color.g, achPanel.GetComponent<Image>().color.b, achPanel.GetComponent<Image>().color.a + (Time.deltaTime / 1f));
            achImage.color = new Color(achImage.color.r, achImage.color.g, achImage.color.b, achPanel.GetComponent<Image>().color.a);
            yield return null;
        }

        StartCoroutine(achFadeOut());
    }

    IEnumerator achFadeOut()
    {
        achPanel.GetComponent<Image>().color = new Color(achPanel.GetComponent<Image>().color.r, achPanel.GetComponent<Image>().color.g, achPanel.GetComponent<Image>().color.b, 1);
        achImage.color = new Color(achImage.color.r, achImage.color.g, achImage.color.b, 1);
        while (achPanel.GetComponent<Image>().color.a > 0.0f)
        {
            achPanel.GetComponent<Image>().color = new Color(achPanel.GetComponent<Image>().color.r, achPanel.GetComponent<Image>().color.g, achPanel.GetComponent<Image>().color.b, achPanel.GetComponent<Image>().color.a - (Time.deltaTime / 3f));
            achImage.color = new Color(achImage.color.r, achImage.color.g, achImage.color.b, achPanel.GetComponent<Image>().color.a);
            yield return null;
        }
        achPanel.SetActive(false);
        popUpPlaying = false;
    }

    public void tutorialAch()
    {
        if (PlayerPrefs.GetFloat("FirstGame") == 0)
        {
            PlayerPrefs.SetFloat("FirstGame", 1);
            achPanel.SetActive(true);
            achImage.sprite = oneSmallStep;
            popUpPlaying = true;
            StartCoroutine(achFadeIn());
        }
    }

    void levelsClearedChecker(){
        if(gameData.totalLevels >= 5){
            //achBoarder.color = new Color32(255,255,255,255);

            if(gameData.totalLevels >= 5 && gameData.totalLevels < 25 && !popUpPlaying)
            {
                //first color
                
                if(PlayerPrefs.GetInt("levelsCleared1") == 0)
                {
                    PlayerPrefs.SetInt("levelsCleared1", 1);
                    achPanel.SetActive(true);
                    achImage.sprite = ExplorerT1;
                    popUpPlaying = true;
                    StartCoroutine(achFadeIn());
                }
            }
            else if(gameData.totalLevels >= 25 && gameData.totalLevels < 50 && !popUpPlaying)
            {
                //second color
                
                if(PlayerPrefs.GetInt("levelsCleared2") == 0)
                {
                    PlayerPrefs.SetInt("levelsCleared2", 1);
                    achPanel.SetActive(true);
                    achImage.sprite = ExplorerT2;
                    popUpPlaying = true;
                    StartCoroutine(achFadeIn());
                }
            }
            else if(gameData.totalLevels >= 50 && !popUpPlaying)
            {
                //third color
                
                if(PlayerPrefs.GetInt("levelsCleared3") == 0)
                {
                    PlayerPrefs.SetInt("levelsCleared3", 1);
                    achPanel.SetActive(true);
                    achImage.sprite = ExplorerT3;
                    popUpPlaying = true;
                    StartCoroutine(achFadeIn());
                }
            }
             
        }
        /*
        else{
            achBoarder.color = new Color32(0,0,0,0);
            achTitle.enabled = false;
        }
        */
    }

    void pegsDestroyedChecker(){
        if(gameData.totalPegs >= 100){
            //achBoarder.color = new Color32(255,255,255,255);
            
            if(gameData.totalPegs >= 100 && gameData.totalPegs < 500 && !popUpPlaying)
            {
                //first color
                
                if(PlayerPrefs.GetInt("pegsDestroyed1") == 0){
                    PlayerPrefs.SetInt("pegsDestroyed1", 1);
                    achPanel.SetActive(true);
                    achImage.sprite = DestroyerT1;
                    popUpPlaying = true;
                    StartCoroutine(achFadeIn());
                }
            }
            else if(gameData.totalPegs>= 500 && gameData.totalPegs < 1000 && !popUpPlaying)
            {
                //second color
                
                if(PlayerPrefs.GetInt("pegsDestroyed2") == 0)
                {
                    PlayerPrefs.SetInt("pegsDestroyed2", 1);
                    achPanel.SetActive(true);
                    achImage.sprite = DestroyerT2;
                    popUpPlaying = true;
                    StartCoroutine(achFadeIn());
                }
            }
            else if(gameData.totalPegs >= 1000 && !popUpPlaying)
            {
                //third color
                
                if(PlayerPrefs.GetInt("pegsDestroyed3") == 0)
                {
                    PlayerPrefs.SetInt("pegsDestroyed3", 1);
                    achPanel.SetActive(true);
                    achImage.sprite = DestroyerT3;
                    popUpPlaying = true;
                    StartCoroutine(achFadeIn());
                }
            }
            //StartCoroutine(achFadeIn());
        }
        /*
        else{
            achBoarder.color = new Color32(0,0,0,0);
            achTitle.enabled = false;
        }
        */
    }

    void shotFiredChecker(){
        if(gameData.shotsFired >= 10){
            //achBoarder.color = new Color32(255,255,255,255);
            
            if(gameData.shotsFired >= 10 && gameData.shotsFired < 100 && !popUpPlaying)
            {
                //first color
                
                if(PlayerPrefs.GetInt("shotsDestroyed1") == 0)
                {
                    PlayerPrefs.SetInt("shotsDestroyed1", 1);
                    achPanel.SetActive(true);
                    achImage.sprite = SpaceCadetT1;
                    popUpPlaying = true;
                    StartCoroutine(achFadeIn());
                }
            }
            else if(gameData.shotsFired >= 100 && gameData.shotsFired < 250 && !popUpPlaying)
            {
                //second color
                
                if(PlayerPrefs.GetInt("shotsDestroyed2") == 0)
                {
                    PlayerPrefs.SetInt("shotsDestroyed2", 1);
                    achPanel.SetActive(true);
                    achImage.sprite = SpaceCadetT2;
                    popUpPlaying = true;
                    StartCoroutine(achFadeIn());
                }
            }
            else if(gameData.shotsFired >= 250 && !popUpPlaying)
            {
                //third color
                
                if(PlayerPrefs.GetInt("shotsDestroyed3") == 0){
                    PlayerPrefs.SetInt("shotsDestroyed3", 1);
                    achPanel.SetActive(true);
                    achImage.sprite = SpaceCadetT3;
                    popUpPlaying = true;
                    StartCoroutine(achFadeIn());
                }
            }
            //StartCoroutine(achFadeIn());
        }
        /*
        else{
            achBoarder.color = new Color32(0,0,0,0);
            achTitle.enabled = false;
        }
        */
    }

    void projScoreChecker(){
        
        //if(gameData.highestProjScore >= 10){
            //achBoarder.color = new Color32(255,255,255,255);
            
            if(gameData.highestProjScore >= 100 && gameData.highestProjScore < 500 && PlayerPrefs.GetInt("projectileScore1") == 0 && !popUpPlaying)
        {
                //first color
                
                //if(){
                    PlayerPrefs.SetInt("projectileScore1", 1);
                    achPanel.SetActive(true);
                    achImage.sprite = AchieverT1;
                    popUpPlaying = true;   
                    StartCoroutine(achFadeIn());
                    
                //}
            }
            else if(gameData.highestProjScore >= 500 && gameData.highestProjScore < 1000 && PlayerPrefs.GetInt("projectileScore1") == 1 && !popUpPlaying)
        {
                //second color
                
                if(PlayerPrefs.GetInt("projectileScore2") == 0){
                PlayerPrefs.SetInt("projectileScore2", 1);
                    achPanel.SetActive(true);
                    achImage.sprite = AchieverT2;
                    popUpPlaying = true;
                    StartCoroutine(achFadeIn());
                    
                }
            }
            else if(gameData.highestProjScore >= 1000 && PlayerPrefs.GetInt("projectileScore2") == 1 && !popUpPlaying)
        {
                //third color
                
                if(PlayerPrefs.GetInt("projectileScore3") == 0){
                PlayerPrefs.SetInt("projectileScore3", 1);
                    achPanel.SetActive(true);
                    achImage.sprite = AchieverT3;
                    popUpPlaying = true;
                    StartCoroutine(achFadeIn());
                    
                }
            }
            //StartCoroutine(achFadeIn());
        //}
        /*
        else{
            achBoarder.color = new Color32(0,0,0,0);
            achTitle.enabled = false;
        }
        */
    }
}
