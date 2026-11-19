using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Handles displaying and adding functionality to the post-game UI.
/// </summary>
public class PostGameUIManager : MonoBehaviour
{
    // The UXML file containing the post game UI.
    // Loaded at the start of the scene and stays unchanged.
    [SerializeField, Tooltip("The UXML file containing the post game UI.")]
    VisualTreeAsset postGameUIAsset;

    // A reference to the component that actually displays the UXML file on the screen.
    UIDocument _UIDocumentComponent;

    /// <summary>
    /// This function is called when the object is instantiated, after all Awake() calls.
    /// Gets a reference to the UI document component, registers callbacks and initializes the UI.
    /// </summary>
    void Start()
    {
        // This displays the post-game UI on the screen.
        _UIDocumentComponent = GetComponent<UIDocument>();
        _UIDocumentComponent.visualTreeAsset = postGameUIAsset;

        // Links the main menu button to its handler function.
        Button mainMenuButton = _UIDocumentComponent.rootVisualElement.Query<Button>("MainMenuButton");
        mainMenuButton.UnregisterCallback<ClickEvent>(HandleMainMenuButtonClicked);
        mainMenuButton.RegisterCallback<ClickEvent>(HandleMainMenuButtonClicked);

        // Links the results upload button to its handler function and initializes its accessibility based on host/client status.
        Button uploadToLeaderboardButton = _UIDocumentComponent.rootVisualElement.Query<Button>("SendToLeaderboardButton");
        if (NetworkManager.Singleton.IsHost)
        {
            uploadToLeaderboardButton.UnregisterCallback<ClickEvent>(HandleUploadToLeaderboardButtonClicked);
            uploadToLeaderboardButton.RegisterCallback<ClickEvent>(HandleUploadToLeaderboardButtonClicked);
        } else
            uploadToLeaderboardButton.SetEnabled(false);

        // links the hole selection dropdown with its value-change-handler function.
        DropdownField holeSelectDropdownField = _UIDocumentComponent.rootVisualElement.Query<DropdownField>("HoleSelectDropdownField");
        holeSelectDropdownField.UnregisterCallback<ChangeEvent<string>>(HandleHoleSelectionChanged);
        holeSelectDropdownField.RegisterCallback<ChangeEvent<string>>(HandleHoleSelectionChanged);

        // Populates the UI with relevant data.
        ConstructCourseResultsUI();
        ConstructHoleSelectUI();
    }

    /// <summary>
    /// Adds the cumulated player score stats to the UI.
    /// </summary>
    void ConstructCourseResultsUI()
    {
        // When the course reaches its end, the GameMaster inserts the list of cumulative course stats as the first element on the records list.
        List<PlayerResult> cumulativeCourseResults = GameMaster.Instance.PlayerResultRecords[0];

        // By setting the list as the source, added records will be automatically populated with the data at the record's index.
        MultiColumnListView courseResultsColumnListView = _UIDocumentComponent.rootVisualElement.Query<MultiColumnListView>("CourseResultsColumnListView");
        courseResultsColumnListView.itemsSource = cumulativeCourseResults;

        // Constructs the columns by pointing the fields at the appropriate values.
        courseResultsColumnListView.columns["PlayerName"].makeCell = () => new Label();
        courseResultsColumnListView.columns["PlayerName"].bindCell = (VisualElement associatedElement, int index) =>
        {
            PlayerResult playerResult = (PlayerResult)courseResultsColumnListView.itemsSource[index];
            ((Label)associatedElement).text = playerResult.PlayerName;
        };

        courseResultsColumnListView.columns["PlayerScore"].makeCell = () => new Label();
        courseResultsColumnListView.columns["PlayerScore"].bindCell = (VisualElement associatedElement, int index) =>
        {
            PlayerResult playerResult = (PlayerResult)courseResultsColumnListView.itemsSource[index];
            ((Label)associatedElement).text = playerResult.PlayerScore.ToString();
        };

        courseResultsColumnListView.columns["PlayerHits"].makeCell = () => new Label();
        courseResultsColumnListView.columns["PlayerHits"].bindCell = (VisualElement associatedElement, int index) =>
        {
            PlayerResult playerResult = (PlayerResult)courseResultsColumnListView.itemsSource[index];
            ((Label)associatedElement).text = playerResult.PlayerHits.ToString();
        };

        courseResultsColumnListView.columns["PlayerTimeSpent"].makeCell = () => new Label();
        courseResultsColumnListView.columns["PlayerTimeSpent"].bindCell = (VisualElement associatedElement, int index) =>
        {
            PlayerResult playerResult = (PlayerResult)courseResultsColumnListView.itemsSource[index];

            // Displays the time in a '#m #s' format.
            string timeString = $"{(int)(playerResult.PlayerTimeSpent / 60)}m {(int)(playerResult.PlayerTimeSpent % 60)}s";

            ((Label)associatedElement).text = timeString;
        };
    }

