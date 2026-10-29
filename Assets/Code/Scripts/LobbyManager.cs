using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode.Transports.SinglePlayer;
using Unity.Multiplayer.PlayMode;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance {private set; get;}

    public int maxPlayers = 4;

    UnityTransport _unityTransport;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    void Start()
    {
        _unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (CurrentPlayer.IsMainEditor)
            BeginHosting(false, 12345);
        else
            ConnectToHost("127.0.0.1", "12345");
    }

    void BeginHosting(bool localMultiplayer, ushort hostPort)
    {
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = _unityTransport;

        _unityTransport.SetConnectionData("127.0.0.1", hostPort, localMultiplayer ? null : "0.0.0.0");

        NetworkManager.Singleton.NetworkConfig.NetworkTopology = NetworkTopologyTypes.ClientServer;

        NetworkManager.Singleton.ConnectionApprovalCallback = HandleConnectionApproval;
        NetworkManager.Singleton.OnServerStarted += ServerStartedHandler;

        NetworkManager.Singleton.StartHost();
    }

    void ServerStartedHandler()
    {
        if (NetworkManager.Singleton.IsHost)
            GameMaster.Instance.AddPlayer("Host", NetworkManager.Singleton.LocalClientId, System.Guid.NewGuid().ToString());
        else
            Debug.LogWarning("Non-host received server started event.");
    }

    void HandleConnectionApproval(NetworkManager.ConnectionApprovalRequest approvalRequest, NetworkManager.ConnectionApprovalResponse approvalResponse)
    {
        if (NetworkManager.Singleton.ConnectedClients.Count < maxPlayers)
        {
            approvalResponse.Reason = "The lobby is full already.";
            approvalResponse.Approved = false;
        }

        approvalResponse.CreatePlayerObject = false;

        string playerInformationText = System.Text.Encoding.ASCII.GetString(approvalRequest.Payload);
        string[] playerInformationArray = playerInformationText.Split(",");
        string playerName, playerID;

        if (playerInformationArray.Length != 2)
        {
            approvalResponse.Reason = "Missing player information. Need name and identifier (GUID).";
            approvalResponse.Approved = false;
        } else
        {
            playerName = playerInformationArray[0];
            playerID = playerInformationArray[1];

            GameMaster.Instance.AddPlayer(playerName, approvalRequest.ClientNetworkId, playerID);

            approvalResponse.Approved = true;
        }
    }

    void ConnectToHost(string hostIPAddress, string hostPort)
    {
        string playerInformation = "ClientName," + System.Guid.NewGuid().ToString();
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(playerInformation);
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = _unityTransport;

        _unityTransport.SetConnectionData(hostIPAddress, ushort.Parse(hostPort));
        NetworkManager.Singleton.StartClient();
    }
}
