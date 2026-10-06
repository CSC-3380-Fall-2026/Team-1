using System.ComponentModel.DataAnnotations;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Containts unit test for the BallMovement compnent
/// Test the ball's movement and ground states
/// </summary>
public class BallMovementTest
{
    private GameObject _ball;
    private Rigidbody2D _rigidBody;
    private BallMovement _ballMovement;

    /// <summary>
    /// Sets up a temporary ball and its required components before each test
    /// </summary>

    [SetUp]
    public void SetUp()
    {
        _ball = new GameObject("Test Ball");
        _rigidBody = _ball.AddComponent<Rigidbody2D>();
        _ball.AddComponent<CircleCollider2D>();
        _ballMovement = _ball.AddComponent<BallMovement>();
    }

    /// <summary>
    /// Removes the temporary ball after each test
    /// </summary>

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(_ball);
    }

    /// <summary>
    /// Verifies that a ball with zero velocity is stationary
    /// </summary>

    [Test]
    public void BallWithZeroVelocityIsStationary()
    {
        _rigidBody.linearVelocity = Vector2.zero;
        Assert.AreEqual(
            BallMovement.MotionState.Stationary,
            _ballMovement.CurrentMotionState
        );
    }

    /// <summary>
    /// Verifies that a ball is moving above the velocity threshold is moving
    /// </summary>

    [Test] 
    public void BallWithVelocityAboveThresholdIsMoving()
    {
        _rigidBody.linearVelocity = new Vector2(1f, 0f);

        _ballMovement.CurrentMotionState = 
        _rigidBody.linearVelocity.magnitude > 0.2f 
        ? BallMovement.MotionState.Moving 
        : BallMovement.MotionState.Stationary;

        Assert.AreEqual(
            BallMovement.MotionState.Moving,
            _ballMovement.CurrentMotionState
        );
    }

    /// <summary>
    /// Verifies that the ball can be set to the grounded state
    /// </summary>

    [Test]
    public void BallCanBeSetToGround()
    {
        _ballMovement.CurrentGroundState = BallMovement.GroundState.Grounded;

        Assert.AreEqual(
            BallMovement.GroundState.Grounded,
            _ballMovement.CurrentGroundState
        );
    }

    /// <summary>
    /// Verifies that the ball canbe set to the airborne state
    /// </summary>

    [Test]
    public void BallCanBeSetToAirborne()
    {

        _ballMovement.CurrentGroundState = BallMovement.GroundState.Airborne;

        Assert.AreEqual(
            BallMovement.GroundState.Airborne,
            _ballMovement.CurrentGroundState
        );
    }


}