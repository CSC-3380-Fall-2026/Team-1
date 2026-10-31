using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Contains state and behavior related to UI elements.
/// </summary>
public class UIManager : MonoBehaviour
{
    // A reference to the UXML file containing the testing UI.
    [SerializeField, Tooltip("The UXML file containing the layout of the game UI used during testing.")]
    VisualTreeAsset _gameUIAsset;

    // A reference to the component on this GameObject that handles displaying UXML files on the screen.
    UIDocument _UIDocumentComponent;

    /// <summary>
    /// This function is called immediately the GameObject is instantiated.
    /// Gets a reference to the UIDocument component and subscribes to relevant events.
    /// </summary>
    void Awake()
    {
        // Gets a reference to the UIDocument component used for displaying UXML files.
        _UIDocumentComponent = GetComponent<UIDocument>();

        // Tells the UIDocument component to display the given gameUI.
        _UIDocumentComponent.visualTreeAsset = _gameUIAsset;

        // Links the HostGame button to the function that handles hosting the game.
        Button hostGameButton = _UIDocumentComponent.rootVisualElement.Query<Button>("HostGameButton");
        hostGameButton.RegisterCallback<ClickEvent>(HostGameButtonClicked);

        // Links the JoinGame button to the function that handles joining a game.
        Button joinGameButton = _UIDocumentComponent.rootVisualElement.Query<Button>("JoinGameButton");
        joinGameButton.RegisterCallback<ClickEvent>(JoinGameButtonClicked);

        // Links the SpawnBall button to the function that handles spawning the players' ball objects.
        Button spawnBallsButton = _UIDocumentComponent.rootVisualElement.Query<Button>("SpawnBallsButton");
        spawnBallsButton.RegisterCallback<ClickEvent>(SpawnBallsButtonClicked);

        // In order to maintain an accurate list of connected clients in the UI, this component
        // needs to be able to tell when a change has been made to the list of players.
        // The list only becomes interactable once networking has begun, so subscribing to the events
        // immediately the scene begins will not be allowed; the link has to happen while connected to
        // the server. The OnClientConnectedCallback event is raised when any client connects (including host)
        // So subscribing the player list UI update functions at that point works.
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong newClientID) =>
        {
            // Only want to subscribe once (when the local client/host connects), not on every new connection.
            if (newClientID == NetworkManager.Singleton.LocalClientId)
            {
                // The functions are not called when first linked to the event, so need a manual call.
                ConstructPlayerList();

                // Subscribes the players list construction function to occur when the players list changes or on new turn.
                GameMaster.Instance.Players.OnListChanged += ConstructPlayerList;
                GameMaster.Instance.CurrentPlayerTurn.OnValueChanged += (oldTurn, newTurn) => {ConstructPlayerList();};
            }
        };
    }

    /// <summary>
    /// Start is called once before the first execution of Update after the MonoBehaviour is created.
    /// Hides unwanted UI and shows the relevant parts.
    /// </summary>
    void Start()
    {
        ShowLobbyUI(true);
        ShowGameplayUI(false);
    }

    /// <summary>
    /// Changes the visibility of UI elements involved in managing the lobby.
    /// </summary>
    /// <param name="showLobbyUI">Whether the UI elements will be shown or hidden.</param>
    void ShowLobbyUI(bool showLobbyUI)
    {
        // Gets a reference to the root UI element of the lobby management UI elements. Shows/Hides based on given value.
        VisualElement lobbyRootVisualElement = _UIDocumentComponent.rootVisualElement.Query("LobbyBackground");
        lobbyRootVisualElement.visible = showLobbyUI;
    }

    /// <summary>
    /// Changes the visibility of the UI elements relevant to gameplay.
    /// </summary>
    /// <param name="showGameplayUI">Whether the Ui elements will be shown or hidden.</param>
    void ShowGameplayUI(bool showGameplayUI)
    {
        // Gets a reference to the root UI element of the players list UI elements. Shows/Hides based on given value.
        VisualElement connectedPlayersListVisualElement = _UIDocumentComponent.rootVisualElement.Query("ConnectedPlayersBackground");
        connectedPlayersListVisualElement.visible = showGameplayUI;

        // Gets a reference to the button used to spawn the ball objects. Shows/Hides based on given value.
        // Also only shows if the current client is the host, as only the host should be able to spawn balls.
        Button spawnBallsButton = _UIDocumentComponent.rootVisualElement.Query<Button>("SpawnBallsButton");
        spawnBallsButton.visible = showGameplayUI && NetworkManager.Singleton.IsHost;
    }

    /// <summary>
    /// Reconstructs UI element showing the list of connected players and other relevant information.
    /// </summary>
    /// <param name="playerListChangeInfo">Information regarding the changes to the list.</param>
    void ConstructPlayerList(NetworkListEvent<Player> playerListChangeInfo = default(NetworkListEvent<Player>))
    {
        // Gets a reference to the list view UI element containing the list of players.
        ListView connectedPlayersListView = _UIDocumentComponent.rootVisualElement.Query<ListView>("ConnectedPlayersListView");

        // Initializes a new list of strings to hold the players' information.
        List<string> playerInfoList = new List<string>(GameMaster.Instance.Players.Count);

        // Updates the list above with each player's information.
        for (int i = 0; i < GameMaster.Instance.Players.Count; i++)
        {
            // Constructs the string added to the ListView.
            // Displays each player's ID, name, whether local or remote and whose turn it is.
            string playerInfo = string.Format("{0}: {1}{2}{3}",
                GameMaster.Instance.Players[i].PlayerClientID,
                GameMaster.Instance.Players[i].PlayerName,
                (GameMaster.Instance.Players[i].PlayerClientID == NetworkManager.Singleton.LocalClientId) ? " (Local)" : " (Remote)",
                (GameMaster.Instance.CurrentPlayerTurn.Value == i) ? " (Turn)" : "");

            // Adds the newly constructed string to the previously created list.
            playerInfoList.Add(playerInfo);
        }

        // Tells the ListView to use the created list of player information strings as a source for its elements.
        connectedPlayersListView.itemsSource = playerInfoList;

        // Tells the ListView to recreate its elements with the newly given source.
        connectedPlayersListView.Rebuild();
    }

    /// <summary>
    /// Implements behavior to start hosting a game when a relevant button is clicked.
    /// This function is subscribed to the button's click event in the Awake function.
    /// </summary>
    /// <param name="clickEvent">Contains information regarding the click event.</param>
    void HostGameButtonClicked(ClickEvent clickEvent)
    {
        // Gets a reference to the group of radio buttons that let the player indicate whether their hosted game should be local or LAN.
        // Determines whether LAN is used based on whether the value is 0 (first element, local) or 1 (second element, LAN).
        RadioButtonGroup localOrLANRadioButton = _UIDocumentComponent.rootVisualElement.Query<RadioButtonGroup>("LocalOrLanRadioButtonGroup");
        bool dontUseLAN = localOrLANRadioButton.value == 0;

        // Gets a reference to the integer input box that lets the player input what port they want the game server listening on.
        IntegerField hostPortIntegerField = _UIDocumentComponent.rootVisualElement.Query<IntegerField>("HostPortIntegerField");
        int hostPort = hostPortIntegerField.value;

        // Tells the lobby manager to start hosting (acting as a server and a client simultaneously) using the gathered information.
        LobbyManager.Instance.BeginHosting(dontUseLAN, (ushort)hostPort);

        // Hides UI relevant to the lobby and shows UI relevant to gameplay.
        // Probably change to game scene instead later, when the lobby has its own scene separate from gameplay.
        ShowLobbyUI(false);
        ShowGameplayUI(true);
    }

    /// <summary>
    /// Implements behavior to join an existing game session when a relevant button is clicked.
    /// This function is subscibed to the button's click event in the Awake function.
    /// </summary>
    /// <param name="clickEvent">Contains information regarding the click event.</param>
    void JoinGameButtonClicked(ClickEvent clickEvent)
    {
        // Gets a reference to the text box in which the user has input the host's IP address and stores its value.
        TextField hostIPTextField = _UIDocumentComponent.rootVisualElement.Query<TextField>("IPAddressTextField");
        string hostIPAddress = hostIPTextField.value;

        // Gets a reference to the integer input box indicating the port that the host's server is listening on and stores its value.
        IntegerField hostPortIntegerField = _UIDocumentComponent.rootVisualElement.Query<IntegerField>("JoinPortIntegerField");
        int hostPort = hostPortIntegerField.value;

        // Tells the lobby manager to connect to the host/server using the gathered information.
        LobbyManager.Instance.ConnectToHost(hostIPAddress, hostPort.ToString());

        // Hides UI relevant to the lobby and shows UI relevant to the gameplay.
        // Probably change to game scene instead later, when the lobby has its own scene separate from gameplay.
        ShowLobbyUI(false);
        ShowGameplayUI(true);
    }

    /// <summary>
    /// Implements behavior to let the host spawn the players' ball objects.
    /// This function is subscibed to the button's click event in the Awake function.
    /// </summary>
    /// <param name="clickEvent">Contains information regarding the click event.</param>
    void SpawnBallsButtonClicked(ClickEvent clickEvent)
    {
        // Tells the GameMaster to spawn the players' ball objects.
        GameMaster.Instance.SpawnBallsRPC();

        // Gets a reference to the button UI element that raises this event and hides it from the UI.
        Button spawnBallsButton = _UIDocumentComponent.rootVisualElement.Query<Button>("SpawnBallsButton");
        spawnBallsButton.visible = false;
    }
}