using UnityEngine;

/// <summary>
/// Contains information about an individual player.
/// </summary>
/// <remarks>
/// Will contain the player's customization choices.
/// </remarks>
[CreateAssetMenu(fileName = "New Player", menuName = "ChronoShot/Player")]
public class Player : ScriptableObject
{
    // The player's name.
    public string PlayerName;

    public ulong PlayerClientID;

    public string PlayerGUID;

    // The player's score on the current hole.
    public float Score;

    // Later, a reference to a ScriptableObject with cosmetic choices.
    // Maybe a reference to a settings object for player-specific settings.

    /// <summary>
    /// Constructor. Only sets the name for now.
    /// </summary>
    /// <param name="playerName">The name of the new player.</param>
    public Player(string playerName, ulong playerClientID, string playerGUID)
    {
        PlayerName = playerName;
        PlayerClientID = playerClientID;
        PlayerGUID = playerGUID;
    }
}
