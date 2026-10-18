using UnityEngine;

/// <summary>
/// An aggregator of holes and information relevant to them.
/// </summary>
[CreateAssetMenu(fileName = "New Course", menuName = "ChronoShot/Course")]
public class Course : ScriptableObject
{
    // The list of holes this course contains.
    public Hole[] Holes;

    // The difficulty level of the course. Higher is harder.
    public int Difficulty = 1;
}
