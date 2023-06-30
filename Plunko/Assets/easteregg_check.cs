using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class easteregg_check : MonoBehaviour
{
    public Button names;
    public GameObject EasterEggPanel;
    public AudioSource blipSelect;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetFloat("FirstGame") == 1 &&
            PlayerPrefs.GetInt("pegsDestroyed3") == 1 &&
            PlayerPrefs.GetInt("shotsDestroyed3") == 1 &&
            PlayerPrefs.GetInt("levelsCleared3") == 1 &&
            PlayerPrefs.GetInt("projectileScore3") == 1)
        {
            names.enabled = true;
        }
        else
        {
            names.enabled = false;
        }
    }

    public void openEasterEgg()
    {
        blipSelect.Play();
        EasterEggPanel.SetActive(true);
    }

    public void closeEasterEgg()
    {
        blipSelect.Play();
        EasterEggPanel.SetActive(false);
    }

    public void openKevin()
    {
        Application.OpenURL("https://kevinlewis.net/");
    }

    public void openAdam()
    {
        Application.OpenURL("https://adamcarter.dev/");
    }
}
