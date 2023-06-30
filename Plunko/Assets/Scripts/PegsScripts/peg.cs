using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class peg : MonoBehaviour
{
    private Spawning spawn;
    private void Start()
    {
        spawn = GameObject.Find("Spawner").GetComponent<Spawning>();
    }
    private void OnDestroy()
    {
        if (Shooter.gameOver == false)
        {
            spawn.reduceCount();
        }
    }
}
