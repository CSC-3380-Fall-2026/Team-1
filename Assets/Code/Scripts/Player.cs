using System;

/// <summary>
/// Contains information about an individual player.
/// </summary>
/// <remarks>
/// Will contain the player's customization choices.
/// </remarks>
public class Player
{
    // The player's name.
    public String Name;

    // The player's score on the current hole.
    public float Score;

    // Later, a reference to a ScriptableObject with cosmetic choices.
    // Maybe a reference to a settings object for player-specific settings.
}
