
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Contains unit tests for ChronoShot integration with the ball movement system.
/// </summary>
public class ChronoShotBallMovementTest
{
    private GameObject _gameMasterObject;
    private GameMaster _gameMaster;
    private GameObject _ball;
    private Rigidbody2D _ballRigidbody;
    private BallMovement _ballMovement;

    /// <summary>
    /// Creates the GameMaster and ball used for each test.
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

        Time.timeScale = 1.0f;
    }

    /// <summary>
    /// Verifies that the ball retains its movement state and velocity during slow motion.
    /// </summary>
    [Test]
    public void BallRemainsMovingDuringChronoShot()
    {
        _ballRigidbody.linearVelocity = new Vector2(5f, 2f);
        _ballMovement.CurrentMotionState = BallMovement.MotionState.Moving;

        _gameMaster.SetTimeRate(0.2f);

        Assert.AreEqual(0.2f, Time.timeScale);
        Assert.AreNotEqual(Vector2.zero, _ballRigidbody.linearVelocity);
        Assert.AreEqual(
            BallMovement.MotionState.Moving,
            _ballMovement.CurrentMotionState
        );
    }

    /// <summary>
    /// Verifies that the ball remains airborne during slow motion.
    /// </summary>
    [Test]
    public void BallRemainsAirborneDuringChronoShot()
    {
        _gameMaster.SetTimeRate(0.2f);

        Assert.AreEqual(0.2f, Time.timeScale);
        Assert.AreEqual(
            BallMovement.GroundState.Airborne,
            _ballMovement.CurrentGroundState
        );
    }

    /// <summary>
    /// Verifies that the ball retains its movement after slow motion ends.
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
        Assert.AreEqual(
            BallMovement.MotionState.Moving,
            _ballMovement.CurrentMotionState
        );
    }

    /// <summary>
    /// Verifies that slow motion does not incorrectly change the ball's movement or ground state.
    /// </summary>
    [Test]
    public void ChronoShotDoesNotChangeBallState()
    {
        _ballRigidbody.linearVelocity = new Vector2(5f, 2f);
        _ballMovement.CurrentMotionState = BallMovement.MotionState.Moving;
        _ballMovement.CurrentGroundState = BallMovement.GroundState.Airborne;

        _gameMaster.SetTimeRate(0.2f);

        Assert.AreEqual(
            BallMovement.MotionState.Moving,
            _ballMovement.CurrentMotionState
        );
        Assert.AreEqual(
            BallMovement.GroundState.Airborne,
            _ballMovement.CurrentGroundState
        );

        _gameMaster.ResetChronoShot();

        Assert.AreEqual(
            BallMovement.MotionState.Moving,
            _ballMovement.CurrentMotionState
        );
        Assert.AreEqual(
            BallMovement.GroundState.Airborne,
            _ballMovement.CurrentGroundState
        );
    }

    /// <summary>
    /// Removes temporary GameObjects and restores normal time after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _gameMaster.ResetChronoShot();

        GameObject.DestroyImmediate(_ball);
        GameObject.DestroyImmediate(_gameMasterObject);
    }
}
