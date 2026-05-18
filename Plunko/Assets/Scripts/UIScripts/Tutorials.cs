using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorials : MonoBehaviour
{
    [Header("Tutorial Pages")]
    [SerializeField] private GameObject Tutorial1;
    [SerializeField] private GameObject Tutorial2;
    [SerializeField] private GameObject Tutorial3;

    [Header("Buttons")]
    [SerializeField] private GameObject NextTip;
    [SerializeField] private GameObject PrevTip;

    private const int FirstTutorial = 1;
    private const int LastTutorial = 3;

    private int whichTutorial = FirstTutorial;
    private AudioSource blipSelect;

    private void Start() {
        CacheReferences();
        UpdateTutorialPage();
    }

    private void CacheReferences() {
        GameObject audioObject = GameObject.Find("blipSelect");

        if (audioObject != null) {
            blipSelect = audioObject.GetComponent<AudioSource>();
        }
    }

    private void UpdateTutorialPage() {
        SetPanelActive(Tutorial1, whichTutorial == 1);
        SetPanelActive(Tutorial2, whichTutorial == 2);
        SetPanelActive(Tutorial3, whichTutorial == 3);

        SetPanelActive(PrevTip, whichTutorial > FirstTutorial);
        SetPanelActive(NextTip, whichTutorial < LastTutorial);
    }

    public void nextTutorial() {
        whichTutorial = Mathf.Clamp(whichTutorial + 1, FirstTutorial, LastTutorial);

        PlaySelectSound();
        UpdateTutorialPage();
    }

    public void prevTutorial() {
        whichTutorial = Mathf.Clamp(whichTutorial - 1, FirstTutorial, LastTutorial);

        PlaySelectSound();
        UpdateTutorialPage();
    }

    private void SetPanelActive(GameObject panel, bool active) {
        if (panel != null) {
            panel.SetActive(active);
        }
    }

    private void PlaySelectSound() {
        if (blipSelect != null) {
            blipSelect.Play();
        }
    }
}