using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Contains unit test for the game's time dilation and Chronoshot behavior.
/// </summary>
public class TimeDilationTest
{
    private GameObject _gameMasterObject;
    private GameMaster _gameMaster;

    /// <summary>
    /// Creates a temporary GameMaster and restores normal time before each test.
    /// </summary>

    [SetUp]
    public void SetUp()
    {
        _gameMasterObject = new GameObject("Test GameMaster");
        _gameMaster = _gameMasterObject.AddComponent<GameMaster>();

        Time.timeScale = 1.0f;
    }

    /// <summary>
    /// Removes the temporary GameMaster after each test.
    /// </summary>

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(_gameMasterObject);
    }

    /// <summary>
    /// Verifies that the game begins at normal time speed.
    /// </summary>

    [Test]
    public void GameStartAtNormalTimeSpeed()
    {
        Assert.AreEqual(1.0f, Time.timeScale);
    }

    /// <summary>
    /// Verifies that setting a slower time rate activates slow motion.
    /// </summary>

    [Test]
    public void SetTimeRateSlowsGame()
    {
        _gameMaster.SetTimeRateRPC(0.5f);
        Assert.AreEqual(0.5f, Time.timeScale);
    }

    /// <summary>
    /// Verifies that the normal time rate is restored after slow motion ends.
    /// </summary>

    [Test]
    public void SetTimeRateRestoreNormalSpeed()
    {
        _gameMaster.SetTimeRateRPC(0.5f);
        _gameMaster.SetTimeRateRPC();

        Assert.AreEqual(1.0f, Time.timeScale);
    }

    /// <summary>
    /// Verifies that Chronoshot changes from active slow motion back to its inactive
    /// normal time state.
    /// </summary>

    [Test]
    public void ChronoShotChangesActiveAndInactiveState()
    {
        _gameMaster.SetTimeRateRPC(0.5f);
        Assert.AreNotEqual(1.0f, Time.timeScale);
        _gameMaster.ResetChronoShot();
        Assert.AreEqual(1.0f, Time.timeScale);
    }
}