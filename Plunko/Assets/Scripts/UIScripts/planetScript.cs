using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planetScript : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float minSpeed = 0.3f;
    [SerializeField] private float maxSpeed = 0.7f;
    [SerializeField] private float destroyXPosition = 12.5f;

    [Header("Size Settings")]
    [SerializeField] private float minSize = 7f;
    [SerializeField] private float maxSize = 9f;

    private float moveSpeed;

    private void Start() {
        if (moveSpeed <= 0f) {
            RandomizePlanet();
        }
    }

    private void Update() {
        transform.position += transform.right * moveSpeed * Time.deltaTime;

        if (transform.position.x > destroyXPosition) {
            Destroy(gameObject);
        }
    }

    private void RandomizePlanet() {
        moveSpeed = Random.Range(minSpeed, maxSpeed);

        float randomSize = Random.Range(minSize, maxSize);
        transform.localScale = new Vector2(randomSize, randomSize);
    }

    public float GetMoveSpeed() {
        return moveSpeed;
    }

    public void SetMoveSpeed(float speed) {
        moveSpeed = speed;
    }
}