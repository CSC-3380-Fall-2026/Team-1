using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class HoleCompletionTest
{
    /
    const int FinalHoleID = 2; 

    
    static readonly float[] HoleBaseScores = { 1000f, 2000f, 3000f }; // each hole has a different base score so that tests can tell which hole were used

    const BindingFlags NonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;

    GameObject _gameMasterObject;
    GameMaster _gameMaster;
    Course _course;
    Hole[] _holes;
    GameObject _cupObject;
    Goal _cup;

    readonly List<Player> _players = new List<Player>();
    readonly List<GameObject> _balls = new List<GameObject>();

    [SetUp]
    public void SetUp()
    {
        _players.Clear();
        _balls.Clear();

        
        _gameMasterObject = new GameObject("Test GameMaster");
        _gameMaster = _gameMasterObject.AddComponent<GameMaster>();
        typeof(GameMaster).GetMethod("Awake", NonPublicInstance).Invoke(_gameMaster, null);

        
        _holes = new Hole[3];
        for (int i = 0; i < _holes.Length; i++)
        {
            _holes[i] = ScriptableObject.CreateInstance<Hole>();
            _holes[i].BaseScore = HoleBaseScores[i];
        }

        _course = ScriptableObject.CreateInstance<Course>();
        _course.Holes = _holes;

        SetPrivateField("_currentCourse", _course);
        SetPrivateField("_currentHoleID", FinalHoleID);

        
        _cupObject = new GameObject("Test Cup"); // the cup needs a collider on the same object
        _cupObject.AddComponent<BoxCollider2D>();
        _cup = _cupObject.AddComponent<Goal>();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject ball in _balls)
            Object.DestroyImmediate(ball);

        foreach (Player player in _players)
            Object.DestroyImmediate(player);

        foreach (Hole hole in _holes)
            Object.DestroyImmediate(hole);

        Object.DestroyImmediate(_course);
        Object.DestroyImmediate(_cupObject);
        Object.DestroyImmediate(_gameMasterObject);

        
        typeof(GameMaster).GetProperty("Instance").GetSetMethod(true).Invoke(null, new object[] { null }); // clears the static reference so it does not leak into other tests
    }

    [Test]
    public void HoleCompletes_WhenBallEntersCup()
    {
        GameObject ball = AddPlayerAndBall("Player 1");
        StartGame();

        EnterCup(ball);

        Assert.IsFalse(ball.activeSelf, "The ball should be removed once it is in the cup.");
        Assert.AreEqual(FinalHoleID + 1, GetCurrentHoleID(), "The hole should be marked as completed.");
    }


    [Test]
    public void HoleDoesNotComplete_WhenBallHasNotEnteredCup()
    {
        GameObject ball = AddPlayerAndBall("Player 1");
        StartGame();

        Assert.IsTrue(ball.activeSelf);
        Assert.AreEqual(FinalHoleID, GetCurrentHoleID());
        Assert.AreEqual(0f, _players[0].Score);
    }

    [Test]
    public void HoleDoesNotComplete_WhileAnotherBallIsStillInPlay()
    {
        GameObject ball1 = AddPlayerAndBall("Player 1");
        GameObject ball2 = AddPlayerAndBall("Player 2");
        StartGame();

        EnterCup(ball1);

        Assert.IsFalse(ball1.activeSelf);
        Assert.IsTrue(ball2.activeSelf, "The second ball is still playing.");
        Assert.AreEqual(FinalHoleID, GetCurrentHoleID(), "The hole should not be complete yet.");
    }

    [Test]
    public void HoleCompletes_WhenLastBallEntersCup()
    {
        GameObject ball1 = AddPlayerAndBall("Player 1");
        GameObject ball2 = AddPlayerAndBall("Player 2");
        StartGame();

        EnterCup(ball1);
        EnterCup(ball2);

        Assert.AreEqual(FinalHoleID + 1, GetCurrentHoleID());
    }

   
    [Test]
    public void HoleCompletion_OnlyTriggersOnce()
    {
        GameObject ball = AddPlayerAndBall("Player 1");
        StartGame();

        EnterCup(ball);
        EnterCup(ball);
        EnterCup(ball);

        Assert.AreEqual(FinalHoleID + 1, GetCurrentHoleID(), "The hole should only advance once.");
        Assert.AreEqual(HoleBaseScores[FinalHoleID], _players[0].Score, 0.5f, "The score should only be awarded once.");
    }

   
    [Test]
    public void CorrectHoleIsCompleted()
    {
        GameObject ball = AddPlayerAndBall("Player 1");
        StartGame();

        EnterCup(ball);

        
        Assert.AreEqual(HoleBaseScores[FinalHoleID], _players[0].Score, 0.5f); // score uses the base score of the hole index 
        
        Assert.AreEqual(FinalHoleID + 1, GetCurrentHoleID()); // moves forward by  one hole
    }

    [Test]
    public void ScoreIsAwardedToCorrectPlayer()
    {
        GameObject ball1 = AddPlayerAndBall("Player 1");
        AddPlayerAndBall("Player 2");
        StartGame();

        EnterCup(ball1);

        Assert.Greater(_players[0].Score, 0f);
        Assert.AreEqual(0f, _players[1].Score);
    }

    GameObject AddPlayerAndBall(string playerName)
    {
        Player player = ScriptableObject.CreateInstance<Player>();
        player.PlayerName = playerName;
        player.Score = 0f;

        GameObject ball = new GameObject("Ball " + playerName);
        ball.AddComponent<Rigidbody2D>();
        ball.AddComponent<CircleCollider2D>();
        ball.AddComponent<BallMovement>();
        ball.AddComponent<BallTimeManager>();
        BallState state = ball.AddComponent<BallState>();
        state.BallID = _balls.Count;
        state.PlayerName = playerName;

        _players.Add(player);
        _balls.Add(ball);
        return ball;
    }

    void StartGame()
    {
        SetPrivateField("_players", new List<Player>(_players));
        SetPrivateField("_balls", new List<GameObject>(_balls));
        SetPrivateField("_currentPlayerTurn", 0);

        for (int i = 0; i < _balls.Count; i++)
            _balls[i].GetComponent<BallMovement>().enabled = (i == 0);
    }

    void EnterCup(GameObject ball)
    {
        MethodInfo trigger = typeof(Goal).GetMethod("OnTriggerEnter2D", NonPublicInstance);
        trigger.Invoke(_cup, new object[] { ball.GetComponent<Collider2D>() });
    }

    void SetPrivateField(string fieldName, object value)
    {
        FieldInfo field = typeof(GameMaster).GetField(fieldName, NonPublicInstance);
        Assert.IsNotNull(field, "GameMaster has no field named " + fieldName);
        field.SetValue(_gameMaster, value);
    }

    int GetCurrentHoleID() // reads the index of the hole GameMaster is currently running on
    {
        FieldInfo field = typeof(GameMaster).GetField("_currentHoleID", NonPublicInstance);
        return (int)field.GetValue(_gameMaster);
    }
}
