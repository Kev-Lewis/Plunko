using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shooter : MonoBehaviour
{
    private enum MobileShotState
    {
        Idle,
        Charging,
        Released
    }

    [Header("Firing Variables")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private float projSpeed;
    [SerializeField] private float maxChargeSpeed;
    [SerializeField] private float chargeSpeed;
    [SerializeField] private Transform firePoint;
    [SerializeField] public int ammoCount;
    [SerializeField] private GameObject aimingArrow;

    [Header("Trajectory Preview")]
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private int numberOfPoints = 35;
    [SerializeField] private bool usePredictiveCollisionTrace = true;
    [SerializeField] private bool showAimingArrow = false;
    [SerializeField] private LayerMask traceCollisionMask = ~0;
    [SerializeField] private float traceTimeStep = 0.04f;
    [SerializeField] private float traceRadius = 0.10f;
    [SerializeField] private int maxTraceBounces = 2;
    [SerializeField] private float traceBounceDamping = 0.85f;
    [SerializeField] private float traceSkinWidth = 0.12f;

    [Header("UI Variables")]
    [SerializeField] private Text ammoText;
    [SerializeField] private Text maxChargeText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text gameOverScoreText;
    [SerializeField] private Text gameOverHighScoreText;
    [SerializeField] private Image chargeBar;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject chargeImage;
    [SerializeField] private GameObject shootButton;
    [SerializeField] private Sprite[] shootButtonSprites;
    [SerializeField] private GameObject cancelButton;
    [SerializeField] private GameObject settings_button;

    [Header("Popups")]
    public Text scorePopUpText;
    public Text unlockedPopUpText;
    public Image imageOfPeg;
    [HideInInspector] public bool fadeText = false;

    [Header("Background")]
    [SerializeField] private GameObject[] planets;

    [Header("Customization")]
    [SerializeField] private Sprite originShooter;
    [SerializeField] private Sprite customShooterSprite;

    [HideInInspector] public int startingAmmoCount;
    [HideInInspector] public int prevScore = 0;

    public static int totalScore;
    public static bool shooting;
    public static bool gameOver;
    public static bool gameOverTextChecker;
    public static bool chanShootAgain = true;
    public static int totalPegsToSave;
    public static int totalLevelsToSave;

    public GameData gameData;

    private const float BaseArrowLength = 0.75f;

    private Camera mainCamera;
    private Vector2 aimDirection;
    private GameObject[] trajectoryPoints;
    private MobileShotState mobileShotState = MobileShotState.Idle;

    private float startingProjSpeed;
    private float arrowLength = BaseArrowLength;
    private bool chargeUpDown = true;
    private bool cancelOrNot = true;
    private bool isDesktop = true;
    private bool chargePlaying;
    private bool settingsOpen;
    private bool shootingInputLocked;
    private bool shotChargeStarted;

    private int globalMulti;
    private int localScore;
    private int activeArrowProjectiles;

    private AudioSource chargeAudio;
    private AudioSource shootAudio;
    private SettingsManager settingsManager;
    private GameObject settingsButton;
    private AchPopUp achPopScript;
    private Text multiplierText;

    private Coroutine scorePopupCoroutine;
    private Coroutine scoreCountCoroutine;
    private Coroutine unlockedPopupCoroutine;
    private Coroutine inputLockCoroutine;

    private bool bonus100Awarded;
    private bool bonus250Awarded;
    private bool bonus500Awarded;
    private bool bonus1000Awarded;

    private bool ArrowProjectileActive {
        get { return activeArrowProjectiles > 0; }
    }

    private void Awake() {
        mainCamera = Camera.main;
        gameData = SaveSystem.Load();
        isDesktop = SystemInfo.deviceType != DeviceType.Handheld;

        if (isDesktop && shootButton != null) {
            shootButton.SetActive(false);
        }

        ApplyShooterCustomization();
    }

    private void Start() {
        CacheSceneReferences();
        InitializeTrajectoryPoints();
        InitializeRunState();
        StartCoroutine(spawnPlanet());
    }

    private void Update() {
        UpdateUI();

        if (CanControlShooter()) {
            UpdateAiming();
            HandleChargingAndShooting();
        }
        else {
            HideTrajectoryPoints();
        }

        SaveHighScoreIfNeeded();
        GameOverChecker();
    }

    private void ApplyShooterCustomization() {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null) {
            return;
        }

        int customShooter = PlayerPrefs.GetInt("customShooter");

        if (customShooter == 0 && originShooter != null) {
            spriteRenderer.sprite = originShooter;
        }
        else if (customShooter == 1 && customShooterSprite != null) {
            spriteRenderer.sprite = customShooterSprite;
        }
    }

    private void CacheSceneReferences() {
        GameObject achObject = GameObject.Find("AchPopUpController");
        if (achObject != null) {
            achPopScript = achObject.GetComponent<AchPopUp>();
        }

        if (achPopScript != null && PlayerPrefs.GetFloat("FirstGame") == 0) {
            achPopScript.tutorialAch();
        }

        GameObject multiplierObject = GameObject.Find("multiplierText");
        if (multiplierObject != null) {
            multiplierText = multiplierObject.GetComponent<Text>();
        }

        GameObject chargeObject = GameObject.Find("charge");
        if (chargeObject != null) {
            chargeAudio = chargeObject.GetComponent<AudioSource>();
        }

        GameObject shootObject = GameObject.Find("shoot");
        if (shootObject != null) {
            shootAudio = shootObject.GetComponent<AudioSource>();
        }

        GameObject managerObject = GameObject.Find("Script_Manager");
        if (managerObject != null) {
            settingsManager = managerObject.GetComponent<SettingsManager>();
        }

        settingsButton = GameObject.Find("SettingsButton");
        if (settingsButton != null) {
            settingsButton.SetActive(true);
        }
    }

    private void InitializeTrajectoryPoints() {
        trajectoryPoints = new GameObject[numberOfPoints];

        for (int i = 0; i < trajectoryPoints.Length; i++) {
            trajectoryPoints[i] = Instantiate(pointPrefab, transform.position, Quaternion.identity);
            trajectoryPoints[i].SetActive(false);
        }
    }

    private void InitializeRunState() {
        startingAmmoCount = ammoCount;
        startingProjSpeed = projSpeed;
        arrowLength = BaseArrowLength;
        chargeUpDown = true;
        chanShootAgain = true;
        shooting = false;
        gameOver = false;
        settingsOpen = false;
        chargePlaying = false;
        shootingInputLocked = false;
        shotChargeStarted = false;
        globalMulti = 1;
        localScore = 0;
        activeArrowProjectiles = 0;
        mobileShotState = MobileShotState.Idle;

        if (aimingArrow != null) {
            aimingArrow.SetActive(false);
        }

        if (chargeBar != null) {
            chargeBar.enabled = true;
        }
    }

    private bool CanControlShooter() {
        bool tutorialOpen = settingsManager != null && settingsManager.getTutorialOpen();
        return !gameOver && !settingsOpen && !tutorialOpen && !shootingInputLocked;
    }

    private bool CanShoot() {
        return cancelOrNot && !shooting && ammoCount > 0 && chanShootAgain && !ArrowProjectileActive && !shootingInputLocked;
    }

    private bool IsChargeHeld() {
        return (isDesktop && Input.GetMouseButton(0)) || (!isDesktop && mobileShotState == MobileShotState.Charging);
    }

    private bool IsShotReleased() {
        return (isDesktop && Input.GetMouseButtonUp(0)) || (!isDesktop && mobileShotState == MobileShotState.Released);
    }

    private void UpdateAiming() {
        if (mainCamera == null) {
            mainCamera = Camera.main;
        }

        if (mainCamera == null) {
            return;
        }

        if (!isDesktop && mobileShotState != MobileShotState.Idle) {
            return;
        }

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 rawDirection = mouseWorldPosition - transform.position;

        if (rawDirection.sqrMagnitude <= 0.0001f) {
            return;
        }

        aimDirection = rawDirection.normalized;

        float rotationZ = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
    }

    private void HandleChargingAndShooting() {
        if (IsChargeHeld() && CanShoot()) {
            ContinueChargingShot();
            shotChargeStarted = true;
        }

        if (IsShotReleased() && CanShoot() && shotChargeStarted) {
            ReleaseChargedShot();
            return;
        }

        if (IsShotReleased() && !shotChargeStarted) {
            ResetChargeState(false);
        }
    }

    private void ContinueChargingShot() {
        SetChargeVisualsActive(true);
        UpdateChargeSpeed();
        UpdateAimingArrow();
        UpdateTrajectoryPreview();
        PlayChargeAudio();
        UpdateChargeBar();
    }

    private void UpdateChargeSpeed() {
        float direction = chargeUpDown ? 1f : -1f;

        projSpeed += chargeSpeed * direction * Time.deltaTime;
        projSpeed = Mathf.Clamp(projSpeed, startingProjSpeed, maxChargeSpeed);

        arrowLength += 0.15f * direction * Time.deltaTime;
        arrowLength = Mathf.Max(BaseArrowLength, arrowLength);

        if (projSpeed >= maxChargeSpeed) {
            chargeUpDown = false;
        }
        else if (projSpeed <= startingProjSpeed) {
            chargeUpDown = true;
        }
    }

    private void UpdateAimingArrow() {
        if (aimingArrow == null) {
            return;
        }

        if (!showAimingArrow) {
            aimingArrow.SetActive(false);
            return;
        }

        aimingArrow.SetActive(true);
        aimingArrow.transform.localScale = new Vector2(arrowLength, aimingArrow.transform.localScale.y);
    }

    private void PlayChargeAudio() {
        if (chargeAudio == null || chargePlaying) {
            return;
        }

        chargeAudio.Play();
        chargePlaying = true;
    }

    private void ReleaseChargedShot() {
        shooting = true;
        localScore = 0;
        globalMulti = 1;
        shotChargeStarted = false;

        ResetShotBonusAmmo();
        Fire();

        if (shootAudio != null) {
            shootAudio.Play();
        }

        ResetChargeState(false);
    }

    private void ResetChargeState(bool allowShootAgainAfterDelay) {
        shotChargeStarted = false;

        arrowLength = BaseArrowLength;
        projSpeed = startingProjSpeed;
        chargeUpDown = true;
        mobileShotState = MobileShotState.Idle;

        if (aimingArrow != null) {
            aimingArrow.SetActive(false);
        }

        if (chargeAudio != null) {
            chargeAudio.Stop();
        }

        chargePlaying = false;

        if (chargeBar != null) {
            chargeBar.fillAmount = 0f;
            chargeBar.enabled = false;
        }

        SetShootButtonSprite(0);
        HideTrajectoryPoints();

        if (allowShootAgainAfterDelay) {
            StartCoroutine(resetShot());
        }
    }

    private void SetChargeVisualsActive(bool active) {
        if (chargeBar != null) {
            chargeBar.enabled = active;
        }
    }

    private void UpdateChargeBar() {
        if (chargeBar == null) {
            return;
        }

        chargeBar.fillAmount = projSpeed / maxChargeSpeed;
        chargeBar.color = Color.Lerp(Color.red, Color.green, chargeBar.fillAmount);
    }

    private void UpdateTrajectoryPreview() {
        if (trajectoryPoints == null || trajectoryPoints.Length == 0) {
            return;
        }

        if (usePredictiveCollisionTrace) {
            UpdateCollisionTrajectoryPreview();
        }
        else {
            UpdateSimpleGravityTrajectoryPreview();
        }
    }

    private void UpdateSimpleGravityTrajectoryPreview() {
        for (int i = 0; i < trajectoryPoints.Length; i++) {
            float t = i * traceTimeStep;
            Vector2 point = (Vector2)firePoint.position + ((Vector2)firePoint.right * projSpeed * t) + 0.5f * Physics2D.gravity * (t * t);
            SetTrajectoryPoint(i, point);
        }
    }

    private void UpdateCollisionTrajectoryPreview() {
        Vector2 position = firePoint.position;
        Vector2 velocity = (Vector2)firePoint.right * projSpeed;

        int visiblePointIndex = 0;
        int bounceCount = 0;
        Collider2D lastHitCollider = null;

        HideTrajectoryPoints();

        for (int step = 0; step < numberOfPoints && visiblePointIndex < trajectoryPoints.Length; step++) {
            Vector2 nextVelocity = velocity + Physics2D.gravity * traceTimeStep;
            Vector2 nextPosition = position + velocity * traceTimeStep + 0.5f * Physics2D.gravity * traceTimeStep * traceTimeStep;
            Vector2 travel = nextPosition - position;

            if (travel.sqrMagnitude <= 0.0001f) {
                break;
            }

            RaycastHit2D hit = CastTrajectorySegment(position, travel);

            if (hit.collider != null && !ShouldIgnoreTraceHit(hit, lastHitCollider)) {
                Vector2 hitPosition = hit.point;

                SetTrajectoryPoint(visiblePointIndex, hitPosition);
                visiblePointIndex++;

                velocity = Vector2.Reflect(nextVelocity, hit.normal) * traceBounceDamping;
                position = hitPosition + hit.normal * traceSkinWidth;
                lastHitCollider = hit.collider;

                bounceCount++;
                if (bounceCount >= maxTraceBounces) {
                    break;
                }
            }
            else {
                position = nextPosition;
                velocity = nextVelocity;
                lastHitCollider = null;

                SetTrajectoryPoint(visiblePointIndex, position);
                visiblePointIndex++;
            }
        }
    }

    private RaycastHit2D CastTrajectorySegment(Vector2 start, Vector2 travel) {
        Vector2 direction = travel.normalized;
        float distance = travel.magnitude;

        if (traceRadius > 0f) {
            return Physics2D.CircleCast(start, traceRadius, direction, distance, traceCollisionMask);
        }

        return Physics2D.Raycast(start, direction, distance, traceCollisionMask);
    }

    private bool ShouldIgnoreTraceHit(RaycastHit2D hit, Collider2D lastHitCollider) {
        if (hit.collider == null) {
            return true;
        }

        if (hit.collider == lastHitCollider) {
            return true;
        }

        Transform hitTransform = hit.collider.transform;

        if (hitTransform == transform || hitTransform.IsChildOf(transform)) {
            return true;
        }

        return false;
    }

    private void SetTrajectoryPoint(int index, Vector2 position) {
        if (index < 0 || index >= trajectoryPoints.Length || trajectoryPoints[index] == null) {
            return;
        }

        trajectoryPoints[index].SetActive(true);
        trajectoryPoints[index].transform.position = position;
    }

    private void HideTrajectoryPoints() {
        if (trajectoryPoints == null) {
            return;
        }

        for (int i = 0; i < trajectoryPoints.Length; i++) {
            if (trajectoryPoints[i] != null) {
                trajectoryPoints[i].SetActive(false);
            }
        }
    }

    private void UpdateUI() {
        if (gameData == null) {
            gameData = SaveSystem.Load();
        }

        if (ammoText != null) {
            ammoText.text = "Ammo: " + ammoCount;
        }

        if (scoreText != null) {
            scoreText.text = "Score: " + totalScore;
        }

        if (gameOverHighScoreText != null && gameData != null) {
            gameOverHighScoreText.text = "Highscore: " + gameData.highScore;
        }

        if (fadeText && !ArrowProjectileActive) {
            if (multiplierText != null) {
                multiplierText.text = "";
            }

            StartScorePopup();
        }

        bool hasCharge = chargeBar != null && chargeBar.fillAmount > 0.01f;

        if (maxChargeText != null) {
            maxChargeText.enabled = hasCharge;
        }

        if (chargeImage != null) {
            chargeImage.SetActive(hasCharge);
        }
    }

    private void StartScorePopup() {
        if (scorePopupCoroutine != null) {
            StopCoroutine(scorePopupCoroutine);
        }

        if (scorePopUpText != null) {
            scorePopUpText.color = new Color(scorePopUpText.color.r, scorePopUpText.color.g, scorePopUpText.color.b, 0f);
        }

        scorePopupCoroutine = StartCoroutine(scorePopUpFadeIn());
    }

    private IEnumerator scorePopUpFadeIn() {
        SaveShotStats();
        chargeButtonResetForCompatibility();
        fadeText = false;

        if (scorePopUpText == null) {
            yield break;
        }

        int shotScore = localScore;

        if (shotScore > 0) {
            SaveHighestProjectileScoreIfNeeded();
            scorePopUpText.text = "0";
        }
        else {
            scorePopUpText.text = "0";
        }

        scorePopUpText.color = ScorePopupColor(shotScore, 0f);

        while (scorePopUpText.color.a < 1f) {
            Color c = scorePopUpText.color;
            c.a += Time.deltaTime / 0.25f;
            scorePopUpText.color = c;
            yield return null;
        }

        if (shotScore > 0) {
            if (scoreCountCoroutine != null) {
                StopCoroutine(scoreCountCoroutine);
            }

            yield return StartCoroutine(countTextUp(shotScore));
        }

        yield return new WaitForSeconds(0.75f);

        StartCoroutine(scorePopUpFadeOut());
    }

    private IEnumerator scorePopUpFadeOut() {
        if (scorePopUpText == null) {
            yield break;
        }

        Color c = scorePopUpText.color;
        c.a = 1f;
        scorePopUpText.color = c;

        while (scorePopUpText.color.a > 0f) {
            c = scorePopUpText.color;
            c.a -= Time.deltaTime / 1f;
            scorePopUpText.color = c;
            yield return null;
        }
    }

    private IEnumerator countTextUp(int newVal) {
        if (scorePopUpText == null) {
            yield break;
        }

        float countDuration = Mathf.Clamp(newVal * 0.015f, 0.35f, 1.25f);
        float elapsed = 0f;

        while (elapsed < countDuration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / countDuration);

            int displayValue = Mathf.RoundToInt(Mathf.Lerp(0, newVal, t));
            scorePopUpText.text = displayValue.ToString("N0");

            yield return null;
        }

        scorePopUpText.text = newVal.ToString("N0");
    }

    private Color ScorePopupColor(int score, float alpha) {
        Color color;

        if (score < 10) {
            color = Color.red;
        }
        else if (score < 100) {
            color = Color.white;
        }
        else {
            color = Color.green;
        }

        color.a = alpha;
        return color;
    }

    private void SaveShotStats() {
        gameData = SaveSystem.Load();

        if (gameData == null) {
            return;
        }

        gameData.totalPegs += totalPegsToSave;
        gameData.totalLevels += totalLevelsToSave;
        SaveSystem.Save(gameData);

        totalPegsToSave = 0;
        totalLevelsToSave = 0;
    }

    private void PrintHighestProjectileScoreCheck(int shotScoreToStore) {
        int currentStoredHigh = gameData != null ? gameData.highestProjScore : -1;
        bool willSave = shotScoreToStore > currentStoredHigh;

        Debug.Log(
            "SHOT SCORE CHECK | " +
            "localScore: " + localScore +
            " | shotScoreToStore: " + shotScoreToStore +
            " | current highestProjScore: " + currentStoredHigh +
            " | will save: " + willSave
        );
    }

    private void SaveHighestProjectileScoreIfNeeded() {
        if (gameData == null) {
            gameData = SaveSystem.Load();
        }

        if (gameData == null) {
            return;
        }

        int shotScoreToStore = localScore;

        PrintHighestProjectileScoreCheck(shotScoreToStore);

        if (shotScoreToStore > gameData.highestProjScore) {
            gameData.highestProjScore = shotScoreToStore;
            SaveSystem.Save(gameData);

            Debug.Log("NEW highestProjScore SAVED: " + gameData.highestProjScore);
        }
    }

    private void SaveHighScoreIfNeeded() {
        if (gameData == null) {
            gameData = SaveSystem.Load();
        }

        if (gameData == null) {
            return;
        }

        if (totalScore > gameData.highScore) {
            gameData.highScore = totalScore;
            SaveSystem.Save(gameData);
        }
    }

    private IEnumerator unlockedPopUpFadeIn() {
        if (unlockedPopUpText == null || imageOfPeg == null) {
            yield break;
        }

        SetUnlockedPopupAlpha(0f);

        while (unlockedPopUpText.color.a < 1f) {
            SetUnlockedPopupAlpha(unlockedPopUpText.color.a + Time.deltaTime / 1f);
            yield return null;
        }

        StartCoroutine(unlockedPopUpFadeOut());
    }

    private IEnumerator unlockedPopUpFadeOut() {
        if (unlockedPopUpText == null || imageOfPeg == null) {
            yield break;
        }

        SetUnlockedPopupAlpha(1f);

        while (unlockedPopUpText.color.a > 0f) {
            SetUnlockedPopupAlpha(unlockedPopUpText.color.a - Time.deltaTime / 3f);
            yield return null;
        }
    }

    private void SetUnlockedPopupAlpha(float alpha) {
        alpha = Mathf.Clamp01(alpha);

        Color textColor = unlockedPopUpText.color;
        textColor.a = alpha;
        unlockedPopUpText.color = textColor;

        Color imageColor = imageOfPeg.color;
        imageColor.a = alpha;
        imageOfPeg.color = imageColor;
    }

    public void startUnlockedPopUp(GameObject incoming_peg) {
        if (incoming_peg == null || imageOfPeg == null) {
            return;
        }

        SpriteRenderer pegSprite = incoming_peg.GetComponent<SpriteRenderer>();
        if (pegSprite != null) {
            imageOfPeg.sprite = pegSprite.sprite;
        }

        if (unlockedPopupCoroutine != null) {
            StopCoroutine(unlockedPopupCoroutine);
        }

        unlockedPopupCoroutine = StartCoroutine(unlockedPopUpFadeIn());
    }

    public void chargeShot() {
        if (shootingInputLocked) {
            return;
        }

        mobileShotState = MobileShotState.Charging;
        cancelOrNot = true;

        if (!shooting) {
            SetShootButtonSprite(1);
        }
    }

    public void releaseShot() {
        if (shootingInputLocked) {
            return;
        }

        if (mobileShotState == MobileShotState.Charging) {
            mobileShotState = MobileShotState.Released;
            shotChargeStarted = true;
        }

        SetShootButtonSprite(0);
        HideTrajectoryPoints();
    }

    public void cancelShot() {
        if (mobileShotState != MobileShotState.Charging) {
            return;
        }

        mobileShotState = MobileShotState.Idle;
        shotChargeStarted = false;
        cancelOrNot = false;

        if (!shootingInputLocked) {
            chanShootAgain = true;
        }

        shooting = false;
        ResetChargeState(true);
    }

    private void SetShootButtonSprite(int spriteIndex) {
        if (shootButton == null || shootButtonSprites == null || shootButtonSprites.Length <= spriteIndex) {
            return;
        }

        Image buttonImage = shootButton.GetComponent<Image>();
        if (buttonImage != null) {
            buttonImage.sprite = shootButtonSprites[spriteIndex];
        }
    }

    private void chargeButtonResetForCompatibility() {
        mobileShotState = MobileShotState.Idle;
    }

    private void GameOverChecker() {
        if (ammoCount <= 0) {
            if (scorePopupCoroutine != null) {
                StopCoroutine(scorePopupCoroutine);
            }

            if (scoreCountCoroutine != null) {
                StopCoroutine(scoreCountCoroutine);
            }

            if (scorePopUpText != null) {
                scorePopUpText.color = new Color(scorePopUpText.color.r, scorePopUpText.color.g, scorePopUpText.color.b, 0f);
            }

            if (gameOverScoreText != null) {
                gameOverScoreText.text = "Score: " + totalScore;
            }

            SaveHighScoreIfNeeded();

            if (gameData != null && gameOverHighScoreText != null) {
                gameOverHighScoreText.text = "Highscore: " + gameData.highScore;
            }
        }

        if (gameOver) {
            if (settingsButton != null) {
                settingsButton.SetActive(false);
            }

            if (gameOverPanel != null) {
                gameOverPanel.SetActive(true);
            }

            prevScore = 0;

            if (scorePopUpText != null) {
                scorePopUpText.color = new Color(scorePopUpText.color.r, scorePopUpText.color.g, scorePopUpText.color.b, 0f);
            }

            if (multiplierText != null) {
                multiplierText.text = "";
            }

            if (!isDesktop) {
                cancelOrNot = false;

                if (!shootingInputLocked) {
                    chanShootAgain = true;
                }

                shooting = false;
                ResetChargeState(true);
            }
        }
        else {
            if (ammoCount > 0 && !shootingInputLocked) {
                chanShootAgain = true;
            }

            if (gameOverPanel != null) {
                gameOverPanel.SetActive(false);
            }
        }
    }

    public bool getSettingsOpen() {
        return settingsOpen;
    }

    public void setSettingsToClose() {
        if (settingsOpen) {
            StartCoroutine(readyToPlay());
            return;
        }

        if (settings_button != null) {
            settings_button.SetActive(false);
        }

        if (aimingArrow != null) {
            aimingArrow.SetActive(false);
        }

        settingsOpen = true;

        if (chargeAudio != null) {
            chargeAudio.Stop();
        }

        chargePlaying = false;

        if (chargeBar != null) {
            chargeBar.enabled = false;
        }

        fadeText = false;

        if (scorePopupCoroutine != null) {
            StopCoroutine(scorePopupCoroutine);
        }

        if (scorePopUpText != null) {
            scorePopUpText.color = new Color(scorePopUpText.color.r, scorePopUpText.color.g, scorePopUpText.color.b, 0f);
        }

        HideTrajectoryPoints();
    }

    private IEnumerator readyToPlay() {
        yield return new WaitForSeconds(0.5f);
        settingsOpen = false;

        if (settings_button != null) {
            settings_button.SetActive(true);
        }
    }

    private void Fire() {
        gameData = SaveSystem.Load();

        if (gameData != null) {
            gameData.shotsFired++;
            SaveSystem.Save(gameData);
        }

        GameObject spawnedBullet = Instantiate(projectile, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = spawnedBullet.GetComponent<Rigidbody2D>();

        if (rb != null) {
            rb.velocity = firePoint.right * projSpeed;
        }

        ammoCount--;
    }

    public void addTotalScore(int score, int multi) {
        int amount = score * multi;
        totalScore += amount;
        localScore += amount;
    }

    public void addScore(int score) {
        int amount = score * globalMulti;
        totalScore += amount;
        localScore += amount;
    }

    public int getLocalScore() {
        return localScore;
    }

    public int getGlobalMulti() {
        return globalMulti;
    }

    public void increaseGlobalMulti() {
        globalMulti *= 2;
    }

    public bool isShooting() {
        return shooting;
    }

    public void activateArrowProj() {
        activeArrowProjectiles++;
    }

    public void deactivateArrowProj() {
        activeArrowProjectiles = Mathf.Max(0, activeArrowProjectiles - 1);
    }

    public bool returnGameOver() {
        return gameOver;
    }

    public void clearSave() {
        if (gameData == null) {
            gameData = SaveSystem.Load();
        }

        if (gameData != null) {
            gameData.highScore = 0;
            SaveSystem.Save(gameData);
        }
    }

    public void resetLocalScore() {
        totalScore = 0;
        localScore = 0;
    }

    public void LockShootingInput(float seconds) {
        if (inputLockCoroutine != null) {
            StopCoroutine(inputLockCoroutine);
        }

        inputLockCoroutine = StartCoroutine(LockShootingInputRoutine(seconds));
    }

    private IEnumerator LockShootingInputRoutine(float seconds) {
        shootingInputLocked = true;
        chanShootAgain = false;
        shotChargeStarted = false;
        mobileShotState = MobileShotState.Idle;
        ResetChargeState(false);
        HideTrajectoryPoints();

        yield return new WaitForSeconds(seconds);

        while (Input.GetMouseButton(0) || Input.touchCount > 0) {
            yield return null;
        }

        yield return null;

        shootingInputLocked = false;
        chanShootAgain = true;
        shotChargeStarted = false;
        inputLockCoroutine = null;
    }

    private IEnumerator spawnPlanet() {
        int waitTime = Random.Range(25, 35);
        yield return new WaitForSeconds(waitTime);

        if (planets != null && planets.Length > 0) {
            int randoPlanet = Random.Range(0, planets.Length);
            Instantiate(planets[randoPlanet], new Vector2(-12.0f, Random.Range(-6.5f, 6.5f)), Quaternion.identity);
        }

        StartCoroutine(spawnPlanet());
    }

    private IEnumerator resetShot() {
        yield return new WaitForSeconds(1f);

        if (!shootingInputLocked) {
            cancelOrNot = true;
        }
    }

    private void OnDestroy() {
        if (gameData != null) {
            SaveSystem.Save(gameData);
        }
    }

    public void CheckShotBonusAmmo() {
        int shotScore = getLocalScore();

        if (shotScore >= 100 && !bonus100Awarded) {
            bonus100Awarded = true;
            ammoCount += 1;
        }

        if (shotScore >= 250 && !bonus250Awarded) {
            bonus250Awarded = true;
            ammoCount += 1;
        }

        if (shotScore >= 500 && !bonus500Awarded) {
            bonus500Awarded = true;
            ammoCount += 1;
        }

        if (shotScore >= 1000 && !bonus1000Awarded) {
            bonus1000Awarded = true;
            ammoCount += 2;
        }
    }

    private void ResetShotBonusAmmo() {
        bonus100Awarded = false;
        bonus250Awarded = false;
        bonus500Awarded = false;
        bonus1000Awarded = false;
    }

    public List<PlanetSaveData> GetCurrentPlanetState() {
        List<PlanetSaveData> planetData = new List<PlanetSaveData>();

        planetScript[] activePlanets = FindObjectsOfType<planetScript>();

        foreach (planetScript planet in activePlanets) {
            if (planet == null || !planet.gameObject.activeInHierarchy) {
                continue;
            }

            PlanetSaveData data = new PlanetSaveData();

            data.saveId = GetSaveIdFromObjectName(planet.gameObject.name);
            data.position = planet.transform.position;
            data.scale = planet.transform.localScale;
            data.moveSpeed = planet.GetMoveSpeed();

            planetData.Add(data);
        }

        return planetData;
    }

    public void RestorePlanetState(List<PlanetSaveData> savedPlanets) {
        ClearActivePlanets();

        if (savedPlanets == null || savedPlanets.Count == 0) {
            return;
        }

        for (int i = 0; i < savedPlanets.Count; i++) {
            PlanetSaveData data = savedPlanets[i];
            GameObject prefab = GetPlanetPrefabBySaveId(data.saveId);

            if (prefab == null) {
                Debug.LogWarning("No planet prefab mapped for saveId: " + data.saveId);
                continue;
            }

            GameObject restoredPlanet = Instantiate(prefab, data.position, Quaternion.identity);
            restoredPlanet.transform.localScale = data.scale;

            planetScript planet = restoredPlanet.GetComponent<planetScript>();
            if (planet != null) {
                planet.SetMoveSpeed(data.moveSpeed);
            }
        }
    }

    private void ClearActivePlanets() {
        planetScript[] activePlanets = FindObjectsOfType<planetScript>();

        foreach (planetScript planet in activePlanets) {
            if (planet != null) {
                Destroy(planet.gameObject);
            }
        }
    }

    private GameObject GetPlanetPrefabBySaveId(string saveId) {
        if (string.IsNullOrWhiteSpace(saveId) || planets == null) {
            return null;
        }

        for (int i = 0; i < planets.Length; i++) {
            if (planets[i] == null) {
                continue;
            }

            if (string.Equals(planets[i].name, saveId, System.StringComparison.OrdinalIgnoreCase)) {
                return planets[i];
            }
        }

        return null;
    }

    private string GetSaveIdFromObjectName(string objectName) {
        if (string.IsNullOrWhiteSpace(objectName)) {
            return "";
        }

        return objectName.Replace("(Clone)", "").Trim();
    }
}