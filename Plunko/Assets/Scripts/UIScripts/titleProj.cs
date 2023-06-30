using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class titleProj : MonoBehaviour
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
        if (collision.gameObject.tag == "Ball")
        {
            rb.AddForce(new Vector2(Random.Range(-5, 5), Random.Range(-5, 5)), ForceMode2D.Impulse);
        }
        else
        {
            rb.velocity = Vector3.Reflect(lastVelocity, collision.contacts[0].normal);
        }
    }
}
