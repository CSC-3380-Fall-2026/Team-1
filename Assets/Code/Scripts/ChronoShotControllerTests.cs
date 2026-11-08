
using NUnit.Framework;
using UnityEngine;

public class ChronoShotControllerTests
{
    private GameObject testObject;
    private BallMovement ballMovement;
    private ChronoShotController chronoShotController;

    [SetUp]
    public void SetUp()
    {
        testObject = new GameObject("ChronoShotTest"); // creates a test object for the components

        testObject.AddComponent<Rigidbody2D>(); // adds physics for the ball
        ballMovement = testObject.AddComponent<BallMovement>(); // 
        chronoShotController = testObject.AddComponent<ChronoShotController>(); // adds the chronoshot controller

        ballMovement.CurrentGroundState = BallMovement.GroundState.Airborne; 
        chronoShotController.MaxActivations = 3; // sets the maximum number of activations
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(testObject); // removes the test object after each test
    }

    [Test]
    public void TryActivate_AirborneBall_ActivatesChronoShot()
    {
        
        bool result = chronoShotController.TryActivate();

        Assert.IsTrue(result); // checks that activation was successful
        Assert.IsTrue(chronoShotController.IsActive); 
    }

    [Test]
    public void TryActivate_GroundedBall_DoesNotActivateChronoShot()
    {
        ballMovement.CurrentGroundState = BallMovement.GroundState.Grounded; // places the ball on the ground

       
        bool result = chronoShotController.TryActivate();

        Assert.IsFalse(result); // checks that activation was rejected
        Assert.IsFalse(chronoShotController.IsActive); 
        Assert.AreEqual(0, chronoShotController.ActivationCount); // checks that no activation was used
    }

    [Test]
    public void TryActivate_SuccessfulActivation_TracksUsage()
    {
        chronoShotController.TryActivate(); 

        Assert.AreEqual(1, chronoShotController.ActivationCount); 

        chronoShotController.EndChronoShot(); // ends the current chronoshot session

        chronoShotController.TryActivate(); // activates chronoshot again

        Assert.AreEqual(2, chronoShotController.ActivationCount); 
    }

    [Test]
    public void TryActivate_ActivationLimitReached_PreventsFurtherActivation()
    {
        
        for (int i = 0; i < chronoShotController.MaxActivations; i++)
        {
            bool result = chronoShotController.TryActivate(); // attempts another activation

            Assert.IsTrue(result); // checks that the available activation works

            chronoShotController.EndChronoShot(); 
        }

        
        bool finalResult = chronoShotController.TryActivate();

        Assert.IsFalse(finalResult); // checks that additional activation is rejected
        Assert.AreEqual(3, chronoShotController.ActivationCount); 
    }

    [Test]
    public void ResetActivations_UsedActivations_RestoresAvailability()
    {
        chronoShotController.TryActivate(); // uses the first activation
        chronoShotController.EndChronoShot(); 

        chronoShotController.ResetActivations(); // resets the available activations

        Assert.AreEqual(0, chronoShotController.ActivationCount); 

        
        bool result = chronoShotController.TryActivate();

        Assert.IsTrue(result); // checks that activation is available again
        Assert.IsTrue(chronoShotController.IsActive); 
    }
}
