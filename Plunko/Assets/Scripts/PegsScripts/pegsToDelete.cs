using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pegsToDelete : MonoBehaviour
{
    //public GameData gameData;
    private void Awake() {
        //gameData = SaveSystem.Load();
    }
    private Shooter shoot;
    // Start is called before the first frame update
    void Start()
    {
        //shoot = GameObject.Find("Shooter").GetComponent<Shooter>();
    }

    private void OnDestroy() {
        /*
        if(!shoot.returnGameOver())
        {
            gameData = SaveSystem.Load();
            gameData.totalPegs++;
            SaveSystem.Save(gameData);
            //print(gameData.totalPegs);
        }
        */
    }
}
