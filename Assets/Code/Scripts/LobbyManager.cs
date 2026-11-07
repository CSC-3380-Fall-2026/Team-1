using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

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
        else {
            Instance = this;

            // Allows the object to persist between scenes.
            DontDestroyOnLoad(gameObject);
        }
    }
    void Start()
    {
        // Gets a reference to the UnityTransport component on the GameObject for hosting/joining a game.
        _unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    /// <summary>
    /// Starts hosting a lobby for local (on device only) or LAN (on device/on network) multiplayer.
    /// </summary>
    /// <param name="dontHostOnLAN">Determines whether or not the host will listen on LocalHost or any interface.</param>
    /// <param name="hostPort">The port that will be used to handle connections.</param>
    public void BeginHosting(bool dontHostOnLAN, ushort hostPort)
    {
        // Tells the NetworkManager to use UnityTransport as the transport layer.
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = _unityTransport;

        // Sets the connection information. Enables listening on all interfaces if dontHostOnLAN is false.
        _unityTransport.SetConnectionData("127.0.0.1", hostPort, dontHostOnLAN ? null : "0.0.0.0");

        // Tells the NetworkManager to use a ClientServer network topology. Includes simultaneous Server/Client (host).
        NetworkManager.Singleton.NetworkConfig.NetworkTopology = NetworkTopologyTypes.ClientServer;

        // Constructs the payload consisting of the player's GUID.
        string playerInformation = LocalGameData.Instance.GUID;

        // Sets the payload to the previously constructed string.
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(playerInformation);

        // Tells the NetworkManager to handle new connections via a callback rather than automatically accepting them.
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;

        // Subscribes relevant functions to events involving networking states.
        NetworkManager.Singleton.ConnectionApprovalCallback += HandleConnectionApproval;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnection;

        // Starts hosting the lobby based on the previously set information (device acts as both the server and a client).
        NetworkManager.Singleton.StartHost();
    }

    /// <summary>
    /// Handles connection approval requests made by a client trying to connect to the host.
    /// </summary>
    /// <param name="approvalRequest">Contains the ID of the client and a payload of custom data (currently name and GUID).</param>
    /// <param name="approvalResponse">The response that will be sent back to the client.</param>
    void HandleConnectionApproval(NetworkManager.ConnectionApprovalRequest approvalRequest, NetworkManager.ConnectionApprovalResponse approvalResponse)
    {
        // Denies the request if the lobby is full.
        if (NetworkManager.Singleton.ConnectedClients.Count >= maxPlayers)
        {
            approvalResponse.Reason = "The lobby is full already.";
            approvalResponse.Approved = false;
            return;
        }

        // Converts the client's connection request payload (currently the GUID only) to a string.
        string playerGUID = System.Text.Encoding.ASCII.GetString(approvalRequest.Payload);

        // Determines whether to deny the connection based on if the round is ongoing and the client is a new or returning player.
        if (GameMaster.Instance != null && GameMaster.Instance.CurrentGameState != GameMaster.GameState.Lobby)
        {
            // If a player in the players list has the same GUID as the connecting client, the client is reconnecting.
            bool foundPlayerWithGUID = false;

            foreach (Player player in GameMaster.Instance.Players)
            {
                if (player.PlayerGUID == playerGUID)
                {
                    foundPlayerWithGUID = true;
                    break;
                }
            }

            // Allows reconnecting clients even if the game is ongoing, denies new clients.
            if (foundPlayerWithGUID)
            {
                Debug.Log($"Client {approvalRequest.ClientNetworkId} reconnected.");
                approvalResponse.Approved = true;

                // Tells the client not to create an object representing the player.
                // No need to create a player object, as the GameManager handles that functionality.
                approvalResponse.CreatePlayerObject = false;

                return;
            } else
            {
                approvalResponse.Reason = "A new player cannot join a game already in progress.";
                approvalResponse.Approved = false;
                return;
            }
        }

        // If the request was not ended prematurely, this is a new player.
        Debug.Log($"New client {approvalRequest.ClientNetworkId} approved.");
        approvalResponse.Approved = true;

        // Tells the client not to create an object representing the player.
        // No need to create a player object, as the GameManager handles that functionality.
        approvalResponse.CreatePlayerObject = false;
    }

    /// <summary>
    /// A handler for the OnClientDisconnected callback.
    /// Unsubscribes from callbacks and changes the scene to a pre-lobby scene if this client disconnected.
    /// </summary>
    /// <param name="clientID">The network ID of the client that disconnected.</param>
    void HandleClientDisconnection(ulong clientID)
    {
        Debug.Log($"Client {clientID} disconnected.");

        // Checks if this local client is the one disconnecting.
        if (clientID == NetworkManager.Singleton.LocalClientId)
        {
            // Closes open connections and resets the NetworkManager, leaving it available to host/connect again.
            NetworkManager.Singleton.Shutdown();

            // Unsubscribes from callbacks.
            NetworkManager.Singleton.ConnectionApprovalCallback -= HandleConnectionApproval;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnection;

            // Changes the scene to a pre-lobby one.
            SceneManager.LoadScene("GameMode");
        }

        // If the disconnection happened mid-lobby, remove all the client's players.
        if (GameMaster.Instance.CurrentGameState == GameMaster.GameState.Lobby)
            GameMaster.Instance.RemoveAllPlayersWithClientID(clientID);
    }

    /// <summary>
    /// Connects to a host's lobby as a client.
    /// </summary>
    /// <param name="hostIPAddress">The IP address of the host (loopback address works for same-device multiplayer).</param>
    /// <param name="hostPort">The port listening for connections on the hosting device.</param>
    public void ConnectToHost(string hostIPAddress, string hostPort)
    {
        // Constructs the payload consisting of the player's GUID.
        string playerInformation = LocalGameData.Instance.GUID;

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