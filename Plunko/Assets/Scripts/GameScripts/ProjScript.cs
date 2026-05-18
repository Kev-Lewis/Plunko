using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProjScript : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private ParticleSystem ps;
    [SerializeField] private ParticleSystem level_clear_ps;

    [Header("Projectiles")]
    [SerializeField] private GameObject ArrowProj;

    [Header("Customization")]
    [SerializeField] private Sprite customProj;

    private const int BasePegScore = 10;
    private const float BounceDamping = 0.9f;
    private const float ArrowProjectileSpeed = 7.5f;

    private Rigidbody2D rb;
    private Vector3 lastVelocity;
    private Spawning spawn;
    private Shooter shoot;
    private Transform blackHole;
    private Text multiplierText;
    private GameData gameData;

    private AudioSource popAudio;
    private AudioSource shootAudio;
    private AudioSource ammoPlusAudio;
    private AudioSource ammoMinusAudio;
    private AudioSource blackHoleAudio;
    private AudioSource levelClearAudio;
    private AudioSource loseAudio;

    private float startingPitch;
    private int tempScore;
    private int totalPegCounter;
    private bool groundHitProcessed;

    private static int currentLevel = 1;

    private void Awake() {
        ApplyProjectileCustomization();
        gameData = SaveSystem.Load();
    }

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

    private void ApplyProjectileCustomization() {
        if (PlayerPrefs.GetInt("customProj") == 1 && customProj != null) {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null) {
                spriteRenderer.sprite = customProj;
            }
        }

        if (PlayerPrefs.GetInt("customTrail") == 1) {
            TrailRenderer trailRenderer = GetComponent<TrailRenderer>();

            if (trailRenderer != null) {
                trailRenderer.colorGradient = BuildCustomTrailGradient();
            }
        }
    }

    private Gradient BuildCustomTrailGradient() {
        Gradient gradient = new Gradient();

        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.gray, 0.0f),
                new GradientColorKey(Color.yellow, 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 1.0f)
            }
        );

        return gradient;
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
        levelClearAudio = FindAudioSource("level_clear");
        loseAudio = FindAudioSource("lose");

        if (popAudio != null) {
            startingPitch = popAudio.pitch;
        }
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
            case "Nuke":
                HitNuke(other.gameObject);
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
        PlayPopAndRaisePitch();
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

        PlayPopAndRaisePitch();
    }

    private void HitTwoHitPeg(GameObject peg) {
        RegisterPegHit();

        twoHitScript twoHit = peg.GetComponent<twoHitScript>();
        if (twoHit != null && twoHit.hit()) {
            DestroyPegWithParticles(peg);
            AddScoreToShot(BasePegScore * 2);
        }

        PlayPopAndRaisePitch();
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

    private void HitNuke(GameObject peg) {
        RegisterPegHit();
        Destroy(peg);
    }

    private void HitGround() {
        if (groundHitProcessed) {
            return;
        }

        groundHitProcessed = true;

        Debug.Log("Projectile hit ground. RunManager exists: " + (RunManager.Instance != null));

        Shooter.totalPegsToSave += totalPegCounter;
        ResetPopPitch();

        tempScore = 0;

        if (shoot != null) {
            shoot.prevScore = Shooter.totalScore;
        }

        Shooter.shooting = false;

        if (spawn != null && spawn.IsBoardCleared()) {
            ClearLevel();
        }

        if (gameData != null) {
            SaveSystem.Save(gameData);
        }

        if (shoot != null && shoot.ammoCount <= 0) {
            TriggerGameOver();

            if (RunManager.Instance != null) {
                Debug.Log("Marking run finished from HitGround()");
                RunManager.Instance.MarkRunFinishedNextFrame();
            }
        }
        else {
            FinishShotNormally();

            if (RunManager.Instance != null) {
                Debug.Log("Saving run checkpoint from HitGround()");
                RunManager.Instance.SaveRunCheckpointNextFrame();
            }
        }

        Destroy(gameObject);
    }

    private void ClearLevel() {
        DestroyAllOfType<AmmoMinus>();
        DestroyAllOfType<AmmoPlus>();
        DestroyAllOfType<blackHolePeg>();
        DestroyAllOfType<x2Peg>();
        DestroyAllOfType<pegsToDelete>();
        DestroyArrowProjectiles();

        spawn.unlockNewPeg();
        SpawnLevelClearParticles();
        spawn.resetList();
        spawn.SpawnObjects();
        spawn.addLevelsCleared();

        if (shoot != null) {
            shoot.ammoCount += 5;
        }

        PlaySound(levelClearAudio);

        Shooter.totalScore += 50 * currentLevel;
        currentLevel++;
        Shooter.totalLevelsToSave++;
        SetMultiplierText("");
    }

    private void TriggerGameOver() {
        Shooter.gameOver = true;

        ParticleSystem[] particleObjects = FindObjectsOfType<ParticleSystem>();

        foreach (ParticleSystem particleObject in particleObjects) {
            if (particleObject != null) {
                Destroy(particleObject.gameObject);
            }
        }

        PlaySound(loseAudio);
    }

    private void FinishShotNormally() {
        if (shoot != null) {
            shoot.fadeText = true;
        }

        PlaySound(ammoMinusAudio);
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

        PlaySound(ammoPlusAudio);
        SlowProjectile(4f);
        ClearTrail();
        UpdateMultiplierText();
    }

    private void HitBlackHole(GameObject blackHolePeg) {
        RegisterPegHit();
        Destroy(blackHolePeg);
        TeleportToBlackHoleExit();
        PlaySound(blackHoleAudio);
        SlowProjectile(2f);
        ClearTrail();
    }

    private void HitSlide(GameObject slide) {
        PlayPopAndRaisePitch();
        StartCoroutine(deleteSlide(slide));
    }

    private void RegisterPegHit() {
        totalPegCounter++;
    }

    private void AddScoreToShot(int scoreValue) {
        if (shoot == null) {
            return;
        }

        shoot.addTotalScore(scoreValue, shoot.getGlobalMulti());
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

    private void SpawnLevelClearParticles() {
        if (level_clear_ps == null) {
            return;
        }

        GameObject levelClearObject = GameObject.Find("levelclear");

        if (levelClearObject != null) {
            Instantiate(level_clear_ps, levelClearObject.transform.position, level_clear_ps.transform.rotation);
        }
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

    private void DestroyArrowProjectiles() {
        arrowProjScript[] arrowProjectiles = FindObjectsOfType<arrowProjScript>();

        foreach (arrowProjScript arrowProjectile in arrowProjectiles) {
            if (arrowProjectile != null) {
                SpawnHitParticles(arrowProjectile.transform.position);
                Destroy(arrowProjectile.gameObject);
            }
        }
    }

    private void DestroyAllOfType<T>() where T : Component {
        T[] objects = FindObjectsOfType<T>();

        foreach (T obj in objects) {
            if (obj != null) {
                Destroy(obj.gameObject);
            }
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

    private void PlayPopAndRaisePitch() {
        PlaySound(popAudio);
        ChangePitch();
    }

    private void PlaySound(AudioSource audioSource) {
        if (audioSource != null) {
            audioSource.Play();
        }
    }

    private void ChangePitch() {
        if (popAudio != null && popAudio.pitch <= 7f) {
            popAudio.pitch += 0.15f;
        }
    }

    private void ResetPopPitch() {
        if (popAudio != null) {
            popAudio.pitch = startingPitch;
        }
    }

    private void UpdateMultiplierText() {
        if (shoot != null) {
            SetMultiplierText("X" + shoot.getGlobalMulti());
        }
    }

    private void SetMultiplierText(string text) {
        if (multiplierText != null) {
            multiplierText.text = text;
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

    public int getScore() {
        return BasePegScore;
    }
}