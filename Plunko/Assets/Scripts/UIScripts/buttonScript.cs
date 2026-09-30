using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonScript : MonoBehaviour
{
    [Header("Optional UI Buttons")]
    [SerializeField] private GameObject newRunButton;

    private Shooter shoot;
    private Spawning spawn;
    private GameObject settingsButton;
    private AudioSource levelClearAudio;

    private void Start() {
        CacheReferences();
        UpdateButtonVisibility();
    }

    private void CacheReferences() {
        if (settingsButton == null) {
            settingsButton = GameObject.Find("SettingsButton");
        }

        if (shoot == null) {
            GameObject shooterObject = GameObject.Find("Shooter");

            if (shooterObject != null) {
                shoot = shooterObject.GetComponent<Shooter>();
            }
        }

        if (spawn == null) {
            GameObject spawnerObject = GameObject.Find("Spawner");

            if (spawnerObject != null) {
                spawn = spawnerObject.GetComponent<Spawning>();
            }
        }

        if (levelClearAudio == null) {
            GameObject audioObject = GameObject.Find("level_clear");

            if (audioObject != null) {
                levelClearAudio = audioObject.GetComponent<AudioSource>();
            }
        }
    }

    private void UpdateButtonVisibility() {
        if (newRunButton == null) {
            return;
        }

        bool hasActiveRun = RunManager.Instance != null && RunManager.Instance.HasActiveRunSave();
        newRunButton.SetActive(hasActiveRun);
    }

    public void RestartGame() {
        CacheReferences();

        DeleteAllPegs();
        DeleteAllProjectiles();

        ResetStaticGameState();
        ResetShooterState();
        ResetSpawnerState();

        PlaySound(levelClearAudio);
        StartCoroutine(resetGame());
    }

    private IEnumerator resetGame() {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.2f);

        if (RunManager.Instance != null) {
            RunManager.Instance.StartFreshRunFromButton();
        }
        else {
            if (spawn != null) {
                spawn.resetList();
                spawn.SpawnObjects();
            }

            Shooter.gameOver = false;
            Shooter.gameOverTextChecker = true;
        }

        if (settingsButton != null) {
            settingsButton.SetActive(true);
        }

        UpdateButtonVisibility();
    }

    public void NewRun() {
        StartCoroutine(NewRunRoutine());
    }

    private IEnumerator NewRunRoutine() {
        CacheReferences();

        DeleteAllPegs();
        DeleteAllProjectiles();

        ResetStaticGameState();
        ResetShooterState();
        ResetSpawnerState();

        yield return new WaitForEndOfFrame();

        if (RunManager.Instance != null) {
            RunManager.Instance.StartFreshRunFromButton();
        }
        else if (spawn != null) {
            spawn.resetList();
            spawn.resetUnlockedPegs();
            spawn.SpawnObjects();
        }

        if (settingsButton != null) {
            settingsButton.SetActive(true);
        }

        PlaySound(levelClearAudio);
        UpdateButtonVisibility();
    }

    public void cleanGame() {
        CacheReferences();

        DeleteAllPegs();
        DeleteAllProjectiles();

        ResetStaticGameState();
        ResetShooterState();

        if (spawn != null) {
            spawn.resetList();
        }

        UpdateButtonVisibility();
    }

    public void OpenMenu() {
        CacheReferences();

        DeleteAllPegs();
        DeleteAllProjectiles();

        ResetStaticGameState();
        ResetShooterState();

        if (spawn != null) {
            spawn.resetList();
        }

        if (settingsButton != null) {
            settingsButton.SetActive(true);
        }

        SceneManager.LoadScene("Menu");
    }

    private void ResetStaticGameState() {
        Shooter.gameOver = false;
        Shooter.gameOverTextChecker = true;
        Shooter.totalScore = 0;
        Shooter.shooting = false;
        Shooter.chanShootAgain = true;
        Shooter.totalPegsToSave = 0;
        Shooter.totalLevelsToSave = 0;
    }

    private void ResetShooterState() {
        if (shoot == null) {
            return;
        }

        int startingAmmo = shoot.startingAmmoCount > 0 ? shoot.startingAmmoCount : shoot.ammoCount;

        shoot.ammoCount = startingAmmo;
        shoot.prevScore = 0;
        shoot.resetLocalScore();
    }

    private void ResetSpawnerState() {
        if (spawn == null) {
            return;
        }

        spawn.resetUnlockedPegs();
    }

    private void DeleteAllPegs() {
        pegsToDelete[] pegs = FindObjectsOfType<pegsToDelete>();

        foreach (pegsToDelete peg in pegs) {
            if (peg != null) {
                Destroy(peg.gameObject);
            }
        }
    }

    private void DeleteAllProjectiles() {
        ProjScript[] projectiles = FindObjectsOfType<ProjScript>();

        foreach (ProjScript projectile in projectiles) {
            if (projectile != null) {
                Destroy(projectile.gameObject);
            }
        }

        arrowProjScript[] arrowProjectiles = FindObjectsOfType<arrowProjScript>();

        foreach (arrowProjScript arrowProjectile in arrowProjectiles) {
            if (arrowProjectile != null) {
                Destroy(arrowProjectile.gameObject);
            }
        }
    }

    private void PlaySound(AudioSource audioSource) {
        if (audioSource != null) {
            audioSource.Play();
        }
    }
}