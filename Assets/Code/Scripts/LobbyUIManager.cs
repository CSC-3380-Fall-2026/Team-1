using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField]
    VisualTreeAsset _lobbyUIAsset;

    [SerializeField]
    VisualTreeAsset _clientInfoAsset;

    [SerializeField]
    VisualTreeAsset _playerInfoAsset;

    UIDocument _UIDocumentComponent;

    void Awake()
    {
        _UIDocumentComponent = GetComponent<UIDocument>();

        _UIDocumentComponent.visualTreeAsset = _lobbyUIAsset;

        if (!NetworkManager.Singleton.IsHost)
        {
            VisualElement startExitButtonsContainer = _UIDocumentComponent.rootVisualElement.Query("StartExitContainer");
            startExitButtonsContainer.visible = false;

            VisualElement disconnectButtonContainer = _UIDocumentComponent.rootVisualElement.Query("DisconnectContainer");
            disconnectButtonContainer.visible = true;

            Button disconnectButton = disconnectButtonContainer.Query<Button>("DisconnectButton");
            disconnectButton.RegisterCallback<ClickEvent>(HandleDisconnectButtonClicked);
        }
    }

    void Start()
    {
        NetworkManager.Singleton.OnConnectionEvent += (networkManager, connectionEventData) => {ConstructClientsListUI();};
        GameMaster.Instance.Players.OnListChanged += (networkListEventInfo) => {ConstructPlayersListUI();};

        ConstructClientsListUI();
        ConstructPlayersListUI();
    }

    void ConstructClientsListUI()
    {
        List<VisualElement> clientsInfoCards = new List<VisualElement>();

        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            VisualElement clientInfoRootVisualElement = _clientInfoAsset.CloneTree();

            Label clientInfoIdLabel = clientInfoRootVisualElement.Query<Label>("ClientIDLabel");

            clientInfoIdLabel.text = $"Client ID: {client.ClientId}";
            clientInfoIdLabel.text += $"{(client.ClientId == 0 ? " (Host)" : "")}";
            clientInfoIdLabel.text += $"{(client.ClientId == NetworkManager.Singleton.LocalClientId ? " (You)" : "")}";

            Button clientKickButton = clientInfoRootVisualElement.Query<Button>("KickButton");

            clientKickButton.RegisterCallback<ClickEvent>((clickEventDetails) => {
                HandleKickClientButtonClicked(clickEventDetails, client.ClientId);
            });

            clientKickButton.visible = client.ClientId == 0;

            if (!NetworkManager.Singleton.IsHost)
                clientKickButton.SetEnabled(false);

            clientsInfoCards.Add(clientInfoRootVisualElement);
        }

        ScrollView clientsListScrollView = _UIDocumentComponent.rootVisualElement.Query<ScrollView>("ClientsListScrollView");
        clientsListScrollView.dataSource = clientsInfoCards;
    }

    void ConstructPlayersListUI()
    {
        List<VisualElement> playerInfoCards = new List<VisualElement>();

        for (int i = 0; i < GameMaster.Instance.Players.Count; i++)
        {
            VisualElement playerInfoRootVisualElement = _playerInfoAsset.CloneTree();

            Label playerInfoNameLabel = playerInfoRootVisualElement.Query<Label>("PlayerNameLabel");
            playerInfoNameLabel.text = GameMaster.Instance.Players[i].PlayerName.ToString();

            Label localRemoteLabel = playerInfoRootVisualElement.Query<Label>("LocalOrRemoteLabel");
            localRemoteLabel.text = (GameMaster.Instance.Players[i].PlayerClientID == NetworkManager.Singleton.LocalClientId) ? "Local" : "Remote";

            Button removePlayerButton = playerInfoRootVisualElement.Query<Button>("RemovePlayerButton");

            removePlayerButton.RegisterCallback<ClickEvent>((clickEventDetails) => {
                HandleRemovePlayerButtonClicked(clickEventDetails, i);
            });

            removePlayerButton.SetEnabled(NetworkManager.Singleton.IsHost ||
                GameMaster.Instance.Players[i].PlayerClientID == NetworkManager.Singleton.LocalClientId);

            playerInfoCards.Add(playerInfoRootVisualElement);
        }

        ScrollView playersListScrollView = _UIDocumentComponent.rootVisualElement.Query<ScrollView>("PlayersListScrollView");
        playersListScrollView.dataSource = playerInfoCards;
    }

    void HandleKickClientButtonClicked(ClickEvent clickEventDetails, ulong clientID)
    {
        NetworkManager.Singleton.DisconnectClient(clientID, "Kicked by host.");
    }

    void HandleRemovePlayerButtonClicked(ClickEvent clickEventDetails, int playerPositionInPlayersList)
    {
        GameMaster.Instance.RemovePlayerRPC(playerPositionInPlayersList);
    }

    void HandleDisconnectButtonClicked(ClickEvent clickEventDetails)
    {
        NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId, "Clicked the disconnect button in the lobby.");
    }
}
