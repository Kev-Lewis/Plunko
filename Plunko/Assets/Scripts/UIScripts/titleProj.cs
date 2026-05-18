using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class titleProj : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private float minimumSpeed = 2f;
    [SerializeField] private float maximumSpeed = 8f;

    [Header("Ball Hit Settings")]
    [SerializeField] private float minHitForce = -5f;
    [SerializeField] private float maxHitForce = 5f;

    private Rigidbody2D rb;
    private Vector2 lastVelocity;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        if (rb != null) {
            lastVelocity = rb.velocity;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (rb == null || collision.contactCount == 0) {
            return;
        }

        if (collision.gameObject.CompareTag("Ball")) {
            AddRandomImpulse();
            return;
        }

        ReflectVelocity(collision);
    }

    private void AddRandomImpulse() {
        Vector2 randomForce = new Vector2(
            Random.Range(minHitForce, maxHitForce),
            Random.Range(minHitForce, maxHitForce)
        );

        rb.AddForce(randomForce, ForceMode2D.Impulse);
    }

    private void ReflectVelocity(Collision2D collision) {
        float speed = Mathf.Clamp(lastVelocity.magnitude, minimumSpeed, maximumSpeed);
        Vector2 direction = Vector2.Reflect(lastVelocity.normalized, collision.contacts[0].normal);

        rb.velocity = direction * speed;
    }
}