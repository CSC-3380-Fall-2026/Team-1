using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// A controller for the UI of the Game Mode selection scene.
/// Effectively a middle-man between the UI and the LobbyManager component.
/// </summary>
public class LobbyUIManager : MonoBehaviour
{
    // A reference to the asset describing the lobby's UI.
    [SerializeField, Tooltip("The UXML file containing the lobby's UI.")]
    VisualTreeAsset _lobbyUIAsset;

    // A reference to the UXML asset that will fill the clients list view as new players connect to the server.
    [SerializeField, Tooltip("The UXML file containing the visual that will be inserted into the clients list.")]
    VisualTreeAsset _clientInfoAsset;

    // A reference to the UXML asset that will fill the players list view as new player profiles are added.
    [SerializeField, Tooltip("The UXML file containing the visual that will be inserted into the players list.")]
    VisualTreeAsset _playerInfoAsset;

    // A reference to the UI Document component on this object.
    UIDocument _UIDocumentComponent;

    /// <summary>
    /// Called when the gameObject is first instantiated.
    /// Registers callbacks and determines initial UI state based on host/client status.
    /// </summary>
    void Awake()
    {
        // Gets a reference to the component that actually displays the UI.
        _UIDocumentComponent = GetComponent<UIDocument>();

        // Tells the UI display component which UXML asset to display.
        _UIDocumentComponent.visualTreeAsset = _lobbyUIAsset;

        // Changes the buttons visible based on host/client status.
        if (!NetworkManager.Singleton.IsHost)
        {
            // Hides the start/exit game buttons if the caller is a client. Leaves visible if host.
            VisualElement startExitButtonsContainer = _UIDocumentComponent.rootVisualElement.Query("StartExitContainer");
            startExitButtonsContainer.visible = false;

            // Makes the disconnect button visible if the caller is a client. Hides if host.
            VisualElement disconnectButtonContainer = _UIDocumentComponent.rootVisualElement.Query("DisconnectContainer");
            disconnectButtonContainer.visible = true;

            // Links the disconnect button to disconnect functionality.
            Button disconnectButton = disconnectButtonContainer.Query<Button>("DisconnectButton");
            disconnectButton.RegisterCallback<ClickEvent>(HandleDisconnectButtonClicked);
        }

        // General button function registry.
        Button addPlayerProfileButton = _UIDocumentComponent.rootVisualElement.Query<Button>("AddPlayerProfileButton");
        addPlayerProfileButton.RegisterCallback<ClickEvent>(HandleAddPlayerProfileButtonClicked);

        Button startGameButton = _UIDocumentComponent.rootVisualElement.Query<Button>("StartGameButton");
        startGameButton.RegisterCallback<ClickEvent>(HandleStartGameButtonClicked);

        Button exitGameButton = _UIDocumentComponent.rootVisualElement.Query<Button>("ExitGameButton");
        exitGameButton.RegisterCallback<ClickEvent>(HandleDisconnectButtonClicked);
    }

    /// <summary>
    /// This function is called after all objects' Awake() functions have been completed.
    /// Registers to off-gameObject callbacks and initializes the UI elements.
    /// </summary>
    void Start()
    {
        // OnConnectionEvent is raised every connection/disconnection.
        NetworkManager.Singleton.OnConnectionEvent += HandleConnectionEvent;

        // OnListChanged is raised whenever the players list changes.
        GameMaster.Instance.Players.OnListChanged += HandlePlayerListChangedEvent;

        // These are not called when first subscribed, so need to be called manually.
        ConstructClientsListUI();
        ConstructPlayersListUI();
        ConstructPlayerProfilesListUI();
    }

