using NUnit.Framework;
using UnityEngine;

public class TimeDilationTest
{
    private GameObject _gameMasterObject;
    private GameMaster _gameMaster;

    [SetUp]
    public void SetUp()
    {
        _gameMasterObject = new GameObject("Test GameMaster");
        _gameMaster = _gameMasterObject.AddComponent<GameMaster>();

        Time.timeScale = 1.0f;
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1.0f;
        GameObject.DestroyImmediate(_gameMasterObject);
    }

    [Test]
    public void GameStartAtNormalTimeSpeed()
    {
        Assert.AreEqual(1.0f, Time.timeScale);
    }

    [Test]
    public void SetTimeRateSlowsGame()
    {
        _gameMaster.SetTimeRate(0.5f);
        Assert.AreEqual(0.5f, Time.timeScale);
    }

    [Test]
    public void SetTimeRateRestoreNormalSpeed()
    {
        _gameMaster.SetTimeRate(0.5f);
        _gameMaster.SetTimeRate();

        Assert.AreEqual(1.0f, Time.timeScale);
    }

    [Test]
    public void ChronoShotChangesActiveAndInactiveState()
    {
        _gameMaster.SetTimeRate(0.5f);
        Assert.AreNotEqual(1.0f, Time.timeScale);
        _gameMaster.ResetChronoShot();
        Assert.AreEqual(1.0f, Time.timeScale);
    }
}