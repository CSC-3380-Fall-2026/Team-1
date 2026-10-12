using UnityEngine;

    public class StokeCounter : MonoBehaviour
{
                                        
             // this stores the current amount of strokes made.
    private int strokeCount = 0;

            
    public int CurrentStrokes => strokeCount;

            // this is called when a golf shot is made
    public void RegisterStroke()
    {
        strokeCount++;
        Debug.Log("Current strokes: " + strokeCount);
    }

             // resets the count 
    public void ResetStrokes()
    {
        strokeCount = 0;
        Debug.Log("Stroke count reset.");
    }
}




