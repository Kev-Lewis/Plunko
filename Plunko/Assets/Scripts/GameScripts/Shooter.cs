using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shooter : MonoBehaviour
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
    public static bool gameOver, gameOverTextChecker;
    private bool chargeUpDown = true;
    public static bool chanShootAgain = true;
    private bool pcOrMobile = true;
    public static int totalPegsToSave;
    public static int totalLevelsToSave;
    // sounds
    bool charge_playing;
    private AudioSource charge;
    private AudioSource shoot;

    private SettingsManager set;
    private bool settingsOpen;
    private GameObject settingsButton;

    public GameData gameData;

    public GameObject settings_button;

    // slides
    private int globalMulti;
    private int localScore;

    // arrowProj
    private int amt_of_arrowProj_active;
    private bool arrowProjActive;

    private AchPopUp achPopScript;
    private Text multiplierText;

    //customization stuff
    [SerializeField] private Sprite originShooter;
    [SerializeField] private Sprite customShooterSprite;
   

    private void Awake() {
        gameData = SaveSystem.Load();
        if(SystemInfo.deviceType == DeviceType.Handheld){
            pcOrMobile = false;
        }else{
            shootButton.SetActive(false);
        }

        if(PlayerPrefs.GetInt("customShooter") == 0){
            GetComponent<SpriteRenderer>().sprite = originShooter;
        }
        if(PlayerPrefs.GetInt("customShooter") == 1){
            GetComponent<SpriteRenderer>().sprite = customShooterSprite;
        }
        
        
    }
    
    void Start()
    {
        achPopScript = GameObject.Find("AchPopUpController").GetComponent<AchPopUp>();
        if (PlayerPrefs.GetFloat("FirstGame") == 0)
        {
            achPopScript.tutorialAch();
        }
        //used to initlize points
        points = new GameObject[numberOfPoints];
        for(int i = 0; i < numberOfPoints; i++){
            points[i] = Instantiate(pointPrefab, transform.position, Quaternion.identity);
        }
        multiplierText = GameObject.Find("multiplierText").GetComponent<Text>();
        startingAmmoCount = ammoCount;
        startingProjSpeed = projSpeed;
        chargeUpDown = true;
        chanShootAgain = true;
        shooting = false;
        gameOver = false;
        chargeBar.enabled = true;
        globalMulti = 1;
        amt_of_arrowProj_active = 0;
        arrowProjActive = false;
        // sounds
        charge = GameObject.Find("charge").GetComponent<AudioSource>();
        charge_playing = false;
        shoot = GameObject.Find("shoot").GetComponent<AudioSource>();
        set = GameObject.Find("Script_Manager").GetComponent<SettingsManager>();
        settingsOpen = false;
        settingsButton = GameObject.Find("SettingsButton");
        settingsButton.SetActive(true);
        StartCoroutine(spawnPlanet());
    }

    //points function
    Vector2 pointPos(float t){
        Vector2 currPos = (Vector2)firePoint.transform.position + ((Vector2)difference * projSpeed * t) + 0.5f * Physics2D.gravity * (t*t);
        return currPos;
    }
    void updatePoints(){
        for(int i = 0; i < points.Length; i++){
            points[i].SetActive(true);
            points[i].transform.position = pointPos(i * 0.1f);  //can play around with the number
        }
    }

    IEnumerator scorePopUpFadeIn(){
        gameData = SaveSystem.Load();
        gameData.totalPegs += totalPegsToSave;
        gameData.totalLevels += totalLevelsToSave;
        SaveSystem.Save(gameData);
        totalPegsToSave = 0;
        totalLevelsToSave = 0;
        //print(gameData.totalPegs);

        //scorePopUpText.text = "" + localScore;
        if(localScore > 0){
            if(localScore > gameData.highestProjScore){
                gameData.highestProjScore = localScore;
                SaveSystem.Save(gameData);
            }
            StartCoroutine(countTextUp(localScore));
        }
        else{
            scorePopUpText.text = "0";
        }
        chargeButtonShoot = 0;
        fadeText = false;
        if(getLocalScore() < 10){
            scorePopUpText.color = Color.red;
        }
        else if(getLocalScore() < 100){
            scorePopUpText.color = Color.white;
        }
        if(getLocalScore() >= 100){
            scorePopUpText.color = Color.green;
        }
        
        scorePopUpText.color = new Color(scorePopUpText.color.r, scorePopUpText.color.g, scorePopUpText.color.b, 0);
        
        while(scorePopUpText.color.a < 1.0f){
            scorePopUpText.color = new Color(scorePopUpText.color.r, scorePopUpText.color.g, scorePopUpText.color.b, scorePopUpText.color.a + (Time.deltaTime/1f)); 
            yield return null;
        }
        
        //might want to move this out to control delay before fade out
        StartCoroutine(scorePopUpFadeOut());
    }
    IEnumerator scorePopUpFadeOut(){
        scorePopUpText.color = new Color(scorePopUpText.color.r, scorePopUpText.color.g, scorePopUpText.color.b, 1);
        while(scorePopUpText.color.a > 0.0f){
            scorePopUpText.color = new Color(scorePopUpText.color.r, scorePopUpText.color.g, scorePopUpText.color.b, scorePopUpText.color.a - (Time.deltaTime/1f)); 
            yield return null;
        }
    }

    private IEnumerator countTextUp(int newVal){
        //WaitForSeconds wait = new WaitForSeconds(1f/countFPS);
        WaitForSeconds wait = new WaitForSeconds(10f/(float)newVal);
        int prevValue = countVal;
        int stepAmount;
        if(newVal - prevValue < 0){
            stepAmount = (Mathf.FloorToInt((newVal-prevValue)/(countFPS*duration))*10);
        }
        else{
            stepAmount = (Mathf.CeilToInt((newVal-prevValue) / (countFPS*duration))*10);
        }

        if(prevValue < newVal){
            while(prevValue < newVal){
                prevValue+=stepAmount;
                if(prevValue > newVal){
                    prevValue = newVal;
                }
                scorePopUpText.text = prevValue.ToString("N0");
                yield return wait;
            }
        }
        else{
            while(prevValue > newVal){
                prevValue+=stepAmount;
                if(prevValue < newVal){
                    prevValue = newVal;
                }
                scorePopUpText.text = prevValue.ToString("N0");
                yield return wait;
            }
        }
    }

    IEnumerator unlockedPopUpFadeIn()
    {
        unlockedPopUpText.color = new Color(unlockedPopUpText.color.r, unlockedPopUpText.color.g, unlockedPopUpText.color.b, 0);

        while (unlockedPopUpText.color.a < 1.0f)
        {
            unlockedPopUpText.color = new Color(unlockedPopUpText.color.r, unlockedPopUpText.color.g, unlockedPopUpText.color.b, unlockedPopUpText.color.a + (Time.deltaTime / 1f));
            imageOfPeg.color = new Color(imageOfPeg.color.r, imageOfPeg.color.g, imageOfPeg.color.b, unlockedPopUpText.color.a);
            yield return null;
        }

        StartCoroutine(unlockedPopUpFadeOut());
    }

    IEnumerator unlockedPopUpFadeOut()
    {
        unlockedPopUpText.color = new Color(unlockedPopUpText.color.r, unlockedPopUpText.color.g, unlockedPopUpText.color.b, 1);
        while (unlockedPopUpText.color.a > 0.0f)
        {
            unlockedPopUpText.color = new Color(unlockedPopUpText.color.r, unlockedPopUpText.color.g, unlockedPopUpText.color.b, unlockedPopUpText.color.a - (Time.deltaTime / 3f));
            imageOfPeg.color = new Color(imageOfPeg.color.r, imageOfPeg.color.g, imageOfPeg.color.b, unlockedPopUpText.color.a);
            yield return null;
        }
    }
    
    public void startUnlockedPopUp(GameObject incoming_peg)
    {
        imageOfPeg.sprite = incoming_peg.GetComponent<SpriteRenderer>().sprite;
        StartCoroutine(unlockedPopUpFadeIn());
    }

    private void UpdateUI(){
        

        if(localScore > gameData.highestProjScore){
                gameData.highestProjScore = localScore;
                SaveSystem.Save(gameData);
        }
        ammoText.text = "Ammo: " + ammoCount;
        scoreText.text = "Score: " + totalScore;
        gameOverHighScoreText.text = "Highscore: " + gameData.highScore;

        if(fadeText){
            if (!arrowProjActive)
            {
                multiplierText.text = "";
                StopCoroutine(scorePopUpFadeIn());
                scorePopUpText.color = new Color(scorePopUpText.color.r, scorePopUpText.color.g, scorePopUpText.color.b, 0);
                StartCoroutine(scorePopUpFadeIn());
            }
        }
        if(chargeBar.fillAmount <= .01){
            maxChargeText.enabled = false;
            chargeImage.SetActive(false);
        }
        else{
            maxChargeText.enabled = true;
            chargeImage.SetActive(true);
        }
    }
    private void Aiming(){
        //if(pcOrMobile){
            //difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        //}
        //else{
            //difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        //}
        //difference.Normalize();

        //line below is for CLAMPING
        if(chargeButtonShoot == 0){
            difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            difference.Normalize();
            rotation_z = (Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg); //Mathf.Clamp((Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg), -180, 0);
        
            transform.rotation = Quaternion.Euler(0f, 0f, rotation_z);  //can add an offset to the rotz if needed   
        }
    }
    //for the button
    public void chargeShot(){
        //cancelButton.SetActive(true);
        chargeButtonShoot = 1;
        cancelOrNot = true;
        //shooting = false;
        if(shooting == false){
            shootButton.GetComponent<Image>().sprite = shootButtonSprites[1];
        }
    }
    public void releaseShot(){
        //cancelButton.SetActive(false);
        if(chargeButtonShoot == 1){
            chargeButtonShoot = 2;
        }
        shootButton.GetComponent<Image>().sprite = shootButtonSprites[0];
        for(int i = 0; i < points.Length; i++){
            points[i].SetActive(false);
        }
    }
    public void cancelShot(){
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
            for(int i = 0; i < points.Length; i++){
               points[i].SetActive(false);
            }
        }
    }

    //for pc controls
    private void ChargingNShooting(){
        //Input.GetMouseButton(0) || 
        if(((Input.GetMouseButton(0) && pcOrMobile) || (chargeButtonShoot == 1 && pcOrMobile == false)) && cancelOrNot && shooting == false && ammoCount > 0 && chanShootAgain && !arrowProjActive)
        {
            chargeBar.enabled = true;
            //aimingArrow.SetActive(true);

            //used to update the aiming points
            updatePoints();

            if(chargeUpDown){
                projSpeed += chargeSpeed * Time.deltaTime;
                arrowXPos += .15f * Time.deltaTime;
                arrowLength += .15f * Time.deltaTime;
                projSpeed = Mathf.Clamp(projSpeed, startingProjSpeed, maxChargeSpeed);
                if(projSpeed >=maxChargeSpeed){
                    chargeUpDown = false;
                }
            }
            else{
                projSpeed -= chargeSpeed * Time.deltaTime;
                arrowXPos -= .15f * Time.deltaTime;
                arrowLength -= .15f * Time.deltaTime;
                projSpeed = Mathf.Clamp(projSpeed, startingProjSpeed, maxChargeSpeed);
                if(projSpeed <= startingProjSpeed){
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
            aimingArrow.transform.localScale = new Vector2(arrowLength,aimingArrow.transform.localScale.y);
            ChargeBarFiller();
            ColorChanger();
        }
        // 
        if(((Input.GetMouseButtonUp(0) && pcOrMobile) || (chargeButtonShoot == 2 && pcOrMobile == false)) && cancelOrNot && ammoCount > 0 && shooting == false && chanShootAgain && !arrowProjActive)
        {
            arrowLength = .75f;
            arrowXPos = 0;
            shooting = true;
            localScore = 0;
            globalMulti = 1;
            aimingArrow.SetActive(false);
            charge.Stop();
            charge_playing = false;
            //print(projSpeed);
            Fire();
            shoot.Play();
            projSpeed = startingProjSpeed;
            chargeBar.fillAmount = 0;
            chargeBar.enabled = false;
            for(int i = 0; i < points.Length; i++){
               points[i].SetActive(false);
            }
        }
    }

    private void highScoreUpdater(){
        if(totalScore > gameData.highScore){
            gameData.highScore = totalScore;
            SaveSystem.Save(gameData);
        }
    }
    private void GameOverChecker(){
        if(ammoCount <= 0){
            StopCoroutine(scorePopUpFadeIn());
            StopCoroutine(countTextUp(1));
            //StartCoroutine(scorePopUpFadeOut());
            scorePopUpText.color = new Color(scorePopUpText.color.r, scorePopUpText.color.g, scorePopUpText.color.b, 0);
            
            chanShootAgain = false;
            
            gameOverScoreText.text = "Score: " + totalScore;
            //GameOver, save score, update leaderboards and display gameover UI (leaderboards, restart button, settings)
            if (totalScore > gameData.highScore){
                gameData.highScore = totalScore;
                SaveSystem.Save(gameData);
            }
            gameOverHighScoreText.text = "Highscore: " + gameData.highScore;
        }

        //used to control when the gameover panel is activated (has to be done with bools so that it doesn't show up until ball hits bottom)
        if(gameOver){
            settingsButton.SetActive(false);
            gameOverPanel.SetActive(true);
            prevScore = 0;
            scorePopUpText.color = new Color(scorePopUpText.color.r, scorePopUpText.color.g, scorePopUpText.color.b, 0);
            multiplierText.text = "";
            if (!pcOrMobile)
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
        else{
            //settingsButton.SetActive(false);
            chanShootAgain = true;
            gameOverPanel.SetActive(false);
        }
    }
    
    public void clearSave(){
        gameData.highScore = 0;
    }

    void Update()
    {
        
        UpdateUI();
        if (!gameOver && !settingsOpen && !set.getTutorialOpen())
        {
            Aiming();

            ChargingNShooting();

            
        }
        if(settingsOpen || set.getTutorialOpen()){
            for(int i = 0; i < points.Length; i++){
               points[i].SetActive(false);
            }
        }
        highScoreUpdater(); //so it's constantly saving, may have to change
        GameOverChecker();
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
        fadeText = false;
        StopCoroutine(scorePopUpFadeOut());
        StopCoroutine(scorePopUpFadeIn());
        scorePopUpText.color = new Color(scorePopUpText.color.r, scorePopUpText.color.g, scorePopUpText.color.b, 0);
        }
    }

    IEnumerator readyToPlay()
    {
        yield return new WaitForSeconds(0.5f);
        settingsOpen = false;
        settings_button.SetActive(true);
    }

    void ColorChanger(){
        Color chargeColor = Color.Lerp(Color.red, Color.green, (projSpeed/maxChargeSpeed));
        chargeBar.color = chargeColor;
    }

    void ChargeBarFiller(){
        chargeBar.fillAmount = projSpeed / maxChargeSpeed;
    }

    void Fire(){
        gameData = SaveSystem.Load();
        gameData.shotsFired++;
        SaveSystem.Save(gameData);
        GameObject spawnedBullet = Instantiate(projectile, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = spawnedBullet.GetComponent<Rigidbody2D>();
        rb.velocity = firePoint.right * projSpeed;
        //rb.AddForce(firePoint.right * projSpeed, ForceMode2D.Impulse);        //origional firing
        ammoCount--;
    }

    public void addTotalScore(int score, int multi)
    {
        totalScore += score * multi;
        localScore += score * multi;
    }

    IEnumerator spawnPlanet(){
        int waitTime = Random.Range(25,35);
        yield return new WaitForSeconds(waitTime);
        int randoPlanet = Random.Range(0,3);
        Instantiate(planets[randoPlanet], new Vector2(-12.0f,Random.Range(-6.5f,6.5f)), Quaternion.identity);
        StartCoroutine(spawnPlanet());
    }
    IEnumerator resetShot(){
        
        yield return new WaitForSeconds(1f);
        cancelOrNot = true;
    }

    public void addScore(int score)
    {
        totalScore += score * globalMulti;
        localScore += score * globalMulti;
    }

    public int getLocalScore()
    {
        return localScore;
    }

    public int getGlobalMulti()
    {
        return globalMulti;
    }

    public void increaseGlobalMulti()
    {
        globalMulti = globalMulti * 2;
    }

    public bool isShooting()
    {
        return shooting;
    }

    public void activateArrowProj()
    {
        amt_of_arrowProj_active++;
        if (amt_of_arrowProj_active > 0)
        {
            arrowProjActive = true;
        }
    }

    public void deactivateArrowProj()
    {
        amt_of_arrowProj_active--;
        if (amt_of_arrowProj_active == 0)
        {
            arrowProjActive = false;
        }
    }

    public bool returnGameOver()
    {
        return gameOver;
    }

    private void OnDestroy() {
        SaveSystem.Save(gameData);
        
        //print(gameData.shotsFired);
    }

    public void resetLocalScore()
    {
        totalScore = 0;
        localScore = 0;
    }
}
