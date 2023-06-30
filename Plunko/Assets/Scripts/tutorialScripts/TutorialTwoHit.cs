using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTwoHit : MonoBehaviour
{
    private SpriteRenderer sr;
    public Sprite[] sprites;
    private int whichSprite;
    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        whichSprite = 0;
        sr.sprite = sprites[whichSprite];
    }

    public bool hit()
    {
        whichSprite++;
        if (whichSprite < 2)
        {
            sr.sprite = sprites[whichSprite];
            return false;
        }
        return true;
    }
}
