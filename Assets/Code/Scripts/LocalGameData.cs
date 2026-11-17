using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
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
    public List<Player> PlayerProfiles {private set; get;}

    // Other classes, like the 'Manage Player Profiles' related UI, need to perform
    // actions (like reconstructing the UI) when the list of player profiles changes.
    public delegate void PlayerProfilesListChangedEvent();
    public PlayerProfilesListChangedEvent PlayerProfilesListChanged;

    // A unique identifier that will be generated when the application is opened.
    // Used to determine if a client is reconnecting (GUID match) or a new player.
    public string GUID;

    // This list will contain all the courses in the game.
    [Tooltip("A list of all the courses in the game.")]
    public Course[] Courses;

    /// <summary>
    /// This function is called when the gameObject is first instantiated.
    /// Handles singleton logic, sets the GUID and loads player profiles from the disk.
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

        // The GUID will be a unique identifier for the lifetime of the game window.
        GUID = System.Guid.NewGuid().ToString();

        // Really only needs to be done on startup, so here. This object will exist the entire game's lifespan.
        LoadAllPlayerProfilesFromDisk();
    }

    /// <summary>
    /// This function updates existing player profiles in the PlayerProfiles list or adds a new one if none
    /// with the same name exist.
    /// </summary>
    /// <param name="newPlayerProfile">The Player object containing information about the new/updated player.</param>
    public void AddPlayerProfile(Player newPlayerProfile)
    {
        // This function should update a profile if it exists and create a new one if not.
        // It would be easier to check using PlayerProfiles.Contains(newPlayerProfile), but what if both have
        // the same name but different score values stored? Have to check manually to use the name as the identifier.
        int indexOfProfileWithSameName = -1;

        for (int i = 0; i < PlayerProfiles.Count; i++)
        {
            if (PlayerProfiles[i].PlayerName == newPlayerProfile.PlayerName)
            {
                indexOfProfileWithSameName = i;
                break;
            }
        }

        // If the index remaind at -1, then no profile with the same name was found. If not, one was found.
        if (indexOfProfileWithSameName != -1)
        {
            Debug.Log("Found player with same name. Overwriting.");
            PlayerProfiles[indexOfProfileWithSameName] = newPlayerProfile;
        } else
        {
            Debug.Log("Added new player profile.");
            PlayerProfiles.Add(newPlayerProfile);
        }

        // Calls all other functions interested in performing actions when the list of player profiles is changed.
        PlayerProfilesListChanged?.Invoke();

        // It would be more efficient to save player profiles when leaving the 'Manage Player Profiles' window.
        SaveAllPlayerProfilesToDisk();
    }

    /// <summary>
    /// This function removes a player profile from the PlayerProfiles list if the index supplied is valid.
    /// </summary>
    /// <param name="indexOfProfileInProfilesList">The index of the player profile in the PlayerProfiles list to be removed.</param>
    public void RemovePlayerProfile(int indexOfProfileInProfilesList)
    {
        // Bounds checking the index supplied.
        if (indexOfProfileInProfilesList < 0 || indexOfProfileInProfilesList >- PlayerProfiles.Count)
        {
            Debug.LogError($"Tried to reference out-of-range index: {indexOfProfileInProfilesList}.");
            return;
        }

        // No other checks needed after bounds setting.
        PlayerProfiles.RemoveAt(indexOfProfileInProfilesList);

        // It would be more efficient to save player profiles when leaving the 'Manage Player Profiles' window.
        SaveAllPlayerProfilesToDisk();
    }

    /// <summary>
    /// Uses PlayerPrefs to store information about the supplied player in the disk.
    /// Each Player variable's value is stored using "<PlayerName>.<VariableName>" as the key.
    /// </summary>
    /// <param name="playerProfileToSave">The Player object containing the information to store in PlayerPrefs.</param>
    void SavePlayerProfileToDisk(Player playerProfileToSave)
    {
        // The keys for each player profile should look like "<PlayerName>.<VariableName>"
        // For example, the PlayerName value would be "'Test Player 1'.PlayerName"
        // The prefix therefore is just the player's name.
        string prefix = playerProfileToSave.PlayerName.ToString() + ".";

        // Every variable should be added in the same format as the player name is below.
        string playerNameKey = prefix + "PlayerName";
        string playerNameValue = playerProfileToSave.PlayerName.ToString();
        PlayerPrefs.SetString(playerNameKey, playerNameValue);

        // When loading all player profiles, a list of all the keys containing player information is needed.
        // However, it isn't possible to return a list of all keys containing a string.
        // If it was, each profile could have been saved like "SavedProfile.<PlayerName>.<VariableName>" and all
        // keys with staring with "SavedProfile" would be identifiable is a profile.
        // To get around that limitation, a string containing a list of saved player names would be maintained and searched.
        string previousListOfSavedProfileNames = PlayerPrefs.GetString("SavedPlayerProfileNames");
        if (!previousListOfSavedProfileNames.Split(",").Contains(playerNameValue))
        {
            // Don't want to start a comma-separated list with a comma, as it could make the first value (when separated) an empty string.
            string newListOfSavedProfileNames = previousListOfSavedProfileNames == string.Empty ? playerNameValue : $"{previousListOfSavedProfileNames},{playerNameValue}";
            PlayerPrefs.SetString("SavedPlayerProfileNames", newListOfSavedProfileNames);
        }
    }

    /// <summary>
    /// Saves all player profiles in the PlayerProfiles list to the disk.
    /// </summary>
    void SaveAllPlayerProfilesToDisk()
    {
        // To account for the possibility that a profile has been removed, as doing so doesn't
        // remove the name from the list of saved player names.
        PlayerPrefs.SetString("SavedPlayerProfileNames", string.Empty);

        foreach (Player playerProfile in PlayerProfiles)
            SavePlayerProfileToDisk(playerProfile);
    }

    /// <summary>
    /// Gets information about a player from PlayerPrefs using its name as a reference.
    /// </summary>
    /// <param name="playerProfileName">The name of the player whose information should be retrieved.</param>
    /// <returns>A Player object containing the retrieved data. An empty Player object if retrieval failed.</returns>
    Player LoadPlayerProfileFromDisk(string playerProfileName)
    {
        // This is the player object whose information will be replaced with that of the loaded player.
        Player loadedPlayerProfile = new Player();

        // Each saved player profile's information is stored in a "<PlayerName>.<VariableName>" format, so
        // It would be simpler to store the "<PlayerName>" prefix to avoid longer repetition.
        string prefix = playerProfileName + ".";

        // Each loaded variable should be added/checked the same way the player's name is below.
        string playerNameKey = prefix + "PlayerName";

        if (PlayerPrefs.HasKey(playerNameKey))
            loadedPlayerProfile.PlayerName = PlayerPrefs.GetString(playerNameKey);
        else
        {
            Debug.LogError("Couldn't find a player profile with that name.");

            // Returning an empty profile should be taken as an indicator that there was a problem
            // encountered when trying to load the profile.
            loadedPlayerProfile = new Player();
        }

        return loadedPlayerProfile;
    }

    /// <summary>
    /// Adds the player profiles stored via PlayerPrefs to the PlayerProfiles list.
    /// </summary>
    /// <param name="emptyPlayerProfilesListBeforehand">Clears the PlayerProfiles list before loading new ones from the disk if true.</param>
    void LoadAllPlayerProfilesFromDisk(bool emptyPlayerProfilesListBeforehand = true)
    {
        // Unlikely that there would ever be a need to load player profiles from the disk without clear the current
        // list beforehand, but just in case.
        if (emptyPlayerProfilesListBeforehand)
            PlayerProfiles.Clear();

        // This list is constructed when player profiles are saved.
        // It is a comma-separated list of the names of those player profiles.
        string[] playerProfileNames = PlayerPrefs.GetString("SavedPlayerProfileNames").Split(",");

        // Loads each player based on the names in the previously supplied comma-separated list.
        foreach (string playerProfileName in playerProfileNames)
        {
            Player loadedPlayerProfile = LoadPlayerProfileFromDisk(playerProfileName);

            if (loadedPlayerProfile.PlayerName == default(FixedString64Bytes))
            {
                Debug.LogWarning("Failed to load a player. Skipped.");
                continue;
            }

            PlayerProfiles.Add(loadedPlayerProfile);
        }

        // Calls all other functions interested in performing actions when the list of player profiles is changed.
        PlayerProfilesListChanged?.Invoke();
    }
}