using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// This function handles displaying the main menu's UI and assigning behavior to UI elements.
/// </summary>
public class MainMenuUIManager : MonoBehaviour
{
    // A reference to the UXML file containing the UI of the first menu displayed in the game.
    [SerializeField, Tooltip("The UXML file containing the initial menu's UI.")]
    VisualTreeAsset _mainMenuUIAsset;

    // A reference to the UXML file containing the UI of the settings menu.
    [SerializeField, Tooltip("The UXML file containing the settings menu UI.")]
    VisualTreeAsset _manageProfilesUIAsset;

    // A reference to the UXML file containing the UI of the settings menu.
    [SerializeField, Tooltip("The UXML file containing the settings menu UI.")]
    VisualTreeAsset _settingsMenuUIAsset;

    // A reference to the UI Document component on the main menu's UI manager.
    // Changing the source asset changes the UI displayed.
    UIDocument _UIDocumentComponent;

    // Used to determine if this client will host a game or join a game when handling the confirm button's click event.
    bool _hosting;

    /// <summary>
    /// This function is called after all Awake() calls on all gameObjects initially in the scene.
    /// Stores a reference to the UI Document component and shows the main menu.
    /// </summary>
    void Start()
    {
        // A reference to the UI Document component is needed, as it is what actually displays the UI.
        _UIDocumentComponent = GetComponent<UIDocument>();

        // The main menu should be the first UI element displayed upon entering this scene.
        ShowMainMenu();
    }

    /// <summary>
    /// Handles displaying the main menu's UI.
    /// Sets relevant click event handlers and disables play buttons based on player profile availability.
    /// </summary>
    void ShowMainMenu()
    {
        // Tells the UI document component to display the main menu's UI.
        _UIDocumentComponent.visualTreeAsset = _mainMenuUIAsset;

        // Each button needs to be pointed at a function to execute when it is clicked.
        // Unregister to avoid duplicates; does nothing if not subcribed before.
        Button singlePlayerButton = _UIDocumentComponent.rootVisualElement.Query<Button>("SingleplayerButton");
        singlePlayerButton.UnregisterCallback<ClickEvent>(HandleSingleplayerButtonClicked);
        singlePlayerButton.RegisterCallback<ClickEvent>(HandleSingleplayerButtonClicked);

        Button hostLANGameButton = _UIDocumentComponent.rootVisualElement.Query<Button>("HostLANGameButton");
        hostLANGameButton.UnregisterCallback<ClickEvent>(HandleHostLANGameButtonClicked);
        hostLANGameButton.RegisterCallback<ClickEvent>(HandleHostLANGameButtonClicked);

        Button joinLANGameButton = _UIDocumentComponent.rootVisualElement.Query<Button>("JoinLANGameButton");
        joinLANGameButton.UnregisterCallback<ClickEvent>(HandleJoinLANGameButtonClicked);
        joinLANGameButton.RegisterCallback<ClickEvent>(HandleJoinLANGameButtonClicked);

        Button managePlayerProfilesButton = _UIDocumentComponent.rootVisualElement.Query<Button>("ManagePlayerProfilesButton");
        managePlayerProfilesButton.RegisterCallback<ClickEvent>(HandleManagePlayerProfilesButtonClicked);

        Button settingsButton = _UIDocumentComponent.rootVisualElement.Query<Button>("SettingsButton");
        settingsButton.UnregisterCallback<ClickEvent>(HandleSettingsButtonClicked);
        settingsButton.RegisterCallback<ClickEvent>(HandleSettingsButtonClicked);

        Button quitButton = _UIDocumentComponent.rootVisualElement.Query<Button>("QuitButton");
        quitButton.UnregisterCallback<ClickEvent>(HandleQuitButtonClicked);
        quitButton.RegisterCallback<ClickEvent>(HandleQuitButtonClicked);

        // Starting or joining a game should only be allowed if at least one player profile has been made.
        bool enablePlayButtons = LocalGameData.Instance.PlayerProfiles.Count > 0;
        singlePlayerButton.SetEnabled(enablePlayButtons);
        hostLANGameButton.SetEnabled(enablePlayButtons);
        joinLANGameButton.SetEnabled(enablePlayButtons);
    }

