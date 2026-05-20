using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSpeedManager : MonoBehaviour
{
    public static GameSpeedManager Instance { get; private set; }

    public enum PlaySpeed
    {
        Slow = 0,
        Normal = 1,
        Fast = 2,
        SuperFast = 3
    }

    private const string SpeedPrefKey = "playSpeedIndex";

    private PlaySpeed currentSpeed = PlaySpeed.Normal;
    private bool shotSpeedActive;
    private bool gameplayPaused;
    private bool shotSpeedWasActiveBeforePause;

    private float baseFixedDeltaTime;
    private float baseMaximumDeltaTime;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        baseFixedDeltaTime = Time.fixedDeltaTime;
        baseMaximumDeltaTime = Time.maximumDeltaTime;

        LoadSpeed();
        ResetToNormalSpeed();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() {
        if (Instance == this) {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            gameplayPaused = false;
            ResetToNormalSpeed();
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // Menus, tutorials, loading screens, etc. should always run at normal speed.
        gameplayPaused = false;
        shotSpeedWasActiveBeforePause = false;
        ResetToNormalSpeed();
    }

    public static GameSpeedManager GetOrCreate() {
        if (Instance != null) {
            return Instance;
        }

        GameObject managerObject = new GameObject("GameSpeedManager");
        return managerObject.AddComponent<GameSpeedManager>();
    }

    public static void BeginShotSpeed() {
        GetOrCreate().ApplyShotSpeed();
    }

    public static void EndShotSpeed() {
        GetOrCreate().ResetToNormalSpeed();
    }

    public static void PauseGameplay() {
        GetOrCreate().PauseGameplayInternal();
    }

    public static void ResumeGameplay() {
        GetOrCreate().ResumeGameplayInternal();
    }

    public static bool IsGameplayPaused() {
        return Instance != null && Instance.gameplayPaused;
    }

    public static int GetSavedSpeedIndex() {
        return PlayerPrefs.GetInt(SpeedPrefKey, (int)PlaySpeed.Normal);
    }

    public static void SetSpeedIndex(int speedIndex) {
        GetOrCreate().SetSpeed(speedIndex);
    }

    public void SetSpeed(int speedIndex) {
        speedIndex = Mathf.Clamp(speedIndex, 0, 3);

        currentSpeed = (PlaySpeed)speedIndex;

        PlayerPrefs.SetInt(SpeedPrefKey, speedIndex);
        PlayerPrefs.Save();

        // Changing the dropdown should not affect the menu.
        // It only updates active shot speed if a shot is already running.
        if (shotSpeedActive && SceneManager.GetActiveScene().name == "infiniteLevel") {
            ApplyShotSpeed();
        }
        else {
            ResetToNormalSpeed();
        }
    }

    public float GetCurrentMultiplier() {
        switch (currentSpeed) {
            case PlaySpeed.Slow:
                return 0.5f;
            case PlaySpeed.Fast:
                return 2f;
            case PlaySpeed.SuperFast:
                return 4f;
            default:
                return 1f;
        }
    }

    public string GetCurrentSpeedName() {
        switch (currentSpeed) {
            case PlaySpeed.Slow:
                return "Slow";
            case PlaySpeed.Fast:
                return "Fast";
            case PlaySpeed.SuperFast:
                return "Super Fast";
            default:
                return "Normal";
        }
    }

    private void LoadSpeed() {
        int savedSpeedIndex = PlayerPrefs.GetInt(SpeedPrefKey, (int)PlaySpeed.Normal);
        savedSpeedIndex = Mathf.Clamp(savedSpeedIndex, 0, 3);

        currentSpeed = (PlaySpeed)savedSpeedIndex;
    }

    private void ApplyShotSpeed() {
        // Only gameplay shots should use the chosen speed.
        if (SceneManager.GetActiveScene().name != "infiniteLevel") {
            ResetToNormalSpeed();
            return;
        }

        shotSpeedActive = true;

        // If the options menu pauses gameplay during a shot, remember that shot speed is active
        // but do not unpause until ResumeGameplay() is called.
        if (gameplayPaused) {
            Time.timeScale = 0f;
            Time.fixedDeltaTime = baseFixedDeltaTime;
            Time.maximumDeltaTime = baseMaximumDeltaTime;
            return;
        }

        float multiplier = GetCurrentMultiplier();

        Time.timeScale = multiplier;
        Time.fixedDeltaTime = baseFixedDeltaTime * multiplier;
        Time.maximumDeltaTime = baseMaximumDeltaTime * multiplier;
    }

    private void ResetToNormalSpeed() {
        shotSpeedActive = false;

        // Do not accidentally unpause from a speed reset while the options menu is open.
        if (gameplayPaused) {
            Time.timeScale = 0f;
            Time.fixedDeltaTime = baseFixedDeltaTime;
            Time.maximumDeltaTime = baseMaximumDeltaTime;
            return;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = baseFixedDeltaTime;
        Time.maximumDeltaTime = baseMaximumDeltaTime;
    }

    private void PauseGameplayInternal() {
        // Only the actual gameplay scene should pause. Menu settings should stay normal.
        if (SceneManager.GetActiveScene().name != "infiniteLevel") {
            return;
        }

        if (gameplayPaused) {
            return;
        }

        shotSpeedWasActiveBeforePause = shotSpeedActive;
        gameplayPaused = true;

        Time.timeScale = 0f;
        Time.fixedDeltaTime = baseFixedDeltaTime;
        Time.maximumDeltaTime = baseMaximumDeltaTime;
    }

    private void ResumeGameplayInternal() {
        if (!gameplayPaused) {
            return;
        }

        gameplayPaused = false;

        if (SceneManager.GetActiveScene().name == "infiniteLevel" && shotSpeedWasActiveBeforePause) {
            ApplyShotSpeed();
        }
        else {
            ResetToNormalSpeed();
        }

        shotSpeedWasActiveBeforePause = false;
    }
}