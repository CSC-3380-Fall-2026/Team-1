using System;

/// <summary>
/// This is a container for only the player information relevant to a player's performance in a course/hole
/// The GameMaster will maintain a list of PlayerResults for each hole in a course and the course in total.
/// </summary>
[Serializable]
public struct PlayerResult
{
    // The name of the player whose results are recorded here.
    public string PlayerName;

    // The score the player attained in the context this result applies to.
    // This class could be used to record the score for a singular hole or the cumulative course total.
    public float PlayerScore;

    // The number of hits made in the context this result applies to.
    // This class could be used to record the launch count for a singular hole or the cumulative course total.
    public int PlayerHits;

    // The amount of time spent in the context this result applies to.
    // This class could be used to record the time spent for a singular hole or the cumulative course total.
    public float PlayerTimeSpent;

    /// <summary>
    /// Creates a player result object using the information from a player.
    /// </summary>
    /// <param name="player">The player whose results will be recorded in the returned record.</param>
    /// <returns>The PlayerResult object containing the player's record.</returns>
    public static PlayerResult FromPlayer(Player player)
    {
        return new PlayerResult
        {
            PlayerName = player.PlayerName.ToString(),
            PlayerScore = player.PlayerScore,
            PlayerHits = player.PlayerHits,
            PlayerTimeSpent = player.PlayerTimeSpent
        };
    }
}