    /// <summary>
    /// This function hides or shows the ip/port input UI relevant for multiplayer.
    /// </summary>
    /// <param name="show">Will hide the input UI if false and show if true.</param>
    /// <param name="hosting">will tailor input for hosting if true, joining if false.</param>
    void ShowMultiplayerSection(bool show, bool hosting = true)
    {
        // Shows and hides relevant UI element containers based on the 'show' value.
        VisualElement hostJoinButtonsContainerVisualElement = _UIDocumentComponent.rootVisualElement.Query("HostJoinButtonsContainerVisualElement");
        hostJoinButtonsContainerVisualElement.style.display = show ? DisplayStyle.None : DisplayStyle.Flex;

        VisualElement hostInformationContainerVisualElement = _UIDocumentComponent.rootVisualElement.Query("HostInformationContainerVisualElement");
        hostInformationContainerVisualElement.style.display = (show && hosting) ? DisplayStyle.Flex : DisplayStyle.None;

        VisualElement joinInformationContainerVisualElement = _UIDocumentComponent.rootVisualElement.Query("JoinInformationContainerVisualElement");
        joinInformationContainerVisualElement.style.display = (show && !hosting) ? DisplayStyle.Flex : DisplayStyle.None;

        // Need button references to subscribe/unsubscribe from click events based on 'show' value.
        Button hostCancelButton = _UIDocumentComponent.rootVisualElement.Query<Button>("HostCancelButton");
        Button hostConfirmButton = _UIDocumentComponent.rootVisualElement.Query<Button>("HostConfirmButton");
        Button joinCancelButton = _UIDocumentComponent.rootVisualElement.Query<Button>("JoinCancelButton");
        Button joinConfirmButton = _UIDocumentComponent.rootVisualElement.Query<Button>("JoinConfirmButton");

        // Subscribes/unsubscribes from relevant functions based on supplied information.
        // Unsubscribes before subscribing to avoid duplicate registration.
        if (show && hosting)
        {
            hostCancelButton.UnregisterCallback<ClickEvent>(HandleCancelButtonClicked);
            hostCancelButton.RegisterCallback<ClickEvent>(HandleCancelButtonClicked);

            hostConfirmButton.UnregisterCallback<ClickEvent>(HandleConfirmButtonClicked);
            hostConfirmButton.RegisterCallback<ClickEvent>(HandleConfirmButtonClicked);
        } else if (show && !hosting)
        {
            joinCancelButton.UnregisterCallback<ClickEvent>(HandleCancelButtonClicked);
            joinCancelButton.RegisterCallback<ClickEvent>(HandleCancelButtonClicked);

            joinConfirmButton.UnregisterCallback<ClickEvent>(HandleConfirmButtonClicked);
            joinConfirmButton.RegisterCallback<ClickEvent>(HandleConfirmButtonClicked);
        } else
        {
            hostCancelButton.UnregisterCallback<ClickEvent>(HandleCancelButtonClicked);
            hostConfirmButton.UnregisterCallback<ClickEvent>(HandleConfirmButtonClicked);
            joinCancelButton.UnregisterCallback<ClickEvent>(HandleCancelButtonClicked);
            joinConfirmButton.UnregisterCallback<ClickEvent>(HandleConfirmButtonClicked);
        }

        // The callbacks need this information to let the LobbyManager know whether to join or host a game.
        _hosting = hosting;
    }

    /// <summary>
    /// Handles displaying the manage profiles menu UI.
    /// Registers click event callbacks as well.
    /// </summary>
    void ShowManageProfilesMenu()
    {
        // Changes the displayed UI asset to the 'manage profiles' UXML file.
        _UIDocumentComponent.visualTreeAsset = _manageProfilesUIAsset;

        // Subscribes to the click events of buttons in the settings menu.
        // Unsubscribes first to avoid duplicate registration.
        Button backToMainMenuButton = _UIDocumentComponent.rootVisualElement.Query<Button>("BackToMainMenuButton");
        backToMainMenuButton.UnregisterCallback<ClickEvent>(HandleBackToMainMenuButtonClicked);
        backToMainMenuButton.RegisterCallback<ClickEvent>(HandleBackToMainMenuButtonClicked);
    }

    /// <summary>
    /// Handles displaying the settings menu UI.
    /// Registers click event callbacks as well.
    /// </summary>
    void ShowSettingsMenu()
    {
        // Changes the displayed UI asset to the settings UXML file.
        _UIDocumentComponent.visualTreeAsset = _settingsMenuUIAsset;

        // Subscribes to the click events of buttons in the settings menu.
        // Unsubscribes first to avoid duplicate registration.
        Button backToMainMenuButton = _UIDocumentComponent.rootVisualElement.Query<Button>("BackToMainMenuButton");
        backToMainMenuButton.UnregisterCallback<ClickEvent>(HandleBackToMainMenuButtonClicked);
        backToMainMenuButton.RegisterCallback<ClickEvent>(HandleBackToMainMenuButtonClicked);
    }

