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
        // Create a temporary ball for each test
        ball = new GameObject("Test Ball");

        // Adds the components needed to test the ball
        rigidBody = ball.AddComponent<Rigidbody2D>();
        ball.AddComponent<CircleCollider2D>();
        ballMovement = ball.AddComponent<BallMovement>();
    }

    [TearDown]
    public void TearDown()
    {
        // Delete the temporary ball after each test
        GameObject.DestroyImmediate(ball);
    }

    [Test]
    public void BallWithZeroVelocityIsStationary()
    {
        // Give the ball no movement
        rigidBody.linearVelocity = Vector2.zero;

        // Check that the ball is considered stationary 
        Assert.AreEqual(
            BallMovement.MotionState.Stationary,
            ballMovement.CurrentMotionState
        );
    }

    [Test] 
    public void BallWithVelocityAboveThresholdIsMoving()
    {
        // Give the ball enough velocity to be considered moving 
        rigidBody.linearVelocity = new Vector2(1f, 0f);

        ballMovement.CurrentMotionState = 
        rigidBody.linearVelocity.magnitude > 0.2f 
        ? BallMovement.MotionState.Moving 
        : BallMovement.MotionState.Stationary;

        // Check that the ball is considered moving
        Assert.AreEqual(
            BallMovement.MotionState.Moving,
            ballMovement.CurrentMotionState
        );
    }

    [Test]
    public void BallCanBeSetToGround()
    {
        // Set the ball's state to ground
        ballMovement.CurrentGroundState = BallMovement.GroundState.Grounded;

        // Check that the ball is now on the ground
        Assert.AreEqual(
            BallMovement.GroundState.Grounded,
            ballMovement.CurrentGroundState
        );
    }

    [Test]
    public void BallCanBeSetToAirborne()
    {

        // Set the ball's state to airborne
        ballMovement.CurrentGroundState = BallMovement.GroundState.Airborne;

        // Check that the ball is now airborne
        Assert.AreEqual(
            BallMovement.GroundState.Airborne,
            ballMovement.CurrentGroundState
        );
    }

}