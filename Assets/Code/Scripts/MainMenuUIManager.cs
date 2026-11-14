using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

/// <summary>
/// This function handles displaying the main menu's UI and assigning behavior to UI elements.
/// </summary>
public class MainMenuUIManager : MonoBehaviour
{
    // A reference to the UXML file containing the UI of the first menu displayed in the game.
    [SerializeField, Tooltip("The UXML file containing the initial menu's UI.")]
    VisualTreeAsset _mainMenuUIAsset;

    // A reference to the UI Document component on the main menu's UI manager.
    // Changing the source asset changes the UI displayed.
    UIDocument _UIDocument;

    /// <summary>
    /// This function is called after all Awake() calls on all gameObjects initially in the scene.
    /// Stores a reference to the UI Document component and shows the main menu.
    /// </summary>
    void Start()
    {
        // A reference to the UI Document component is needed, as it is what actually displays the UI.
        _UIDocument = GetComponent<UIDocument>();

        // The main menu should be the first UI element displayed upon entering this scene.
        ShowMainMenu();
    }

    /// <summary>
    /// Handles displaying the main menu's UI.
    /// Sets relevant click event handlers and disables play buttons based on player profile availability.
    /// </summary>
    void ShowMainMenu()
    {
        // Removes any click event handlers registered to any buttons in the previously shown UI.
        UnregisterButtonClickEventHandlers();

        // Tells the UI document component to display the main menu's UI.
        _UIDocument.visualTreeAsset = _mainMenuUIAsset;

        // Each button needs to be pointed at a function to execute when it is clicked.
        Button singlePlayerButton = _UIDocument.rootVisualElement.Query<Button>("SingleplayerButton");
        singlePlayerButton.RegisterCallback<ClickEvent>(HandleSingleplayerButtonClicked);

        Button hostLANGameButton = _UIDocument.rootVisualElement.Query<Button>("HostLANGameButton");
        hostLANGameButton.RegisterCallback<ClickEvent>(HandleHostLANGameButtonClicked);

        Button joinLANGameButton = _UIDocument.rootVisualElement.Query<Button>("JoinLANGameButton");
        joinLANGameButton.RegisterCallback<ClickEvent>(HandleJoinLANGameButtonClicked);

        Button managePlayerProfilesButton = _UIDocument.rootVisualElement.Query<Button>("ManagePlayerProfilesButton");
        managePlayerProfilesButton.RegisterCallback<ClickEvent>(HandleManagePlayerProfilesButtonClicked);

        Button settingsButton = _UIDocument.rootVisualElement.Query<Button>("SettingsButton");
        settingsButton.RegisterCallback<ClickEvent>(HandleSettingsButtonClicked);

        Button quitButton = _UIDocument.rootVisualElement.Query<Button>("QuitButton");
        quitButton.RegisterCallback<ClickEvent>(HandleQuitButtonClicked);

        // Starting or joining a game should only be allowed if at least one player profile has been made.
        singlePlayerButton.SetEnabled(LocalGameData.Instance.PlayerProfiles.Count > 0);
        hostLANGameButton.SetEnabled(LocalGameData.Instance.PlayerProfiles.Count > 0);
        joinLANGameButton.SetEnabled(LocalGameData.Instance.PlayerProfiles.Count > 0);
    }

    void HandleSingleplayerButtonClicked(ClickEvent clickEventInfo)
    {
        
    }

    void HandleHostLANGameButtonClicked(ClickEvent clickEventInfo)
    {
        
    }

    void HandleJoinLANGameButtonClicked(ClickEvent clickEventInfo)
    {
        
    }

    void HandleManagePlayerProfilesButtonClicked(ClickEvent clickEventInfo)
    {
        
    }

    void HandleSettingsButtonClicked(ClickEvent clickEventInfo)
    {
        
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
    /// Unsubscribes from the on-click events of all buttons in the currently displayed UI window.
    /// </summary>
    void UnregisterButtonClickEventHandlers()
    {
        if (_UIDocument == null || _UIDocument.rootVisualElement == null)
            return;

        foreach (Button button in _UIDocument.rootVisualElement.Query<Button>().ToList())
            button.UnregisterAllRemovableCallbacks();
    }

    /// <summary>
    /// This function is called just before the GameObject is destroyed (e.g. when switching scenes).
    /// Unsubscribes the UI buttons' click events handlers.
    /// </summary>
    void OnDestroy()
    {
        UnregisterButtonClickEventHandlers();
    }
}
