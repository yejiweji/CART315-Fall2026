using UnityEngine;

public class Ball : MonoBehaviour
{
    public float speed = 100.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
   private void Start()
    {
    float x = 1.0f;
    float y = 0.0f; 

    if (Random.value < 0.5f) x = -1.0f;
    else x = 1.0f;

    if (Random.value < 0.5f) y = -1.0f;
    else y = 1.0f;

    y=y* Random.Range(0.5f, 1.0f); // This line modifies the y value to be a random value between 0.5 and 1.0, which adds some variability to the ball's initial direction.

        Vector2 direction = new Vector2(x,y);

        _rigidbody.AddForce(direction * speed); // This adds a force to the ball in the specified direction, scaled by the speed variable.
    }
}
