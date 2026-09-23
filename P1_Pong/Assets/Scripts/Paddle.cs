using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class Paddle : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private MotherHelper _mother;

    public float speed = 10.0f;

    public Vector2 direction;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _mother = GetComponent<MotherHelper>(); // stays null on the CPU paddle
    }

    // FixedUpdate is called once per physics update
    private void FixedUpdate()
    {
        if (direction.sqrMagnitude == 0) return;

        _rigidBody.AddForce(direction * speed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_mother == null) return;

        Ball ball = collision.gameObject.GetComponent<Ball>();
        if (ball == null) return;

        _mother.RegisterPlayerHit();
    }
}