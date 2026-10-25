using System.Collections.Generic; 
using UnityEngine; // allous us to use unity features.

public class CourseStrokeTracker : MonoBehaviour
{
 
    private readonly List<int> holeStrokes = new List<int>();    // stores the stroke count for each individual hole

   
    private int currentHole = 1; // keeps track of each current hole 

    
    public int CurrentHole => currentHole; 

    
    public int CurrentHoleStrokes =>
        GetStrokesForHole(currentHole); // retrives the stroke count for the current hole

    
    public int TotalStrokes // calculates all the strokes for every hole
    {
        get
        {
            int total = 0; // starts  total at zero.

            
            foreach (int strokes in holeStrokes) // this will add the strokes from all the holes.
            {
                total += strokes;
            }

            return total; // returns  total strokes
        }
    }

    
    public void StartCourse(int numberOfHoles) // starts the new course and resets all of the stroke counted.
    {
        holeStrokes.Clear(); // deletes the previous stroke counts.

        
        for (int i = 0; i < numberOfHoles; i++) // makes a stroke counter for all the holes
        {
            holeStrokes.Add(0); // Starts each hole at zero strokes.
        }

        currentHole = 1; // the current hole is set to Hole 1.
    }

    
    public void RegisterStroke()
    {
        
        if (currentHole < 1 ||
            currentHole > holeStrokes.Count) // is the hole valid?
        {
            Debug.LogWarning("No valid hole is selected."); // will display a warning 
            return; 
        }

        holeStrokes[currentHole - 1]++; // this will add a stroke to the current hole 
    }

   
    public void SetCurrentHole(int holeNumber)
    {
        
        if (holeNumber < 1 ||
            holeNumber > holeStrokes.Count)  
        {
            Debug.LogWarning("Invalid hole number."); // will display a warning
            return; 
        }

        currentHole = holeNumber; // this will update the current hole
    }

    
    public int GetStrokesForHole(int holeNumber)
    {
       
        if (holeNumber < 1 ||
            holeNumber > holeStrokes.Count)
        {
            return 0; // this will return to  zero if the hole is invalid.
        }

        return holeStrokes[holeNumber - 1]; // this will return that hole's strokes
    }
}
