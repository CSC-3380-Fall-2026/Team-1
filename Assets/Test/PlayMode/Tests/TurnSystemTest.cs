using System.Collections.Generic;
using System.Reflection;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using Unity.Collections;

/// <summary>
/// Test the multiplayer turn system and verifies that player
/// turns and ball controll are handled correctly
/// </summary>
public class TurnSystemTest
{
    // References to the objects and components thats used for multiplayer turn system test
    GameObject _gameMasterObject;
    GameMaster _gameMaster; 

    // References used to manage the network host during PlayMode tests
    GameObject _networkManagerObject;
    NetworkManager _networkManager;

    GameObject _ball1;
    GameObject _ball2;
    BallMovement _ballMovement1;
    BallMovement _ballMovement2;
    BallState _ballState1;
    BallState _ballState2;
    Player _player1;
    Player _player2;

    /// <summary>
    /// This creates the GameMaster, players, and balls used for each turn 
    /// system testing
    /// </summary>
    [SetUp]
    public void SetUp()
    {

        // Creates a NetworkManager for multiplayer turn system tests
        _networkManagerObject = new GameObject("Test NetworkManager");
        _networkManager = _networkManagerObject.AddComponent<NetworkManager>();

        UnityTransport transport =
            _networkManagerObject.AddComponent<UnityTransport>();

        _networkManager.NetworkConfig = new NetworkConfig();
        _networkManager.NetworkConfig.NetworkTransport = transport;

        // Starts a local network host
        Assert.IsTrue(
            _networkManager.StartHost(),
            "Failed to start the test network host."
        );

        // Loads the existing GameMaster prefab
        GameObject gameMasterPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Level/Prefabs/GameMaster.prefab"
            );

        Assert.IsNotNull(gameMasterPrefab, "GameMaster prefab could not be found.");

        // Creates and spawns GameMaster on the network
        _gameMasterObject = UnityEngine.Object.Instantiate(gameMasterPrefab);
        _gameMaster = _gameMasterObject.GetComponent<GameMaster>();

        Assert.IsNotNull(_gameMaster, "GameMaster component is missing.");

        NetworkObject gameMasterNetworkObject =
            _gameMasterObject.GetComponent<NetworkObject>();

        Assert.IsNotNull(gameMasterNetworkObject, "NetworkObject component is missing.");

        gameMasterNetworkObject.Spawn();

        // Initializes Player 1
        _player1 = new Player();
        _player1.PlayerName = "Player 1";

        // Initializes Player 2
        _player2 = new Player();
        _player2.PlayerName = "Player 2";

