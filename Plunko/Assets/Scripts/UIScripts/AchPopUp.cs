using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchPopUp : MonoBehaviour
{
    private enum AchievementStatType
    {
        TotalLevels,
        TotalPegs,
        ShotsFired,
        HighestProjectileScore
    }

    private class AchievementDefinition
    {
        public string playerPrefsKey;
        public AchievementStatType statType;
        public int threshold;
        public Sprite sprite;

        public AchievementDefinition(string playerPrefsKey, AchievementStatType statType, int threshold, Sprite sprite) {
            this.playerPrefsKey = playerPrefsKey;
            this.statType = statType;
            this.threshold = threshold;
            this.sprite = sprite;
        }
    }

    [Header("UI")]
    [SerializeField] private GameObject achPanel;
    [SerializeField] private Image achImage;

    [Header("General Achievements")]
    [SerializeField] private Sprite oneSmallStep;

    [Header("Shots Fired Achievements")]
    [SerializeField] private Sprite SpaceCadetT1;
    [SerializeField] private Sprite SpaceCadetT2;
    [SerializeField] private Sprite SpaceCadetT3;

    [Header("Levels Cleared Achievements")]
    [SerializeField] private Sprite ExplorerT1;
    [SerializeField] private Sprite ExplorerT2;
    [SerializeField] private Sprite ExplorerT3;

    [Header("Pegs Destroyed Achievements")]
    [SerializeField] private Sprite DestroyerT1;
    [SerializeField] private Sprite DestroyerT2;
    [SerializeField] private Sprite DestroyerT3;

    [Header("Projectile Score Achievements")]
    [SerializeField] private Sprite AchieverT1;
    [SerializeField] private Sprite AchieverT2;
    [SerializeField] private Sprite AchieverT3;

    [Header("Check Settings")]
    [SerializeField] private float automaticCheckInterval = 0.5f;

    public GameData gameData;

    private Image achPanelImage;
    private AudioSource achievementAudio;
    private bool popUpPlaying;
    private float checkTimer;

    private readonly List<AchievementDefinition> achievements = new List<AchievementDefinition>();
    private readonly Queue<Sprite> achievementQueue = new Queue<Sprite>();

    private void Start() {
        CacheReferences();
        BuildAchievementList();

        popUpPlaying = false;
        checkTimer = automaticCheckInterval;

        if (achPanel != null) {
            achPanel.SetActive(false);
        }
    }

    private void Update() {
        checkTimer -= Time.deltaTime;

        if (checkTimer > 0f) {
            return;
        }

        checkTimer = automaticCheckInterval;
        checkForAchPopUp();
    }

    private void CacheReferences() {
        if (achPanel != null) {
            achPanelImage = achPanel.GetComponent<Image>();
        }

        GameObject audioObject = GameObject.Find("level_clear");
        if (audioObject != null) {
            achievementAudio = audioObject.GetComponent<AudioSource>();
        }
    }

    private void BuildAchievementList() {
        achievements.Clear();

        achievements.Add(new AchievementDefinition("levelsCleared1", AchievementStatType.TotalLevels, 5, ExplorerT1));
        achievements.Add(new AchievementDefinition("levelsCleared2", AchievementStatType.TotalLevels, 25, ExplorerT2));
        achievements.Add(new AchievementDefinition("levelsCleared3", AchievementStatType.TotalLevels, 50, ExplorerT3));

        achievements.Add(new AchievementDefinition("pegsDestroyed1", AchievementStatType.TotalPegs, 100, DestroyerT1));
        achievements.Add(new AchievementDefinition("pegsDestroyed2", AchievementStatType.TotalPegs, 500, DestroyerT2));
        achievements.Add(new AchievementDefinition("pegsDestroyed3", AchievementStatType.TotalPegs, 1000, DestroyerT3));

        achievements.Add(new AchievementDefinition("shotsDestroyed1", AchievementStatType.ShotsFired, 10, SpaceCadetT1));
        achievements.Add(new AchievementDefinition("shotsDestroyed2", AchievementStatType.ShotsFired, 100, SpaceCadetT2));
        achievements.Add(new AchievementDefinition("shotsDestroyed3", AchievementStatType.ShotsFired, 250, SpaceCadetT3));

        achievements.Add(new AchievementDefinition("projectileScore1", AchievementStatType.HighestProjectileScore, 100, AchieverT1));
        achievements.Add(new AchievementDefinition("projectileScore2", AchievementStatType.HighestProjectileScore, 500, AchieverT2));
        achievements.Add(new AchievementDefinition("projectileScore3", AchievementStatType.HighestProjectileScore, 1000, AchieverT3));
    }

    public void checkForAchPopUp() {
        gameData = SaveSystem.Load();

        if (gameData == null) {
            return;
        }

        for (int i = 0; i < achievements.Count; i++) {
            CheckAchievement(achievements[i]);
        }

        PlayerPrefs.Save();

        if (!popUpPlaying) {
            ShowNextQueuedAchievement();
        }
    }

    private void CheckAchievement(AchievementDefinition achievement) {
        if (achievement == null || achievement.sprite == null) {
            return;
        }

        if (PlayerPrefs.GetInt(achievement.playerPrefsKey) == 1) {
            return;
        }

        int statValue = GetAchievementStatValue(achievement.statType);

        if (statValue < achievement.threshold) {
            return;
        }

        PlayerPrefs.SetInt(achievement.playerPrefsKey, 1);
        achievementQueue.Enqueue(achievement.sprite);
    }

    private int GetAchievementStatValue(AchievementStatType statType) {
        if (gameData == null) {
            return 0;
        }

        switch (statType) {
            case AchievementStatType.TotalLevels:
                return gameData.totalLevels;
            case AchievementStatType.TotalPegs:
                return gameData.totalPegs;
            case AchievementStatType.ShotsFired:
                return gameData.shotsFired;
            case AchievementStatType.HighestProjectileScore:
                return gameData.highestProjScore;
            default:
                return 0;
        }
    }

    public void tutorialAch() {
        if (PlayerPrefs.GetFloat("FirstGame") != 0) {
            return;
        }

        PlayerPrefs.SetFloat("FirstGame", 1);
        PlayerPrefs.Save();

        QueueAchievement(oneSmallStep);
    }

    private void QueueAchievement(Sprite achievementSprite) {
        if (achievementSprite == null) {
            return;
        }

        achievementQueue.Enqueue(achievementSprite);

        if (!popUpPlaying) {
            ShowNextQueuedAchievement();
        }
    }

    private void ShowNextQueuedAchievement() {
        if (achievementQueue.Count <= 0 || achPanel == null || achImage == null) {
            popUpPlaying = false;
            return;
        }

        Sprite nextAchievement = achievementQueue.Dequeue();

        achPanel.SetActive(true);
        achImage.sprite = nextAchievement;
        popUpPlaying = true;

        StartCoroutine(AchievementFadeIn());
    }

    private IEnumerator AchievementFadeIn() {
        PlayAchievementSound();
        SetAchievementAlpha(0f);

        while (GetPanelAlpha() < 1f) {
            SetAchievementAlpha(GetPanelAlpha() + Time.deltaTime / 1f);
            yield return null;
        }

        yield return new WaitForSeconds(0.75f);

        StartCoroutine(AchievementFadeOut());
    }

    private IEnumerator AchievementFadeOut() {
        SetAchievementAlpha(1f);

        while (GetPanelAlpha() > 0f) {
            SetAchievementAlpha(GetPanelAlpha() - Time.deltaTime / 3f);
            yield return null;
        }

        if (achPanel != null) {
            achPanel.SetActive(false);
        }

        popUpPlaying = false;
        ShowNextQueuedAchievement();
    }

    private void PlayAchievementSound() {
        if (achievementAudio != null) {
            achievementAudio.Play();
        }
    }

    private void SetAchievementAlpha(float alpha) {
        alpha = Mathf.Clamp01(alpha);

        if (achPanelImage != null) {
            Color panelColor = achPanelImage.color;
            panelColor.a = alpha;
            achPanelImage.color = panelColor;
        }

        if (achImage != null) {
            Color imageColor = achImage.color;
            imageColor.a = alpha;
            achImage.color = imageColor;
        }
    }

    private float GetPanelAlpha() {
        if (achPanelImage == null) {
            return 0f;
        }

        return achPanelImage.color.a;
    }
}