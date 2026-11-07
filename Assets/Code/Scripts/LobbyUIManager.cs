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

        Button addPlayerProfileButton = _UIDocumentComponent.rootVisualElement.Query<Button>("AddPlayerProfileButton");
        addPlayerProfileButton.RegisterCallback<ClickEvent>(HandleAddPlayerProfileButtonClicked);

        Button startGameButton = _UIDocumentComponent.rootVisualElement.Query<Button>("StartGameButton");
        startGameButton.RegisterCallback<ClickEvent>(HandleStartGameButtonClicked);

        Button exitGameButton = _UIDocumentComponent.rootVisualElement.Query<Button>("ExitGameButton");
        exitGameButton.RegisterCallback<ClickEvent>(HandleExitButtonClicked);
    }

    void Start()
    {
        NetworkManager.Singleton.OnConnectionEvent += HandleConnectionEvent;
        GameMaster.Instance.Players.OnListChanged += HandlePlayerListChangedEvent;

        ConstructClientsListUI();
        ConstructPlayersListUI();
        ConstructPlayerProfilesListUI();
    }

    void ConstructClientsListUI()
    {
        ScrollView clientsListScrollView = _UIDocumentComponent.rootVisualElement.Query<ScrollView>("ClientsListScrollView");
        clientsListScrollView.Clear();

        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            VisualElement clientInfoRootVisualElement = _clientInfoAsset.CloneTree();

            Label clientInfoIdLabel = clientInfoRootVisualElement.Query<Label>("ClientIDLabel");

            clientInfoIdLabel.text = $"Client ID: {client.ClientId}";
            clientInfoIdLabel.text += $"{(client.ClientId == 0 ? " (Host)" : "")}";
            clientInfoIdLabel.text += $"{(client.ClientId == NetworkManager.Singleton.LocalClientId ? " (You)" : "")}";

            Button clientKickButton = clientInfoRootVisualElement.Query<Button>("KickButton");
            clientKickButton.userData = client.ClientId;

            clientKickButton.RegisterCallback<ClickEvent>((clickEventDetails) => {
                HandleKickClientButtonClicked(clickEventDetails, (ulong)clientKickButton.userData);
            });

            clientKickButton.visible = client.ClientId != 0;

            if (!NetworkManager.Singleton.IsHost)
                clientKickButton.SetEnabled(false);

            clientsListScrollView.Add(clientInfoRootVisualElement);
        }
    }

    void ConstructPlayersListUI()
    {
        ScrollView playersListScrollView = _UIDocumentComponent.rootVisualElement.Query<ScrollView>("PlayersListScrollView");
        playersListScrollView.Clear();

        for (int i = 0; i < GameMaster.Instance.Players.Count; i++)
        {
            VisualElement playerInfoRootVisualElement = _playerInfoAsset.CloneTree();

            Label playerInfoNameLabel = playerInfoRootVisualElement.Query<Label>("PlayerNameLabel");
            playerInfoNameLabel.text = GameMaster.Instance.Players[i].PlayerName.ToString();

            Label localRemoteLabel = playerInfoRootVisualElement.Query<Label>("LocalOrRemoteLabel");
            localRemoteLabel.text = (GameMaster.Instance.Players[i].PlayerClientID == NetworkManager.Singleton.LocalClientId) ? "Local" : "Remote";

            Button removePlayerButton = playerInfoRootVisualElement.Query<Button>("RemovePlayerButton");
            removePlayerButton.userData = i;

            removePlayerButton.RegisterCallback<ClickEvent>((clickEventDetails) => {
                HandleRemovePlayerButtonClicked(clickEventDetails, (int)removePlayerButton.userData);
            });

            removePlayerButton.SetEnabled(NetworkManager.Singleton.IsHost ||
                GameMaster.Instance.Players[i].PlayerClientID == NetworkManager.Singleton.LocalClientId);

            playersListScrollView.Add(playerInfoRootVisualElement);
        }
    }

    void ConstructPlayerProfilesListUI()
    {
        DropdownField playerProfileDropdownField = _UIDocumentComponent.rootVisualElement.Query<DropdownField>("AddPlayerProfileDropdownField");

        playerProfileDropdownField.choices.Clear();

        if (LocalGameData.Instance.PlayerProfiles.Count == 0)
        {
            Debug.Log("No profiles in LocalGameData");
            return;
        }

        for (int i = 0; i < LocalGameData.Instance.PlayerProfiles.Count; i++)
            playerProfileDropdownField.choices.Add(LocalGameData.Instance.PlayerProfiles[i].PlayerName.ToString());

        playerProfileDropdownField.index = 0;

        Button addPlayerProfileButton = _UIDocumentComponent.rootVisualElement.Query<Button>("AddPlayerProfileButton");
        addPlayerProfileButton.SetEnabled(playerProfileDropdownField.choices.Count > 0);
    }

    void HandleAddPlayerProfileButtonClicked(ClickEvent clickEventDetails)
    {
        DropdownField playerProfileDropdownField = _UIDocumentComponent.rootVisualElement.Query<DropdownField>("AddPlayerProfileDropdownField");

        Player selectedPlayerProfile = new Player();

        foreach (Player playerProfile in LocalGameData.Instance.PlayerProfiles)
        {
            if (playerProfileDropdownField.value == playerProfile.PlayerName)
            {
                selectedPlayerProfile = playerProfile;
                break;
            }
        }

        if (selectedPlayerProfile.PlayerName == null)
        {
            Debug.LogError("No player with that name was found in player profiles.");
            return;
        }

        GameMaster.Instance.AddPlayerRPC(selectedPlayerProfile.PlayerName.ToString(), NetworkManager.Singleton.LocalClientId, LocalGameData.Instance.GUID);
    }

    void HandleKickClientButtonClicked(ClickEvent clickEventDetails, ulong clientID)
    {
        NetworkManager.Singleton.DisconnectClient(clientID, "Kicked by host.");
    }

    void HandleStartGameButtonClicked(ClickEvent clickEventDetails)
    {
        GameMaster.Instance.StartCourse();
    }

    void HandleExitButtonClicked(ClickEvent clickEventDetails)
    {
        NetworkManager.Singleton.Shutdown();
    }

    void HandleRemovePlayerButtonClicked(ClickEvent clickEventDetails, int playerPositionInPlayersList)
    {
        Debug.Log($"Trying to remove {playerPositionInPlayersList}");
        GameMaster.Instance.RemovePlayerRPC(playerPositionInPlayersList);
    }

    void HandleDisconnectButtonClicked(ClickEvent clickEventDetails)
    {
        NetworkManager.Singleton.Shutdown();
    }

    void HandleConnectionEvent(NetworkManager networkManager, ConnectionEventData connectionEventData)
    {
        ConstructClientsListUI();
    }

    void HandlePlayerListChangedEvent(NetworkListEvent<Player> networkListEvent)
    {
        ConstructPlayersListUI();
    }
}
