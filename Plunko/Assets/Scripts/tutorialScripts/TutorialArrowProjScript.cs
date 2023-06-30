using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialArrowProjScript : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector3 lastVelocity;
    [SerializeField] private ParticleSystem ps;
    [SerializeField] private GameObject ArrowProj;
    private Tutorial_Shooter shoot;
    private Transform blackHole;
    private AudioSource shoot_audio;
    private AudioSource audioClip;
    // Start is called before the first frame update
    void Start()
    {
        audioClip = GameObject.Find("pop").GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        shoot = GameObject.Find("Shooter").GetComponent<Tutorial_Shooter>();
        blackHole = GameObject.Find("portalPaddle").GetComponent<Transform>();
        shoot_audio = GameObject.Find("shoot").GetComponent<AudioSource>();
    }

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
            shoot.lowerTutorialPegCount();
            if (shoot.getTutorialPegsActive() <= 0 && SceneManager.GetActiveScene().name == "Tutorial3")
            {
                shoot.tutorial2Open();
            }
            Destroy(gameObject);
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
            shoot.lowerTutorialPegCount();
        }
    }
}
