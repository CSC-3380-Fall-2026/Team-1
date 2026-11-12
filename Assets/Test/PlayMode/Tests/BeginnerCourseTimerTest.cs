using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Tests the beginner course timer and verifies that completion time is measureed correctly throughout the course
/// </summary>
public class BeginnerCourseTimerTest
{
    GameObject _ball;
    BallMovement _ballMovement;
    BallTimeManager _ballTimeManager;
    GameObject _networkManagerObject;
    NetworkManager _networkManager;
    GameObject _gameMasterObject;
    GameMaster _gameMaster;

    /// <summary>
    /// Creates a temporary ball with the components needed to test the course timer
    /// before each test runs
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _networkManagerObject = new GameObject("Test NetworkManger");
        _networkManager = _networkManagerObject.AddComponent<NetworkManager>();

        UnityTransport transport = _networkManagerObject.AddComponent<UnityTransport>();
        _networkManager.NetworkConfig = new NetworkConfig();
        _networkManager.NetworkConfig.NetworkTransport = transport;

        Assert.IsTrue(_networkManager.StartHost(), "Failed to start host for test");


        // Loads the existing GameMaster prefab
        GameObject gameMasterPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Level/Prefabs/GameMaster.prefab"
            );

        Assert.IsNotNull(
            gameMasterPrefab,
            "GameMaster prefab could not be found."
        );

        // Creates the GameMaster before spawning the ball
        _gameMasterObject = UnityEngine.Object.Instantiate(gameMasterPrefab);
        _gameMaster = _gameMasterObject.GetComponent<GameMaster>();

        Assert.IsNotNull(
            _gameMaster,
            "GameMaster component is missing."
        );

        // Spawns the GameMaster on the network
        NetworkObject gameMasterNetworkObject =
            _gameMasterObject.GetComponent<NetworkObject>();

        Assert.IsNotNull(gameMasterNetworkObject);

        gameMasterNetworkObject.Spawn();


        Course testCourse = AssetDatabase.LoadAssetAtPath<Course>(
            "Assets/Level/ScriptableObjects/TestCourse.asset"
        );

        Assert.IsNotNull(testCourse, "Test Course asset could not be found.");

        FieldInfo currentCourseField = typeof(GameMaster).GetField(
            "_currentCourse",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(currentCourseField);

        currentCourseField.SetValue(_gameMaster, testCourse);

        // Creates a test player to track accumulated course time
        Player testPlayer = new Player
        {
            PlayerClientID = _networkManager.LocalClientId,
            PlayerTimeSpent = 0f
        };

        _gameMaster.Players.Add(testPlayer);

        Assert.AreEqual(1, _gameMaster.Players.Count);

        GameObject ballPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Level/Prefabs/Ball.prefab");
        Assert.IsNotNull(ballPrefab, "Ball prefab could not be found.");

        _ball = UnityEngine.Object.Instantiate(ballPrefab);
        _ball.name = "Test Ball";

        _ballMovement = _ball.GetComponent<BallMovement>();
        _ballTimeManager = _ball.GetComponent<BallTimeManager>();
        Assert.IsNotNull(_ballMovement);
        Assert.IsNotNull(_ballTimeManager);

        _ballMovement.enabled = false;

        _ball.GetComponent<NetworkObject>().Spawn();
        Assert.IsTrue(_ballTimeManager.IsOwner);
    }

    
    /// <summary>
    /// Cleans up the networked test objects after each timer test
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        if (_networkManager != null && _networkManager.IsListening)
        {
            BallTimeManager[] ballTimers =
                UnityEngine.Object.FindObjectsByType<BallTimeManager>(
                    FindObjectsSortMode.None
                );

            foreach (BallTimeManager ballTimer in ballTimers)
            {
                if (ballTimer == null)
                    continue;

                NetworkObject networkObject =
                    ballTimer.GetComponent<NetworkObject>();

                if (networkObject != null && networkObject.IsSpawned)
                    networkObject.Despawn();
            }

            _networkManager.Shutdown();
        }

        BallTimeManager[] remainingBalls =
            UnityEngine.Object.FindObjectsByType<BallTimeManager>(
                FindObjectsSortMode.None
            );

        foreach (BallTimeManager ballTimer in remainingBalls)
        {
            if (ballTimer != null)
            {
                UnityEngine.Object.DestroyImmediate(ballTimer.gameObject);
            }
        }

        if (_gameMasterObject != null)
            UnityEngine.Object.DestroyImmediate(_gameMasterObject);

        if (_networkManagerObject != null)
            UnityEngine.Object.DestroyImmediate(_networkManagerObject);
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
    
    [UnityTest]
    public IEnumerator TimerContinuesBetweenHoles()
    {
        // Confirm the test starts on the first hole
        FieldInfo currentHoleField = typeof(GameMaster).GetField(
            "_currentHoleID",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(currentHoleField);
        Assert.AreEqual(0, (int)currentHoleField.GetValue(_gameMaster));

        _ballTimeManager.TimeSpentInPlay.Value = 5f;

        _gameMaster.HandleGoalRPC(0);

        float timeout = Time.realtimeSinceStartup + 10f;

        while (SceneManager.GetActiveScene().name != "BeginnerHole2"
            && Time.realtimeSinceStartup < timeout)
        {
            yield return null;
        }

        // Confirm the next hole was loaded
        Assert.AreEqual(
            "BeginnerHole2",
            SceneManager.GetActiveScene().name
        );

        // Confirm the player's accumulated time was preserved
        Assert.AreEqual(
            5f,
            _gameMaster.Players[0].PlayerTimeSpent,
            0.01f
        );

        // Confirm the game advanced to the second hole
        Assert.AreEqual(
            1,
            (int)currentHoleField.GetValue(_gameMaster)
        );
    }

    /// <summary>
    /// Tests that the ball's timer increases accurately while the ball
    /// is in play
    /// </summary>
    /// <returns> An enumerator that allows the test to wait while the timeer runs </returns>
    [UnityTest]
    public IEnumerator TimerIncreasesWhileBallIsInPlay()
    {
        _ballMovement.BallTurnBegan?.Invoke();

        float startingTime = _ballTimeManager.TimeSpentInPlay.Value;

        // Wait one second while the timer runs
        yield return new WaitForSeconds(1f);

        float elapsedTime =
            _ballTimeManager.TimeSpentInPlay.Value - startingTime;

        // Verify that approximately one second was recorded
        Assert.AreEqual(1f, elapsedTime, 0.2f);

        _ballMovement.BallTurnEnded?.Invoke();
    }


    /// <summary>
    /// Tests that the ball's timer uses the correct time-rate modifier
    /// when ChronoShot is activated
    /// </summary>
    /// <returns> An enumerator that allows the timer to run during the test </returns>
    [UnityTest]
    public IEnumerator TimerUsesChronoShotTimeModifier()
    {
        // Sets the aiming time-rate modifier to half speed
        FieldInfo aimingModifierField = typeof(BallTimeManager).GetField(
            "_aimingTimeRateModifier",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(aimingModifierField);
        aimingModifierField.SetValue(_ballTimeManager, 0.5f);

        _ballMovement.BallTurnBegan?.Invoke();

        _ballMovement.BallBeganAiming?.Invoke();

        float startingTime = _ballTimeManager.TimeSpentInPlay.Value;

        // Waits one second while the timer runs
        yield return new WaitForSeconds(1f);

        float elapsedTime =
            _ballTimeManager.TimeSpentInPlay.Value - startingTime;

        // Verifies that the timer recorded approximately half a second.
        Assert.AreEqual(0.5f, elapsedTime, 0.1f);

        _ballMovement.BallStoppedAiming?.Invoke(false);
        _ballMovement.BallTurnEnded?.Invoke();
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
    /// Tests that the ball's timer stops increasing when the player's turn ends
    /// </summary>
    /// <returns> An enumerator that allows the timer to be checked after a delay </returns>
    [UnityTest]
    public IEnumerator TimerPausesWhenTurnEnds()
    {
        _ballMovement.BallTurnBegan?.Invoke();

        // Waits briefly to allow the timer to increase
        yield return new WaitForSeconds(0.5f);

        _ballMovement.BallTurnEnded?.Invoke();

        float stoppedTime = _ballTimeManager.TimeSpentInPlay.Value;

        // Waits to verify that the timer remains paused
        yield return new WaitForSeconds(0.5f);

        // Verifies that no additional time was recorded
        Assert.AreEqual(
            stoppedTime,
            _ballTimeManager.TimeSpentInPlay.Value,
            0.01f
        );
    }

    /// <summary>
    /// Tests that the first player's timer stops increasing
    /// when the turn switches to the second player
    /// </summary>
    /// <returns> An enumerator that allows the timer to be checked after switching turns </returns>
    [UnityTest]
    public IEnumerator TimerPausesWhenSwitchingPlayers()
    {
        Player secondPlayer = new Player
        {
            PlayerName = "Player 2",
            PlayerTimeSpent = 0f
        };

        _gameMaster.Players.Add(secondPlayer);

        // Load the existing ball prefab
        GameObject ballPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Level/Prefabs/Ball.prefab"
        );

        Assert.IsNotNull(ballPrefab);

        GameObject secondBall = UnityEngine.Object.Instantiate(ballPrefab);
        secondBall.name = "Test Ball 2";

        BallMovement secondBallMovement =
            secondBall.GetComponent<BallMovement>();

        secondBallMovement.enabled = false;

        // Spawn the second ball
        secondBall.GetComponent<NetworkObject>().Spawn();

        _ball.GetComponent<BallState>().BallID.Value = 0;
        secondBall.GetComponent<BallState>().BallID.Value = 1;

        // Register both balls with the GameMaster
        FieldInfo ballsField = typeof(GameMaster).GetField(
            "_balls",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(ballsField);

        ballsField.SetValue(
            _gameMaster,
            new List<GameObject> { _ball, secondBall }
        );

        _gameMaster.CurrentPlayerTurn.Value = 0;

        _ballMovement.BallTurnBegan?.Invoke();

        // Wait briefly so Player 1's timer increases
        yield return new WaitForSeconds(0.5f);

        _gameMaster.NextTurnRPC();

        float stoppedTime = _ballTimeManager.TimeSpentInPlay.Value;

        // Wait to verify that Player 1's timer stays paused
        yield return new WaitForSeconds(0.5f);

        // Verify that Player 1's timer did not increase
        Assert.AreEqual(
            stoppedTime,
            _ballTimeManager.TimeSpentInPlay.Value,
            0.01f
        );
    }

    /// <summary>
    /// Tests that the timer resets when a new course begins
    /// </summary>
    [Test]
    public void TimerStartsAtZeroForNewBall()
    {

        // A newly created ball timer starts at zero
        Assert.AreEqual(0f, _ballTimeManager.TimeSpentInPlay.Value);
    }





}