using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Contains multiple unit test for the hazard interactions with the ball
/// </summary>
public class HazardTest
{
    GameObject _ball;
    Rigidbody2D _rigidBody;
    CircleCollider2D _ballCollider;
    BallMovement _ballMovement;
    GameObject _waterObject;
    Water _water;

    /// <summary>
    /// Sets up a temporary ball and water hazard before each test
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _ball = new GameObject("Test Ball");
        _ball.tag = "Ball";
        _rigidBody = _ball.AddComponent<Rigidbody2D>();
        _ballCollider = _ball.AddComponent<CircleCollider2D>();
        _ballMovement = _ball.AddComponent<BallMovement>();
        _waterObject = new GameObject("Test Water");
        _waterObject.AddComponent<BoxCollider2D>().isTrigger = true;
        _water = _waterObject.AddComponent<Water>();
    }

    /// <summary>
    /// Removes temporary objects after each test
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(_ball);
        GameObject.DestroyImmediate(_waterObject);
    }

    /// <summary>
    /// Verifies that water detects a bal entering the hazard
    /// </summary>
    [Test]
    public void BallDetectsContactWithWater()
    {
        /*
        Reflection is used here to access the private OnTriggerEnter2D method in
        Water. Unity normally calls this method automatically when a collider enters the
        watter trigger, but the test calls it directly to simulate the ball entering
        the hazard and verify the resulting behavior
        */

        MethodInfo onTriggerEnter = typeof(Water).GetMethod(
            "OnTriggerEnter2D", BindingFlags.NonPublic | BindingFlags.Instance
        );

        onTriggerEnter.Invoke(_water, new object[] { _ballCollider });

        Assert.AreEqual(1.0f, _rigidBody.linearDamping);
    }

    /// <summary>
    /// Verifies that water hazard behavior is triggered when the ball enters the water
    /// </summary>
    [Test]
    public void WaterHazardBehaviorIsTriggered()
    {
        // Reflection calls the private water trigger method to simulate the ball entering the hazard
        MethodInfo onTriggerEnter = typeof(Water).GetMethod(
            "OnTriggerEnter2D", BindingFlags.NonPublic | BindingFlags.Instance
        );

        onTriggerEnter.Invoke(_water, new object[] { _ballCollider });
        Assert.AreEqual(1.0f, _rigidBody.linearDamping);
        Assert.AreEqual(1.0f, _rigidBody.angularDamping);
    }

    /// <summary>
    /// Verifies that a normal course object does not incorrectly trigger water 
    /// hazard behavior
    /// </summary>
    [Test]
    public void NormalCourseObjectDoesNotTriggerHazard()
    {
        GameObject normalObject = new GameObject("Normal Course Object");
        CircleCollider2D normalCollider = normalObject.AddComponent<CircleCollider2D>();
        Rigidbody2D normalRigidBody = normalObject.AddComponent<Rigidbody2D>();

        /* 
        Reflection calls the private water trigger method with a normal course
        object to verify that non-ball objects are ignored
        */

        MethodInfo onTriggerEnter = typeof(Water).GetMethod(
            "OnTriggerEnter2D", BindingFlags.NonPublic | BindingFlags.Instance
        );

        onTriggerEnter.Invoke(_water, new object[] { normalCollider });
        Assert.AreEqual(0.0f, normalRigidBody.linearDamping);
        GameObject.DestroyImmediate(normalObject);
    }

    /// <summary>
    /// Verifies that the ball returns to its normal damping after leaving the
    /// water hazard
    /// </summary>
    [Test]
    public void BallRespondsCorrectlyAfterLeavingWater()
    {
        /*
        Reflection calls the private water trigger methods simulate the ball 
        entering and then leaving the water hazard
        */

        MethodInfo onTriggerEnter = typeof(Water).GetMethod(
            "OnTriggerEnter2D", BindingFlags.NonPublic | BindingFlags.Instance
        );

        MethodInfo onTriggerExit = typeof(Water).GetMethod(
            "OnTriggerExit2D", BindingFlags.NonPublic | BindingFlags.Instance
        );

        onTriggerEnter.Invoke(_water, new object[] { _ballCollider });
        onTriggerExit.Invoke(_water, new object[] { _ballCollider });

        Assert.AreEqual(0.0f, _rigidBody.linearDamping);
        Assert.AreEqual(0.05f, _rigidBody.angularDamping);
    }

}