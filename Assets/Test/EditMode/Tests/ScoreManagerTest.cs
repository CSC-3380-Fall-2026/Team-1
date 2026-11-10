using NUnit.Framework;
using UnityEngine;


public class ScoreManagerTest // test the stroke counting system in scoremanager 
{
    private GameObject scoreManagerObject;
    private ScoreManager scoreManager;

    [SetUp]
    public void SetUp() // creates a fresh scoremanager before each of the test
    {
        scoreManagerObject = new GameObject("TestScoreManager");
        scoreManager = scoreManagerObject.AddComponent<ScoreManager>();

        scoreManager.ResetScore();
    }

    [TearDown]
    public void TearDown() // removes the test object after each of the test
    {
        Object.DestroyImmediate(scoreManagerObject);
    }

    [Test]
    public void NewGame_StartsWithZeroStrokes() // will verify that a new game will start with zero strokes 
    {
        Assert.AreEqual(0, scoreManager.LaunchCount);
    }

    [Test]
    public void OneShot_IncreasesStrokeCountToOne()
    {
        scoreManager.RegisterLaunch();

        Assert.AreEqual(1, scoreManager.LaunchCount);
    }

    [Test]
    public void TwoShots_IncreaseStrokeCountToTwo() // verifies that multiple shots are counted correctly 
    {
        scoreManager.RegisterLaunch();
        scoreManager.RegisterLaunch();

        Assert.AreEqual(2, scoreManager.LaunchCount);
    }
}
