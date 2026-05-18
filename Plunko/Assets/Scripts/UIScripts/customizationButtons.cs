using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class customizationButtons : MonoBehaviour
{
    [Header("Shooter")]
    public Button shooter1;
    public GameObject shooter1_hl;
    public Button shooter2;
    public GameObject shooter2_hl;

    [Header("Projectile")]
    public Button proj1;
    public GameObject proj1_hl;
    public Button proj2;
    public GameObject proj2_hl;

    [Header("Bucket")]
    public Button bucket1;
    public GameObject bucket1_hl;
    public Button bucket2;
    public GameObject bucket2_hl;

    [Header("Trail")]
    public Button trail1;
    public GameObject trail1_hl;
    public Button trail2;
    public GameObject trail2_hl;

    [Header("Slide")]
    public Button slide1;
    public GameObject slide1_hl;
    public Button slide2;
    public GameObject slide2_hl;

    [Header("Category Buttons")]
    [SerializeField] private GameObject shooterButton;
    [SerializeField] private GameObject projButton;
    [SerializeField] private GameObject bucketButton;
    [SerializeField] private GameObject trailButton;
    [SerializeField] private GameObject slideButton;

    [Header("Audio")]
    [SerializeField] private AudioSource blipSelect;

    [HideInInspector] public static bool resetStuff;

    private void Awake() {
        RefreshCustomizationMenu();
    }

    private void Update() {
        if (!resetStuff) {
            return;
        }

        RefreshCustomizationMenu();
        resetStuff = false;
    }

    private void RefreshCustomizationMenu() {
        CheckAchievementUnlocks();
        CheckHighlights();
    }

    private void CheckHighlights() {
        SetCustomizationSelection("customShooter", shooter1, shooter1_hl, shooter2, shooter2_hl);
        SetCustomizationSelection("customProj", proj1, proj1_hl, proj2, proj2_hl);
        SetCustomizationSelection("customBucket", bucket1, bucket1_hl, bucket2, bucket2_hl);
        SetCustomizationSelection("customTrail", trail1, trail1_hl, trail2, trail2_hl);
        SetCustomizationSelection("customSlide", slide1, slide1_hl, slide2, slide2_hl);
    }

    private void SetCustomizationSelection(string playerPrefsKey, Button defaultButton, GameObject defaultHighlight, Button customButton, GameObject customHighlight) {
        bool customSelected = PlayerPrefs.GetInt(playerPrefsKey) == 1;

        if (defaultHighlight != null) {
            defaultHighlight.SetActive(!customSelected);
        }

        if (customHighlight != null) {
            customHighlight.SetActive(customSelected);
        }

        if (defaultButton != null) {
            defaultButton.interactable = customSelected;
        }

        if (customButton != null) {
            customButton.interactable = !customSelected;
        }
    }

    private void CheckAchievementUnlocks() {
        SetCategoryUnlocked(shooterButton, PlayerPrefs.GetInt("pegsDestroyed3") == 1);
        SetCategoryUnlocked(projButton, PlayerPrefs.GetInt("shotsDestroyed3") == 1);
        SetCategoryUnlocked(bucketButton, PlayerPrefs.GetInt("projectileScore3") == 1);
        SetCategoryUnlocked(trailButton, PlayerPrefs.GetFloat("FirstGame") == 1);
        SetCategoryUnlocked(slideButton, PlayerPrefs.GetInt("levelsCleared3") == 1);
    }

    private void SetCategoryUnlocked(GameObject categoryButton, bool unlocked) {
        if (categoryButton == null || categoryButton.transform.childCount == 0) {
            return;
        }

        Transform buttonTransform = categoryButton.transform.GetChild(0);

        Button button = buttonTransform.GetComponent<Button>();
        if (button != null) {
            button.interactable = unlocked;
        }

        if (buttonTransform.childCount <= 1) {
            return;
        }

        Image iconImage = buttonTransform.GetChild(1).GetComponent<Image>();
        if (iconImage != null) {
            iconImage.color = unlocked ? Color.white : Color.black;
        }
    }

    private void SetCustomizationValue(string playerPrefsKey, int value) {
        PlayerPrefs.SetInt(playerPrefsKey, value);
        PlayerPrefs.Save();

        CheckHighlights();
        PlaySelectSound();
    }

    private void PlaySelectSound() {
        if (blipSelect != null) {
            blipSelect.Play();
        }
    }

    public void revertShooter() {
        SetCustomizationValue("customShooter", 0);
    }

    public void changeShooter() {
        SetCustomizationValue("customShooter", 1);
    }

    public void revertProj() {
        SetCustomizationValue("customProj", 0);
    }

    public void changeProj() {
        SetCustomizationValue("customProj", 1);
    }

    public void revertBucket() {
        SetCustomizationValue("customBucket", 0);
    }

    public void changeBucket() {
        SetCustomizationValue("customBucket", 1);
    }

    public void revertSlide() {
        SetCustomizationValue("customSlide", 0);
    }

    public void changeSlide() {
        SetCustomizationValue("customSlide", 1);
    }

    public void revertTrail() {
        SetCustomizationValue("customTrail", 0);
    }

    public void changeTrail() {
        SetCustomizationValue("customTrail", 1);
    }
}