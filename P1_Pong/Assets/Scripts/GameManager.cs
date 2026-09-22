using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Score score;
    public Ball ball;
    public GameStates gameState;

    private void Start()
    {
        StartRound();
    }

    public void StartRound()
    {
        ball.ResetBall();
        ball.AddStartingForce();
    }

    public void CourtTriggered(int courtId)
    {
        score.IncreaseScore((courtId == 0 ? 1 : 0)); //If left court was triggered, right player scores & vice versa
        StartRound();
    }
}