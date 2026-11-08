
using UnityEngine;

public class ExtremeCourseHazardManager : MonoBehaviour
{
    [Header("Hazard Groups")]
    [SerializeField] private GameObject[] movingHazards;
    [SerializeField] private GameObject[] waterHazards;
    [SerializeField] private GameObject[] outOfBoundsHazards;
    [SerializeField] private GameObject[] otherHazards;

    [Header("Course Settings")]
    [SerializeField] private Transform holeGoal;
    [SerializeField] private Transform playerSpawn;

    
    private void Start() // checks whether the extreme course has all required hazards
    {
        ValidateHazards();
    }

    
    private void ValidateHazards()  // checks that each hazard group has been assigned
    {
        CheckHazardGroup(movingHazards, "moving hazards");
        CheckHazardGroup(waterHazards, "water hazards");
        CheckHazardGroup(outOfBoundsHazards, "out of bounds hazards");
        CheckHazardGroup(otherHazards, "other hazards");

        if (holeGoal == null)
            Debug.LogWarning("extreme course goal is not assigned", this);

        if (playerSpawn == null)
            Debug.LogWarning("extreme course spawn is not assigned", this);
    }

    
    private void CheckHazardGroup(GameObject[] hazards, string groupName)// checks whether a hazard group contains valid objects
    {
        if (hazards == null || hazards.Length == 0)
        {
            Debug.LogWarning(groupName + " have not been assigned", this);
            return;
        }

        foreach (GameObject hazard in hazards)
        {
            if (hazard == null)
            {
                Debug.LogWarning("missing hazard in " + groupName, this);
            }
            else if (!hazard.activeInHierarchy)
            {
                Debug.LogWarning(hazard.name + " is inactive", hazard);
            }
        }
    }

   
    public int GetTotalHazardCount() // returns the total number of assigned hazards
    {
        return CountHazards(movingHazards)
             + CountHazards(waterHazards)
             + CountHazards(outOfBoundsHazards)
             + CountHazards(otherHazards);
    }

    
    private int CountHazards(GameObject[] hazards) // counts valid hazards in a group
    {
        if (hazards == null)
            return 0;

        int count = 0;

        foreach (GameObject hazard in hazards)
        {
            if (hazard != null)
                count++;
        }

        return count;
    }
}

