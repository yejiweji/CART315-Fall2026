using UnityEngine;

public class CPUController : MonoBehaviour
{
    public Ball ball;
    public Paddle paddle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 ballPos = ball.transform.position;
        Vector2 paddlePos = paddle.transform.position;

        paddle.direction = new Vector2(0.0f, (ballPos - paddlePos).y);
    }
}