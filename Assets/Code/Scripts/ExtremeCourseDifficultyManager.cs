
using UnityEngine;

public class ExtremeCourseDifficultyManager : MonoBehaviour
{
    [Header("Course Physics")]
    [SerializeField] private float ballMass = 1f;
    [SerializeField] private float ballLinearDrag = 0.5f;
    [SerializeField] private float ballAngularDrag = 0.5f;
    [SerializeField] private float gravityScale = 1f;

    [Header("Course References")]
    [SerializeField] private Rigidbody2D[] courseBalls;
    [SerializeField] private Transform[] holeGoals;
    [SerializeField] private Transform[] playerSpawns;
    [SerializeField] private GameObject[] courseHazards;
    [SerializeField] private GameObject[] courseObstacles;

    
    private void Start() // applies extreme course physics to the assigned balls
    {
        ApplyCoursePhysics();
        ValidateCourseSetup();
    }

    
    private void ApplyCoursePhysics() // updates the physics values without changing global project settings
    {
        foreach (Rigidbody2D ball in courseBalls)
        {
            if (ball == null)
                continue;

            ball.mass = ballMass;
            ball.linearDamping = ballLinearDrag;
            ball.angularDamping = ballAngularDrag;
            ball.gravityScale = gravityScale;
        }
    }

    
    private void ValidateCourseSetup() // checks that all three holes have goals and spawn locations
    {
        if (holeGoals == null || holeGoals.Length != 3)
            Debug.LogWarning("extreme course should have three assigned hole goals", this);

        if (playerSpawns == null || playerSpawns.Length != 3)
            Debug.LogWarning("extreme course should have three assigned player spawns", this);

        CheckObjects(courseHazards, "hazards");
        CheckObjects(courseObstacles, "obstacles");

        CheckTransforms(holeGoals, "hole goals");
        CheckTransforms(playerSpawns, "player spawns");
    }

   
    private void CheckObjects(GameObject[] objects, string groupName) //  // checks whether assigned objects exist and are active
    {
        if (objects == null)
            return;

        foreach (GameObject item in objects)
        {
            if (item == null)
            {
                Debug.LogWarning("missing object in " + groupName, this);
            }
            else if (!item.activeInHierarchy)
            {
                Debug.LogWarning(item.name + " is inactive", item);
            }
        }
    }

    
    private void CheckTransforms(Transform[] transforms, string groupName) // // checks whether all required transforms are assigned
    {
        if (transforms == null)
            return;

        foreach (Transform item in transforms)
        {
            if (item == null)
                Debug.LogWarning("missing " + groupName + " assignment", this);
        }
    }

   
    public int GetCourseObjectCount()  // returns the number of assigned hazards and obstacles
    {
        return CountObjects(courseHazards) + CountObjects(courseObstacles);
    }

    
    private int CountObjects(GameObject[] objects) // counts valid objects in an array
    {
        if (objects == null)
            return 0;

        int count = 0;

        foreach (GameObject item in objects)
        {
            if (item != null)
                count++;
        }

        return count;
    }
}

