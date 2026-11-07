using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A container of information each client should maintain over the game's lifetime.
/// The information here should be relevant outside of courses as well.
/// For example, settings information and player profiles.
/// </summary>
public class LocalGameData : MonoBehaviour
{
    // A singleton, as only one should exist per client.
    public static LocalGameData Instance {private set; get;}

    // A list of the player profiles stored on this client.
    // Profiles should be made/maintained in a dedicated menu, not here.
    [HideInInspector]
    public List<Player> PlayerProfiles;

    // A unique identifier that will be generated when the application is opened.
    // Used to determine if a client is reconnecting (GUID match) or a new player.
    public string GUID;

    /// <summary>
    /// This function is called when the gameObject is first instantiated.
    /// Handles singleton logic, creates test players and sets the GUID.
    /// </summary>
    void Awake()
    {
        // Singleton handling.
        if (Instance != null && Instance != this)
            Destroy(this);
        else {
            Instance = this;

            // Allows the gameObject to persist between scenes.
            DontDestroyOnLoad(gameObject);
        }

        // In the absence of a UI-controlled way to create new player profiles, a few test ones will be made.
        Player testPlayer1 = new Player();
        Player testPlayer2 = new Player();
        Player testPlayer3 = new Player();
        Player testPlayer4 = new Player();

        testPlayer1.PlayerName = "Test Player 1";
        testPlayer2.PlayerName = "Test Player 2";
        testPlayer3.PlayerName = "Test Player 3";
        testPlayer4.PlayerName = "Test Player 4";

        PlayerProfiles.Add(testPlayer1);
        PlayerProfiles.Add(testPlayer2);
        PlayerProfiles.Add(testPlayer3);
        PlayerProfiles.Add(testPlayer4);

        // The GUID will be a unique identifier for the lifetime of the game window.
        GUID = System.Guid.NewGuid().ToString();
    }
}
