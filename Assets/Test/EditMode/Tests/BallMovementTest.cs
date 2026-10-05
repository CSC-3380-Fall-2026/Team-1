using System.ComponentModel.DataAnnotations;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class BallMovementTest
{
    private GameObject ball;
    private Rigidbody2D rigidBody;
    private BallMovement ballMovement;

    [SetUp]
    public void SetUp()
    {
        ball = new GameObject("Test Ball");
        rigidBody = ball.AddComponent<Rigidbody2D>();
        ball.AddComponent<CircleCollider2D>();
        ballMovement = ball.AddComponent<BallMovement>();
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(ball);
    }

    [Test]
    public void BallWithZeroVelocityIsStationary()
    {
        rigidBody.linearVelocity = Vector2.zero;

        Assert.AreEqual(
            BallMovement.MotionState.Stationary,
            ballMovement.CurrentMotionState
        );
    }

    [Test] 
    public void BallWithVelocityAboveThresholdIsMoving()
    {
        rigidBody.linearVelocity = new Vector2(1f, 0f);

        ballMovement.CurrentMotionState = 
        rigidBody.linearVelocity.magnitude > 0.2f 
        ? BallMovement.MotionState.Moving 
        : BallMovement.MotionState.Stationary;

        Assert.AreEqual(
            BallMovement.MotionState.Moving,
            ballMovement.CurrentMotionState
        );
    }

    [Test]
    public void BallCanBeSetToGround()
    {
        ballMovement.CurrentGroundState = BallMovement.GroundState.Grounded;

        Assert.AreEqual(
            BallMovement.GroundState.Grounded,
            ballMovement.CurrentGroundState
        );
    }

    [Test]
    public void BallCanBeSetToAirborne()
    {
        ballMovement.CurrentGroundState = BallMovement.GroundState.Airborne;

        Assert.AreEqual(
            BallMovement.GroundState.Airborne,
            ballMovement.CurrentGroundState
        );
    }

}