using System.Collections;
using NUnit.Framework;
using UnityEngine;


public class OutOfBoundsTests
{
    private GameObject gameMasterObject;
    private GameObject outOfBoundsObject;
    private GameObject resetPoint;
    private GameObject ball;
    private OutOfBounds outOfBounds;
    private float originalTimeScale;
    private float originalFixedDeltaTime;

    [SetUp]
    public void SetUp()
    {
        originalTimeScale = Time.timeScale;
        originalFixedDeltaTime = Time.fixedDeltaTime;

                    // creates the game master and prevent its normal startup spawning
        gameMasterObject = new GameObject("Test GameMaster");
        GameMaster gameMaster = gameMasterObject.AddComponent<GameMaster>();
        gameMaster.enabled = false;

                // create the reset point
        resetPoint = new GameObject("Test Reset Point");
        resetPoint.transform.position = new Vector2(5f, 5f);
        resetPoint.AddComponent<CircleCollider2D>();

                // create the out of bounds object
        outOfBoundsObject = new GameObject("Test Out Of Bounds");
        outOfBounds = outOfBoundsObject.AddComponent<OutOfBounds>();
        outOfBounds.ResetPoints = new GameObject[] { resetPoint };

             // ball is created
        ball = new GameObject("Test Ball");
        ball.tag = "Ball";
        ball.AddComponent<CircleCollider2D>();
        ball.AddComponent<Rigidbody2D>();

        
        gameMaster.ResetObstructionLayers = 0;
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        if (ball != null)
            Object.DestroyImmediate(ball);

        if (resetPoint != null)
            Object.DestroyImmediate(resetPoint);

        if (outOfBoundsObject != null)
            Object.DestroyImmediate(outOfBoundsObject);

        if (gameMasterObject != null)
            Object.DestroyImmediate(gameMasterObject);

        
        typeof(GameMaster)
            .GetProperty("Instance")
            .GetSetMethod(true)
            .Invoke(null, new object[] { null });
    }

    
    private void TriggerOutOfBounds(Collider2D collider) // helper to invoke the existing private trigger function
    {
        MethodInfo triggerMethod = typeof(OutOfBounds).GetMethod(
            "OnTriggerEnter2D",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        Assert.IsNotNull(triggerMethod, "OutOfBounds trigger method was not found");

        triggerMethod.Invoke(outOfBounds, new object[] { collider });
    }

    [Test]
    public void BallEnteringOutOfBoundsIsReset()
    {
        ball.transform.position = new Vector2(20f, 20f);

        TriggerOutOfBounds(ball.GetComponent<Collider2D>());

        Assert.AreEqual(
            resetPoint.transform.position,
            ball.transform.position,
            "The ball should be moved to the reset point"
        );
    }

    [Test]
    public void NonBallObjectIsNotReset()
    {
        GameObject otherObject = new GameObject("Test Other Object");
        otherObject.transform.position = new Vector2(20f, 20f);
        CircleCollider2D otherCollider = otherObject.AddComponent<CircleCollider2D>();

        Vector3 originalPosition = otherObject.transform.position;

        TriggerOutOfBounds(otherCollider);

        Assert.AreEqual(
            originalPosition,
            otherObject.transform.position,
            "An object without the Ball tag should not be reset"
        );

        Object.DestroyImmediate(otherObject);
    }

    [Test]
    public void ResetBallClearsVelocity()
    {
        Rigidbody2D ballRigidbody = ball.GetComponent<Rigidbody2D>();

        ballRigidbody.linearVelocity = new Vector2(10f, 5f);
        ballRigidbody.angularVelocity = 10f;

        TriggerOutOfBounds(ball.GetComponent<Collider2D>());

        Assert.AreEqual(
            Vector2.zero,
            ballRigidbody.linearVelocity,
            "The ball's linear velocity should be cleared"
        );

        Assert.AreEqual(
            0f,
            ballRigidbody.angularVelocity,
            "The ball's angular velocity should be cleared"
        );
    }

    [Test]
    public void RepeatedOutOfBoundsEventsDoNotThrowErrors()
    {
        Collider2D ballCollider = ball.GetComponent<Collider2D>();

        Assert.DoesNotThrow(() =>
        {
            for (int i = 0; i < 5; i++)
            {
                ball.transform.position = new Vector2(20f + i, 20f + i);
                TriggerOutOfBounds(ballCollider);
            }
        });

        Assert.AreEqual(
            resetPoint.transform.position,
            ball.transform.position,
            "The ball should still reset correctly after repeated events"
        );
    }
}
