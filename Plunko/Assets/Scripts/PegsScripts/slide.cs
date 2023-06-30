using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class slide : MonoBehaviour
{
    private Shooter shoot;
    private AudioSource audioClip;
    private ProjScript proj;
    [SerializeField] private Sprite customSlide;

    private void Start()
    {
        shoot = GameObject.Find("Shooter").GetComponent<Shooter>();
        audioClip = GameObject.Find("pop").GetComponent<AudioSource>();

        if(PlayerPrefs.GetInt("customSlide") == 1){
            GetComponent<SpriteRenderer>().sprite = customSlide;
        }
    }
    private void OnDestroy()
    {
        if(SceneManager.GetActiveScene().name == "infiniteLevel")
        {
            if (shoot.isShooting())
            {
                if (audioClip != null)
                {
                    audioClip.Play();
                }
                proj = GameObject.Find("Projectile(Clone)").GetComponent<ProjScript>();
                proj.afterSlide();
                //shoot.addScore(10);
            }
        }     
        else if (SceneManager.GetActiveScene().name == "Tutorial2")
        {
            if (audioClip != null)
            {
                audioClip.Play();
            }
        }
    }
}
