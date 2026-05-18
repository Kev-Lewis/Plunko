using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Shooter shooter;
    [SerializeField] private Spawning spawner;

    [Header("Run Settings")]
    [SerializeField] private string currentSeed = "PL00001A3F";
    [SerializeField] private bool generateRandomSeedForNewRuns = true;
    [SerializeField] private bool autoLoadRunOnStart = true;
    [SerializeField] private bool autoSaveOnPause = true;
    [SerializeField] private float inputLockDuration = 0.35f;

    private RunSaveData currentRunData;
    private bool hasLoadedRun;
    private bool saveQueued;
    private bool runTransitionInProgress;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("RunManager loaded");
    }

    private void Start() {
        StartCoroutine(InitializeRunAfterSceneStart());
    }

    private IEnumerator InitializeRunAfterSceneStart() {
        yield return null;

        CacheReferences();

        if (autoLoadRunOnStart && HasActiveRunSave()) {
            LoadRun();
        }
        else {
            string seedToUse = generateRandomSeedForNewRuns ? GenerateRandomSeed() : currentSeed;
            StartNewRun(seedToUse);
        }
    }

    private void CacheReferences() {
        if (shooter == null) {
            GameObject shooterObject = GameObject.Find("Shooter");

            if (shooterObject != null) {
                shooter = shooterObject.GetComponent<Shooter>();
            }
        }

        if (spawner == null) {
            GameObject spawnerObject = GameObject.Find("Spawner");

            if (spawnerObject != null) {
                spawner = spawnerObject.GetComponent<Spawning>();
            }
        }
    }

    public void StartNewRun(string seed) {
        if (runTransitionInProgress) {
            Debug.Log("StartNewRun skipped because a run transition is already in progress.");
            return;
        }

        StartCoroutine(StartNewRunRoutine(seed));
    }

    private IEnumerator StartNewRunRoutine(string seed) {
        runTransitionInProgress = true;

        CacheReferences();

        currentSeed = NormalizeSeed(seed);

        Debug.Log("Starting run with seed: " + currentSeed);

        Shooter.totalScore = 0;
        Shooter.totalPegsToSave = 0;
        Shooter.totalLevelsToSave = 0;
        Shooter.gameOver = false;
        Shooter.shooting = false;

        if (shooter != null) {
            int startingAmmo = shooter.startingAmmoCount > 0 ? shooter.startingAmmoCount : shooter.ammoCount;

            shooter.ammoCount = startingAmmo;
            shooter.prevScore = 0;
            shooter.resetLocalScore();

            shooter.RestorePlanetState(new List<PlanetSaveData>());
        }

        if (spawner != null) {
            spawner.ClearCurrentBoard();

            yield return new WaitForEndOfFrame();

            spawner.StartNewSeededRun(currentSeed, true);
        }

        LockShooterInput();

        currentRunData = BuildCurrentRunSaveData();
        currentRunData.hasActiveRun = true;
        hasLoadedRun = true;

        runTransitionInProgress = false;

        SaveRunCheckpoint();
    }

    public void SaveRunCheckpoint() {
        if (runTransitionInProgress) {
            Debug.Log("SaveRunCheckpoint skipped because a run transition is in progress.");
            return;
        }

        Debug.Log("SaveRunCheckpoint() was called");

        CacheReferences();

        currentRunData = BuildCurrentRunSaveData();
        SaveSystem.SaveRun(currentRunData);

        Debug.Log(
            "RUN SAVED | " +
            "Score: " + currentRunData.totalScore +
            " | Ammo: " + currentRunData.ammoCount +
            " | Level: " + currentRunData.levelNumber +
            " | Board Objects: " + currentRunData.boardObjects.Count +
            " | Seed: " + currentRunData.seed
        );
    }

    public void SaveRunCheckpointNextFrame() {
        if (saveQueued) {
            return;
        }

        StartCoroutine(SaveRunCheckpointNextFrameRoutine());
    }

    private IEnumerator SaveRunCheckpointNextFrameRoutine() {
        saveQueued = true;

        yield return new WaitForEndOfFrame();

        saveQueued = false;

        if (runTransitionInProgress) {
            Debug.Log("Next-frame save skipped because a run transition is in progress.");
            yield break;
        }

        if (Shooter.gameOver) {
            MarkRunFinished();
            yield break;
        }

        SaveRunCheckpoint();
    }

    public void LoadRun() {
        if (runTransitionInProgress) {
            Debug.Log("LoadRun skipped because a run transition is already in progress.");
            return;
        }

        CacheReferences();

        RunSaveData loadedData = SaveSystem.LoadRun();

        if (loadedData == null || !loadedData.hasActiveRun || loadedData.gameOver) {
            string seedToUse = generateRandomSeedForNewRuns ? GenerateRandomSeed() : currentSeed;
            StartNewRun(seedToUse);
            return;
        }

        StartCoroutine(LoadRunRoutine(loadedData));
    }

    private IEnumerator LoadRunRoutine(RunSaveData loadedData) {
        runTransitionInProgress = true;

        currentRunData = loadedData;
        currentSeed = NormalizeSeed(loadedData.seed);

        Debug.Log(
            "RUN LOADED | " +
            "Score: " + loadedData.totalScore +
            " | Ammo: " + loadedData.ammoCount +
            " | Level: " + loadedData.levelNumber +
            " | Board Objects: " + loadedData.boardObjects.Count +
            " | Seed: " + loadedData.seed
        );

        Shooter.totalScore = loadedData.totalScore;
        Shooter.totalPegsToSave = loadedData.totalPegsToSave;
        Shooter.totalLevelsToSave = loadedData.totalLevelsToSave;
        Shooter.gameOver = false;
        Shooter.shooting = false;

        if (shooter != null) {
            shooter.ammoCount = loadedData.ammoCount;
            shooter.prevScore = loadedData.previousScore;
            shooter.RestorePlanetState(loadedData.planets);
        }

        if (spawner != null) {
            spawner.StartNewSeededRun(currentSeed, false);
            spawner.SetLevelsCleared(loadedData.levelsCleared);
            spawner.RestoreSpecialPegWeightBonuses(loadedData.pegWeightBonuses);

            spawner.ClearCurrentBoard();

            yield return new WaitForEndOfFrame();

            bool restored = spawner.RestoreBoardState(loadedData.boardObjects);

            if (!restored) {
                Debug.LogWarning("Saved board restore failed. Regenerating board from seed instead.");

                spawner.ClearCurrentBoard();

                yield return new WaitForEndOfFrame();

                spawner.StartNewSeededRun(currentSeed, true);
                spawner.SetLevelsCleared(loadedData.levelsCleared);
            }
        }

        LockShooterInput();

        hasLoadedRun = true;
        runTransitionInProgress = false;
    }

    public void ClearRunSave() {
        SaveSystem.DeleteRunSave();

        currentRunData = null;
        hasLoadedRun = false;
    }

    public void MarkRunFinished() {
        if (runTransitionInProgress) {
            Debug.Log("MarkRunFinished skipped because a run transition is in progress.");
            return;
        }

        CacheReferences();

        currentRunData = BuildCurrentRunSaveData();
        currentRunData.hasActiveRun = false;
        currentRunData.gameOver = true;

        SaveSystem.SaveRun(currentRunData);

        Debug.Log(
            "RUN FINISHED | " +
            "Score: " + currentRunData.totalScore +
            " | Ammo: " + currentRunData.ammoCount +
            " | Level: " + currentRunData.levelNumber +
            " | Board Objects: " + currentRunData.boardObjects.Count +
            " | Seed: " + currentRunData.seed
        );
    }

    public void MarkRunFinishedNextFrame() {
        StartCoroutine(MarkRunFinishedNextFrameRoutine());
    }

    private IEnumerator MarkRunFinishedNextFrameRoutine() {
        yield return new WaitForEndOfFrame();
        MarkRunFinished();
    }

    public void SetSeed(string seed) {
        currentSeed = NormalizeSeed(seed);
    }

    public string GetSeed() {
        return currentSeed;
    }

    public bool HasLoadedRun() {
        return hasLoadedRun;
    }

    public bool HasActiveRunSave() {
        RunSaveData savedRun = SaveSystem.LoadRun();

        if (savedRun == null) {
            return false;
        }

        return savedRun.hasActiveRun && !savedRun.gameOver;
    }

    public bool HasActiveRunInMemory() {
        return currentRunData != null && currentRunData.hasActiveRun && !currentRunData.gameOver;
    }

    public void StartFreshRunFromButton() {
        if (runTransitionInProgress) {
            Debug.Log("StartFreshRunFromButton skipped because a run transition is already in progress.");
            return;
        }

        ClearRunSave();

        string seedToUse = generateRandomSeedForNewRuns ? GenerateRandomSeed() : currentSeed;
        StartNewRun(seedToUse);
    }

    public void StartFreshRunFromButton(string seed) {
        if (runTransitionInProgress) {
            Debug.Log("StartFreshRunFromButton(string) skipped because a run transition is already in progress.");
            return;
        }

        ClearRunSave();
        StartNewRun(seed);
    }

    private RunSaveData BuildCurrentRunSaveData() {
        RunSaveData data = new RunSaveData();

        data.hasActiveRun = true;
        data.seed = currentSeed;

        if (spawner != null) {
            data.levelNumber = spawner.GetCurrentLevelNumber();
            data.levelsCleared = spawner.GetLevelsCleared();
            data.currentBoardPattern = spawner.GetCurrentBoardPatternName();
            data.unlockedPegNames = spawner.GetUnlockedPegNames();
            data.boardObjects = spawner.GetCurrentBoardState();
            data.pegWeightBonuses = spawner.GetSpecialPegWeightBonuses();
        }
        else {
            data.levelNumber = 1;
            data.levelsCleared = 0;
            data.currentBoardPattern = "";
            data.unlockedPegNames = new List<string>();
            data.boardObjects = new List<BoardObjectSaveData>();
            data.pegWeightBonuses = new List<PegWeightSaveData>();
        }

        data.totalScore = Shooter.totalScore;
        data.totalPegsToSave = Shooter.totalPegsToSave;
        data.totalLevelsToSave = Shooter.totalLevelsToSave;
        data.gameOver = Shooter.gameOver;

        if (shooter != null) {
            data.ammoCount = shooter.ammoCount;
            data.previousScore = shooter.prevScore;
            data.planets = shooter.GetCurrentPlanetState();
        }
        else {
            data.ammoCount = 0;
            data.previousScore = 0;
            data.planets = new List<PlanetSaveData>();
        }

        data.activeUpgradeIds = new List<string>();
        data.savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        return data;
    }

    private void LockShooterInput() {
        if (shooter != null) {
            shooter.LockShootingInput(inputLockDuration);
        }
    }

    private string NormalizeSeed(string seed) {
        if (string.IsNullOrWhiteSpace(seed)) {
            return "PL00000000";
        }

        seed = seed.Trim().ToUpper();

        if (!seed.StartsWith("PL")) {
            seed = "PL" + seed;
        }

        string hexPart = seed.Substring(2);

        if (hexPart.Length > 8) {
            hexPart = hexPart.Substring(0, 8);
        }

        while (hexPart.Length < 8) {
            hexPart = "0" + hexPart;
        }

        for (int i = 0; i < hexPart.Length; i++) {
            bool isDigit = hexPart[i] >= '0' && hexPart[i] <= '9';
            bool isHexLetter = hexPart[i] >= 'A' && hexPart[i] <= 'F';

            if (!isDigit && !isHexLetter) {
                return "PL00000000";
            }
        }

        return "PL" + hexPart;
    }

    private string GenerateRandomSeed() {
        const string hexChars = "0123456789ABCDEF";

        string seed = "PL";

        for (int i = 0; i < 8; i++) {
            int index = UnityEngine.Random.Range(0, hexChars.Length);
            seed += hexChars[index];
        }

        return seed;
    }

    private void OnApplicationPause(bool pauseStatus) {
        if (!pauseStatus || !autoSaveOnPause) {
            return;
        }

        if (runTransitionInProgress) {
            Debug.Log("Skipped pause autosave because a run transition is active.");
            return;
        }

        if (currentRunData == null || !currentRunData.hasActiveRun) {
            return;
        }

        if (Shooter.shooting) {
            Debug.Log("Skipped pause autosave because a shot is currently active.");
            return;
        }

        SaveRunCheckpoint();
    }

    private void OnApplicationQuit() {
        if (!autoSaveOnPause) {
            return;
        }

        if (runTransitionInProgress) {
            Debug.Log("Skipped quit autosave because a run transition is active.");
            return;
        }

        if (currentRunData == null || !currentRunData.hasActiveRun) {
            return;
        }

        if (Shooter.shooting) {
            Debug.Log("Skipped quit autosave because a shot is currently active.");
            return;
        }

        SaveRunCheckpoint();
    }
}