using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Tests the beginner course timer and verifies that completion time is measureed correctly throughout the course
/// </summary>
public class BeginnerCourseTimerTest
{
    GameObject _ball;
    BallMovement _ballMovement;
    BallTimeManager _ballTimeManager;

    /// <summary>
    /// Creates a temporary ball with the components needed to test the course timer
    /// before each test runs
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _ball = new GameObject("Test Ball");
        _ball.AddComponent<Rigidbody2D>();
        _ballMovement = _ball.AddComponent<BallMovement>();
        _ballTimeManager = _ball.AddComponent<BallTimeManager>();
    }

    /// <summary>
    /// Tests that the course timer begins measuring time when the game begins
    /// </summary>
    [Test]
    public void TimerStartWhenGameplayBegins()
    {
        /*
        Reflection is used here to access the private StartMeasuringTime method in
        BallTimeManager. The method is private because it is only used internally by
        BallTimeManager, but the test needs to call it directly to verify that the timer 
        begins measuring time correctly
        */

        MethodInfo startMeasuringTime = typeof(BallTimeManager).GetMethod(
            "StartMeasuringTime", BindingFlags.NonPublic | BindingFlags.Instance
        );

        /*
        Reflection is also used to inspect the private _measureTime field so the test
        can verify that the time measurement was enabled
        */

        startMeasuringTime.Invoke(_ballTimeManager, null);
        FieldInfo measureTimeField = typeof(BallTimeManager).GetField(
            "_measureTime", BindingFlags.NonPublic | BindingFlags.Instance
        );

        bool isMeasuringTime = (bool)measureTimeField.GetValue(_ballTimeManager);

        Assert.IsTrue(isMeasuringTime);
    }

    /// <summary>
    /// Tests that the player's accumulated time is not reset when moving between holes
    /// </summary>
    [Test]
    public void TimerContinuesBetweenHoles()
    {
        _ballMovement.BallTurnBegan?.Invoke();

        /*
        Reflection sets the private timer value to simulate time already accumulated
        during the course
        */
        
        typeof(BallTimeManager)
            .GetField("<TimeSpentInPlay>k__BackingField",

            /* 
            Reflection sets an accumulated timer value so the test can verify that completing
            a hole does not reset it
            */

                BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(_ballTimeManager, 5f);
        _ballMovement.BallTurnEnded?.Invoke();
        _ballMovement.BallTurnBegan?.Invoke();
        Assert.AreEqual(5f, _ballTimeManager.TimeSpentInPlay);
    }

    /// <summary>
    /// Tests that completing an individual hole odes not reset the player's accumulated course time
    /// </summary>
    [Test]
    public void IndividualHoleCompletionDoesNotResetTimer()
    {
        typeof(BallTimeManager)
            .GetField("<TimeSpentInPlay>k__BackingField", 
                BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(_ballTimeManager, 5f);
        
        _ballMovement.BallTurnEnded?.Invoke();
        Assert.AreEqual(5f, _ballTimeManager.TimeSpentInPlay);
    }

    /// <summary>
    /// Tests that the timer stops when the final hole of the beginner course is complete
    /// </summary>
    [Test]
    public void TimerStopsWhenFinalHoleIsCompleted()
    {
        /*
        Reflection accesses the private timer methods so the test can simulate starting and 
        stopping time measurement
        */

        MethodInfo startMeasuringTime = typeof(BallTimeManager).GetMethod(
            "StartMeasuringTime", BindingFlags.NonPublic | BindingFlags.Instance
        );

        MethodInfo stopMeasuringTime = typeof(BallTimeManager).GetMethod(
            "StopMeasuringTime", BindingFlags.NonPublic | BindingFlags.Instance
        );

        /* Reflection checks the private timer state to verify that measuring time starts
        and stops correctly
        */
        
        FieldInfo measureTimeField = typeof(BallTimeManager).GetField(
            "_measureTime", BindingFlags.NonPublic | BindingFlags.Instance
        );

        startMeasuringTime.Invoke(_ballTimeManager, null);
        Assert.IsTrue((bool)measureTimeField.GetValue(_ballTimeManager));
        stopMeasuringTime.Invoke(_ballTimeManager, null);
        Assert.IsFalse((bool)measureTimeField.GetValue(_ballTimeManager));
    }

    /// <summary>
    /// Tests that the timer resets when a new course begins
    /// </summary>
    [Test]
    public void TimerResetsWhenNewCourseBegins()
    {
        typeof(BallTimeManager)
            .GetField("<TimeSpentInPlay>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(_ballTimeManager, 10f);

        MethodInfo startMethod = typeof(BallTimeManager).GetMethod(
            "Start",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        startMethod.Invoke(_ballTimeManager, null);

        Assert.AreEqual(0f, _ballTimeManager.TimeSpentInPlay);
    }

}