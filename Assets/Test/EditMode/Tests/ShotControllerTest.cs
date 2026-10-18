using NUnit.Framework;
using UnityEngine; 

/// <summary>
/// This is a unit test for verifying shot power calculations in BallMovement.cs
/// </summary>
public class ShotControllerTest
{
    private GameObject _ballObject;
    private BallMovement _ballMovement;

    /// <summary>
    /// Creates a temporary ball object with the components needed for each shot power
    /// test
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _ballObject = new GameObject("TestBall");
        _ballObject.AddComponent<Rigidbody2D>();
        _ballMovement = _ballObject.AddComponent<BallMovement>();
    }

    /// <summary>
    /// Destroy the temporary ball object after each test
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(_ballObject);
    }

    /// <summary>
    /// Tests that the min drag distance produces min shot power
    /// </summary>
    [Test]
    public void MinimumPower()
    {
        float result = _ballMovement.CalculateLaunchStrength(0f);
        Assert.AreEqual(0f, result);
    }

    /// <summary>
    /// Tests that a normal drag distance produces the expected shot power
    /// </summary>
    [Test]
    public void NormalPower()
    {
        float result = _ballMovement.CalculateLaunchStrength(2.5f);
        Assert.AreEqual(125f, result);
    }

    /// <summary>
    /// Tests that the max drag distance produces the max shot power
    /// </summary>
    [Test]
    public void MaximumPower()
    {
        float result = _ballMovement.CalculateLaunchStrength(5f);
        Assert.AreEqual(250f, result);
    }

    /// <summary>
    /// Tests that a drag distance below the min is clapped to the min shot power
    /// </summary>
    [Test]
    public void PowerBelowMinimum()
    {
        float result = _ballMovement.CalculateLaunchStrength(-1f);
        Assert.AreEqual(0f, result);
    }

    /// <summary>
    /// Tests that the drag distance above the max is clapped to the max shot power
    /// </summary>
    [Test]
    public void PowerAboveMaximum()
    {
        float result = _ballMovement.CalculateLaunchStrength(6f);
        Assert.AreEqual(250f, result);
    }

}