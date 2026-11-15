using System;
using UnityEngine;

/// <summary>
/// An aggregator of holes and information relevant to them.
/// </summary>
[CreateAssetMenu(fileName = "New Course", menuName = "ChronoShot/Course"), Serializable]
public class Course : ScriptableObject
{
    public string CourseName;

    // The list of holes this course contains.
    public Hole[] Holes;

    // A list of images showing off each hole in the course.
    // Will be displayed above its description in the lobby.
    public Texture2D[] CoursePreviewImages;

    // A description of the course. Custom ones will be user-defined.
    public string CourseDescription;

    // The maximum number of players that can be added to the course's player list.
    public int MaxPlayers;

    // The difficulty level of the course. Higher is harder.
    public int Difficulty = 1;
}
