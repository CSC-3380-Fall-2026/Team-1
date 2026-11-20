using NUnit.Framework;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Unit Test for the ball reset system
/// </summary>
public class BallControllerTest
{

    // References to objects and components for testing
    GameObject _gameMasterObject;
    GameObject _holeDataObject;
    GameMaster _gameMaster;
    // References used to manage the network host during PlayMode tests
    GameObject _networkManagerObject;
    NetworkManager _networkManager;
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

        // Creates a NetworkManager with Unity Transport for testing RPCs
        _networkManagerObject = new GameObject("Test NetworkManager");
        _networkManager = _networkManagerObject.AddComponent<NetworkManager>();

        UnityTransport transport = _networkManagerObject.AddComponent<UnityTransport>();

        _networkManager.NetworkConfig = new NetworkConfig();
        _networkManager.NetworkConfig.NetworkTransport = transport;

        // Starts a local host so network RPCs can execute
        Assert.IsTrue(_networkManager.StartHost(), "Failed to start the test network host.");

        // Loads the existing GameMaster prefab
        GameObject gameMasterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Level/Prefabs/GameMaster.prefab"
        );

        Assert.IsNotNull(gameMasterPrefab, "GameMaster prefab could not be found.");

        // Creates the GameMaster for the network test
        _gameMasterObject = UnityEngine.Object.Instantiate(gameMasterPrefab);
        _gameMaster = _gameMasterObject.GetComponent<GameMaster>();

        Assert.IsNotNull(_gameMaster, "GameMaster component is missing.");

        // Spawns GameMaster on the network
        NetworkObject gameMasterNetworkObject =
            _gameMasterObject.GetComponent<NetworkObject>();

        Assert.IsNotNull(gameMasterNetworkObject, "NetworkObject component is missing.");

        gameMasterNetworkObject.Spawn();


        // Loads the existing Ball prefab
        GameObject ballPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Level/Prefabs/Ball.prefab"
        );

        Assert.IsNotNull(ballPrefab, "Ball prefab could not be found.");

        // Creates the test ball using the existing prefab
        _ball = UnityEngine.Object.Instantiate(ballPrefab);
        _ball.name = "Test Ball";

        // Gets the components already attached to the prefab
        _ballRigidbody = _ball.GetComponent<Rigidbody2D>();
        _ballCollider = _ball.GetComponent<CircleCollider2D>();
        _ballRigidbody.gravityScale = 0f;

        Assert.IsNotNull(_ballRigidbody, "Rigidbody2D component is missing.");
        Assert.IsNotNull(_ballCollider, "CircleCollider2D component is missing.");

        // Prevents movement input from interfering with the tests
        BallMovement ballMovement = _ball.GetComponent<BallMovement>();

        if (ballMovement != null)
        {
            ballMovement.enabled = false;
        }

        NetworkObject ballNetworkObject = _ball.GetComponent<NetworkObject>();

        Assert.IsNotNull(ballNetworkObject, "NetworkObject component is missing.");

        ballNetworkObject.Spawn();
        
        // Loads the existing Reset Point prefab
        GameObject resetPointPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Level/Prefabs/Reset Point.prefab"
        );

        Assert.IsNotNull(resetPointPrefab, "Reset Point prefab could not be found.");

        _resetPoint = UnityEngine.Object.Instantiate(resetPointPrefab);
        _resetPoint.name = "Test Reset Point";

        _resetPoint.transform.position = new Vector2(5f, 5f);
        _resetPointCollider = _resetPoint.GetComponent<CircleCollider2D>();


        // Creates the hole data used to identify valid ball reset locations
        _holeDataObject = new GameObject("Test HoleData");
        HoleData holeData = _holeDataObject.AddComponent<HoleData>();
        holeData.enabled = false;
        holeData.ResetLocationObjects = new GameObject[] { _resetPoint };
        holeData.SpawnPointIndices = new int[] { 0 };
    }

    /// <summary>
    /// Tests that the ball returns to the correct reset position
    /// </summary>
    [UnityTest]
    public IEnumerator BallReturnToCorrectPosition()
    {
        _ball.transform.position = new Vector2(10f, 10f);
        _gameMaster.ResetBallRPC(0, new int[] { 0 });
        yield return null;
        Assert.AreEqual(_resetPoint.transform.position, _ball.transform.position);
    }

    /// <summary>
    /// Tests that the ball does not continue moving after being reset
    /// </summary>
    [UnityTest]
    public IEnumerator BallStopsMovingAfterReset()
    {
        _ball.transform.position = new Vector2(10f, 10f);
        _ballRigidbody.linearVelocity = new Vector2(8f, 4f);
        _ballRigidbody.angularVelocity = 3f;
        _gameMaster.ResetBallRPC(0, new int[] { 0 });
        yield return null;
        Assert.AreEqual(Vector2.zero, _ballRigidbody.linearVelocity);
        Assert.AreEqual(0f, _ballRigidbody.angularVelocity);
        Assert.AreEqual(_resetPoint.transform.position, _ball.transform.position);
    }

    /// <summary>
    /// Tests that the ball can reset after going out of bound
    /// </summary>
    [UnityTest]
    public IEnumerator BallCanResetAfterOutOfBounds()
    {
        _ball.transform.position = new Vector2(100f, -100f);
        _ballRigidbody.linearVelocity = new Vector2(10f, -10f);
        _ballRigidbody.angularVelocity = 5f;
        _gameMaster.ResetBallRPC(0, new int[] { 0 });
        yield return null;
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
        if (_networkManager != null && _networkManager.IsListening)
        {
            _networkManager.Shutdown();
        }

        if (_ball != null)
            UnityEngine.Object.DestroyImmediate(_ball);

        if (_resetPoint != null)
            UnityEngine.Object.DestroyImmediate(_resetPoint);

        if (_holeDataObject != null)
            UnityEngine.Object.DestroyImmediate(_holeDataObject);

        if (_gameMasterObject != null)
            UnityEngine.Object.DestroyImmediate(_gameMasterObject);

        if (_networkManagerObject != null)
            UnityEngine.Object.DestroyImmediate(_networkManagerObject);
    }
}

