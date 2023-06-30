using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProjScript : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector3 lastVelocity;
    [SerializeField] private ParticleSystem ps, level_clear_ps;
    [SerializeField] private GameObject ArrowProj;
    private Spawning spawn;
    private Shooter shoot;
    private int score;
    private Transform blackHole;
    private MultiHit mh;
    private twoHitScript th;
    private AudioSource audioClip;
    private int currentLevel = 1;
    private int tempScore = 0;
    private float startingPitch;
    private AudioSource shoot_audio;
    private Text multiplierText;
    public GameData gameData;
    private bool add1, add2, add3, add4;

    private int totalPegCounter = 0;

    [SerializeField] private Sprite customProj;
    //private Text tempScorePopUp;
    // Start is called before the first frame update
    float alpha = 1.0f;
        
    private void Awake() {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.gray, 0.0f), new GradientColorKey(Color.yellow, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );

        gameData = SaveSystem.Load();
        if(PlayerPrefs.GetInt("customProj") == 1){
            GetComponent<SpriteRenderer>().sprite = customProj;
        }
        if(PlayerPrefs.GetInt("customTrail") == 1){
            GetComponent<TrailRenderer>().colorGradient = gradient;
        }
    }

    void Start()
    {
        multiplierText = GameObject.Find("multiplierText").GetComponent<Text>();
        
        audioClip = GameObject.Find("pop").GetComponent<AudioSource>();
        startingPitch = audioClip.pitch;
        score = 10;
        rb = GetComponent<Rigidbody2D>();
        spawn = GameObject.Find("Spawner").GetComponent<Spawning>();
        shoot = GameObject.Find("Shooter").GetComponent<Shooter>();
        shoot_audio = GameObject.Find("shoot").GetComponent<AudioSource>();
        blackHole = GameObject.Find("portalPaddle").GetComponent<Transform>();
        add1 = add2 = add3 = add4 = false;
        //tempScorePopUp = GameObject.Find("ScorePopUp").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        lastVelocity = rb.velocity;
        //print(shoot.prevScore);
    }
    private void updatePegCount(){
        totalPegCounter++;
        //gameData.totalPegs++;
        //print(gameData.totalPegs);
        //SaveSystem.Save(gameData);
        //print(gameData.totalPegs);
    }

    private void checkAddAmmo(int tempScore)
    {
        if (tempScore >= 100 && !add1)
        {
            add1 = true;
            shoot.ammoCount += 1;
        }
        if (tempScore >= 250 && !add2)
        {
            add2 = true;
            shoot.ammoCount += 1;
        }
        if (tempScore >= 500 && !add3)
        {
            add3 = true;
            shoot.ammoCount += 1;
        }
        if (tempScore >= 1000 && !add4)
        {
            add4 = true;
            shoot.ammoCount += 2;
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        var speed = lastVelocity.magnitude;
        var dir = Vector3.Reflect(lastVelocity.normalized, other.contacts[0].normal);
        rb.velocity = (dir*Mathf.Max(speed, 0f) * .9f);

        if(other.gameObject.CompareTag("Peg")){
            updatePegCount();
            if (!shoot.getSettingsOpen())
            {
                Instantiate(ps, new Vector3(other.transform.position.x, other.transform.position.y, ps.transform.position.z), ps.transform.rotation);   //spawns the particle effects, can remove
            }
            
            Destroy(other.gameObject);
            

            shoot.addTotalScore(score, shoot.getGlobalMulti());
            tempScore = (Shooter.totalScore-shoot.prevScore);
            checkAddAmmo(tempScore);
            audioClip = GameObject.Find("pop").GetComponent<AudioSource>();
            audioClip.Play();
            changePitch();
        }
        else if(other.gameObject.CompareTag("Ground")){
            //print("Total pegs ONE: "+gameData.totalPegs);
            //gameData = SaveSystem.Load();

            Shooter.totalPegsToSave += totalPegCounter;
            //print("Total pegs TWO: "+gameData.totalPegs);

            audioClip = GameObject.Find("pop").GetComponent<AudioSource>();
            audioClip.pitch = startingPitch;
            //print(audioClip.pitch);
            //print(totalScore);
            //display UI with that proj's score
            //StartCoroutine(scorePopUpFade());
            //shoot.scorePopUpText.text = "Projectile score: " + shoot.getLocalScore();
            /*
            if(shoot.getLocalScore() < 1){
                shoot.scorePopUpText.color = Color.red;
            }
            else if(shoot.getLocalScore() < 10){
                shoot.scorePopUpText.color = Color.white;
            }
            if(shoot.getLocalScore() >= 10){
                shoot.scorePopUpText.color = Color.green;
                shoot.ammoCount += 1;
            }
            */

            tempScore = 0;
            shoot.prevScore = Shooter.totalScore;
            Shooter.shooting = false;
            if (spawn.spawnCount < 1)
            {
                var ammo_minus_array = FindObjectsOfType<AmmoMinus>();
                foreach (var ammoMinus in ammo_minus_array)
                {
                    Destroy(ammoMinus.gameObject);
                }
                var ammo_plus_array = FindObjectsOfType<AmmoPlus>();
                foreach (var ammoPlus in ammo_plus_array)
                {
                    Destroy(ammoPlus.gameObject);
                }
                var black_hole_array = FindObjectsOfType<blackHolePeg>();
                foreach (var blackHolePeg in black_hole_array)
                {
                    Destroy(blackHolePeg.gameObject);
                }
                var x2_array = FindObjectsOfType<x2Peg>();
                foreach (var x2Peg in x2_array)
                {
                    Destroy(x2Peg.gameObject);
                }
                var pegsToDelete = FindObjectsOfType<pegsToDelete>();
                foreach (var pegToDelete in pegsToDelete)
                {
                    Destroy(pegToDelete.gameObject);
                }
                var arrowProj = FindObjectsOfType<arrowProjScript>();
                foreach (var arrProj in arrowProj)
                {
                    if (!shoot.getSettingsOpen())
                    {
                        Instantiate(ps, new Vector3(arrProj.transform.position.x, arrProj.transform.position.y, ps.transform.position.z), ps.transform.rotation);   //spawns the particle effects, can remove
                    }
                    Destroy(arrProj.gameObject);
                }
                spawn.unlockNewPeg();
                Instantiate(level_clear_ps, GameObject.Find("levelclear").GetComponent<Transform>().position, level_clear_ps.transform.rotation);   //spawns the particle effects, can remove
                spawn.resetList();
                spawn.SpawnObjects();
                shoot.ammoCount += 5;
                audioClip = GameObject.Find("level_clear").GetComponent<AudioSource>();
                audioClip.Play();
                Shooter.totalScore += 50 * currentLevel;
                currentLevel++;
                spawn.addLevelsCleared();
                Shooter.totalLevelsToSave++;
                multiplierText.text = "";
                //gameData.totalLevels++;
                //gameData = SaveSystem.Load();
                //gameData.totalLevels++;
                //print("Total pegs: "+gameData.totalPegs);
                //SaveSystem.Save(gameData);
                //print(gameData.totalLevels);
                //shoot.prevScore = Shooter.totalScore;
            }
            SaveSystem.Save(gameData);
            Destroy(gameObject);  
            if(shoot.ammoCount <= 0){
                Shooter.gameOver = true;
                var particleObjects = FindObjectsOfType<ParticleSystem>();
                foreach(var pO in particleObjects){
                    Destroy(pO.gameObject);
                }
                audioClip = GameObject.Find("lose").GetComponent<AudioSource>();
                audioClip.Play();
            }
            else
            {
                shoot.fadeText = true;
                audioClip = GameObject.Find("ammominus").GetComponent<AudioSource>();
                audioClip.Play();
            }
        }
        else if(other.gameObject.CompareTag("multiplier")){
            
            shoot.increaseGlobalMulti();
            shoot.addTotalScore(1, shoot.getGlobalMulti());
            tempScore = (Shooter.totalScore-shoot.prevScore);
            audioClip = GameObject.Find("ammoplus").GetComponent<AudioSource>();
            audioClip.Play();
            multiplierText.text = "X" + shoot.getGlobalMulti();
            //score = 0;
            //Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("AmmoPlus"))
        {
            updatePegCount();
            if (!shoot.getSettingsOpen())
            {
                Instantiate(ps, new Vector3(other.transform.position.x, other.transform.position.y, ps.transform.position.z), ps.transform.rotation);   //spawns the particle effects, can remove
            }
            Destroy(other.gameObject);

            

            shoot.ammoCount++;
            audioClip = GameObject.Find("ammoplus").GetComponent<AudioSource>();
            audioClip.Play();
        }
        else if (other.gameObject.CompareTag("AmmoMinus"))
        {
            updatePegCount();
            if (!shoot.getSettingsOpen())
            {
                Instantiate(ps, new Vector3(other.transform.position.x, other.transform.position.y, ps.transform.position.z), ps.transform.rotation);   //spawns the particle effects, can remove
            }
            Destroy(other.gameObject);

            

            if(shoot.ammoCount >= 1){
                shoot.ammoCount--;
            }

            audioClip = GameObject.Find("ammominus").GetComponent<AudioSource>();
            audioClip.Play();
        }
        else if (other.gameObject.CompareTag("MultiHitPeg"))
        { 
            updatePegCount();
            mh = other.gameObject.GetComponent<MultiHit>();
            if (mh.hit())
            {
                if (!shoot.getSettingsOpen())
                {
                    Instantiate(ps, new Vector3(other.transform.position.x, other.transform.position.y, ps.transform.position.z), ps.transform.rotation);   //spawns the particle effects, can remove
                }
                Destroy(other.gameObject);

                

                shoot.addTotalScore((score * 3), shoot.getGlobalMulti());
                tempScore = (Shooter.totalScore-shoot.prevScore);
            }
            audioClip = GameObject.Find("pop").GetComponent<AudioSource>();
            audioClip.Play();
            changePitch();
        }
        else if (other.gameObject.CompareTag("TwoHitPeg"))
        {
            updatePegCount();
            th = other.gameObject.GetComponent<twoHitScript>();
            if (th.hit())
            {
                if (!shoot.getSettingsOpen())
                {
                    Instantiate(ps, new Vector3(other.transform.position.x, other.transform.position.y, ps.transform.position.z), ps.transform.rotation);   //spawns the particle effects, can remove
                }
                Destroy(other.gameObject);

                

                shoot.addTotalScore((score * 2), shoot.getGlobalMulti());
                tempScore = (Shooter.totalScore - shoot.prevScore);
                checkAddAmmo(tempScore);
            }
            audioClip = GameObject.Find("pop").GetComponent<AudioSource>();
            audioClip.Play();
            changePitch();
        }
        else if (other.gameObject.CompareTag("wall"))
        {
            audioClip = GameObject.Find("pop").GetComponent<AudioSource>();
            audioClip.Play();
        }
        else if (other.gameObject.CompareTag("x2"))
        {
            updatePegCount();
            audioClip = GameObject.Find("ammoplus").GetComponent<AudioSource>();
            audioClip.Play();
            shoot.increaseGlobalMulti();
            multiplierText.text = "X" + shoot.getGlobalMulti();
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("ArrowUpLeft"))
        {
            updatePegCount();
            shoot_audio.Play();
            Transform t = GameObject.Find("LauncherRight").GetComponent<Transform>();
            GameObject spawnedBullet = Instantiate(ArrowProj, t.position, Quaternion.identity);
            Rigidbody2D rb = spawnedBullet.GetComponent<Rigidbody2D>();
            rb.velocity = new Vector3(-1, 1f, 0) * 7.5f;
            Destroy(other.gameObject);

        }
        else if (other.gameObject.CompareTag("ArrowLeft"))
        {
            updatePegCount();
            shoot_audio.Play();
            Transform t = GameObject.Find("LauncherRight").GetComponent<Transform>();
            GameObject spawnedBullet = Instantiate(ArrowProj, t.position, Quaternion.identity);
            Rigidbody2D rb = spawnedBullet.GetComponent<Rigidbody2D>();
            rb.velocity = new Vector3(-1, 0, 0) * 7.5f;
            Destroy(other.gameObject);
           
        }
        else if (other.gameObject.CompareTag("ArrowRight"))
        {
            updatePegCount();
            shoot_audio.Play();
            Transform t = GameObject.Find("LauncherLeft").GetComponent<Transform>();
            GameObject spawnedBullet = Instantiate(ArrowProj, t.position, Quaternion.identity);
            Rigidbody2D rb = spawnedBullet.GetComponent<Rigidbody2D>();
            rb.velocity = new Vector3(1, 0, 0) * 7.5f;
            Destroy(other.gameObject);
            
        }
        else if (other.gameObject.CompareTag("ArrowUpRight"))
        {
            updatePegCount();
            shoot_audio.Play();
            Transform t = GameObject.Find("LauncherLeft").GetComponent<Transform>();
            GameObject spawnedBullet = Instantiate(ArrowProj, t.position, Quaternion.identity);
            Rigidbody2D rb = spawnedBullet.GetComponent<Rigidbody2D>();
            rb.velocity = new Vector3(1, 1, 0) * 7.5f;
            Destroy(other.gameObject);
            
        }
        else if (other.gameObject.CompareTag("Nuke"))
        {
            updatePegCount();
            Destroy(other.gameObject);
 
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("new_multiplier"))
        { 
            GetComponent<TrailRenderer>().Clear();
            this.transform.position = new Vector3(blackHole.position.x, blackHole.position.y, this.transform.position.z);
            shoot.addTotalScore(10 , shoot.getGlobalMulti());
            shoot.increaseGlobalMulti();
            tempScore = (Shooter.totalScore - shoot.prevScore);
            checkAddAmmo(tempScore);
            audioClip = GameObject.Find("ammoplus").GetComponent<AudioSource>();
            audioClip.Play();
            lastVelocity = rb.velocity / 4f;
            rb.velocity = rb.velocity / 4f;
            GetComponent<TrailRenderer>().Clear();
            //shoot.ammoCount++;
            multiplierText.text = "X" + shoot.getGlobalMulti();
        }
        else if (collision.gameObject.CompareTag("BlackHole"))
        {
            updatePegCount();
            Destroy(collision.gameObject);

            

            GetComponent<TrailRenderer>().Clear();
            this.transform.position = new Vector3(blackHole.position.x, blackHole.position.y, this.transform.position.z);
            audioClip = GameObject.Find("blackhole").GetComponent<AudioSource>();
            audioClip.Play();
            lastVelocity = rb.velocity / 2f;
            rb.velocity = rb.velocity / 2f;
            GetComponent<TrailRenderer>().Clear();
        }
        else if (collision.gameObject.CompareTag("slide"))
        {
            audioClip = GameObject.Find("pop").GetComponent<AudioSource>();
            audioClip.Play();
            changePitch();
            StartCoroutine(deleteSlide(collision.gameObject));
        }
    }
    private void changePitch(){
        if(audioClip.pitch <= 7){
            audioClip.pitch+=.15f;
        }
    }
    IEnumerator deleteSlide(GameObject other)
    {
        updatePegCount();
        yield return new WaitForSeconds(0.5f);
        Destroy(other.gameObject);
    }

    public void setTempScore(int score)
    {
        tempScore = score;
    }

    public void afterSlide()
    {
        shoot.addTotalScore(score, shoot.getGlobalMulti());
        tempScore = (Shooter.totalScore - shoot.prevScore);
        checkAddAmmo(tempScore);
    }

    public int getScore()
    {
        return score;
    }
}
