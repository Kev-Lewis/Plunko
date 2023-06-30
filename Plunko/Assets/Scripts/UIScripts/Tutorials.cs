using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorials : MonoBehaviour
{
    public GameObject Tutorial1;
    public GameObject Tutorial2;
    public GameObject Tutorial3;
    public GameObject NextTip;
    public GameObject PrevTip;
    private int whichTutorial;
    private AudioSource audioClip;
    // Start is called before the first frame update
    void Start()
    {
        whichTutorial = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (whichTutorial == 1)
        {
            Tutorial1.SetActive(true);
            Tutorial2.SetActive(false);
            Tutorial3.SetActive(false);
            NextTip.SetActive(true);
            PrevTip.SetActive(false);
        }
        else if (whichTutorial == 2)
        {
            Tutorial1.SetActive(false);
            Tutorial2.SetActive(true);
            Tutorial3.SetActive(false);
            PrevTip.SetActive(true);
            NextTip.SetActive(true);
        }
        else if (whichTutorial == 3)
        {
            Tutorial1.SetActive(false);
            Tutorial2.SetActive(false);
            Tutorial3.SetActive(true);
            PrevTip.SetActive(true);
            NextTip.SetActive(false);
        }
    }

    public void nextTutorial()
    {
        audioClip = GameObject.Find("blipSelect").GetComponent<AudioSource>();
        audioClip.Play();
        whichTutorial++;
    }

    public void prevTutorial()
    {
        audioClip = GameObject.Find("blipSelect").GetComponent<AudioSource>();
        audioClip.Play();
        whichTutorial--;
    }
}
