using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiHit : MonoBehaviour
{
    private SpriteRenderer sr;
    public Sprite[] sprites;
    private Spawning spawn;
    private int whichSprite;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        whichSprite = 0;
        sr.sprite = sprites[whichSprite];
        spawn = GameObject.Find("Spawner").GetComponent<Spawning>();
    }

    public bool hit()
    {
        whichSprite++;
        if (whichSprite < 3)
        {
            sr.sprite = sprites[whichSprite];
            return false;
        }
        return true;
    }

    private void OnDestroy()
    {
        if (Shooter.gameOver == false)
        {
            spawn.reduceCount();
        }
    }
}