    /// <summary>
    /// This function is called when the "singleplayer" button is clicked in the UI.
    /// Moves to the lobby, with a SinglePlayerTransport rather than UnityTransport.
    /// </summary>
    /// <param name="clickEventInfo">Contains information about the button click event.</param>
    void HandleSingleplayerButtonClicked(ClickEvent clickEventInfo)
    {
        LobbyManager.Instance.BeginHosting(true, default(ushort));
        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    /// <summary>
    /// This function is called when the "Host LAN Game" button is clicked in the UI.
    /// Shows the host game input UI and hides the host/join buttons.
    /// </summary>
    /// <param name="clickEventInfo">Contains information about the button click event.</param>
    void HandleHostLANGameButtonClicked(ClickEvent clickEventInfo)
    {
        ShowMultiplayerSection(true, true);
    }

    /// <summary>
    /// This function is called when the "Join LAN Game" button is clicked in the UI.
    /// Shows the join game input UI and hides the host/join buttons.
    /// </summary>
    /// <param name="clickEventInfo">Contains information about the button click event.</param>
    void HandleJoinLANGameButtonClicked(ClickEvent clickEventInfo)
    {
        ShowMultiplayerSection(true, false);
    }

    /// <summary>
    /// This function is called when the "Cancel" button is clicked in the host/join input UI.
    /// Shows host/join buttons and hides the input UI.
    /// </summary>
    /// <param name="clickEventInfo">Contains information about the button click event.</param>
    void HandleCancelButtonClicked(ClickEvent clickEventInfo)
    {
        ShowMultiplayerSection(false);
    }

    /// <summary>
    /// This function is called when the "Confirm" button is clicked in the input UI.
    /// Passes relevant information to the LobbyManager to host/join a game.
    /// </summary>
    /// <param name="clickEventInfo">Contains information about the button click event.</param>
    void HandleConfirmButtonClicked(ClickEvent clickEventInfo)
    {
        // Need to pass the input information to the LobbyManager.
        IntegerField hostPortIntegerField = _UIDocumentComponent.rootVisualElement.Query<IntegerField>("HostPortIntegerField");
        TextField ipAddressTextField = _UIDocumentComponent.rootVisualElement.Query<TextField>("IPAddressTextField");
        IntegerField joinPortIntegerField = _UIDocumentComponent.rootVisualElement.Query<IntegerField>("JoinPortIntegerField");

        // Tells the LobbyManager to host/join based on input and _hosting value.
        if (_hosting)
        {
            LobbyManager.Instance.BeginHosting(false, (ushort)hostPortIntegerField.value);
            NetworkManager.Singleton.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
        } else
            LobbyManager.Instance.ConnectToHost(ipAddressTextField.text, joinPortIntegerField.text);
    }

    /// <summary>
    /// This function is called when the "Manage Player Profiles" button is clicked in the input UI.
    /// Changes the menu being displayed to the manage profiles menu.
    /// </summary>
    /// <param name="clickEventInfo">Contains information about the button click event.</param>
    void HandleManagePlayerProfilesButtonClicked(ClickEvent clickEventInfo)
    {
        ShowManageProfilesMenu();
    }

    /// <summary>
    /// This function is called when the settings button in the main menu is clicked.
    /// Unloads the main menu UI and loads the settings UI.
    /// </summary>
    /// <param name="clickEventInfo">Contains information about the click event.</param>
    void HandleSettingsButtonClicked(ClickEvent clickEventInfo)
    {
        ShowSettingsMenu();
    }

    /// <summary>
    /// This function is called when the quit button is clicked in the main menu's UI.
    /// Closes the game.
    /// </summary>
    /// <param name="clickEventInfo">Contains information about the button click.</param>
    void HandleQuitButtonClicked(ClickEvent clickEventInfo)
    {
        Application.Quit();
    }

    /// <summary>
    /// This funnction is called when a button in the UI intended to lead back to the main menu is clicked.
    /// Unloads the UI of the current menu and loads the main menu's UI.
    /// It will be used in multiple different menus, as its functionality is quite generic. 
    /// </summary>
    /// <param name="clickEventInfo"></param>
    void HandleBackToMainMenuButtonClicked(ClickEvent clickEventInfo)
    {
        ShowMainMenu();
    }

    /// <summary>
    /// Unsubscribes from the on-click events of all visual elements in the currently displayed UI window.
    /// </summary>
    void UnregisterUIElementEventHandlers()
    {
        if (_UIDocumentComponent == null || _UIDocumentComponent.rootVisualElement == null)
            return;

        foreach (VisualElement visualElement in _UIDocumentComponent.rootVisualElement.Query().ToList())
            visualElement.UnregisterAllRemovableCallbacks();
    }

    /// <summary>
    /// This function is called just before the GameObject is destroyed (e.g. when switching scenes).
    /// Unsubscribes the UI buttons' click events handlers.
    /// </summary>
    void OnDestroy()
    {
        UnregisterUIElementEventHandlers();
    }
}
