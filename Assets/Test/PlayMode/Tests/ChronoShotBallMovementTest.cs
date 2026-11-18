
using UnityEditor;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Tests the integration between ChronoShot and the ball movement system.
/// </summary>
public class ChronoShotBallMovementTest
{
    // References to objects and components used during testing.
    GameObject _networkManagerObject;
    NetworkManager _networkManager;
    GameObject _gameMasterObject;
    GameMaster _gameMaster;
    GameObject _ball;
    Rigidbody2D _ballRigidbody;
    BallMovement _ballMovement;

    /// <summary>
    /// Creates and initializes the network manager, GameMaster, and Ball prefab for each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // Creates the networking transport needed to start a test host.
        _networkManagerObject = new GameObject("Test NetworkManager");
        UnityTransport transport = _networkManagerObject.AddComponent<UnityTransport>();

        // Configures and starts the NetworkManager.
        _networkManager = _networkManagerObject.AddComponent<NetworkManager>();
        _networkManager.NetworkConfig = new NetworkConfig();
        _networkManager.NetworkConfig.NetworkTransport = transport;

        Assert.IsTrue(_networkManager.StartHost(), "Failed to start the test network host.");

        // Loads and instantiates the GameMaster prefab.
        GameObject gameMasterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Level/Prefabs/GameMaster.prefab"
        );

        Assert.IsNotNull(gameMasterPrefab, "GameMaster prefab could not be loaded.");

        _gameMasterObject = Object.Instantiate(gameMasterPrefab);
        _gameMaster = _gameMasterObject.GetComponent<GameMaster>();
        _gameMasterObject.GetComponent<NetworkObject>().Spawn();

        // Loads and instantiates the actual Ball prefab.
        GameObject ballPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Level/Prefabs/Ball.prefab"
        );

        Assert.IsNotNull(ballPrefab, "Ball prefab could not be loaded.");

        _ball = Object.Instantiate(ballPrefab);
        _ball.name = "Test Ball";

        // Gets the components required by the tests.
        _ballRigidbody = _ball.GetComponent<Rigidbody2D>();
        _ballMovement = _ball.GetComponent<BallMovement>();

        Assert.IsNotNull(_ballRigidbody, "Rigidbody2D component is missing.");
        Assert.IsNotNull(_ballMovement, "BallMovement component is missing.");

        // Initializes the ball in an airborne state.
        _ballMovement.enabled = false;
        _ballMovement.CurrentGroundState = BallMovement.GroundState.Airborne;

        NetworkObject ballNetworkObject = _ball.GetComponent<NetworkObject>();

        Assert.IsNotNull(ballNetworkObject, "NetworkObject component is missing.");

        ballNetworkObject.Spawn();

        Time.timeScale = 1.0f;
    }

    /// <summary>
    /// Verifies that the ball retains its velocity and moving state during ChronoShot.
    /// </summary>
    [Test]
    public void BallRemainsMovingDuringChronoShot()
    {
        _ballRigidbody.linearVelocity = new Vector2(5f, 2f);
        _ballMovement.CurrentMotionState = BallMovement.MotionState.Moving;

        _gameMaster.SetTimeRateRPC(0.2f);

        Assert.AreEqual(0.2f, Time.timeScale);
        Assert.AreNotEqual(Vector2.zero, _ballRigidbody.linearVelocity);
        Assert.AreEqual(
            BallMovement.MotionState.Moving,
            _ballMovement.CurrentMotionState
        );
    }

    /// <summary>
    /// Verifies that the ball remains airborne while ChronoShot is active.
    /// </summary>
    [Test]
    public void BallRemainsAirborneDuringChronoShot()
    {
        _ballMovement.CurrentGroundState = BallMovement.GroundState.Airborne;

        _gameMaster.SetTimeRateRPC(0.2f);

        Assert.AreEqual(0.2f, Time.timeScale);
        Assert.AreEqual(
            BallMovement.GroundState.Airborne,
            _ballMovement.CurrentGroundState
        );
    }

    /// <summary>
    /// Verifies that normal ball movement resumes after ChronoShot ends.
    /// </summary>
    [Test]
    public void BallMovementContinuesAfterChronoShotEnds()
    {
        _ballRigidbody.linearVelocity = new Vector2(5f, 2f);
        _ballMovement.CurrentMotionState = BallMovement.MotionState.Moving;

        _gameMaster.SetTimeRateRPC(0.2f);
        _gameMaster.ResetChronoShot();

        Assert.AreEqual(1.0f, Time.timeScale);
        Assert.AreNotEqual(Vector2.zero, _ballRigidbody.linearVelocity);
        Assert.AreEqual(
            BallMovement.MotionState.Moving,
            _ballMovement.CurrentMotionState
        );
    }

    /// <summary>
    /// Verifies that ChronoShot does not incorrectly change the ball's movement or ground state.
    /// </summary>
    [Test]
    public void ChronoShotDoesNotChangeBallState()
    {
        _ballRigidbody.linearVelocity = new Vector2(5f, 2f);
        _ballMovement.CurrentMotionState = BallMovement.MotionState.Moving;
        _ballMovement.CurrentGroundState = BallMovement.GroundState.Airborne;

        _gameMaster.SetTimeRateRPC(0.2f);

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
    /// Shuts down the test network and destroys the temporary GameObjects after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1.0f;

        if (_networkManager != null && _networkManager.IsListening)
        {
            _networkManager.Shutdown();
        }

        if (_ball != null)
        {
            GameObject.DestroyImmediate(_ball);
        }

        if (_gameMasterObject != null)
        {
            GameObject.DestroyImmediate(_gameMasterObject);
        }

        if (_networkManagerObject != null)
        {
            GameObject.DestroyImmediate(_networkManagerObject);
        }
    }
}
