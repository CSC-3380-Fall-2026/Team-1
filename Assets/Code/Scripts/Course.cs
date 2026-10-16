using TextMateSharp.Themes;

/// <summary>
/// An aggregator of holes and information relevant to them.
/// </summary>
public class Course
{
    // The list of holes this course contains.
    public Hole[] Holes;

    // The difficulty level of the course. Higher is harder.
    public int Difficulty;
}
