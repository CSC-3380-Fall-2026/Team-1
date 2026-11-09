using NUnit.Framework;
using UnityEngine;


public class ChronoShotControllerTests
{
    private GameObject _testObject;
    private BallMovement _ballMovement;
    private ChronoShotController _chronoShotController;

    [SetUp]
    public void SetUp()
    {
        _testObject = new GameObject("ChronoShotTest");

        
        _testObject.AddComponent<Rigidbody2D>();
        _ballMovement = _testObject.AddComponent<BallMovement>();  // this BallMovement needs a Rigidbody2D, so it is added first
        _chronoShotController = _testObject.AddComponent<ChronoShotController>();

        _ballMovement.CurrentGroundState = BallMovement.GroundState.Airborne;
        _chronoShotController.MaxActivations = 3;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_testObject);
    }

    // verified ChronoShot can activate while the ball is airborne.
    
    [Test]
    public void TryActivate_AirborneBall_ActivatesChronoShot()
    {
        bool result = _chronoShotController.TryActivate();

        Assert.IsTrue(result);
        Assert.IsTrue(_chronoShotController.IsActive);
    }

    [Test]
    public void TryActivate_GroundedBall_DoesNotActivateChronoShot()
    {
        _ballMovement.CurrentGroundState = BallMovement.GroundState.Grounded;

        bool result = _chronoShotController.TryActivate();

        Assert.IsFalse(result);
        Assert.IsFalse(_chronoShotController.IsActive);
        Assert.AreEqual(0, _chronoShotController.ActivationCount);
    }

    [Test]
    public void TryActivate_SuccessfulActivation_TracksUsage()
    {
        _chronoShotController.TryActivate();
        Assert.AreEqual(1, _chronoShotController.ActivationCount);

        _chronoShotController.EndChronoShot();

        _chronoShotController.TryActivate();
        Assert.AreEqual(2, _chronoShotController.ActivationCount);
    }

    [Test]
    public void TryActivate_ActivationLimitReached_PreventsFurtherActivation() // verifies the activation once the limit has been reached
    {
        for (int i = 0; i < _chronoShotController.MaxActivations; i++)
        {
            bool result = _chronoShotController.TryActivate();

            Assert.IsTrue(result);

            _chronoShotController.EndChronoShot();
        }

        bool finalResult = _chronoShotController.TryActivate();

        Assert.IsFalse(finalResult);
        Assert.AreEqual(3, _chronoShotController.ActivationCount);
    }

    [Test]
    public void ResetActivations_UsedActivations_RestoresAvailability() // this will verify the activations will reset 
    {
        _chronoShotController.TryActivate();
        _chronoShotController.EndChronoShot();

        _chronoShotController.ResetActivations();

        Assert.AreEqual(0, _chronoShotController.ActivationCount);

        bool result = _chronoShotController.TryActivate();

        Assert.IsTrue(result);
        Assert.IsTrue(_chronoShotController.IsActive);
    }

    [Test]
    public void TryActivate_AlreadyActive_DoesNotCountTwice() // verifies chronoshot that is currently running, cant be activated
    {
        _chronoShotController.TryActivate();

        bool secondResult = _chronoShotController.TryActivate();

        Assert.IsFalse(secondResult);
        Assert.AreEqual(1, _chronoShotController.ActivationCount);
    }
}

