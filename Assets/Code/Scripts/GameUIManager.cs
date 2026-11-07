using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Contains state and behavior related to mid-course UI elements.
/// </summary>
public class GameUIManager : MonoBehaviour
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
    }

    void Start()
    {
        // The functions are not called when first linked to the event, so need a manual call.
        ConstructPlayerList();

        // Subscribes the players list construction function to occur when the players list changes or on new turn.
        GameMaster.Instance.Players.OnListChanged += ConstructPlayerList;
        GameMaster.Instance.CurrentPlayerTurn.OnValueChanged += (oldTurn, newTurn) => {ConstructPlayerList();};
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
            bool playerDisconnected = !NetworkManager.Singleton.ConnectedClients.ContainsKey(GameMaster.Instance.Players[i].PlayerClientID);

            // Constructs the string added to the ListView.
            // Displays each player's ID, name, whether local or remote, whose turn it is and whether the player is disconnected.
            string playerInfo = string.Format("{0}: {1}{2}{3}{4}",
                GameMaster.Instance.Players[i].PlayerClientID,
                GameMaster.Instance.Players[i].PlayerName,
                (GameMaster.Instance.Players[i].PlayerClientID == NetworkManager.Singleton.LocalClientId) ? " (Local)" : " (Remote)",
                (GameMaster.Instance.CurrentPlayerTurn.Value == i) ? " (Turn)" : "",
                playerDisconnected ? " (Disconnected)" : "");

            // Adds the newly constructed string to the previously created list.
            playerInfoList.Add(playerInfo);
        }

        // Tells the ListView to use the created list of player information strings as a source for its elements.
        connectedPlayersListView.itemsSource = playerInfoList;

        // Tells the ListView to recreate its elements with the newly given source.
        connectedPlayersListView.Rebuild();
    }
}