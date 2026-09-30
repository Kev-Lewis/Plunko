using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class slide : MonoBehaviour
{
    [Header("Customization")]
    [SerializeField] private Sprite customSlide;

    private Shooter shoot;
    private AudioSource popAudio;

    private void Start() {
        CacheReferences();
        ApplyCustomization();
    }

    private void CacheReferences() {
        GameObject shooterObject = GameObject.Find("Shooter");
        if (shooterObject != null) {
            shoot = shooterObject.GetComponent<Shooter>();
        }

        GameObject popObject = GameObject.Find("pop");
        if (popObject != null) {
            popAudio = popObject.GetComponent<AudioSource>();
        }
    }

    private void ApplyCustomization() {
        if (PlayerPrefs.GetInt("customSlide") != 1 || customSlide == null) {
            return;
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) {
            spriteRenderer.sprite = customSlide;
        }
    }

    private void OnDestroy() {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Tutorial2") {
            PlayPopSound();
            return;
        }

        if (currentScene != "infiniteLevel") {
            return;
        }

        if (shoot == null || !shoot.isShooting()) {
            return;
        }

        PlayPopSound();
        AwardSlideScore();
    }

    private void PlayPopSound() {
        if (popAudio != null) {
            popAudio.Play();
        }
    }

    private void AwardSlideScore() {
        ProjScript mainProjectile = FindObjectOfType<ProjScript>();

     if (mainProjectile != null) {
            mainProjectile.afterSlide();
            return;
        }

        arrowProjScript arrowProjectile = FindObjectOfType<arrowProjScript>();

        if (arrowProjectile != null) {
            arrowProjectile.afterSlide();
        }
    }
}