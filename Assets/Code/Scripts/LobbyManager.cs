using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

/// <summary>
/// Contains properties and behavior related to the handling of player connections.
/// </summary>
public class LobbyManager : MonoBehaviour
{
    // A singleton. Only one LobbyManager will exist at a time and it can be referenced like
    // a static variable.
    public static LobbyManager Instance {private set; get;}

    // The maximum number of players allowed in the game.
    [Tooltip("The maximum number of players allowed in the game.")]
    public int maxPlayers = 4;

    // A reference to the UnityTransport component on the LobbyManager GameObject.
    UnityTransport _unityTransport;

    /// <summary>
    /// Called when the object is first instantiated.
    /// Ensures that only one instance exists at a time and stores reference to UnityTransport component.
    /// </summary>
    void Awake()
    {
        // Singleton handling.
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        // Gets a reference to the UnityTransport component on the GameObject for hosting/joining a game.
        _unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    /// <summary>
    /// Starts hosting a lobby for local (on device only) or LAN (on device/on network) multiplayer.
    /// </summary>
    /// <param name="dontHostOnLAN">Determines whether or not the host will listen on LocalHost or any interface.</param>
    /// <param name="hostPort">The port that will be used to handle connections.</param>
    void BeginHosting(bool dontHostOnLAN, ushort hostPort)
    {
        // Tells the NetworkManager to use UnityTransport as the transport layer.
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = _unityTransport;

        // Sets the connection information. Enables listening on all interfaces if dontHostOnLAN is false.
        _unityTransport.SetConnectionData("127.0.0.1", hostPort, dontHostOnLAN ? null : "0.0.0.0");

        // Tells the NetworkManager to use a ClientServer network topology. Includes simultaneous Server/Client (host).
        NetworkManager.Singleton.NetworkConfig.NetworkTopology = NetworkTopologyTypes.ClientServer;

        // Subscribes relevant functions to events involving networking states.
        NetworkManager.Singleton.ConnectionApprovalCallback = HandleConnectionApproval;
        NetworkManager.Singleton.OnServerStarted += ServerStartedHandler;

        // Starts hosting the lobby based on the previously set information (device acts as both the server and a client).
        NetworkManager.Singleton.StartHost();
    }

    /// <summary>
    /// A callback function called when the server is started.
    /// Adds a player for the host if the server was started in host mode rather than a dedicated server.
    /// </summary>
    void ServerStartedHandler()
    {
        // Adds a player for the host if the handler is executing on the host, throws a warning if executing on a client.
        if (NetworkManager.Singleton.IsHost)
            GameMaster.Instance.AddPlayer("Host", NetworkManager.Singleton.LocalClientId, System.Guid.NewGuid().ToString());
        else if (NetworkManager.Singleton.IsClient)
            Debug.LogWarning("Non-host client received server started event.");
    }

    /// <summary>
    /// Handles connection approval requests made by a client trying to connect to the host.
    /// </summary>
    /// <param name="approvalRequest">Contains the ID of the client and a payload of custom data (currently name and GUID).</param>
    /// <param name="approvalResponse">The response that will be sent back to the client.</param>
    void HandleConnectionApproval(NetworkManager.ConnectionApprovalRequest approvalRequest, NetworkManager.ConnectionApprovalResponse approvalResponse)
    {
        // Denies the request if the lobby is full.
        if (NetworkManager.Singleton.ConnectedClients.Count < maxPlayers)
        {
            approvalResponse.Reason = "The lobby is full already.";
            approvalResponse.Approved = false;
            return;
        }

        // Tells the client not to create an object representing the player.
        // No need to create a player object, as the GameManager handles that functionality.
        approvalResponse.CreatePlayerObject = false;

        // Splits the request's payload, consisting of a comma-separated ASCII string in byte format, into separate strings.
        string playerInformationText = System.Text.Encoding.ASCII.GetString(approvalRequest.Payload);
        string[] playerInformationArray = playerInformationText.Split(",");

        // Containers for the player name and GUID. Filled if the payload is formatted correctly.
        string playerName, playerGUID;

        // Rejects the connection request if the payload was formatted incorrectly.
        if (playerInformationArray.Length != 2)
        {
            approvalResponse.Reason = "Missing player information. Need name and identifier (GUID) in a comma-separated list (no spaces).";
            approvalResponse.Approved = false;
            return;
        } else
        {
            // Fills the name and ID with the appropriate values from the comma-separated list.
            playerName = playerInformationArray[0];
            playerGUID = playerInformationArray[1];

            // Adds a player for the new connection using the previously gathered information.
            GameMaster.Instance.AddPlayer(playerName, approvalRequest.ClientNetworkId, playerGUID);

            // Changes the response to inform the client that the request was approved.
            approvalResponse.Approved = true;

            Debug.Log($"Client {approvalRequest.ClientNetworkId} approved.");
        }
    }

    /// <summary>
    /// Connects to a host's lobby as a client.
    /// </summary>
    /// <param name="hostIPAddress">The IP address of the host (loopback address works for same-device multiplayer).</param>
    /// <param name="hostPort">The port listening for connections on the hosting device.</param>
    void ConnectToHost(string hostIPAddress, string hostPort)
    {
        // Constructs the payload consisting of the player's name and GUID.
        string playerInformation = "ClientName," + System.Guid.NewGuid().ToString();

        // Sets the payload to the previously constructed string.
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(playerInformation);

        // Tells the network manager to use UnityTransport as its transport layer handler.
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = _unityTransport;

        // Informs the network manager of the IP and port of the device hosting the lobby that this client will attempt to connect to.
        _unityTransport.SetConnectionData(hostIPAddress, ushort.Parse(hostPort));

        // Connects to the host using the previously provided information.
        NetworkManager.Singleton.StartClient();
    }
}