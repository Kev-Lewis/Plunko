using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Tutorial_Proj : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector3 lastVelocity;
    [SerializeField] private ParticleSystem ps, level_clear_ps;
    private Tutorial_Shooter shoot;
    private TutorialMultiHit mh;
    private TutorialTwoHit th;
    private AudioSource audioClip;
    private Transform blackHole;
    private AudioSource shoot_audio;
    [SerializeField] private GameObject ArrowProj;

    // Start is called before the first frame update
    void Start()
    {
        audioClip = GameObject.Find("pop").GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        shoot = GameObject.Find("Shooter").GetComponent<Tutorial_Shooter>();
        if (SceneManager.GetActiveScene().name == "Tutorial3")
        {
            blackHole = GameObject.Find("portalPaddle").GetComponent<Transform>();
        }
        shoot_audio = GameObject.Find("shoot").GetComponent<AudioSource>();
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

        if (other.gameObject.CompareTag("Ground"))
        {
            Tutorial_Shooter.shooting = false;
            shoot.setChargeButtonShoot();
            Destroy(gameObject);
            if (shoot.getTutorialPegsActive() <= 0 && SceneManager.GetActiveScene().name == "TutorialPC")
            {
                SceneManager.LoadScene("Tutorial2");
            }
            else if (shoot.getTutorialPegsActive() <= 0 && SceneManager.GetActiveScene().name == "TutorialMobile")
            {
                SceneManager.LoadScene("Tutorial2");
            }
            else if (shoot.getTutorialPegsActive() <= 0 && SceneManager.GetActiveScene().name == "Tutorial2")
            {
                SceneManager.LoadScene("Tutorial3");
            }
            else if (shoot.getTutorialPegsActive() <= 0 && SceneManager.GetActiveScene().name == "Tutorial3")
            {
                shoot.tutorial2Open();
            }
        }
        else if (other.gameObject.CompareTag("TutorialPeg"))
        {
            audioClip = GameObject.Find("pop").GetComponent<AudioSource>();
            if (!shoot.getSettingsOpen())
            {
                Instantiate(ps, new Vector3(other.transform.position.x, other.transform.position.y, ps.transform.position.z), ps.transform.rotation);   //spawns the particle effects, can remove
            }
            audioClip.Play();
            shoot.lowerTutorialPegCount();
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("TutorialMultiHit"))
        {
            mh = other.gameObject.GetComponent<TutorialMultiHit>();
            if (mh.hit())
            {
                if (!shoot.getSettingsOpen())
                {
                    Instantiate(ps, new Vector3(other.transform.position.x, other.transform.position.y, ps.transform.position.z), ps.transform.rotation);   //spawns the particle effects, can remove
                }
                shoot.lowerTutorialPegCount();
                Destroy(other.gameObject);
            }
            audioClip = GameObject.Find("pop").GetComponent<AudioSource>();
            audioClip.Play();
        }
        else if (other.gameObject.CompareTag("TutorialTwoHitPeg"))
        {
            th = other.gameObject.GetComponent<TutorialTwoHit>();
            if (th.hit())
            {
                if (!shoot.getSettingsOpen())
                {
                    Instantiate(ps, new Vector3(other.transform.position.x, other.transform.position.y, ps.transform.position.z), ps.transform.rotation);   //spawns the particle effects, can remove
                }
                shoot.lowerTutorialPegCount();
                Destroy(other.gameObject);
            }
            audioClip = GameObject.Find("pop").GetComponent<AudioSource>();
            audioClip.Play();
        }
        else if (other.gameObject.CompareTag("wall"))
        {
            audioClip = GameObject.Find("pop").GetComponent<AudioSource>();
            audioClip.Play();
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
            GetComponent<TrailRenderer>().Clear();
            this.transform.position = new Vector3(blackHole.position.x, blackHole.position.y, this.transform.position.z);
            audioClip = GameObject.Find("ammoplus").GetComponent<AudioSource>();
            audioClip.Play();
            lastVelocity = rb.velocity / 4f;
            rb.velocity = rb.velocity / 4f;
            GetComponent<TrailRenderer>().Clear();
        }
        else if (collision.gameObject.CompareTag("BlackHole"))
        {
            Destroy(collision.gameObject);
            GetComponent<TrailRenderer>().Clear();
            this.transform.position = new Vector3(blackHole.position.x, blackHole.position.y, this.transform.position.z);
            audioClip = GameObject.Find("blackhole").GetComponent<AudioSource>();
            audioClip.Play();
            lastVelocity = rb.velocity / 2f;
            rb.velocity = rb.velocity / 2f;
            GetComponent<TrailRenderer>().Clear();
            shoot.lowerTutorialPegCount();
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
}
