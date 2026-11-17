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

    // This player profile will contain information about the player profile currently being edited in
    // the 'Manage Player Profiles' window.
    Player _currentlyEditedPlayerProfile;

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

        // The list of player profiles in the dropdown should update whenever the player profiles list changes.
        // This in turn causes the profile customization section to update as well.
        LocalGameData.Instance.PlayerProfilesListChanged -= ConstructPlayerProfilesListUI;
        LocalGameData.Instance.PlayerProfilesListChanged += ConstructPlayerProfilesListUI;

        // Subscribing does not call the function, so the initial setup needs to be called manually.
        ConstructPlayerProfilesListUI();
        ConstructPlayerProfileEditUI();

        // Subscribes to the click/value change events of elements in the 'manage profiles' menu.
        // Unsubscribes first to avoid duplicate registration.
        Button backToMainMenuButton = _UIDocumentComponent.rootVisualElement.Query<Button>("BackToMainMenuButton");
        backToMainMenuButton.UnregisterCallback<ClickEvent>(HandleBackToMainMenuButtonClicked);
        backToMainMenuButton.RegisterCallback<ClickEvent>(HandleBackToMainMenuButtonClicked);

        DropdownField profileSelectDropdownField = _UIDocumentComponent.rootVisualElement.Query<DropdownField>("ProfileSelectDropdownField");
        profileSelectDropdownField.UnregisterValueChangedCallback(HandleSelectedPlayerProfileDropdownChanged);
        profileSelectDropdownField.RegisterValueChangedCallback(HandleSelectedPlayerProfileDropdownChanged);

        Button AddNewPlayerProfileButton = _UIDocumentComponent.rootVisualElement.Query<Button>("AddNewPlayerProfileButton");
        AddNewPlayerProfileButton.UnregisterCallback<ClickEvent>(HandleAddNewPlayerProfileButtonClicked);
        AddNewPlayerProfileButton.RegisterCallback<ClickEvent>(HandleAddNewPlayerProfileButtonClicked);

        Button deletePlayerProfileButton = _UIDocumentComponent.rootVisualElement.Query<Button>("DeletePlayerProfileButton");
        deletePlayerProfileButton.UnregisterCallback<ClickEvent>(HandleDeletePlayerProfileButtonClicked);
        deletePlayerProfileButton.RegisterCallback<ClickEvent>(HandleDeletePlayerProfileButtonClicked);

        Button savePlayerProfileButton = _UIDocumentComponent.rootVisualElement.Query<Button>("SavePlayerProfileButton");
        savePlayerProfileButton.UnregisterCallback<ClickEvent>(HandleSavePlayerProfileButtonClicked);
        savePlayerProfileButton.RegisterCallback<ClickEvent>(HandleSavePlayerProfileButtonClicked);

        TextField newProfileNameTextField = _UIDocumentComponent.rootVisualElement.Query<TextField>("NewPlayerNameTextField");
        newProfileNameTextField.UnregisterValueChangedCallback(HandleAddNewPlayerNameTextFieldChanged);
        newProfileNameTextField.RegisterValueChangedCallback(HandleAddNewPlayerNameTextFieldChanged);

        Button confirmAddPlayerProfile = _UIDocumentComponent.rootVisualElement.Query<Button>("ConfirmNewPlayerProfileButton");
        confirmAddPlayerProfile.UnregisterCallback<ClickEvent>(HandleConfirmAddNewPlayerButtonClicked);
        confirmAddPlayerProfile.RegisterCallback<ClickEvent>(HandleConfirmAddNewPlayerButtonClicked);
    }

    /// <summary>
    /// Fills the player profile selection dropdown with the names of LocalGameData's player profiles.
    /// Disables/enables UI elements which should only be interactable if the dropdown has any choices available.
    /// </summary>
    /// <param name="changeIndex">The index of a newly added/removed player profile in the LocalGameData's player profiles list.</param>
    void ConstructPlayerProfilesListUI(int changeIndex = -1)
    {
        // This dropdown will contain a list of player profile names in the index order they are stored in LocalGameData.
        DropdownField profileSelectDropdownField = _UIDocumentComponent.rootVisualElement.Query<DropdownField>("ProfileSelectDropdownField");

        // Editing the dropdown's members requires it to be enabled first.
        profileSelectDropdownField.SetEnabled(true);

        // Need to clear the list before adding new values, or there will be duplicate entries/a lack of changes.
        profileSelectDropdownField.choices.Clear();

        // Populates the dropdown choices with the names of players in the LocalGameData's player profiles list.
        foreach (Player playerProfile in LocalGameData.Instance.PlayerProfiles)
            profileSelectDropdownField.choices.Add(playerProfile.PlayerName.ToString());

        // changeIndex will be either the index (in the LocalGameData's PlayerProfiles list) of a newly added profile, the previous index
        // of a removed element, or -1 for general changes (e.g. deleting the entire list/loading multiple profiles from the disk).
        // For the aforementioned general changes (changeIndex = -1), the selected profile should default to the first in the list.
        // For newly added profiles (changeIndex = new profile's index), the selected profile will be the last element in the dropdown.
        // When a profile is removed, the next element after it in the list should be selected (index 0 removed, 1 becomes 0, 0 selected).
        profileSelectDropdownField.index = (changeIndex < 0) ? 0 : Mathf.Clamp(changeIndex, -1, profileSelectDropdownField.choices.Count - 1);

        // For some reason, the visual text value refuses to update to match the index in some cases.
        // This issue is resolved by manually changing the value to match the index.
        if (profileSelectDropdownField.index >= 0)
            profileSelectDropdownField.value = profileSelectDropdownField.choices[profileSelectDropdownField.index];

        // The button to delete player profiles shouldn't be able to be clicked while there are no profiles to delete.
        Button deletePlayerProfileButton = _UIDocumentComponent.rootVisualElement.Query<Button>("DeletePlayerProfileButton");
        deletePlayerProfileButton.SetEnabled(profileSelectDropdownField.text != string.Empty);

        // Updating the player profile customization window is intended to be done whenever the selected player profile in the dropdown
        // changes, but the event is not raised when dropdown choices are added/removed/selected via code, only via the UI itself.
        // Therefore, the handler needs to be called manually.
        HandleSelectedPlayerProfileDropdownChanged(default);

        // There is no need for the dropdown to be interactable if there are no choices to select.
        profileSelectDropdownField.SetEnabled(profileSelectDropdownField.choices.Count > 0);
    }

    /// <summary>
    /// Sets up the player profile customization UI based on whether a new profile is being created or an existing one customized.
    /// </summary>
    /// <param name="newPlayerProfile">The UI for creating a new player profile will be shown if true, customizing existing if false.</param>
    void ConstructPlayerProfileEditUI(bool newPlayerProfile = false)
    {
        // Two different sets of UI elements need to be shown/hidden in alternating manner based on whether the player is adding a new
        // player profile or customizing an existing one.
        VisualElement newPlayerProfileElementsContainer = _UIDocumentComponent.rootVisualElement.Query("NewPlayerProfileElementsContainer");
        VisualElement existingPlayerProfileElementsContainer = _UIDocumentComponent.rootVisualElement.Query("ExistingPlayerProfileElementsContainer");

        // The element set for adding a new profile only needs to be shown if indicated via argument or there is no profile currently being customized.
        bool showNewPlayerUI = newPlayerProfile || _currentlyEditedPlayerProfile.PlayerName.ToString() == string.Empty;

        // Hides/shows the UI element sets based on the operation being performed.
        newPlayerProfileElementsContainer.style.display = showNewPlayerUI ? DisplayStyle.Flex : DisplayStyle.None;
        existingPlayerProfileElementsContainer.style.display = showNewPlayerUI ? DisplayStyle.None : DisplayStyle.Flex;

        // Initializes the UI elements required for the addition/customization of a player profile.
        if (showNewPlayerUI)
        {
            TextField newProfileNameTextField = _UIDocumentComponent.rootVisualElement.Query<TextField>("NewPlayerNameTextField");
            newProfileNameTextField.value = string.Empty;

            Button confirmAddPlayerProfile = _UIDocumentComponent.rootVisualElement.Query<Button>("ConfirmNewPlayerProfileButton");
            confirmAddPlayerProfile.SetEnabled(false);
        } else
        {
            Label playerNameLabel = _UIDocumentComponent.rootVisualElement.Query<Label>("PlayerNameLabel");
            playerNameLabel.text = _currentlyEditedPlayerProfile.PlayerName.ToString();
        }
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
        LobbyManager.Instance.BeginHosting(true, default);
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
    /// Handler for the situation where the "Add New Profile" button is clicked.
    /// Shows the profile customization window in "add player profile" mode rather than "customize" mode.
    /// </summary>
    /// <param name="clickEventInfo">Contains information about the button click event.</param>
    void HandleAddNewPlayerProfileButtonClicked(ClickEvent clickEventInfo)
    {
        ConstructPlayerProfileEditUI(true);
    }

    /// <summary>
    /// Handler for the situation where the "Delete Profile" button is clicked.
    /// Tells the LocalGameData object to delete the profile whose index matches that of the dropdown's selection.
    /// </summary>
    /// <param name="clickEventInfo">Contains information about the button click event.</param>
    void HandleDeletePlayerProfileButtonClicked(ClickEvent clickEventInfo)
    {
        // The elements in the player profiles dropdown and the list in LocalGameData have the same indices.
        DropdownField profileSelectDropdownField = _UIDocumentComponent.rootVisualElement.Query<DropdownField>("ProfileSelectDropdownField");
        LocalGameData.Instance.RemovePlayerProfile(profileSelectDropdownField.index);
    }

    /// <summary>
    /// Handler for the situation where the user types anything in the player name input box.
    /// Enables/disables the button to add a new player based on the availability of the typed name.
    /// </summary>
    /// <param name="changeEventInfo">Contains information about the text field change event.</param>
    void HandleAddNewPlayerNameTextFieldChanged(ChangeEvent<string> changeEventInfo)
    {
        Button confirmAddPlayerProfile = _UIDocumentComponent.rootVisualElement.Query<Button>("ConfirmNewPlayerProfileButton");
        TextField newProfileNameTextField = _UIDocumentComponent.rootVisualElement.Query<TextField>("NewPlayerNameTextField");

        // The button to add the new profile to the profiles list should only be clickable if a profile with the same name
        // doesn't exist already and the input field isn't blank.
        bool allowedToSavePlayerName =
            LocalGameData.Instance.PlayerProfilesListContainsName(newProfileNameTextField.value) == -1 &&
            newProfileNameTextField.value != string.Empty;

        confirmAddPlayerProfile.SetEnabled(allowedToSavePlayerName);
    }

    /// <summary>
    /// Handler for the situation where the button to save the customizations made to a player profile is clicked.
    /// Updates the profile in the LocalGameData's profile list.
    /// </summary>
    /// <param name="clickEventInfo">Contains information about the button click event.</param>
    void HandleSavePlayerProfileButtonClicked(ClickEvent clickEventInfo)
    {
        LocalGameData.Instance.AddPlayerProfile(_currentlyEditedPlayerProfile);
    }

    /// <summary>
    /// Handler for the situation where a different player profile's name is selected in the dropdown menu.
    /// Changes the customization screen to display the newly selected profile.
    /// </summary>
    /// <param name="changeEventInfo">Contains information about the dropdown selection change event.</param>
    void HandleSelectedPlayerProfileDropdownChanged(ChangeEvent<string> changeEventInfo)
    {
        DropdownField profileSelectDropdownField = _UIDocumentComponent.rootVisualElement.Query<DropdownField>("ProfileSelectDropdownField");

        // Stores a copy of the profile to be customized: the one with the same index in the LocalGameData's profiles list.
        // If there isn't a profile available at the dropdown's index, a blank profile (new) is stored instead.
        if (LocalGameData.Instance.PlayerProfiles.Count > profileSelectDropdownField.index && profileSelectDropdownField.index >= 0)
            _currentlyEditedPlayerProfile = LocalGameData.Instance.PlayerProfiles[profileSelectDropdownField.index];
        else
            _currentlyEditedPlayerProfile = new Player();

        // Changes the UI to display the new profile's customization choices.
        ConstructPlayerProfileEditUI();
    }

    /// <summary>
    /// Handles the situation where the button to finalize the addition of a new player profile is clicked.
    /// Tells the LocalGameData to add a player with the selected name.
    /// </summary>
    /// <param name="clickEventinfo">Contains information about the button's click event.</param>
    void HandleConfirmAddNewPlayerButtonClicked(ClickEvent clickEventinfo)
    {
        TextField newProfileNameTextField = _UIDocumentComponent.rootVisualElement.Query<TextField>("NewPlayerNameTextField");
        Player newPlayer = new Player {PlayerName = newProfileNameTextField.value};
        LocalGameData.Instance.AddPlayerProfile(newPlayer);
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
        LocalGameData.Instance.PlayerProfilesListChanged -= ConstructPlayerProfilesListUI;
    }
}
