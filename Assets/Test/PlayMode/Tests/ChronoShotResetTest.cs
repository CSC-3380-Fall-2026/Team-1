using System.Reflection;
using NUnit.Framework;
using UnityEngine; 
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine.TestTools;

/// <summary>
/// Contains unit test for the Chronoshot reset system
/// </summary>
public class ChronoShotResetTest
{
    private GameObject _gameMasterObject;
    private GameMaster _gameMaster;
    // References used to manage the network host during PlayMode tests
    GameObject _networkManagerObject;
    NetworkManager _networkManager;


    /// <summary>
    /// This is a temporary GameMaster and it restores normal time before each test
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _networkManagerObject = new GameObject("Test NetworkManager");
        _networkManager = _networkManagerObject.AddComponent<NetworkManager>();

        UnityTransport transport =
            _networkManagerObject.AddComponent<UnityTransport>();

        _networkManager.NetworkConfig = new NetworkConfig();
        _networkManager.NetworkConfig.NetworkTransport = transport;

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

        Time.timeScale = 1.0f;
    }


    /// <summary>
    /// Remvoes the temp GameMaster after each test
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        if (_networkManager != null && _networkManager.IsListening)
        {
            _networkManager.Shutdown();
        }

        if (_gameMasterObject != null)
        {
            UnityEngine.Object.DestroyImmediate(_gameMasterObject);
        }

        if (_networkManagerObject != null)
        {
            UnityEngine.Object.DestroyImmediate(_networkManagerObject);
        }

        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
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