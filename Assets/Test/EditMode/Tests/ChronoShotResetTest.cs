
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Contains unit tests for the ChronoShot reset system.
/// </summary>
public class ChronoShotResetTest
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
        Time.timeScale = 1.0f;
    }

    /// <summary>
    /// Verifies that ChronoShot can be reset repeatedly without errors.
    /// </summary>
    [Test]
    public void ChronoShotCanResetRepeatedly()
    {
        _gameMaster.SetTimeRate(0.2f);

        Assert.DoesNotThrow(() =>
        {
            _gameMaster.ResetChronoShot();
            _gameMaster.ResetChronoShot();
            _gameMaster.ResetChronoShot();
        });

        Assert.AreEqual(1.0f, Time.timeScale);
    }

    /// <summary>
    /// Verifies that ChronoShot can be activated again after being reset.
    /// </summary>
    [Test]
    public void ChronoShotCanActivateAgainAfterReset()
    {
        _gameMaster.SetTimeRate(0.2f);
        _gameMaster.ResetChronoShot();

        Assert.AreEqual(1.0f, Time.timeScale);

        _gameMaster.SetTimeRate(0.2f);

        Assert.AreEqual(0.2f, Time.timeScale);
    }
}
