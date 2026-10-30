using System.Reflection;
using NUnit.Framework;
using UnityEngine; 

/// <summary>
/// Contains unit test for the Chronoshot reset system
/// </summary>
public class ChronoShotResetTest
{
    private GameObject _gameMasterObject;
    private GameMaster _gameMaster;


    /// <summary>
    /// This is a temporary GameMaster and it restores normal time before each test
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _gameMasterObject = new GameObject("Test GameMaster");
        _gameMaster = _gameMasterObject.AddComponent<GameMaster>();

        Time.timeScale = 1.0f;
    }


    /// <summary>
    /// Remvoes the temp GameMaster after each test
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(_gameMasterObject);
    }

    /// <summary>
    /// Verifies that ChronoShot becomes inactive after it resets
    /// </summary>
    [Test]
    public void ChronoShotBecomesInactiveAfterReset()
    {
        _gameMaster.SetTimeRateRPC(0.2f);
        _gameMaster.ResetChronoShot();
        Assert.AreEqual(1.0f, Time.timeScale);
    }

    /// <summary>
    /// Verfiy that ChronoShot reset restores the normal game time speed
    ////summary>
    [Test]
    public void ChronoShotResetRestoreNormalTimeSpeed()
    {
        _gameMaster.SetTimeRateRPC(0.5f);
        Assert.AreEqual(0.5f, Time.timeScale);
        _gameMaster.ResetChronoShot();
        Assert.AreEqual(1.0f, Time.timeScale);
    }

    /// <summary>
    /// Verifies that ChronoShot can be reset repeatedly without causeing any errors
    /// </summary>
    [Test]
    public void ChronoShotCanResetRepeatedly()
    {
        _gameMaster.SetTimeRateRPC(0.2f);
        {
            _gameMaster.SetTimeRateRPC(0.2f);
            Assert.DoesNotThrow(() =>
            {
                _gameMaster.ResetChronoShot();
                _gameMaster.ResetChronoShot();
                _gameMaster.ResetChronoShot();
            });

            Assert.AreEqual(1.0f, Time.timeScale);
        }
    }

    /// <summary>
    /// Verifies that Chronoshot can be activated again after being reset
    /// </summary>
    [Test] 
    public void ChronoShotCanActivateAgainAfterReset()
    {
        _gameMaster.SetTimeRateRPC(0.2f);
        _gameMaster.ResetChronoShot();
        Assert.AreEqual(1.0f, Time.timeScale);
        _gameMaster.SetTimeRateRPC(0.2f);
        Assert.AreEqual(0.2f, Time.timeScale);
    }
}