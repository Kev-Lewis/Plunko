using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonScript : MonoBehaviour
{
    private Shooter shoot;
    private Spawning spawn;
    private GameObject settingsButton;
    private AudioSource audioClip;

    private void Start() {
        settingsButton = GameObject.Find("SettingsButton");
    }

    public void RestartGame(){
        spawn = GameObject.Find("Spawner").GetComponent<Spawning>();
        shoot = GameObject.Find("Shooter").GetComponent<Shooter>();
        DeleteAllPegs();
        Shooter.totalScore = 0;
        shoot.ammoCount = shoot.startingAmmoCount;
        spawn.resetUnlockedPegs();
        audioClip = GameObject.Find("level_clear").GetComponent<AudioSource>();
        audioClip.Play();
        StartCoroutine(resetGame());
    }

    IEnumerator resetGame()
    {
        yield return new WaitForSeconds(0.2f);
        spawn.resetList();
        spawn.SpawnObjects();
        Shooter.gameOver = false;
        Shooter.gameOverTextChecker = true;
        settingsButton.SetActive(true);
    }

    private void DeleteAllPegs(){
        var pegsToDelete = FindObjectsOfType<pegsToDelete>();
        foreach(var pegToDelete in pegsToDelete){
            Destroy(pegToDelete.gameObject);
        }
    }

    public void cleanGame()
    {
        DeleteAllPegs();
        shoot = GameObject.Find("Shooter").GetComponent<Shooter>();
        spawn = GameObject.Find("Spawner").GetComponent<Spawning>();
        Shooter.gameOver = false;
        Shooter.totalScore = 0;
        Shooter.shooting = false;
        Shooter.chanShootAgain = true;
        shoot.ammoCount = shoot.startingAmmoCount;
        spawn.resetList();
    }

    public void OpenMenu(){
        spawn = GameObject.Find("Spawner").GetComponent<Spawning>();
        shoot = GameObject.Find("Shooter").GetComponent<Shooter>();
        DeleteAllPegs();
        settingsButton.SetActive(true);
        Shooter.gameOver = false;
        Shooter.totalScore = 0;
        shoot.ammoCount = shoot.startingAmmoCount;
        spawn.resetList();
        SceneManager.LoadScene("Menu");
    }
}
