
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Starting Score")]
    [SerializeField] private float startingScore = 1000f; // sets the players starting score

    [Header("Launch Penalty")]
    [SerializeField] private float launchMultiplier = 0.9f; // reduces the score after each launch

    [Header("Time Penalty")]
    [SerializeField] private float secondsPerPenalty = 10f; // sets how often time penalties occur
    [SerializeField] private float timeMultiplier = 0.9f; 

    [Header("ChronoShot Settings")]
    [SerializeField] private float chronoShotTimeFactor = 1f; 

    private float currentScore; // stores the player's current score
    private float timeTaken; 
    private float penaltyTimer; // tracks time until the next penalty

    private int launchCount; 

    private bool timerStarted; // checks whether the hole timer has started
    private bool holeCompleted; 
    private bool chronoShotActive; 

    public float CurrentScore => currentScore; // provides access to the current score
    public float TimeTaken => timeTaken; // provides access to the total completion time
    public int LaunchCount => launchCount; 

    private void Start()
    {
        ResetScore(); // initializes the score and resets the hole statistics
    }

    private void Update()
    {
        if (!timerStarted || holeCompleted) 
            return;

        float elapsedTime = Time.unscaledDeltaTime; // tracks real time regardless of unity's time scale

        if (chronoShotActive) 
        {
            elapsedTime *= chronoShotTimeFactor; // applies the configured chronoshot time adjustment
        }

        timeTaken += elapsedTime; 
        penaltyTimer += elapsedTime; // tracks time toward the next score penalty

        while (penaltyTimer >= secondsPerPenalty) // checks whether another time penalty is due
        {
            penaltyTimer -= secondsPerPenalty; // resets the penalty interval
            currentScore *= timeMultiplier; // applies the time-based score reduction
        }
    }

    public void RegisterLaunch()
    {
        if (holeCompleted) // prevents launches from counting after completion
            return;

        if (!timerStarted) 
        {
            timerStarted = true; // starts the hole timer
        }

        launchCount++; // adds another launch to the total
        currentScore *= launchMultiplier; // applies the launch score penalty
    }

    public void SetChronoShotActive(bool isActive)
    {
        chronoShotActive = isActive; // updates the current chronoshot state
    }

    public void CompleteHole()
    {
        if (holeCompleted) // prevents the hole from being completed twice
            return;

        holeCompleted = true; 
        timerStarted = false;

        Debug.Log("Hole Completed!"); // displays the completion message
        Debug.Log("Final Score: " + Mathf.RoundToInt(currentScore)); // displays the final score
        Debug.Log("Time Taken: " + timeTaken.ToString("F2") + " seconds"); // displays the completion time
        Debug.Log("Total Launches: " + launchCount); // displays the total number of launches
    }

    public void ResetScore()
    {
        currentScore = startingScore; // restores the starting score

        timeTaken = 0f; 
        penaltyTimer = 0f; // resets the time penalty counter

        launchCount = 0; // clears the previous launch count

        timerStarted = false; 
        holeCompleted = false; 
        chronoShotActive = false; // resets chronoshot to inactive
    }
}
