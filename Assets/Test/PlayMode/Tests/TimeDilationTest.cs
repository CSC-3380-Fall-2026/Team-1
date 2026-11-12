using NUnit.Framework;
using UnityEngine;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine.TestTools;

/// <summary>
/// Contains unit test for the game's time dilation and Chronoshot behavior.
/// </summary>
public class TimeDilationTest
{
    private GameObject _gameMasterObject;
    private GameMaster _gameMaster;
    // References used to manage the network host during PlayMode tests
    GameObject _networkManagerObject;
    NetworkManager _networkManager;

    /// <summary>
    /// Creates a temporary GameMaster and restores normal time before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // Creates a NetworkManager for the PlayMode tests
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

        Time.timeScale = 1.0f;
    }

    /// <summary>
    /// Removes the temporary GameMaster after each test.
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