    /// <summary>
    /// Fills the clients list view with UI elements related to each client.
    /// </summary>
    void ConstructClientsListUI()
    {
        // Gets a reference to the scroll view acting as a container for the list of clients. Clears existing elements.
        ScrollView clientsListScrollView = _UIDocumentComponent.rootVisualElement.Query<ScrollView>("ClientsListScrollView");
        clientsListScrollView.Clear();

        // Makes and adds a UI element for each connected client.
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            // Makes a copy of the client information UI document's root.
            VisualElement clientInfoRootVisualElement = _clientInfoAsset.CloneTree();

            // Gets a reference to the identifier section of the client UI element and sets the text to the relevant information.
            Label clientInfoIdLabel = clientInfoRootVisualElement.Query<Label>("ClientIDLabel");
            clientInfoIdLabel.text = $"Client ID: {client.ClientId}";
            clientInfoIdLabel.text += $"{(client.ClientId == 0 ? " (Host)" : "")}";
            clientInfoIdLabel.text += $"{(client.ClientId == NetworkManager.Singleton.LocalClientId ? " (You)" : "")}";

            // Gets a reference to the button that kicks the client and stores the client ID in the button.
            // Passing the ID to the callback directly leaves all callbacks pointing towards the last client on the list.
            Button clientKickButton = clientInfoRootVisualElement.Query<Button>("KickButton");
            clientKickButton.userData = client.ClientId;

            // Informs the button to kick the relevant client when clicked.
            clientKickButton.RegisterCallback<ClickEvent>((clickEventDetails) => {
                HandleKickClientButtonClicked(clickEventDetails, (ulong)clientKickButton.userData);
            });

            // Hides the kick button if the client is the host. The host should not be able to kick themself.
            clientKickButton.visible = client.ClientId != 0;

            // Disables the ability to kick clients unless the local client is the host.
            if (!NetworkManager.Singleton.IsHost)
                clientKickButton.SetEnabled(false);

            // Adds this client's UI element to the list.
            clientsListScrollView.Add(clientInfoRootVisualElement);
        }
    }

    /// <summary>
    /// Fills the players list view with UI elements related to each player profile.
    /// </summary>
    void ConstructPlayersListUI()
    {
        // Gets a reference to the scroll view acting as a container for the list of players. Clears existing elements.
        ScrollView playersListScrollView = _UIDocumentComponent.rootVisualElement.Query<ScrollView>("PlayersListScrollView");
        playersListScrollView.Clear();

        // Makes and adds a UI element for each player profile in the game.
        for (int i = 0; i < GameMaster.Instance.Players.Count; i++)
        {
            // Makes a copy of the player profile UI template's root.
            VisualElement playerInfoRootVisualElement = _playerInfoAsset.CloneTree();

            // Changes the template's text to indicate the player's name.
            Label playerInfoNameLabel = playerInfoRootVisualElement.Query<Label>("PlayerNameLabel");
            playerInfoNameLabel.text = GameMaster.Instance.Players[i].PlayerName.ToString();

            // Changes the template's text to indicate whether the player belongs to the local client or a remote client.
            Label localRemoteLabel = playerInfoRootVisualElement.Query<Label>("LocalOrRemoteLabel");
            localRemoteLabel.text = (GameMaster.Instance.Players[i].PlayerClientID == NetworkManager.Singleton.LocalClientId) ? "Local" : "Remote";

            // Gets a reference to the button that removes and stores the player profile ID in the button.
            // Passing the ID to the callback directly leaves all callbacks pointing towards the last player profile on the list.
            Button removePlayerButton = playerInfoRootVisualElement.Query<Button>("RemovePlayerButton");
            removePlayerButton.userData = i;

            // Informs the button to remove the relevant player profile when clicked.
            removePlayerButton.RegisterCallback<ClickEvent>((clickEventDetails) => {
                HandleRemovePlayerButtonClicked(clickEventDetails, (int)removePlayerButton.userData);
            });

            // Only the owning client should be able to remove the player profiles they own.
            removePlayerButton.SetEnabled(GameMaster.Instance.Players[i].PlayerClientID == NetworkManager.Singleton.LocalClientId);

            // Adds the player profile's UI element to the list.
            playersListScrollView.Add(playerInfoRootVisualElement);
        }
    }

    /// <summary>
    /// Adds the local client's player profiles to the dropdown that lets them select which to add to the game.
    /// </summary>
    void ConstructPlayerProfilesListUI()
    {
        // Gets a reference to the local player profiles dropdown and clears existing elements.
        DropdownField playerProfileDropdownField = _UIDocumentComponent.rootVisualElement.Query<DropdownField>("AddPlayerProfileDropdownField");
        playerProfileDropdownField.choices.Clear();

        // No need to do anything more if there are no player profiles on this client.
        if (LocalGameData.Instance.PlayerProfiles.Count == 0)
        {
            Debug.Log("No profiles in LocalGameData");
            return;
        }

        // Adds the name of every local player profile to the dropdown.
        for (int i = 0; i < LocalGameData.Instance.PlayerProfiles.Count; i++)
            playerProfileDropdownField.choices.Add(LocalGameData.Instance.PlayerProfiles[i].PlayerName.ToString());

        // Initializes the dropdown menu to the first profile on the list.
        playerProfileDropdownField.index = 0;

        // Enables the button that adds the selected player profile to the game if there is at least one in the list.
        Button addPlayerProfileButton = _UIDocumentComponent.rootVisualElement.Query<Button>("AddPlayerProfileButton");
        addPlayerProfileButton.SetEnabled(playerProfileDropdownField.choices.Count > 0);
    }

    /// <summary>
    /// Implements functionality to actually add the player profile to the game.
    /// </summary>
    /// <param name="clickEventDetails">Contains information about the click that happened on the button.</param>
    void HandleAddPlayerProfileButtonClicked(ClickEvent clickEventDetails)
    {
        // A reference to the dropdown is needed to read the current value.
        DropdownField playerProfileDropdownField = _UIDocumentComponent.rootVisualElement.Query<DropdownField>("AddPlayerProfileDropdownField");

        
        // Finds the player profile with the same name as the selected value in the dropdown menu.
        Player selectedPlayerProfile = new Player();
        foreach (Player playerProfile in LocalGameData.Instance.PlayerProfiles)
        {
            if (playerProfileDropdownField.value == playerProfile.PlayerName)
            {
                selectedPlayerProfile = playerProfile;
                break;
            }
        }

        // Handles the possibility that there is no profile selected.
        if (selectedPlayerProfile.PlayerName == null)
        {
            Debug.LogError("No player with that name was found in player profiles.");
            return;
        }

        // Adds the player profile to the players list.
        GameMaster.Instance.AddPlayerRPC(selectedPlayerProfile.PlayerName.ToString(), NetworkManager.Singleton.LocalClientId, LocalGameData.Instance.GUID);
    }

    /// <summary>
    /// Handles the callback raised when the kick client button is clicked.
    /// Disconnects the client with the given ID.
    /// </summary>
    /// <param name="clickEventDetails">Contains information about the click on this button.</param>
    /// <param name="clientID">The network ID of the client to be kicked.</param>
    void HandleKickClientButtonClicked(ClickEvent clickEventDetails, ulong clientID)
    {
        NetworkManager.Singleton.DisconnectClient(clientID, "Kicked by host.");
    }

    /// <summary>
    /// Handles the callback raised when the start game button is clicked.
    /// Tells the GameMaster to start the course with the current players and clients.
    /// </summary>
    /// <param name="clickEventDetails">Contains information about the click on this button.</param>
    void HandleStartGameButtonClicked(ClickEvent clickEventDetails)
    {
        GameMaster.Instance.StartCourse();
    }

    /// <summary>
    /// Handles the callback raised when the remove player profile button is clicked.
    /// Removes the player from the players list on the GameMaster.
    /// </summary>
    /// <param name="clickEventDetails">Contains information about the click on this button.</param>
    /// <param name="playerPositionInPlayersList"></param>
    void HandleRemovePlayerButtonClicked(ClickEvent clickEventDetails, int playerPositionInPlayersList)
    {
        GameMaster.Instance.RemovePlayerRPC(playerPositionInPlayersList);
    }

    /// <summary>
    /// Handles the callback raised when the disconnect button is clicked by a client.
    /// Turns off the server/client.
    /// </summary>
    /// <param name="clickEventDetails">Contains information about the click on this button.</param>
    void HandleDisconnectButtonClicked(ClickEvent clickEventDetails)
    {
        NetworkManager.Singleton.Shutdown();
    }

    /// <summary>
    /// Handles the callback raised when a client connects/disconnects.
    /// Reconstructs the connected clients list.
    /// </summary>
    /// <param name="networkManager">The NetworkManager component that raised this event.</param>
    /// <param name="connectionEventData">Information about the new connection/disconnection.</param>
    void HandleConnectionEvent(NetworkManager networkManager, ConnectionEventData connectionEventData)
    {
        ConstructClientsListUI();
    }

    /// <summary>
    /// Handles the callback raised when the players NetworkList is changed.
    /// Reconstructs the players UI list.
    /// </summary>
    /// <param name="networkListEvent">Information about the change made to the list.</param>
    void HandlePlayerListChangedEvent(NetworkListEvent<Player> networkListEvent)
    {
        ConstructPlayersListUI();
    }
}
