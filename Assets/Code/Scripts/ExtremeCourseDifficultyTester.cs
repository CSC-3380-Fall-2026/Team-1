
using UnityEngine;

public class ExtremeCourseDifficultyTester : MonoBehaviour
{
    [Header("Course Settings")]
    [SerializeField] private int totalHoles = 3;

    [Header("Playtest Statistics")]
    [SerializeField] private int completedHoles = 0;
    [SerializeField] private int failedShots = 0;
    [SerializeField] private int ballResets = 0;
    [SerializeField] private int physicsIssues = 0;

    [Header("Difficulty Settings")]
    [SerializeField] private float minimumLaunchForce = 2f;
    [SerializeField] private float maximumLaunchForce = 15f;

    
    private bool[] holeCompletionStatus; // will track whether each hole has been completed during testing

    
    private void Start() // will initializes the testing information when each of the course starts
    {
        holeCompletionStatus = new bool[totalHoles];

        Debug.Log("extreme course difficulty testing started");
    }

    
    public void CompleteHole(int holeNumber) // will record when a player successfully completes a hole
    {
        if (!IsValidHole(holeNumber))
        {
            return;
        }

        if (holeCompletionStatus[holeNumber - 1])
        {
            return;
        }

        holeCompletionStatus[holeNumber - 1] = true;
        completedHoles++;

        Debug.Log("extreme course hole " + holeNumber + " completed");

        
        if (completedHoles >= totalHoles) // checks for  whether all three holes have been completed
        {
            PrintTestResults();
        }
    }

    
    public void RecordFailedShot() // records all of the failed shots so difficult sections can be identified
    {
        failedShots++;

        Debug.Log("failed shot recorded");
    }

    
    public void RecordBallReset() // will record when the golf ball gets reset after entering a hazard
    {
        ballResets++;

        Debug.Log("ball reset recorded");
    }

    
    public void RecordPhysicsIssue()
    {
        physicsIssues++;

        Debug.LogWarning("physics issue recorded during extreme course testing");
    }

   
    public bool IsLaunchForceValid(float launchForce)
    {
        return launchForce >= minimumLaunchForce &&
               launchForce <= maximumLaunchForce;
    }

    
    private bool IsValidHole(int holeNumber) // verifies that the requested hole number even exists
    {
        return holeNumber >= 1 && holeNumber <= totalHoles;
    }

    
    private void PrintTestResults() // will display the collected playtesting information in the unity console
    {
        Debug.Log("===== extreme course test results =====");
        Debug.Log("completed holes: " + completedHoles);
        Debug.Log("failed shots: " + failedShots);
        Debug.Log("ball resets: " + ballResets);
        Debug.Log("physics issues: " + physicsIssues);

        
        if (completedHoles == totalHoles && physicsIssues == 0) // report whether the entire course was successfully completed
        {
            Debug.Log("extreme course testing completed successfully");
        }
        else
        {
            Debug.LogWarning("extreme course needs additional testing");
        }
    }
}

