using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Tutorial_Shooter : MonoBehaviour
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

    [Header("Tutorial UI")]
    [SerializeField] private GameObject tutorial;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject tutorial2;
    [SerializeField] private GameObject PlayButton;

    [Header("Tutorial State")]
    [SerializeField] public int tutorialPegsActive;

    [Header("Background")]
    [SerializeField] private GameObject[] planets;

    [HideInInspector] public int startingAmmoCount;
    [HideInInspector] public int prevScore = 0;

    public Text scorePopUpText;
    public Text unlockedPopUpText;
    public Image imageOfPeg;
    [HideInInspector] public bool fadeText = false;

    public static int totalScore;
    public static bool shooting;
    public static bool gameOver;
    public static bool chanShootAgain = true;

    public GameData gameData;

    private const float BaseArrowLength = 0.75f;

    private Camera mainCamera;
    private GameObject[] trajectoryPoints;
    private MobileShotState mobileShotState = MobileShotState.Idle;

    private float startingProjSpeed;
    private float arrowLength = BaseArrowLength;
    private bool chargeUpDown = true;
    private bool cancelOrNot = true;
    private bool isDesktop = true;
    private bool chargePlaying;
    private bool settingsOpen;
    private bool tutorialOpen = true;

    private AudioSource chargeAudio;
    private AudioSource shootAudio;
    private AudioSource blipSelectAudio;
    private SettingsManager settingsManager;

    private void Awake() {
        mainCamera = Camera.main;
        gameData = SaveSystem.Load();
        isDesktop = SystemInfo.deviceType != DeviceType.Handheld;

        if (isDesktop && shootButton != null) {
            shootButton.SetActive(false);
        }
    }

    private void Start() {
        CacheSceneReferences();
        InitializeTrajectoryPoints();
        InitializeTutorialState();
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
    }

    private void CacheSceneReferences() {
        chargeAudio = FindAudioSource("charge");
        shootAudio = FindAudioSource("shoot");
        blipSelectAudio = FindAudioSource("blipSelect");

        GameObject managerObject = GameObject.Find("Script_Manager");
        if (managerObject != null) {
            settingsManager = managerObject.GetComponent<SettingsManager>();
        }

        GameObject settingsButton = GameObject.Find("SettingsButton");
        if (settingsButton != null) {
            settingsButton.SetActive(true);
        }
    }

    private AudioSource FindAudioSource(string objectName) {
        GameObject audioObject = GameObject.Find(objectName);
        return audioObject != null ? audioObject.GetComponent<AudioSource>() : null;
    }

    private void InitializeTrajectoryPoints() {
        trajectoryPoints = new GameObject[numberOfPoints];

        for (int i = 0; i < trajectoryPoints.Length; i++) {
            trajectoryPoints[i] = Instantiate(pointPrefab, transform.position, Quaternion.identity);
            trajectoryPoints[i].SetActive(false);
        }
    }

    private void InitializeTutorialState() {
        startingAmmoCount = ammoCount;
        startingProjSpeed = projSpeed;
        arrowLength = BaseArrowLength;
        chargeUpDown = true;
        chanShootAgain = true;
        shooting = false;
        gameOver = false;
        settingsOpen = false;
        tutorialOpen = true;
        chargePlaying = false;
        mobileShotState = MobileShotState.Idle;

        if (aimingArrow != null) {
            aimingArrow.SetActive(false);
        }

        if (chargeBar != null) {
            chargeBar.enabled = true;
        }
    }

    private bool CanControlShooter() {
        bool tutorialSettingsOpen = settingsManager != null && settingsManager.getTutorialOpen();
        return !settingsOpen && !tutorialOpen && !tutorialSettingsOpen && tutorialPegsActive > 0;
    }

    private bool CanShoot() {
        return cancelOrNot && !shooting && chanShootAgain;
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
        Vector2 aimDirection = mouseWorldPosition - transform.position;

        if (aimDirection.sqrMagnitude <= 0.0001f) {
            return;
        }

        aimDirection.Normalize();

        float rotationZ = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
    }

    private void HandleChargingAndShooting() {
        if (IsChargeHeld() && CanShoot()) {
            ContinueChargingShot();
        }

        if (IsShotReleased() && CanShoot()) {
            ReleaseChargedShot();
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

        Fire();

        if (shootAudio != null) {
            shootAudio.Play();
        }

        ResetChargeState(false);
    }

    private void ResetChargeState(bool allowShootAgainAfterDelay) {
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
        bool hasCharge = chargeBar != null && chargeBar.fillAmount > 0.01f;

        if (maxChargeText != null) {
            maxChargeText.enabled = hasCharge;
        }

        if (chargeImage != null) {
            chargeImage.SetActive(hasCharge);
        }

        if (ammoText != null) {
            ammoText.text = "Ammo: " + ammoCount;
        }

        if (scoreText != null) {
            scoreText.text = "";
        }

        if (gameOverPanel != null) {
            gameOverPanel.SetActive(false);
        }
    }

    public void chargeShot() {
        mobileShotState = MobileShotState.Charging;
        cancelOrNot = true;

        if (!shooting) {
            SetShootButtonSprite(1);
        }
    }

    public void releaseShot() {
        if (mobileShotState == MobileShotState.Charging) {
            mobileShotState = MobileShotState.Released;
        }

        SetShootButtonSprite(0);
        HideTrajectoryPoints();
    }

    public void cancelShot() {
        if (mobileShotState != MobileShotState.Charging) {
            return;
        }

        mobileShotState = MobileShotState.Idle;
        cancelOrNot = false;
        chanShootAgain = true;
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

        HideTrajectoryPoints();
    }

    private IEnumerator readyToPlay() {
        yield return new WaitForSeconds(0.5f);

        settingsOpen = false;

        if (settings_button != null) {
            settings_button.SetActive(true);
        }
    }

    public void closeTutorial() {
        if (tutorial != null) {
            tutorial.SetActive(false);
        }

        if (continueButton != null) {
            continueButton.SetActive(false);
        }

        if (blipSelectAudio != null) {
            blipSelectAudio.Play();
        }

        StartCoroutine(closeTutorialEnum());
    }

    private IEnumerator closeTutorialEnum() {
        yield return new WaitForSeconds(0.5f);
        tutorialOpen = false;
    }

    private void Fire() {
        if (projectile == null || firePoint == null) {
            return;
        }

        GameObject spawnedBullet = Instantiate(projectile, firePoint.position, firePoint.rotation);
        Rigidbody2D projectileRb = spawnedBullet.GetComponent<Rigidbody2D>();

        if (projectileRb != null) {
            projectileRb.velocity = firePoint.right * projSpeed;
        }
    }

    private IEnumerator resetShot() {
        yield return new WaitForSeconds(1f);
        cancelOrNot = true;
    }

    public void lowerTutorialPegCount() {
        tutorialPegsActive--;
    }

    public int getTutorialPegsActive() {
        return tutorialPegsActive;
    }

    public void tutorial2Open() {
        if (tutorial2 != null) {
            tutorial2.SetActive(true);
        }

        if (PlayButton != null) {
            PlayButton.SetActive(true);
        }
    }

    public void play() {
        if (blipSelectAudio != null) {
            blipSelectAudio.Play();
            StartCoroutine(changeScene("infiniteLevel", blipSelectAudio.clip.length));
        }
        else {
            SceneManager.LoadScene("infiniteLevel");
        }
    }

    private IEnumerator changeScene(string scene, float time) {
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(scene);
    }

    public void setChargeButtonShoot() {
        mobileShotState = MobileShotState.Idle;
    }
}