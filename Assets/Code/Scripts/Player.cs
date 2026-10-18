using System;
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
    public String PlayerName;

    // The player's score on the current hole.
    public float Score;

    // Later, a reference to a ScriptableObject with cosmetic choices.
    // Maybe a reference to a settings object for player-specific settings.

    /// <summary>
    /// Constructor. Only sets the name for now.
    /// </summary>
    /// <param name="playerName">The name of the new player.</param>
    public Player(String playerName)
    {
        PlayerName = playerName;
    }
}
