using UnityEditor;
using UnityEngine;

/// <summary>
/// Contains information about a hole in a course.
/// </summary>
[CreateAssetMenu(fileName = "Hole", menuName = "ChronoShot/Hole")]
public class Hole : ScriptableObject
{
    // The scene containing the layout of the hole.
    public SceneAsset Map;

    // The baseline score value that is decreased by the factor amount per shot.
    public float BaseScore = 100000f;

    // The factor by which the base score is decreased per shot.
    public float ShotCountScoreFactor = 0.9f;
}
