using System.Collections.Generic;
using System.Reflection;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Test the multiplayer turn system and verifies that player
/// turns and ball controll are handled correctly
/// </summary>
public class TurnSystemTest
{
    // References to the objects and components thats used for multiplayer turn system test
    GameObject _gameMasterObject;
    GameMaster _gameMaster;
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

        // This initializes the GameMaster used for turn system testing
        _gameMasterObject = new GameObject("GameMaster");
        _gameMaster = _gameMasterObject.AddComponent<GameMaster>();

        // Initializes Player 1
        _player1 = ScriptableObject.CreateInstance<Player>();
        _player1.PlayerName = "Player 1";

        // Initializes Player 2
        _player2 = ScriptableObject.CreateInstance<Player>();
        _player2.PlayerName = "Player 2";

        // Initializes both players owned balls assigns their player info
        _ball1 = new GameObject("Ball 1");
        _ball1.AddComponent<Rigidbody2D>();
        _ballMovement1 = _ball1.AddComponent<BallMovement>();
        _ballState1 = _ball1.AddComponent<BallState>();
        _ballState1.BallID = 0;
        _ballState1.PlayerName = _player1.PlayerName;
        _ball2 = new GameObject("Ball 2");
        _ball2.AddComponent<Rigidbody2D>();
        _ballMovement2 = _ball2.AddComponent<BallMovement>();
        _ballState2 = _ball2.AddComponent<BallState>();
        _ballState2.BallID = 1;
        _ballState2.PlayerName = _player2.PlayerName;

        // Gets access to the GameMaster's private player and ball lists for testing
        FieldInfo playerField = typeof(GameMaster).GetField(
        "_players", BindingFlags.NonPublic | BindingFlags.Instance
        );

        FieldInfo ballsField = typeof(GameMaster).GetField(
            "_balls", BindingFlags.NonPublic | BindingFlags.Instance
        );

        // Populates the GameMaster witht eh two test players and their corresponding balls
        playerField.SetValue(
            _gameMaster,
            new List<Player> { _player1, _player2 }
        );

        // Sets Player 1 as the starting player by enabling only their ball movement
        ballsField.SetValue(
            _gameMaster,
            new List<GameObject> { _ball1, _ball2 }
        );

        _ballMovement1.enabled = true;
        _ballMovement2.enabled = false;
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
        _gameMaster.NextTurn();
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
        _gameMaster.NextTurn();
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
        _gameMaster.NextTurn();
        Assert.IsFalse(_ballMovement1.enabled);
        Assert.IsTrue(_ballMovement2.enabled);
        _gameMaster.NextTurn();
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
        _gameMaster.NextTurn();
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
        _gameMaster.NextTurn();
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

        LogAssert.Expect(
            LogType.Warning,
            "No balls are active.\nIf not all have reached goal, something is wrong."
        );

        _gameMaster.NextTurn();
    }
    
}