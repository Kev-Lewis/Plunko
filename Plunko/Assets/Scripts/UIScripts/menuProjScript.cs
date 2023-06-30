using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class menuProjScript : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 lastVelocity;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        lastVelocity = rb.velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        rb.velocity = Vector3.Reflect(lastVelocity, collision.contacts[0].normal);
    }
}
