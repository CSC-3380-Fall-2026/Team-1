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
        MethodInfo startMeasuringTime = typeof(BallTimeManager).GetMethod(
            "StartMeasuringTime", BindingFlags.NonPublic | BindingFlags.Instance
        );

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
        typeof(BallTimeManager)
            .GetField("<TimeSpentInPlay>k__BackingField", 
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
        MethodInfo startMeasuringTime = typeof(BallTimeManager).GetMethod(
            "StartMeasuringTime", BindingFlags.NonPublic | BindingFlags.Instance
        );

        MethodInfo stopMeasuringTime = typeof(BallTimeManager).GetMethod(
            "StopMeasuringTime", BindingFlags.NonPublic | BindingFlags.Instance
        );

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