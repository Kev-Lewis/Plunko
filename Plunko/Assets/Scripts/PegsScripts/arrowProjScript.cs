using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class arrowProjScript : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector3 lastVelocity;
    [SerializeField] private ParticleSystem ps;
    [SerializeField] private GameObject ArrowProj;
    private Spawning spawn;
    private Shooter shoot;
    private int score;
    private int scoreMulti;
    private Transform blackHole;
    private MultiHit mh;
    private twoHitScript th;
    private AudioSource audioClip;
    private int tempScore = 0;
    private AudioSource shoot_audio;
    private Text multiplierText;
    // Start is called before the first frame update
    void Start()
    {
        audioClip = GameObject.Find("pop").GetComponent<AudioSource>();
        score = 10;
        rb = GetComponent<Rigidbody2D>();
        spawn = GameObject.Find("Spawner").GetComponent<Spawning>();
        shoot = GameObject.Find("Shooter").GetComponent<Shooter>();
        blackHole = GameObject.Find("portalPaddle").GetComponent<Transform>();
        shoot.activateArrowProj();
        shoot_audio = GameObject.Find("shoot").GetComponent<AudioSource>();
        multiplierText = GameObject.Find("multiplierText").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        lastVelocity = rb.velocity;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        var speed = lastVelocity.magnitude;
        var dir = Vector3.Reflect(lastVelocity.normalized, other.contacts[0].normal);
        rb.velocity = (dir * Mathf.Max(speed, 0f) * .9f);

        if (other.gameObject.CompareTag("Peg"))
        {
            if (!shoot.getSettingsOpen())
            {
                Instantiate(ps, new Vector3(other.transform.position.x, other.transform.position.y, ps.transform.position.z), ps.transform.rotation);   //spawns the particle effects, can remove
            }

            Destroy(other.gameObject);

            shoot.addTotalScore(score, shoot.getGlobalMulti());
            audioClip = GameObject.Find("pop").GetComponent<AudioSource>();
            audioClip.Play();
        }
        else if (other.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("multiplier"))
        {
            shoot.increaseGlobalMulti();
            shoot.addTotalScore(score, scoreMulti);
            tempScore = (Shooter.totalScore - shoot.prevScore);
            audioClip = GameObject.Find("ammoplus").GetComponent<AudioSource>();
            audioClip.Play();
            //score = 0;
            //Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("AmmoPlus"))
        {
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
            if (!shoot.getSettingsOpen())
            {
                Instantiate(ps, new Vector3(other.transform.position.x, other.transform.position.y, ps.transform.position.z), ps.transform.rotation);   //spawns the particle effects, can remove
            }
            Destroy(other.gameObject);

            if (shoot.ammoCount >= 1)
            {
                shoot.ammoCount--;
            }

            audioClip = GameObject.Find("ammominus").GetComponent<AudioSource>();
            audioClip.Play();
        }
        else if (other.gameObject.CompareTag("MultiHitPeg"))
        {
            mh = other.gameObject.GetComponent<MultiHit>();
            if (mh.hit())
            {
                if (!shoot.getSettingsOpen())
                {
                    Instantiate(ps, new Vector3(other.transform.position.x, other.transform.position.y, ps.transform.position.z), ps.transform.rotation);   //spawns the particle effects, can remove
                }
                Destroy(other.gameObject);

                shoot.addTotalScore((score * 3), shoot.getGlobalMulti());
                tempScore = (Shooter.totalScore - shoot.prevScore);
            }
            audioClip = GameObject.Find("pop").GetComponent<AudioSource>();
            audioClip.Play();
        }
        else if (other.gameObject.CompareTag("TwoHitPeg"))
        {
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
            }
            audioClip = GameObject.Find("pop").GetComponent<AudioSource>();
            audioClip.Play();
        }
        else if (other.gameObject.CompareTag("wall"))
        {
            audioClip = GameObject.Find("pop").GetComponent<AudioSource>();
            audioClip.Play();
        }
        else if (other.gameObject.CompareTag("x2"))
        {
            audioClip = GameObject.Find("ammoplus").GetComponent<AudioSource>();
            audioClip.Play();
            shoot.increaseGlobalMulti();
            multiplierText.text = "X" + shoot.getGlobalMulti();
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("ArrowUpLeft"))
        {
            shoot_audio.Play();
            Transform t = GameObject.Find("LauncherRight").GetComponent<Transform>();
            GameObject spawnedBullet = Instantiate(ArrowProj, t.position, Quaternion.identity);
            Rigidbody2D rb = spawnedBullet.GetComponent<Rigidbody2D>();
            rb.velocity = new Vector3(-1, 1f, 0) * 7.5f;
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("ArrowLeft"))
        {
            shoot_audio.Play();
            Transform t = GameObject.Find("LauncherRight").GetComponent<Transform>();
            GameObject spawnedBullet = Instantiate(ArrowProj, t.position, Quaternion.identity);
            Rigidbody2D rb = spawnedBullet.GetComponent<Rigidbody2D>();
            rb.velocity = new Vector3(-1, 0, 0) * 7.5f;
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("ArrowRight"))
        {
            shoot_audio.Play();
            Transform t = GameObject.Find("LauncherLeft").GetComponent<Transform>();
            GameObject spawnedBullet = Instantiate(ArrowProj, t.position, Quaternion.identity);
            Rigidbody2D rb = spawnedBullet.GetComponent<Rigidbody2D>();
            rb.velocity = new Vector3(1, 0, 0) * 7.5f;
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("ArrowUpRight"))
        {
            shoot_audio.Play();
            Transform t = GameObject.Find("LauncherLeft").GetComponent<Transform>();
            GameObject spawnedBullet = Instantiate(ArrowProj, t.position, Quaternion.identity);
            Rigidbody2D rb = spawnedBullet.GetComponent<Rigidbody2D>();
            rb.velocity = new Vector3(1, 1, 0) * 7.5f;
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("new_multiplier"))
        {

            this.transform.position = new Vector3(blackHole.position.x, blackHole.position.y, this.transform.position.z);
            shoot.addTotalScore(score, shoot.getGlobalMulti());
            shoot.increaseGlobalMulti();
            multiplierText.text = "X" + shoot.getGlobalMulti();
            tempScore = (Shooter.totalScore - shoot.prevScore);
            audioClip = GameObject.Find("ammoplus").GetComponent<AudioSource>();
            audioClip.Play();
            lastVelocity = rb.velocity / 4f;
            rb.velocity = rb.velocity / 4f;
            //shoot.ammoCount++;
            GetComponent<TrailRenderer>().Clear();
        }
        else if (collision.gameObject.CompareTag("BlackHole"))
        {
            Destroy(collision.gameObject);

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
            StartCoroutine(deleteSlide(collision.gameObject));
        }
    }

    IEnumerator deleteSlide(GameObject other)
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(other.gameObject);
    }

    public void setTempScore(int score)
    {
        tempScore = score;
    }

    private void OnDestroy()
    {
        shoot.deactivateArrowProj();
    }
}
