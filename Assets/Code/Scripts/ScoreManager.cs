using UnityEngine; // Unity features


// Manages score and time.

public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private float startingScore = 1000f; // this will be the starting score
    [SerializeField] private float launchMultiplier = 0.9f; // this is our launch penalty 

    [Header("Time Penalty Settings")]
    [SerializeField] private float secondsPerPenalty = 10f; // this is the penalty interval
    [SerializeField] private float timeMultiplier = 0.9f; // the time penalty

    [Header("ChronoShot Time Settings")]
    [SerializeField] private float chronoShotTimeFactor = 1f; // the chronoshot"s timer speed 

    // scores and timer values
    private float currentScore;
    private float elapsedTime;
    private float penaltyTimer;


    // each of the time states 
    private bool chronoShotActive;
    private bool timerRunning;


    // each of the current values
    public float CurrentScore => currentScore;
    public float ElapsedTime => elapsedTime;

    private void Start()
    {
        ResetScore(); // this will initialize the score
    }

    private void Update()
    {
        if (!timerRunning)
            return; // this will skip if the timer is stopped 

        float deltaTime = Time.unscaledDeltaTime; // used for getting the elapsed frame time

        if (chronoShotActive)
        {
            deltaTime *= Mathf.Max(0f, chronoShotTimeFactor); // 
        }

        elapsedTime += deltaTime; // tracks the  total time
        penaltyTimer += deltaTime; // tracks the penalty time

        if (secondsPerPenalty > 0f)
        {
            while (penaltyTimer >= secondsPerPenalty)
            {
                penaltyTimer -= secondsPerPenalty; 
                currentScore *= timeMultiplier; 
            }
        }
    }

    
    // this will apply a launch penalty.
   
    public void RegisterLaunch()
    {
        currentScore *= launchMultiplier; 
    }


    public void SetChronoShotActive(bool active)
    {
        chronoShotActive = active; // this will update the chronoshots state.
    }

  
    public void StartHoleTimer()
    {
        elapsedTime = 0f; // resets the elsaped time
        penaltyTimer = 0f; // resets the penatly timer
        chronoShotActive = false; //  chronodhot disabled 
        timerRunning = true; 


    }
    
    public void FinishHole()
    {
        if (!timerRunning)
            return; // prevents duplicate 

        timerRunning = false; // stops timer
        chronoShotActive = false; 

        Debug.Log("Hole completed!"); // this will show a "completion" message 
        Debug.Log("Final Score: " + currentScore); // final score will be displayed 
        Debug.Log("Time Taken: " + elapsedTime.ToString("F2") + " seconds"); // this shows the final time.
    }

        public void ResetScore()
    {
        currentScore = startingScore;
        elapsedTime = 0f;
        penaltyTimer = 0f;
        chronoShotActive = false;
        timerRunning = false;
    } 
} 