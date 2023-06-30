using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Tutorial_Shooter : MonoBehaviour
{
    [Header("Firing variables")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private float projSpeed, maxChargeSpeed, chargeSpeed;
    [SerializeField] private Transform firePoint;
    [SerializeField] public int ammoCount;
    [SerializeField] private GameObject aimingArrow;
    private float startingProjSpeed;
    [HideInInspector] public int startingAmmoCount;
    private float arrowXPos = 0, arrowLength = .75f;
    [HideInInspector] public int prevScore = 0;
    private Vector3 difference;
    private int chargeButtonShoot = 0;
    private bool cancelOrNot = true;
    private GameObject[] points;    //the points to place for the proj path
    [SerializeField] private int numberOfPoints;    //max number of points to place
    [SerializeField] private GameObject pointPrefab;
    private float rotation_z;
    public int tutorialPegsActive;
    private AudioSource audioClip;
    //[Space(20)]

    [Header("UI variables")]
    [SerializeField] private Text ammoText, maxChargeText;
    [SerializeField] private Text scoreText, gameOverScoreText, gameOverHighScoreText;
    [SerializeField] private Image chargeBar;
    [SerializeField] private GameObject gameOverPanel, chargeImage;
    public Text scorePopUpText;
    public Text unlockedPopUpText;
    public Image imageOfPeg;
    [HideInInspector] public bool fadeText = false;
    [SerializeField] private GameObject[] planets;
    [SerializeField] private GameObject shootButton;
    [SerializeField] private Sprite[] shootButtonSprites;
    [SerializeField] private GameObject cancelButton;

    private int countVal;
    [SerializeField] private int countFPS;
    [SerializeField] private float duration;
    //[Space(20)]

    public static int totalScore;
    public static bool shooting;
    public static bool gameOver;
    private bool chargeUpDown = true;
    public static bool chanShootAgain = true;
    private bool pcOrMobile = true;

    // sounds
    bool charge_playing;
    private AudioSource charge;
    private AudioSource shoot;
    
    private SettingsManager set;
    private bool settingsOpen;
    private GameObject settingsButton;

    public GameData gameData;

    public GameObject settings_button;

    private bool tutorialOpen;
    public GameObject tutorial;
    public GameObject continueButton;
    public GameObject tutorial2;
    public GameObject PlayButton;

    private void Awake()
    {
        gameData = SaveSystem.Load();
        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            pcOrMobile = false;
        }
        else
        {
            shootButton.SetActive(false);
        }
    }

    void Start()
    {
        //used to initlize points
        points = new GameObject[numberOfPoints];
        for (int i = 0; i < numberOfPoints; i++)
        {
            points[i] = Instantiate(pointPrefab, transform.position, Quaternion.identity);
        }

        startingProjSpeed = projSpeed;
        chargeUpDown = true;
        chanShootAgain = true;
        shooting = false;
        chargeBar.enabled = true;
        // sounds
        charge = GameObject.Find("charge").GetComponent<AudioSource>();
        charge_playing = false;
        shoot = GameObject.Find("shoot").GetComponent<AudioSource>();
        set = GameObject.Find("Script_Manager").GetComponent<SettingsManager>();
        settingsOpen = false;
        settingsButton = GameObject.Find("SettingsButton");
        settingsButton.SetActive(true);
        tutorialOpen = true;
    }

    //points function
    Vector2 pointPos(float t)
    {
        Vector2 currPos = (Vector2)firePoint.transform.position + ((Vector2)difference * projSpeed * t) + 0.5f * Physics2D.gravity * (t * t);
        return currPos;
    }
    void updatePoints()
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i].SetActive(true);
            points[i].transform.position = pointPos(i * 0.1f);  //can play around with the number
        }
    }

    private void UpdateUI()
    {
        if (chargeBar.fillAmount <= .01)
        {
            maxChargeText.enabled = false;
            chargeImage.SetActive(false);
        }
        else
        {
            maxChargeText.enabled = true;
            chargeImage.SetActive(true);
        }
    }
    private void Aiming()
    {
        //if(pcOrMobile){
        //difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        //}
        //else{
        //difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        //}
        //difference.Normalize();

        //line below is for CLAMPING
        if (chargeButtonShoot == 0)
        {
            difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            difference.Normalize();
            rotation_z = (Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg); //Mathf.Clamp((Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg), -180, 0);

            transform.rotation = Quaternion.Euler(0f, 0f, rotation_z);  //can add an offset to the rotz if needed   
        }
    }
    //for the button
    public void chargeShot()
    {
        //cancelButton.SetActive(true);
        chargeButtonShoot = 1;
        cancelOrNot = true;
        //shooting = false;
        if (shooting == false)
        {
            shootButton.GetComponent<Image>().sprite = shootButtonSprites[1];
        }
    }
    public void releaseShot()
    {
        //cancelButton.SetActive(false);
        if (chargeButtonShoot == 1)
        {
            chargeButtonShoot = 2;
        }
        shootButton.GetComponent<Image>().sprite = shootButtonSprites[0];
        for (int i = 0; i < points.Length; i++)
        {
            points[i].SetActive(false);
        }
    }
    public void cancelShot()
    {
        if (chargeButtonShoot == 1)
        {
            chargeButtonShoot = 0;
            cancelOrNot = false;
            //if(chanShootAgain){
            StartCoroutine(resetShot());
            //}
            //cancelButton.SetActive(true);

            shootButton.GetComponent<Image>().sprite = shootButtonSprites[0];
            chanShootAgain = true;
            shooting = false;

            arrowLength = .75f;
            arrowXPos = 0;

            //shooting = true;
            aimingArrow.SetActive(false);
            charge.Stop();
            charge_playing = false;
            projSpeed = startingProjSpeed;
            chargeBar.enabled = false;
            for (int i = 0; i < points.Length; i++)
            {
                points[i].SetActive(false);
            }
        }
    }

    //for pc controls
    private void ChargingNShooting()
    {
        //Input.GetMouseButton(0) || 
        if (((Input.GetMouseButton(0) && pcOrMobile) || (chargeButtonShoot == 1 && pcOrMobile == false)) && cancelOrNot && shooting == false && chanShootAgain)
        {
            chargeBar.enabled = true;
            //aimingArrow.SetActive(true);

            //used to update the aiming points
            updatePoints();

            if (chargeUpDown)
            {
                projSpeed += chargeSpeed * Time.deltaTime;
                arrowXPos += .15f * Time.deltaTime;
                arrowLength += .15f * Time.deltaTime;
                projSpeed = Mathf.Clamp(projSpeed, startingProjSpeed, maxChargeSpeed);
                if (projSpeed >= maxChargeSpeed)
                {
                    chargeUpDown = false;
                }
            }
            else
            {
                projSpeed -= chargeSpeed * Time.deltaTime;
                arrowXPos -= .15f * Time.deltaTime;
                arrowLength -= .15f * Time.deltaTime;
                projSpeed = Mathf.Clamp(projSpeed, startingProjSpeed, maxChargeSpeed);
                if (projSpeed <= startingProjSpeed)
                {
                    chargeUpDown = true;
                }
            }
            if (!charge_playing)
            {
                charge.Play();
                charge_playing = true;
            }
            //aimingArrow.transform.position = new Vector2(transform.position.x+arrowXPos,aimingArrow.transform.position.y);
            //aimingArrow.transform.position += transform.forward * 2 * Time.deltaTime;
            aimingArrow.transform.localScale = new Vector2(arrowLength, aimingArrow.transform.localScale.y);
            ChargeBarFiller();
            ColorChanger();
        }
        // 
        if (((Input.GetMouseButtonUp(0) && pcOrMobile) || (chargeButtonShoot == 2 && pcOrMobile == false)) && cancelOrNot && shooting == false && chanShootAgain)
        {
            arrowLength = .75f;
            arrowXPos = 0;
            shooting = true;
            aimingArrow.SetActive(false);
            charge.Stop();
            charge_playing = false;
            //print(projSpeed);
            Fire();
            shoot.Play();
            projSpeed = startingProjSpeed;
            chargeBar.fillAmount = 0;
            chargeBar.enabled = false;
            for (int i = 0; i < points.Length; i++)
            {
                points[i].SetActive(false);
            }
        }
    }

    void Update()
    {

        UpdateUI();
        if (!settingsOpen && !tutorialOpen && tutorialPegsActive > 0)
        {
            Aiming();

            ChargingNShooting();


        }
        if (settingsOpen)
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i].SetActive(false);
            }
        }
    }

    public bool getSettingsOpen()
    {
        return settingsOpen;
    }

    public void setSettingsToClose()
    {
        if (settingsOpen)
        {

            StartCoroutine(readyToPlay());
        }
        else
        {
            settings_button.SetActive(false);
            aimingArrow.SetActive(false);
            settingsOpen = true;
            charge.Stop();
            charge_playing = false;
            chargeBar.enabled = false;
        }
    }

    IEnumerator readyToPlay()
    {
        yield return new WaitForSeconds(0.5f);
        settingsOpen = false;
        settings_button.SetActive(true);
    }

    public void closeTutorial()
    {
        tutorial.SetActive(false);
        continueButton.SetActive(false);
        audioClip = GameObject.Find("blipSelect").GetComponent<AudioSource>();
        audioClip.Play();
        StartCoroutine(closeTutorialEnum());
    }

    IEnumerator closeTutorialEnum()
    {
        yield return new WaitForSeconds(0.5f);
        tutorialOpen = false;
    }

    void ColorChanger()
    {
        Color chargeColor = Color.Lerp(Color.red, Color.green, (projSpeed / maxChargeSpeed));
        chargeBar.color = chargeColor;
    }

    void ChargeBarFiller()
    {
        chargeBar.fillAmount = projSpeed / maxChargeSpeed;
    }

    void Fire()
    {
        GameObject spawnedBullet = Instantiate(projectile, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = spawnedBullet.GetComponent<Rigidbody2D>();
        rb.velocity = firePoint.right * projSpeed;
        //rb.AddForce(firePoint.right * projSpeed, ForceMode2D.Impulse);        //origional firing
    }

    IEnumerator resetShot()
    {

        yield return new WaitForSeconds(1f);
        cancelOrNot = true;
    }

    public void lowerTutorialPegCount()
    {
        tutorialPegsActive--;
    }

    public int getTutorialPegsActive()
    {
        return tutorialPegsActive;
    }

    public void tutorial2Open()
    {
        tutorial2.SetActive(true);
        PlayButton.SetActive(true);
    }

    public void play()
    {
        audioClip = GameObject.Find("blipSelect").GetComponent<AudioSource>();
        audioClip.Play();
        StartCoroutine(changeScene("infiniteLevel", audioClip.clip.length));
    }

    IEnumerator changeScene(string scene, float time)
    {
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(scene);
    }

    public void setChargeButtonShoot()
    {
        chargeButtonShoot = 0;
    }
}
