using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial_Proj : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private ParticleSystem ps;
    [SerializeField] private ParticleSystem level_clear_ps;

    [Header("Projectiles")]
    [SerializeField] private GameObject ArrowProj;

    private const float BounceDamping = 0.9f;
    private const float ArrowProjectileSpeed = 7.5f;

    private Rigidbody2D rb;
    private Vector3 lastVelocity;
    private Tutorial_Shooter shoot;
    private Transform blackHole;

    private AudioSource popAudio;
    private AudioSource shootAudio;
    private AudioSource ammoPlusAudio;
    private AudioSource blackHoleAudio;

    private void Start() {
        CacheComponents();
        CacheSceneReferences();
        CacheAudioSources();
    }

    private void Update() {
        if (rb != null) {
            lastVelocity = rb.velocity;
        }
    }

    private void CacheComponents() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void CacheSceneReferences() {
        GameObject shooterObject = GameObject.Find("Shooter");
        if (shooterObject != null) {
            shoot = shooterObject.GetComponent<Tutorial_Shooter>();
        }

        GameObject blackHoleObject = GameObject.Find("portalPaddle");
        if (blackHoleObject != null) {
            blackHole = blackHoleObject.transform;
        }
    }

    private void CacheAudioSources() {
        popAudio = FindAudioSource("pop");
        shootAudio = FindAudioSource("shoot");
        ammoPlusAudio = FindAudioSource("ammoplus");
        blackHoleAudio = FindAudioSource("blackhole");
    }

    private AudioSource FindAudioSource(string objectName) {
        GameObject audioObject = GameObject.Find(objectName);
        return audioObject != null ? audioObject.GetComponent<AudioSource>() : null;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        ApplyBounce(other);

        switch (other.gameObject.tag) {
            case "Ground":
                HitGround();
                break;
            case "TutorialPeg":
                HitTutorialPeg(other.gameObject);
                break;
            case "TutorialMultiHit":
                HitTutorialMultiHit(other.gameObject);
                break;
            case "TutorialTwoHitPeg":
                HitTutorialTwoHit(other.gameObject);
                break;
            case "wall":
                PlaySound(popAudio);
                break;
            case "ArrowUpLeft":
                HitArrowPeg(other.gameObject, "LauncherRight", new Vector3(-1f, 1f, 0f));
                break;
            case "ArrowLeft":
                HitArrowPeg(other.gameObject, "LauncherRight", new Vector3(-1f, 0f, 0f));
                break;
            case "ArrowRight":
                HitArrowPeg(other.gameObject, "LauncherLeft", new Vector3(1f, 0f, 0f));
                break;
            case "ArrowUpRight":
                HitArrowPeg(other.gameObject, "LauncherLeft", new Vector3(1f, 1f, 0f));
                break;
        }
    }

    private void ApplyBounce(Collision2D collision) {
        if (rb == null || collision.contactCount == 0) {
            return;
        }

        float speed = lastVelocity.magnitude;
        Vector3 direction = Vector3.Reflect(lastVelocity.normalized, collision.contacts[0].normal);
        rb.velocity = direction * Mathf.Max(speed, 0f) * BounceDamping;
    }

    private void HitGround() {
        Tutorial_Shooter.shooting = false;

        if (shoot != null) {
            shoot.setChargeButtonShoot();
        }

        Destroy(gameObject);
        HandleTutorialProgression();
    }

    private void HandleTutorialProgression() {
        if (shoot == null || shoot.getTutorialPegsActive() > 0) {
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "TutorialPC" || currentScene == "TutorialMobile") {
            SceneManager.LoadScene("Tutorial2");
        }
        else if (currentScene == "Tutorial2") {
            SceneManager.LoadScene("Tutorial3");
        }
        else if (currentScene == "Tutorial3") {
            shoot.tutorial2Open();
        }
    }

    private void HitTutorialPeg(GameObject peg) {
        SpawnHitParticles(peg.transform.position);
        PlaySound(popAudio);

        if (shoot != null) {
            shoot.lowerTutorialPegCount();
        }

        Destroy(peg);
    }

    private void HitTutorialMultiHit(GameObject peg) {
        TutorialMultiHit multiHit = peg.GetComponent<TutorialMultiHit>();

        if (multiHit != null && multiHit.hit()) {
            SpawnHitParticles(peg.transform.position);

            if (shoot != null) {
                shoot.lowerTutorialPegCount();
            }

            Destroy(peg);
        }

        PlaySound(popAudio);
    }

    private void HitTutorialTwoHit(GameObject peg) {
        TutorialTwoHit twoHit = peg.GetComponent<TutorialTwoHit>();

        if (twoHit != null && twoHit.hit()) {
            SpawnHitParticles(peg.transform.position);

            if (shoot != null) {
                shoot.lowerTutorialPegCount();
            }

            Destroy(peg);
        }

        PlaySound(popAudio);
    }

    private void HitArrowPeg(GameObject peg, string launcherName, Vector3 direction) {
        SpawnArrowProjectile(launcherName, direction);
        Destroy(peg);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        switch (collision.gameObject.tag) {
            case "new_multiplier":
                HitNewMultiplier();
                break;
            case "BlackHole":
                HitBlackHole(collision.gameObject);
                break;
            case "slide":
                HitSlide(collision.gameObject);
                break;
        }
    }

    private void HitNewMultiplier() {
        TeleportToBlackHoleExit();
        PlaySound(ammoPlusAudio);
        SlowProjectile(4f);
        ClearTrail();
    }

    private void HitBlackHole(GameObject blackHolePeg) {
        Destroy(blackHolePeg);
        TeleportToBlackHoleExit();
        PlaySound(blackHoleAudio);
        SlowProjectile(2f);
        ClearTrail();

        if (shoot != null) {
            shoot.lowerTutorialPegCount();
        }
    }

    private void HitSlide(GameObject slide) {
        PlaySound(popAudio);
        StartCoroutine(deleteSlide(slide));
    }

    private void SpawnHitParticles(Vector3 position) {
        if (ps == null || shoot == null || shoot.getSettingsOpen()) {
            return;
        }

        Instantiate(ps, new Vector3(position.x, position.y, ps.transform.position.z), ps.transform.rotation);
    }

    private void SpawnArrowProjectile(string launcherName, Vector3 direction) {
        if (ArrowProj == null) {
            return;
        }

        GameObject launcherObject = GameObject.Find(launcherName);
        if (launcherObject == null) {
            return;
        }

        PlaySound(shootAudio);

        GameObject spawnedBullet = Instantiate(ArrowProj, launcherObject.transform.position, Quaternion.identity);
        Rigidbody2D arrowRb = spawnedBullet.GetComponent<Rigidbody2D>();

        if (arrowRb != null) {
            arrowRb.velocity = direction.normalized * ArrowProjectileSpeed;
        }
    }

    private void TeleportToBlackHoleExit() {
        if (blackHole == null) {
            return;
        }

        ClearTrail();
        transform.position = new Vector3(blackHole.position.x, blackHole.position.y, transform.position.z);
    }

    private void SlowProjectile(float divisor) {
        if (rb == null || divisor <= 0f) {
            return;
        }

        lastVelocity = rb.velocity / divisor;
        rb.velocity = rb.velocity / divisor;
    }

    private void ClearTrail() {
        TrailRenderer trailRenderer = GetComponent<TrailRenderer>();

        if (trailRenderer != null) {
            trailRenderer.Clear();
        }
    }

    private void PlaySound(AudioSource audioSource) {
        if (audioSource != null) {
            audioSource.Play();
        }
    }

    private IEnumerator deleteSlide(GameObject slide) {
        yield return new WaitForSeconds(0.5f);

        if (slide != null) {
            Destroy(slide);
        }
    }
}