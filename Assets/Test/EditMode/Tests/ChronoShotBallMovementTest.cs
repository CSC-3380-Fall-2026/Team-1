using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unit test for ChronoShot integration with the ball movement system
/// </summary>
public class ChronoShotBallMovementTest
{
    // References to objects and components for testing
    GameObject _gameMasterObject;
    GameMaster _gameMaster;
    GameObject _ball;
    Rigidbody2D _ballRigidbody;
    BallMovement _ballMovement;

    /// <summary>
    /// Creates the GameMaster and ball used for each test
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _gameMasterObject = new GameObject("GameMaster");
        _gameMaster = _gameMasterObject.AddComponent<GameMaster>();
        _ball = new GameObject("Ball");
        _ballRigidbody = _ball.AddComponent<Rigidbody2D>();
        _ballMovement = _ball.AddComponent<BallMovement>();
        _ballMovement.CurrentGroundState = BallMovement.GroundState.Airborne;
        _ballMovement.CurrentGroundState = BallMovement.GroundState.Airborne;
        Time.timeScale = 1.0f;
    }

    /// <summary>
    /// Tests that the ball remains moving when ChronoShot slows time
    /// </summary>
    [Test]
    public void BallRemainsMovingDuringChronoShot()
    {
        _ballRigidbody.linearVelocity = new Vector2(5f, 2f);
        _ballMovement.CurrentMotionState = BallMovement.MotionState.Moving;
        _gameMaster.SetTimeRate(0.2f);
        Assert.AreEqual(0.2f, Time.timeScale);
        Assert.AreNotEqual(Vector2.zero, _ballRigidbody.linearVelocity);
        Assert.AreEqual(BallMovement.MotionState.Moving, _ballMovement.CurrentMotionState);
    }

    /// <summary>
    /// Tests that the ball remains airborne while ChronoShot is active
    /// </summary>
    [Test]
    public void BallRemainsAirborneDuringChronoShot()
    {
        _ballMovement.CurrentGroundState = BallMovement.GroundState.Airborne;
        _gameMaster.SetTimeRate(0.2f);
        Assert.AreEqual(0.2f, Time.timeScale);
        Assert.AreEqual(BallMovement.GroundState.Airborne, _ballMovement.CurrentGroundState);
    }

    /// <summary>
    /// Tests that normal ball movement continues after ChronoShot ends
    /// </summary>
    [Test]
    public void BallMovementContinuesAfterChronoShotEnds()
    {
        _ballRigidbody.linearVelocity = new Vector2(5f, 2f);
        _ballMovement.CurrentMotionState = BallMovement.MotionState.Moving;
        _gameMaster.SetTimeRate(0.2f);
        _gameMaster.ResetChronoShot();
        Assert.AreEqual(1.0f, Time.timeScale);
        Assert.AreNotEqual(Vector2.zero, _ballRigidbody.linearVelocity);
        Assert.AreEqual(BallMovement.MotionState.Moving, _ballMovement.CurrentMotionState);
    }

    /// <summary>
    /// Tests that ChronoShot does not incorrectly change the ball's movement or ground state
    /// </summary>
    [Test]
    public void ChronoShotDoesNotChangeBallState()
    {
        _ballRigidbody.linearVelocity = new Vector2(5f, 2f);
        _ballMovement.CurrentMotionState = BallMovement.MotionState.Moving;
        _ballMovement.CurrentGroundState = BallMovement.GroundState.Airborne;
        _gameMaster.SetTimeRate(0.2f);
        Assert.AreEqual(BallMovement.MotionState.Moving, _ballMovement.CurrentMotionState);
        Assert.AreEqual(BallMovement.GroundState.Airborne, _ballMovement.CurrentGroundState);
        _gameMaster.ResetChronoShot();
        Assert.AreEqual(BallMovement.MotionState.Moving, _ballMovement.CurrentMotionState);
        Assert.AreEqual(BallMovement.GroundState.Airborne, _ballMovement.CurrentGroundState);
    }

    /// <summary>
    /// Removes all temporary GameObjects created during each test
    /// </summary>
    [TearDown] 
    public void TearDown()
    {
        Time.timeScale = 1.0f;
        GameObject.DestroyImmediate(_ball);
        GameObject.DestroyImmediate(_gameMasterObject);
    }
}