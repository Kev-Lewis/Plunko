using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class menuProjScript : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private float minimumSpeed = 2f;
    [SerializeField] private float maximumSpeed = 8f;

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

        float speed = Mathf.Clamp(lastVelocity.magnitude, minimumSpeed, maximumSpeed);
        Vector2 direction = Vector2.Reflect(lastVelocity.normalized, collision.contacts[0].normal);

        rb.velocity = direction * speed;
    }
}