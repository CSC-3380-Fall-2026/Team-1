using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unit Test for the ball reset system
/// </summary>
public class BallControllerTest
{

    // References to objects and components for testing
    GameObject _gameMasterObject;
    GameMaster _gameMaster;
    GameObject _ball;
    Rigidbody2D _ballRigidbody;
    CircleCollider2D _ballCollider;
    GameObject _resetPoint;
    CircleCollider2D _resetPointCollider;

    /// <summary>
    /// Creates the GameMaster, ball, and reset point used for each test
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _gameMasterObject = new GameObject("GameMaster");
        _gameMaster = _gameMasterObject.AddComponent<GameMaster>();
        _ball = new GameObject("Ball");
        _ballRigidbody = _ball.AddComponent<Rigidbody2D>();
        _ballCollider = _ball.AddComponent<CircleCollider2D>();
        _resetPoint = new GameObject("ResetPoint");
        _resetPointCollider = _resetPoint.AddComponent<CircleCollider2D>();
        _resetPoint.transform.position = new Vector2(5f, 5f);
    }

    /// <summary>
    /// Tests that the ball returns to the correct reset position
    /// </summary>
    [Test]
    public void BallReturnToCorrectPosition()
    {
        _ball.transform.position = new Vector2(10f, 10f);
        GameObject[] resetPoints = { _resetPoint };
        //_gameMaster.ResetBall(_ball, resetPoints);
        Assert.AreEqual(_resetPoint.transform.position, _ball.transform.position);
    }

    /// <summary>
    /// Tests that the ball's velocity is cleared after a reset
    /// </summary>
    [Test]
    public void VelocityIsClearedAfterReset()
    {
        _ballRigidbody.linearVelocity = new Vector2(10f, 5f);
        _ballRigidbody.angularVelocity = 5f;
        GameObject[] resetPoints = { _resetPoint };
        //_gameMaster.ResetBall(_ball, resetPoints);
        Assert.AreEqual(Vector2.zero, _ballRigidbody.linearVelocity);
        Assert.AreEqual(0f, _ballRigidbody.angularVelocity);
    }

    /// <summary>
    /// Tests that the ball does not continue moving after being reset
    /// </summary>
    [Test]
    public void BallStopsMovingAfterReset()
    {
        _ballRigidbody.linearVelocity = new Vector2(8f, 4f);
        _ballRigidbody.angularVelocity = 3f;
        GameObject[] resetPoints = { _resetPoint };
        //_gameMaster.ResetBall(_ball, resetPoints);
        Assert.AreEqual(Vector2.zero, _ballRigidbody.linearVelocity);
        Assert.AreEqual(0f, _ballRigidbody.angularVelocity);
        Assert.AreEqual(_resetPoint.transform.position, _ball.transform.position);
    }

    /// <summary>
    /// Tests that the ball can reset after going out of bound
    /// </summary>
    [Test]
    public void BallCanResetAfterOutOfBounds()
    {
        _ball.transform.position = new Vector2(100f, -100f);
        _ballRigidbody.linearVelocity = new Vector2(10f, -10f);
        _ballRigidbody.angularVelocity = 5f;
        GameObject[] resetPoints = { _resetPoint };
        //_gameMaster.ResetBall(_ball, resetPoints);
        Assert.AreEqual(_resetPoint.transform.position, _ball.transform.position);
        Assert.AreEqual(Vector2.zero, _ballRigidbody.linearVelocity);
        Assert.AreEqual(0f, _ballRigidbody.angularVelocity);
    }

    /// <summary>
    /// Removes all temporary GameObject created during each test
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(_ball);
        GameObject.DestroyImmediate(_resetPoint);
        GameObject.DestroyImmediate(_gameMasterObject);
    }
}

