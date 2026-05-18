using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class arrowProjScript : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private ParticleSystem ps;

    [Header("Projectiles")]
    [SerializeField] private GameObject ArrowProj;

    private const int BasePegScore = 10;
    private const float BounceDamping = 0.9f;
    private const float ArrowProjectileSpeed = 7.5f;

    private Rigidbody2D rb;
    private Vector3 lastVelocity;
    private Spawning spawn;
    private Shooter shoot;
    private Transform blackHole;
    private Text multiplierText;

    private AudioSource popAudio;
    private AudioSource shootAudio;
    private AudioSource ammoPlusAudio;
    private AudioSource ammoMinusAudio;
    private AudioSource blackHoleAudio;

    private int tempScore;

    private void Start() {
        CacheComponents();
        CacheSceneReferences();
        CacheAudioSources();

        if (shoot != null) {
            shoot.activateArrowProj();
        }
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
        GameObject spawnerObject = GameObject.Find("Spawner");
        if (spawnerObject != null) {
            spawn = spawnerObject.GetComponent<Spawning>();
        }

        GameObject shooterObject = GameObject.Find("Shooter");
        if (shooterObject != null) {
            shoot = shooterObject.GetComponent<Shooter>();
        }

        GameObject blackHoleObject = GameObject.Find("portalPaddle");
        if (blackHoleObject != null) {
            blackHole = blackHoleObject.transform;
        }

        GameObject multiplierObject = GameObject.Find("multiplierText");
        if (multiplierObject != null) {
            multiplierText = multiplierObject.GetComponent<Text>();
        }
    }

    private void CacheAudioSources() {
        popAudio = FindAudioSource("pop");
        shootAudio = FindAudioSource("shoot");
        ammoPlusAudio = FindAudioSource("ammoplus");
        ammoMinusAudio = FindAudioSource("ammominus");
        blackHoleAudio = FindAudioSource("blackhole");
    }

    private AudioSource FindAudioSource(string objectName) {
        GameObject audioObject = GameObject.Find(objectName);
        return audioObject != null ? audioObject.GetComponent<AudioSource>() : null;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        ApplyBounce(other);

        switch (other.gameObject.tag) {
            case "Peg":
                HitNormalPeg(other.gameObject);
                break;
            case "Ground":
                HitGround();
                break;
            case "multiplier":
                HitMultiplier();
                break;
            case "AmmoPlus":
                HitAmmoPlus(other.gameObject);
                break;
            case "AmmoMinus":
                HitAmmoMinus(other.gameObject);
                break;
            case "MultiHitPeg":
                HitMultiHitPeg(other.gameObject);
                break;
            case "TwoHitPeg":
                HitTwoHitPeg(other.gameObject);
                break;
            case "wall":
                PlaySound(popAudio);
                break;
            case "x2":
                HitX2Peg(other.gameObject);
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

    private void HitNormalPeg(GameObject peg) {
        RegisterPegHit();
        DestroyPegWithParticles(peg);
        AddScoreToShot(BasePegScore);
        PlaySound(popAudio);
    }

    private void HitGround() {
        Destroy(gameObject);
    }

    private void HitMultiplier() {
        if (shoot == null) {
            return;
        }

        shoot.increaseGlobalMulti();
        AddScoreToShot(1);
        PlaySound(ammoPlusAudio);
        UpdateMultiplierText();
    }

    private void HitAmmoPlus(GameObject peg) {
        RegisterPegHit();
        DestroyPegWithParticles(peg);

        if (shoot != null) {
            shoot.ammoCount++;
        }

        PlaySound(ammoPlusAudio);
    }

    private void HitAmmoMinus(GameObject peg) {
        RegisterPegHit();
        DestroyPegWithParticles(peg);

        if (shoot != null && shoot.ammoCount >= 1) {
            shoot.ammoCount--;
        }

        PlaySound(ammoMinusAudio);
    }

    private void HitMultiHitPeg(GameObject peg) {
        RegisterPegHit();

        MultiHit multiHit = peg.GetComponent<MultiHit>();

        if (multiHit != null && multiHit.hit()) {
            DestroyPegWithParticles(peg);
            AddScoreToShot(BasePegScore * 3);
        }

        PlaySound(popAudio);
    }

    private void HitTwoHitPeg(GameObject peg) {
        RegisterPegHit();

        twoHitScript twoHit = peg.GetComponent<twoHitScript>();

        if (twoHit != null && twoHit.hit()) {
            DestroyPegWithParticles(peg);
            AddScoreToShot(BasePegScore * 2);
        }

        PlaySound(popAudio);
    }

    private void HitX2Peg(GameObject peg) {
        RegisterPegHit();

        if (shoot != null) {
            shoot.increaseGlobalMulti();
        }

        UpdateMultiplierText();
        Destroy(peg);
        PlaySound(ammoPlusAudio);
    }

    private void HitArrowPeg(GameObject peg, string launcherName, Vector3 direction) {
        RegisterPegHit();
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
        AddScoreToShot(BasePegScore);

        if (shoot != null) {
            shoot.increaseGlobalMulti();
        }

        UpdateMultiplierText();
        SlowProjectile(4f);
        ClearTrail();
        PlaySound(ammoPlusAudio);
    }

    private void HitBlackHole(GameObject blackHolePeg) {
        RegisterPegHit();
        Destroy(blackHolePeg);
        TeleportToBlackHoleExit();
        SlowProjectile(2f);
        ClearTrail();
        PlaySound(blackHoleAudio);
    }

    private void HitSlide(GameObject slide) {
        PlaySound(popAudio);
        StartCoroutine(deleteSlide(slide));
    }

    private void RegisterPegHit() {
        Shooter.totalPegsToSave++;
    }

    private void AddScoreToShot(int scoreValue) {
        if (shoot == null) {
            return;
        }

        shoot.addTotalScore(scoreValue, shoot.getGlobalMulti());

        // This is the important fix.
        // Arrow projectile score now counts toward bonus ammo thresholds.
        shoot.CheckShotBonusAmmo();

        tempScore = shoot.getLocalScore();
    }

    private void DestroyPegWithParticles(GameObject peg) {
        if (peg == null) {
            return;
        }

        SpawnHitParticles(peg.transform.position);
        Destroy(peg);
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

    private void UpdateMultiplierText() {
        if (shoot != null && multiplierText != null) {
            multiplierText.text = "X" + shoot.getGlobalMulti();
        }
    }

    private void PlaySound(AudioSource audioSource) {
        if (audioSource != null) {
            audioSource.Play();
        }
    }

    private IEnumerator deleteSlide(GameObject slide) {
        RegisterPegHit();

        yield return new WaitForSeconds(0.5f);

        if (slide != null) {
            Destroy(slide);
        }
    }

    public void setTempScore(int score) {
        tempScore = score;
    }

    public void afterSlide() {
        AddScoreToShot(BasePegScore);
    }

    private void OnDestroy() {
        if (shoot != null) {
            shoot.deactivateArrowProj();
        }
    }
}