        // Loads the existing networked ball prefab
        GameObject ballPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Level/Prefabs/Ball.prefab"
        );

        Assert.IsNotNull(ballPrefab, "Ball prefab could not be found.");

        // Creates Player 1's ball from the prefab
        _ball1 = UnityEngine.Object.Instantiate(ballPrefab);
        _ball1.name = "Ball 1";;

        // Gets Player 1's existing prefab components
        _ballMovement1 = _ball1.GetComponent<BallMovement>();
        _ballState1 = _ball1.GetComponent<BallState>();

        Assert.IsNotNull(_ballMovement1, "Player 1 BallMovement is missing.");
        Assert.IsNotNull(_ballState1, "Player 1 BallState is missing.");

        // Creates Player 2's ball from the existing networked prefab
        _ball2 = UnityEngine.Object.Instantiate(ballPrefab);
        _ball2.name = "Ball 2";

        // Gets Player 2's existing prefab components
        _ballMovement2 = _ball2.GetComponent<BallMovement>();
        _ballState2 = _ball2.GetComponent<BallState>();

        Assert.IsNotNull(_ballMovement2, "Player 2 BallMovement is missing.");
        Assert.IsNotNull(_ballState2, "Player 2 BallState is missing.");

        // Spawns both test balls on the network
        _ball1.GetComponent<NetworkObject>().Spawn();
        _ball2.GetComponent<NetworkObject>().Spawn();

        // Assigns player information after both networked balls are spawned
        _ballState1.BallID.Value = 0;
        _ballState1.PlayerName.Value = _player1.PlayerName;

        _ballState2.BallID.Value = 1;
        _ballState2.PlayerName.Value = _player2.PlayerName;

        // Gets access to the GameMaster's private ball list for testing
        FieldInfo ballsField = typeof(GameMaster).GetField(
            "_balls", BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(ballsField, "GameMaster ball list could not be found.");

        ballsField.SetValue(
            _gameMaster,
            new List<GameObject> { _ball1, _ball2 }
        );

        // Registers both players with the networked turn system
        _gameMaster.Players.Add(_player1);
        _gameMaster.Players.Add(_player2);

        // Sets Player 1 as the current player
        _gameMaster.CurrentPlayerTurn.Value = 0;

        _ballMovement1.enabled = true;
        _ballMovement2.enabled = false;
    }

    /// <summary>
    /// Cleans up the network host and temporary objects after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        // Despawns the test balls while the network host is active
        if (_networkManager != null && _networkManager.IsListening)
        {
            if (_ball1 != null)
            {
                NetworkObject ball1Network = _ball1.GetComponent<NetworkObject>();

                if (ball1Network != null && ball1Network.IsSpawned)
                    ball1Network.Despawn();
            }

            if (_ball2 != null)
            {
                NetworkObject ball2Network = _ball2.GetComponent<NetworkObject>();

                if (ball2Network != null && ball2Network.IsSpawned)
                    ball2Network.Despawn();
            }

            // Shuts down the network host
            _networkManager.Shutdown();
        }

        // Destroys any remaining test objects
        if (_ball1 != null)
            UnityEngine.Object.DestroyImmediate(_ball1);

        if (_ball2 != null)
            UnityEngine.Object.DestroyImmediate(_ball2);

        if (_gameMasterObject != null)
            UnityEngine.Object.DestroyImmediate(_gameMasterObject);

        if (_networkManagerObject != null)
            UnityEngine.Object.DestroyImmediate(_networkManagerObject);

        // Restores normal time settings
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
    }

    /// <summary>
    /// Tests that the first player's ball is selected at the beginning
    /// of the turn system
    /// </summary>
    [Test]
    public void FirstPlayerSelectAtBeginningOfTurn()
    {
        Assert.IsTrue(_ballMovement1.enabled);
        Assert.IsFalse(_ballMovement2.enabled);
    }

    /// <summary>
    /// Tests that only the current players ball has its movement component enabled
    /// </summary>
    [Test]
    public void OnlyCurrentPlayerBallCanTakeAction()
    {
        Assert.IsTrue(_ballMovement1.enabled);
        Assert.IsFalse(_ballMovement2.enabled);
        _gameMaster.NextTurnRPC();
        Assert.IsFalse(_ballMovement1.enabled);
        Assert.IsTrue(_ballMovement2.enabled);
    }

    /// <summary>
    /// Tests that the next player's turn begins when the current player's turn ends
    /// </summary>
    [Test]
    public void NextPlayerTurnBeginsAfterCurrentTurnEnds()
    {
        _ballMovement1.CurrentMotionState = BallMovement.MotionState.Stationary;
        _ballMovement1.CurrentGroundState = BallMovement.GroundState.Grounded;
        _gameMaster.NextTurnRPC();
        Assert.IsFalse(_ballMovement1.enabled);
        Assert.IsTrue(_ballMovement2.enabled);
    }

    /// <summary>
    /// Tests that thge turn cycle back to the first player after the final
    /// players turn
    /// </summary>
    [Test]
    public void TurnCyclebackToFirstPlayer()
    {
        _gameMaster.NextTurnRPC();
        Assert.IsFalse(_ballMovement1.enabled);
        Assert.IsTrue(_ballMovement2.enabled);
        _gameMaster.NextTurnRPC();
        Assert.IsTrue(_ballMovement1.enabled);
        Assert.IsFalse(_ballMovement2.enabled);
    }

    /// <summary>
    /// Tests that a player whose ball has already reached the goal is skipped
    /// </summary>
    [Test]
    public void PlayerAtGoalIsSkipped()
    {
        _ball2.SetActive(false);
        _gameMaster.NextTurnRPC();
        Assert.IsTrue(_ballMovement1.enabled);
        Assert.IsFalse(_ball2.activeSelf);
    }

    /// <summary>
    /// Test that turn progession continues correctly when an inactive player is skipped
    /// </summary>
    [Test]
    public void TurnProgressionContinuesWhenPlayerIsSkipped()
    {
        _ball2.SetActive(false);
        _gameMaster.NextTurnRPC();
        Assert.IsTrue(_ball1.activeSelf);
        Assert.IsTrue(_ballMovement1.enabled);
        Assert.IsFalse(_ball2.activeSelf);
    }

    /// <summary>
    /// Tests that the turn system recognizes when all players have reached the goal
    /// </summary>
    [Test]
    public void SystemRecognizesWhenAllPlayersReachedGoal()
    {
        _ball1.SetActive(false);
        _ball2.SetActive(false);

        // Removes the assigned course for this test only.
        // This prevents LoadNextHole() from loading a gameplay scene.
        FieldInfo courseField = typeof(GameMaster).GetField(
            "_currentCourse",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(courseField, "GameMaster course field could not be found.");
        courseField.SetValue(_gameMaster, null);

        

        
        /*
        Verifies that the game attempts to advance after all balls become inactive.
        The test GameMaster has no course assigned.
        This will trigger a warning log message, which is expected behavior.
        */
        
        LogAssert.Expect(
            LogType.Warning,
            "No course is assigned. Unable to load the next hole."
        );

        

        _gameMaster.NextTurnRPC();

        // Confirms that both balls remain inactive after the turn change
        Assert.IsFalse(_ball1.activeSelf, "Player 1's ball should be inactive.");
        Assert.IsFalse(_ball2.activeSelf, "Player 2's ball should be inactive.");
    }
    
}