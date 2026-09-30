using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    private enum MedalTier
    {
        Locked,
        Bronze,
        Silver,
        Gold
    }

    [Header("Menu Projectile")]
    [SerializeField] private GameObject menuProj;

    [Header("Achievement Images")]
    [SerializeField] private Image imageLevelsCleared;
    [SerializeField] private Image imagePegsDestroyed;
    [SerializeField] private Image imageShotsFired;
    [SerializeField] private Image imageProjScore;
    [SerializeField] private Image imageTutorial;

    [Header("Achievement Child Images")]
    [SerializeField] private Image imageLevelsClearedChild;
    [SerializeField] private Image imagePegsDestroyedChild;
    [SerializeField] private Image imageShotsFiredChild;
    [SerializeField] private Image imageProjScoreChild;
    [SerializeField] private Image imageTutorialChild;

    [Header("Achievement Text")]
    [SerializeField] private Text levelsClearedText;
    [SerializeField] private Text pegsDestroyedText;
    [SerializeField] private Text shotsFiredText;
    [SerializeField] private Text projScoreText;
    [SerializeField] private Text tutorialText;

    [Header("Leaderboard UI")]
    [SerializeField] private GameObject leaderboardClose;
    [SerializeField] private GameObject leaderboardClear;
    [SerializeField] private GameObject areYouSurePanel;

    [Header("Run Buttons")]
    [SerializeField] private GameObject continueRunButton;
    [SerializeField] private GameObject newRunButton;

    [Header("Audio")]
    [SerializeField] private AudioSource blipSelect;

    public GameData gameData;

    private readonly Color32 lockedMainColor = new Color32(0, 20, 85, 255);
    private readonly Color32 lockedChildColor = new Color32(0, 0, 0, 255);
    private readonly Color32 unlockedChildColor = new Color32(255, 255, 255, 255);
    private readonly Color32 bronzeColor = new Color32(205, 127, 50, 255);
    private readonly Color32 silverColor = new Color32(192, 192, 192, 255);
    private readonly Color32 goldColor = new Color32(255, 215, 0, 255);

    private void Awake() {
        gameData = SaveSystem.Load();
    }

    private void Start() {
        SpawnMenuProjectile();
        RefreshMenu();
    }

    private void SpawnMenuProjectile() {
        if (menuProj == null) {
            return;
        }

        GameObject menuMeteor = Instantiate(menuProj, Vector3.zero, Quaternion.identity);
        Rigidbody2D rb = menuMeteor.GetComponent<Rigidbody2D>();

        if (rb == null) {
            return;
        }

        rb.gravityScale = 0f;
        rb.AddForce(new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f)), ForceMode2D.Impulse);
    }

    private void RefreshMenu() {
        gameData = SaveSystem.Load();

        if (gameData == null) {
            return;
        }

        UpdateAchievementDisplays();
        UpdateStatText();
        UpdateRunButtons();
    }

    private void UpdateAchievementDisplays() {
        SetAchievementDisplay(
            imageLevelsCleared,
            imageLevelsClearedChild,
            levelsClearedText,
            GetTierFromValue(gameData.totalLevels, 5, 25, 50)
        );

        SetAchievementDisplay(
            imagePegsDestroyed,
            imagePegsDestroyedChild,
            pegsDestroyedText,
            GetTierFromValue(gameData.totalPegs, 100, 500, 1000)
        );

        SetAchievementDisplay(
            imageShotsFired,
            imageShotsFiredChild,
            shotsFiredText,
            GetTierFromValue(gameData.shotsFired, 10, 100, 250)
        );

        SetAchievementDisplay(
            imageProjScore,
            imageProjScoreChild,
            projScoreText,
            GetProjectileScoreTier()
        );

        SetAchievementDisplay(
            imageTutorial,
            imageTutorialChild,
            tutorialText,
            PlayerPrefs.GetFloat("FirstGame") == 1 ? MedalTier.Gold : MedalTier.Locked
        );
    }

    private MedalTier GetTierFromValue(int value, int bronzeThreshold, int silverThreshold, int goldThreshold) {
        if (value >= goldThreshold) {
            return MedalTier.Gold;
        }

        if (value >= silverThreshold) {
            return MedalTier.Silver;
        }

        if (value >= bronzeThreshold) {
            return MedalTier.Bronze;
        }

        return MedalTier.Locked;
    }

    private MedalTier GetProjectileScoreTier() {
        if (PlayerPrefs.GetInt("projectileScore3") != 0) {
            return MedalTier.Gold;
        }

        if (PlayerPrefs.GetInt("projectileScore2") != 0) {
            return MedalTier.Silver;
        }

        if (PlayerPrefs.GetInt("projectileScore1") != 0) {
            return MedalTier.Bronze;
        }

        return MedalTier.Locked;
    }

    private void SetAchievementDisplay(Image mainImage, Image childImage, Text text, MedalTier tier) {
        bool unlocked = tier != MedalTier.Locked;

        if (mainImage != null) {
            mainImage.color = GetTierColor(tier);
        }

        if (childImage != null) {
            childImage.color = unlocked ? unlockedChildColor : lockedChildColor;
        }

        if (text != null) {
            text.enabled = unlocked;
        }
    }

    private Color32 GetTierColor(MedalTier tier) {
        switch (tier) {
            case MedalTier.Bronze:
                return bronzeColor;
            case MedalTier.Silver:
                return silverColor;
            case MedalTier.Gold:
                return goldColor;
            default:
                return lockedMainColor;
        }
    }

    private void UpdateStatText() {
        if (levelsClearedText != null) {
            levelsClearedText.text = "Levels Cleared: " + gameData.totalLevels;
        }

        if (pegsDestroyedText != null) {
            pegsDestroyedText.text = "Pegs Destroyed: " + gameData.totalPegs;
        }

        if (shotsFiredText != null) {
            shotsFiredText.text = "Shots Fired: " + gameData.shotsFired;
        }

        if (projScoreText != null) {
            projScoreText.text = "Highest Projectile Score: " + gameData.highestProjScore;
        }
    }

    private void UpdateRunButtons() {
        bool hasActiveRun = SaveSystem.HasRunSave();

        RunSaveData runSaveData = SaveSystem.LoadRun();
        if (runSaveData != null) {
            hasActiveRun = runSaveData.hasActiveRun && !runSaveData.gameOver;
        }

        if (continueRunButton != null) {
            continueRunButton.SetActive(hasActiveRun);
        }

        if (newRunButton != null) {
            newRunButton.SetActive(true);
        }
    }

    public void ContinueRun() {
        PlaySelectSound();
        SceneManager.LoadScene("infiniteLevel");
    }

    public void NewRun() {
        PlaySelectSound();
        SaveSystem.DeleteRunSave();
        SceneManager.LoadScene("infiniteLevel");
    }

    public void ClearSave() {
        if (gameData == null) {
            gameData = SaveSystem.Load();
        }

        gameData.shotsFired = 0;
        gameData.totalLevels = 0;
        gameData.totalPegs = 0;
        gameData.highestProjScore = 0;
        gameData.highScore = 0;

        ClearAchievementPrefs();
        ClearCustomizationPrefs();

        PlayerPrefs.SetFloat("FirstGame", 0);
        PlayerPrefs.Save();

        SaveSystem.Save(gameData);
        SaveSystem.DeleteRunSave();

        customizationButtons.resetStuff = true;
        RefreshMenu();
    }

    private void ClearAchievementPrefs() {
        string[] achievementKeys = {
            "pegsDestroyed1",
            "pegsDestroyed2",
            "pegsDestroyed3",
            "shotsDestroyed1",
            "shotsDestroyed2",
            "shotsDestroyed3",
            "levelsCleared1",
            "levelsCleared2",
            "levelsCleared3",
            "projectileScore1",
            "projectileScore2",
            "projectileScore3"
        };

        for (int i = 0; i < achievementKeys.Length; i++) {
            PlayerPrefs.SetInt(achievementKeys[i], 0);
        }
    }

    private void ClearCustomizationPrefs() {
        string[] customizationKeys = {
            "customShooter",
            "customProj",
            "customBucket",
            "customSlide",
            "customTrail"
        };

        for (int i = 0; i < customizationKeys.Length; i++) {
            PlayerPrefs.SetInt(customizationKeys[i], 0);
        }
    }

    public void dataAreYouSure() {
        PlaySelectSound();
        SetClearConfirmPanel(true);
    }

    public void yesClear() {
        PlaySelectSound();
        ClearSave();
        SetClearConfirmPanel(false);
    }

    public void noClear() {
        PlaySelectSound();
        SetClearConfirmPanel(false);
    }

    private void SetClearConfirmPanel(bool active) {
        if (leaderboardClose != null) {
            leaderboardClose.SetActive(!active);
        }

        if (leaderboardClear != null) {
            leaderboardClear.SetActive(!active);
        }

        if (areYouSurePanel != null) {
            areYouSurePanel.SetActive(active);
        }
    }

    private void PlaySelectSound() {
        if (blipSelect != null) {
            blipSelect.Play();
        }
    }
}