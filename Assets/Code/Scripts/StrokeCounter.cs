using UnityEngine; 

/// <summary>
/// Tracks the number of valid strokes made by a player and proivdes access
/// to the current stroke count for other game systems and UI
///
/// </summary>
public class StrokeCounter : MonoBehaviour
{
    /// <summary>
    /// The player's current number of strokes. This can be read by other
    /// game systems, but only modified by the StrokeCounter
    /// </summary>
    public  int StrokeCount
    {
        get; 
        private set;
    }

    /// <summary>
    /// Adds one stroke to the player's current stroke count. This
    /// should be called whenever the plauyer performs a valid golf ball
    /// shot
    /// </summary>
    public void AddStroke()
    {
        StrokeCount++;
    }


    /// <summary>
    /// Resets the player's stroke count to zero
    /// </summary>
    public void ResetStrokeCount()
    {
        StrokeCount = 0;
    }
}