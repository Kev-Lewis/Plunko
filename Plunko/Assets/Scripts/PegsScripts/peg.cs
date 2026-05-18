using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class peg : MonoBehaviour
{
    private Spawning spawn;

    private void Start() {
        GameObject spawnerObject = GameObject.Find("Spawner");

        if (spawnerObject != null) {
            spawn = spawnerObject.GetComponent<Spawning>();
        }
    }

    private void OnDestroy() {
        if (!Shooter.gameOver && spawn != null) {
            spawn.reduceCount();
        }
    }
}