    /// <summary>
    /// Adds the player stats for a given hole to the UI.
    /// </summary>
    /// <param name="holeNumber">The index of the hole, starting at 1 for hole 1, etc.</param>
    void ConstructHoleResultsUI(int holeNumber)
    {
        // Retrieves the list of results for the particular hole.
        List<PlayerResult> holeResults = GameMaster.Instance.PlayerResultRecords[holeNumber];

        // By setting the list as the source, added records will be automatically populated with the data at the record's index.
        MultiColumnListView holeResultsColumnListView = _UIDocumentComponent.rootVisualElement.Query<MultiColumnListView>("HoleResultsColumnListView");
        holeResultsColumnListView.itemsSource = holeResults;

        // Constructs the columns by pointing the fields at the appropriate values.
        holeResultsColumnListView.columns["PlayerName"].makeCell = () => new Label();
        holeResultsColumnListView.columns["PlayerName"].bindCell = (VisualElement associatedElement, int index) =>
        {
            PlayerResult playerResult = (PlayerResult)holeResultsColumnListView.itemsSource[index];
            ((Label)associatedElement).text = playerResult.PlayerName;
        };

        holeResultsColumnListView.columns["PlayerScore"].makeCell = () => new Label();
        holeResultsColumnListView.columns["PlayerScore"].bindCell = (VisualElement associatedElement, int index) =>
        {
            PlayerResult playerResult = (PlayerResult)holeResultsColumnListView.itemsSource[index];
            ((Label)associatedElement).text = playerResult.PlayerScore.ToString();
        };

        holeResultsColumnListView.columns["PlayerHits"].makeCell = () => new Label();
        holeResultsColumnListView.columns["PlayerHits"].bindCell = (VisualElement associatedElement, int index) =>
        {
            PlayerResult playerResult = (PlayerResult)holeResultsColumnListView.itemsSource[index];
            ((Label)associatedElement).text = playerResult.PlayerHits.ToString();
        };

        holeResultsColumnListView.columns["PlayerTimeSpent"].makeCell = () => new Label();
        holeResultsColumnListView.columns["PlayerTimeSpent"].bindCell = (VisualElement associatedElement, int index) =>
        {
            PlayerResult playerResult = (PlayerResult)holeResultsColumnListView.itemsSource[index];

            // Displays the time in a '#m #s' format.
            string timeString = $"{(int)(playerResult.PlayerTimeSpent / 60)}m {(int)(playerResult.PlayerTimeSpent % 60)}s";

            ((Label)associatedElement).text = timeString;
        };
    }

    /// <summary>
    /// Sets up the hole selection dropdown menu by filling it with hole numbers.
    /// </summary>
    void ConstructHoleSelectUI()
    {
        // If the choices list isn't cleared first, refreshes will add more rather than rebuilding.
        DropdownField holeSelectDropdownField = _UIDocumentComponent.rootVisualElement.Query<DropdownField>("HoleSelectDropdownField");
        holeSelectDropdownField.choices.Clear();

        // The first index in the Player Result Records list is the cumulative score stats, not hole 0.
        // Therefore, the actual hole count is the records' count - 1.
        int holeCount = GameMaster.Instance.PlayerResultRecords.Count - 1;

        // Populates the dropdown with choices based on the number of holes.
        for (int i = 1; i < holeCount + 1; i++)
            holeSelectDropdownField.choices.Add($"Hole {i}");

        // Selects the first hole initially.
        holeSelectDropdownField.index = 0;

        // Changing the hole selected via the UI would trigger the handler, but via code (as above) does not.
        // Therefore, the handler must be manually called.
        HandleHoleSelectionChanged(default);
    }

    /// <summary>
    /// Handles the event raised when a new value is selected in the hole selection dropdown menu.
    /// Reconstructs the per-hole stats table with data about the selected hole.
    /// </summary>
    /// <param name="changeEventInfo">Contains information about the change event.</param>
    void HandleHoleSelectionChanged(ChangeEvent<string> changeEventInfo)
    {
        DropdownField holeSelectDropdownField = _UIDocumentComponent.rootVisualElement.Query<DropdownField>("HoleSelectDropdownField");
        ConstructHoleResultsUI(holeSelectDropdownField.index + 1);
    }

    /// <summary>
    /// Handles the click event raised when the 'save to leaderboard' button is clicked.
    /// Tells the LeaderboardManager to upload the results stored in the GameMaster to the leaderboard.
    /// </summary>
    /// <param name="clickEventInfo">Contains information about the click event.</param>
    void HandleUploadToLeaderboardButtonClicked(ClickEvent clickEventInfo)
    {
        LeaderboardManager.Instance.SendResultsToLeaderboard(GameMaster.Instance.PlayerResultRecords, GameMaster.Instance.CurrentCourseName.Value.ToString());

        // Only want to allow the results to be sent once, so the button is disabled when it is first clicked.
        Button uploadToLeaderboardButton = _UIDocumentComponent.rootVisualElement.Query<Button>("SendToLeaderboardButton");
        uploadToLeaderboardButton.SetEnabled(false);
    }

    /// <summary>
    /// Handles the click event raised when the 'main menu' button is clicked.
    /// Removes gametime-specific scene-persistent objects and returns to the main menu.
    /// </summary>
    /// <param name="clickEventInfo">Contains information about the click event.</param>
    void HandleMainMenuButtonClicked(ClickEvent clickEventInfo)
    {
        // The game is over and the lobby was not continued, so the client should disconnect and the host should stop hosting.
        NetworkManager.Singleton.Shutdown();

        // The GameMaster is the only scene-persistent object that needs to be destroyed when resetting.
        Destroy(GameMaster.Instance.gameObject);

        // Sends the player back to the main menu.
        SceneManager.LoadScene("MainMenu");
    }
}
