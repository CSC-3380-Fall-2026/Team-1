using System.Collections.Generic;


public static class CourseStrokeTracker // stores the player's stroke counts across all of the holes in a course
{
    
    private static Dictionary<int, Dictionary<int, int>> strokesByHole = // stores the stroke count using the hole number and player ID
        new Dictionary<int, Dictionary<int, int>>();


    public static bool IsCourseStarted { get; private set; } // tracks wheter a course has already started or not 

    
    public static int CurrentHoleIndex { get; private set; } // tracks the current hole across scene changes

    public static void StartNewCourse() // starts a new course and clears all of the previous stroke records
    {
        strokesByHole.Clear();
        CurrentHoleIndex = 0;
        IsCourseStarted = true;
    }

    public static void SetCurrentHoleIndex(int holeIndex) // updates the current hole index
    {
        if (holeIndex < 0)
            return;

        CurrentHoleIndex = holeIndex;
    }

    public static void RecordStroke(int holeIndex, int playerID) // records one of the strokes for a player on a speific hoel
    {
        if (holeIndex < 0 || playerID < 0)
            return;

        if (!IsCourseStarted)
            StartNewCourse();

        if (!strokesByHole.ContainsKey(holeIndex))
        {
            strokesByHole[holeIndex] = new Dictionary<int, int>();
        }

        if (!strokesByHole[holeIndex].ContainsKey(playerID))
        {
            strokesByHole[holeIndex][playerID] = 0;
        }

        strokesByHole[holeIndex][playerID]++;
    }

    public static int GetHoleStrokeCount(int holeIndex, int playerID) // get's one of the player's stroke on a specific hole
    {
        if (strokesByHole.ContainsKey(holeIndex) &&
            strokesByHole[holeIndex].ContainsKey(playerID))
        {
            return strokesByHole[holeIndex][playerID];
        }

        return 0;
    }

    public static int GetPlayerTotalStrokes(int playerID) // gets 1 of the player's total stroke across the entire course 
    {
        int total = 0;

        foreach (var hole in strokesByHole)
        {
            if (hole.Value.ContainsKey(playerID))
            {
                total += hole.Value[playerID];
            }
        }

        return total;
    }

    public static int GetTotalStrokes() // get the combined stroke total for all the players and the holes
    {
        int total = 0;

        foreach (var hole in strokesByHole)
        {
            foreach (var player in hole.Value)
            {
                total += player.Value;
            }
        }

        return total;
    }

    public static void EndCourse() // marks the course as "completed" without deleting its stroke records
    {
        IsCourseStarted = false;
    }
}
