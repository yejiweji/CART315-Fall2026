using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ball : MonoBehaviour
{
    private Rigidbody2D _rigidBody;

    public float speed = 100.0f;

    [Header("Keep the ball moving sideways")]
    [Range(0.5f, 0.95f)] public float minHorizontalShare = 0.75f;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    public void ResetBall()
    {
        _rigidBody.linearVelocity = Vector2.zero;
        _rigidBody.angularVelocity = 0;
        transform.position = Vector3.zero;
    }

    public void AddStartingForce()
    {
        float x = Random.value < 0.5f ? -1.0f : 1.0f;
        float y = (Random.value < 0.5f ? -1.0f : 1.0f) * Random.Range(0.2f, 0.5f);

        Vector2 direction = new Vector2(x, y);

        _rigidBody.AddForce(direction * speed);
    }

    private void FixedUpdate()
    {
        Vector2 v = _rigidBody.linearVelocity;
        float mag = v.magnitude;
        if (mag < 0.01f) return; // ball is stopped between rounds

        float minX = mag * minHorizontalShare;
        if (Mathf.Abs(v.x) < minX)
        {
            float newX = (v.x >= 0f ? 1f : -1f) * minX;
            float newY = (v.y >= 0f ? 1f : -1f) * Mathf.Sqrt(mag * mag - newX * newX);
            _rigidBody.linearVelocity = new Vector2(newX, newY);
        }
    }
}