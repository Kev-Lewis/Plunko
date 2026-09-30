using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTwoHit : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite[] sprites;

    private SpriteRenderer spriteRenderer;
    private int currentHitIndex;

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHitIndex = 0;

        UpdateSprite();
    }

    public bool hit() {
        currentHitIndex++;

        if (currentHitIndex < sprites.Length) {
            UpdateSprite();
            return false;
        }

        return true;
    }

    private void UpdateSprite() {
        if (spriteRenderer == null || sprites == null || sprites.Length == 0) {
            return;
        }

        int spriteIndex = Mathf.Clamp(currentHitIndex, 0, sprites.Length - 1);
        spriteRenderer.sprite = sprites[spriteIndex];
    }
}