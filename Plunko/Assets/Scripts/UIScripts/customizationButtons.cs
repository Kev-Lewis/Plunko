using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class customizationButtons : MonoBehaviour
{
    public Button shooter1;
    public GameObject shooter1_hl;
    public Button shooter2;
    public GameObject shooter2_hl;
    public Button proj1;
    public GameObject proj1_hl;
    public Button proj2;
    public GameObject proj2_hl;
    public Button bucket1;
    public GameObject bucket1_hl;
    public Button bucket2;
    public GameObject bucket2_hl;
    public Button trail1;
    public GameObject trail1_hl;
    public Button trail2;
    public GameObject trail2_hl;
    public Button slide1;
    public GameObject slide1_hl;
    public Button slide2;
    public GameObject slide2_hl;
    public AudioSource blipSelect;

    [SerializeField] private GameObject shooterButton;
    [SerializeField] private GameObject projButton;
    [SerializeField] private GameObject bucketButton;
    [SerializeField] private GameObject trailButton;
    [SerializeField] private GameObject slideButton;
    [HideInInspector] public static bool resetStuff;

    private void Awake() {
        checkAch();
        checkHighlights();
    }
    private void Update() {
        if(resetStuff){
            checkAch();
            checkHighlights();
            resetStuff = false;
        }
    }
    private void checkHighlights(){
        if(PlayerPrefs.GetInt("customShooter") != 1){
            shooter1_hl.SetActive(true);
            shooter2_hl.SetActive(false);
        }
        else{
            shooter2_hl.SetActive(true);
            shooter1_hl.SetActive(false);
        }

        if(PlayerPrefs.GetInt("customProj") != 1){
            proj1_hl.SetActive(true);
            proj2_hl.SetActive(false);
        }
        else{
            proj2_hl.SetActive(true);
            proj1_hl.SetActive(false);
        }

        if(PlayerPrefs.GetInt("customBucket") != 1){
            bucket1_hl.SetActive(true);
            bucket2_hl.SetActive(false);
        }
        else{
            bucket2_hl.SetActive(true);
            bucket1_hl.SetActive(false);
        }

        if(PlayerPrefs.GetInt("customSlide") != 1){
            slide1_hl.SetActive(true);
            slide2_hl.SetActive(false);
        }
        else{
            slide2_hl.SetActive(true);
            slide1_hl.SetActive(false);
        }

        if(PlayerPrefs.GetInt("customTrail") != 1){
            trail1_hl.SetActive(true);
            trail2_hl.SetActive(false);
        }
        else{
            trail2_hl.SetActive(true);
            trail1_hl.SetActive(false);
        }
    }

    private void checkAch(){
        if(PlayerPrefs.GetInt("pegsDestroyed3") == 0){
            shooterButton.transform.GetChild(0).GetComponent<Button>().interactable = false;
            shooterButton.transform.GetChild(0).transform.GetChild(1).GetComponent<Image>().color = Color.black;
        }
        if(PlayerPrefs.GetInt("pegsDestroyed3") == 1){
            //shooterButton.SetActive(true);
            shooterButton.transform.GetChild(0).GetComponent<Button>().interactable = true;
            shooterButton.transform.GetChild(0).transform.GetChild(1).GetComponent<Image>().color = Color.white;
        }

        if(PlayerPrefs.GetInt("shotsDestroyed3") == 0){
            //projButton.SetActive(false);
            projButton.transform.GetChild(0).GetComponent<Button>().interactable = false;
            projButton.transform.GetChild(0).transform.GetChild(1).GetComponent<Image>().color = Color.black;
        }
        if(PlayerPrefs.GetInt("shotsDestroyed3") == 1){
            //projButton.SetActive(true);
            projButton.transform.GetChild(0).GetComponent<Button>().interactable = true;
            projButton.transform.GetChild(0).transform.GetChild(1).GetComponent<Image>().color = Color.white;
        }

        if(PlayerPrefs.GetFloat("FirstGame") == 0){
            //trailButton.SetActive(false);
            trailButton.transform.GetChild(0).GetComponent<Button>().interactable = false;
            trailButton.transform.GetChild(0).transform.GetChild(1).GetComponent<Image>().color = Color.black;
        }
        if(PlayerPrefs.GetFloat("FirstGame") == 1){
            //trailButton.SetActive(true);
            trailButton.transform.GetChild(0).GetComponent<Button>().interactable = true;
            trailButton.transform.GetChild(0).transform.GetChild(1).GetComponent<Image>().color = Color.white;
        }

        if(PlayerPrefs.GetInt("levelsCleared3") == 0){
            //slideButton.SetActive(false);
            slideButton.transform.GetChild(0).GetComponent<Button>().interactable = false;
            slideButton.transform.GetChild(0).transform.GetChild(1).GetComponent<Image>().color = Color.black;
        }
        if(PlayerPrefs.GetInt("levelsCleared3") == 1){
            //slideButton.SetActive(true);
            slideButton.transform.GetChild(0).GetComponent<Button>().interactable = true;
            slideButton.transform.GetChild(0).transform.GetChild(1).GetComponent<Image>().color = Color.white;
        }

        if(PlayerPrefs.GetInt("projectileScore3") == 0){
            //bucketButton.SetActive(false);
            bucketButton.transform.GetChild(0).GetComponent<Button>().interactable = false;
            bucketButton.transform.GetChild(0).transform.GetChild(1).GetComponent<Image>().color = Color.black;
        }
        if(PlayerPrefs.GetInt("projectileScore3") == 1){
            //bucketButton.SetActive(true);
            bucketButton.transform.GetChild(0).GetComponent<Button>().interactable = true;
            bucketButton.transform.GetChild(0).transform.GetChild(1).GetComponent<Image>().color = Color.white;
        }
    }

    public void revertShooter(){
        PlayerPrefs.SetInt("customShooter", 0);
        shooter1.enabled = false;
        shooter2.enabled = true;
        shooter1_hl.SetActive(true);
        shooter2_hl.SetActive(false);
        blipSelect.Play();
    }
    public void changeShooter(){
        PlayerPrefs.SetInt("customShooter", 1);
        shooter1.enabled = true;
        shooter2.enabled = false;
        shooter1_hl.SetActive(false);
        shooter2_hl.SetActive(true);
        blipSelect.Play();
    }
    
    public void revertProj(){
        PlayerPrefs.SetInt("customProj", 0);
        proj1.enabled = false;
        proj2.enabled = true;
        proj1_hl.SetActive(true);
        proj2_hl.SetActive(false);
        blipSelect.Play();
    }
    public void changeProj(){
        PlayerPrefs.SetInt("customProj", 1);
        proj1.enabled = true;
        proj2.enabled = false;
        proj1_hl.SetActive(false);
        proj2_hl.SetActive(true);
        blipSelect.Play();
    }

    public void revertBucket(){
        PlayerPrefs.SetInt("customBucket", 0);
        bucket1.enabled = false;
        bucket2.enabled = true;
        bucket1_hl.SetActive(true);
        bucket2_hl.SetActive(false);
        blipSelect.Play();
    }
    public void changeBucket(){
        PlayerPrefs.SetInt("customBucket", 1);
        bucket1.enabled = true;
        bucket2.enabled = false;
        bucket1_hl.SetActive(false);
        bucket2_hl.SetActive(true);
        blipSelect.Play();
    }

    public void revertSlide(){
        PlayerPrefs.SetInt("customSlide", 0);
        slide1.enabled = false;
        slide2.enabled = true;
        slide1_hl.SetActive(true);
        slide2_hl.SetActive(false);
        blipSelect.Play();
    }
    public void changeSlide(){
        PlayerPrefs.SetInt("customSlide", 1);
        slide1.enabled = true;
        slide2.enabled = false;
        slide1_hl.SetActive(false);
        slide2_hl.SetActive(true);
        blipSelect.Play();
    }

    public void revertTrail(){
        PlayerPrefs.SetInt("customTrail", 0);
        trail1.enabled = false;
        trail2.enabled = true;
        trail1_hl.SetActive(true);
        trail2_hl.SetActive(false);
        blipSelect.Play();
    }
    public void changeTrail(){
        PlayerPrefs.SetInt("customTrail", 1);
        trail1.enabled = true;
        trail2.enabled = false;
        trail1_hl.SetActive(false);
        trail2_hl.SetActive(true);
        blipSelect.Play();
    }
}
