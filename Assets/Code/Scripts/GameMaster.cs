using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A general controller of game state.
/// Performs general game functions that are not specific enough to be delegated to specialized objects and components.
/// </summary>
public class GameMaster : MonoBehaviour
{
    // A GameMaster singleton. Can only be set from within this script.
    // Any references to GameMaster.instance from anywhere will refer to one singular GameMaster, no need to search for it.
    public static GameMaster Instance
    {
        get;
        private set;
    }

    // For testing purposes, the current hole being played.
    [SerializeField]
    Hole _currentHole;

    // The players in the current game.
    [SerializeField, Tooltip("A list of the players in the current game.")]
    List<Player> _players;

    // The index of the player whose turn it is to perform actions in the players list.
    int _currentPlayerTurn = 0;

    // The balls in the current map. Ball index should match player index.
    List<GameObject> _balls = new List<GameObject>();

     // The ball object to be created at the beginning of the round.
    [SerializeField, Tooltip("The ball object to be created at the beginning of the round.")]
    GameObject _ballPrefab;

    // A list of reset points whose locations will be used as reset points.
    [Tooltip("A list of reset points whose locations will be used as spawn points."), Header("Ball Reset")]
    public GameObject[] SpawnPoints;

    // Objects in these layers will be considered to be obstructing the reset point.
    [Tooltip("Objects in these layers will be considered to be obstructing the reset point.")]
    public LayerMask ResetObstructionLayers;

    [SerializeField, Tooltip("The normal rate of passage of time.\n1.0 = regular\n0.0 = frozen"), Header("Time")]
    float _normalTimeRate = 1.0f;

    /// <summary>
    /// Resets a given ball to a chosen location, possibly also resetting status effects.
    /// </summary>
    /// <remarks>
    /// No status effects currently, but assuming a BallStatus component is added with said behavior, will call its reset function.
    /// </remarks>
    /// <param name="ballToBeReset">A reference to the ball we would like to reset.</param>
    /// <param name="resetLocationObjects">GameObjects whose locations are candidates for the reset.</param>
    /// <param name="removeStatusEffects">Whether or not we will remove any status effects currently affecting the ball.</param>
    /// <param name="startFromIndex">Only spawn points on or after this index on the list will be considered.</param>
    public void ResetBall(GameObject ballToBeReset, GameObject[] resetLocationObjects, bool removeStatusEffects = true, int startFromIndex = 0)
    {
        // When we add a ball status component, reset its status effects.
        if (removeStatusEffects);

        // This will be set to the location of the first free reset location provided.
        Vector2 resetLocation = Vector2.zero;

        // Tracks whether or not any reset location is unoccupied so that the ball can be placed there.
        bool resetLocationClear = false;

        // Makes a subarray of the reset location objects starting from the given index.
        GameObject[] consideredResetLocationObjects = resetLocationObjects[startFromIndex..];

        // Checks if each reset location is unobstructed.
        foreach (GameObject resetLocationObject in consideredResetLocationObjects)
        {
            // Needed to determine if the collider is touching anything on the layers we're checking.
            CircleCollider2D resetLocationObjectCollider = resetLocationObject.GetComponent<CircleCollider2D>();

            // It is possible that the ball's radius is not static (powerups, obstruction effects, etc).
            // Therefore, we must ensure the area we check is that of the ball's area.
            resetLocationObjectCollider.radius = ballToBeReset.GetComponent<CircleCollider2D>().radius;

            // Considers the reset location clear if it is not touching anything on the layers we're checking. Obstructed if otherwise.
            resetLocationClear = !resetLocationObjectCollider.IsTouchingLayers(ResetObstructionLayers);

            // Sets the reset location to that of the first unobstructed reset point we see and stops checking.
            if (resetLocationClear)
            {
                resetLocation = resetLocationObject.transform.position;
                break;
            }
        }

        // Stops the reset process and throws an error if none of the reset points are unobstructed.
        if (!resetLocationClear)
        {
            Debug.LogError("All reset locations are obstructed.");
            return;
        }

        // A reference to the ball's rigidbody is needed to reset any added forces and its velocity.
        Rigidbody2D ballToBeResetRigidbody = ballToBeReset.GetComponent<Rigidbody2D>();

        // Removes any added forces on the ball and any linear or angular velocity, ensuring it will be stationary on the next physics update.
        ballToBeResetRigidbody.totalForce = Vector2.zero;
        ballToBeResetRigidbody.linearVelocity = Vector2.zero;
        ballToBeResetRigidbody.angularVelocity = 0;

        // Sets the ball's position to that of the first unobstructed reset point found.
        ballToBeReset.transform.position = resetLocation;

        // Resets ChronoShot so no time dilation carries over after the ball is reset.
        ResetChronoShot();
    }
    
    /// <summary>
    /// Resets ChronoShot to its default inactive state.
    /// </summary>
    public void ResetChronoShot()
    {
        SetTimeRate();
    }

    /// <summary>
    /// Sets the rate of passage of time to a given percentage of its normal speed.
    /// </summary>
    /// <remarks>
    /// It is possible that each ball's time-slow amount could vary (powerups, obstacles, etc).
    /// To account for that, let whoever calls it (ball) set the rate rather than defining a slow amount variable on the GameMaster.
    /// </remarks>
    /// <param name="newTimeRate">
    /// The new rate at which time will pass.
    /// 1.0f = normal, 0.0f = frozen.
    /// If left empty (or set to -1.0f), will be set to normal value defined in GameMaster.
    /// </param>
    public void SetTimeRate(float newTimeRate = -1.0f)
    {
        // If no value given (or -1.0f) set the rate to our set value (in case we decide on a different default).
        if (newTimeRate == -1.0f)
        {
            Time.timeScale = _normalTimeRate;
            return;
        }

        Time.timeScale = newTimeRate;
    }

    /// <summary>
    /// Calculates a ball's resulting score based on the number of shots made and the current hole's score modifiers.
    /// </summary>
    /// <remarks>
    /// Needs to account for real time taken as well.
    /// </remarks>
    /// <param name="shotCount">The number of shots made before reaching the goal.</param>
    /// <returns>The score calculated given the number of shots and the current hole's values.</returns>
    float CalculateScore(int shotCount)
    {
        return (float)(_currentHole.BaseScore * Mathf.Pow(_currentHole.ShotCountScoreFactor, shotCount));
    }

    /// <summary>
    /// Goes through the list of players and spawns a ball for each one at the given spawn points.
    /// Uses the first unobstructed one found in the list.
    /// </summary>
    /// <remarks>
    /// Every ball after the first will have its movement component disabled: not its turn.
    /// </remarks>
    void SpawnBalls()
    {
        // The following operations occur for each player in the _players list.
        for (int i = 0; i < _players.Count; i++)
        {
            // To prevent constant re-referencing.
            Player player = _players[i];

            // Creates a ball at the origin.
            GameObject playerBall = Instantiate(_ballPrefab);

            // Configures the ball's initial state. Its ID should be its new position in the _balls list.
            BallState playerBallState = playerBall.GetComponent<BallState>();
            playerBallState.BallID = _balls.Count;
            playerBallState.PlayerName = player.PlayerName;

            // Moves the new ball to an unobstructed spawnpoint by the reset function.
            // Every ball will be spawned before the next physics update, so reset cannot check for obstruction.
            // To get around this, spawnpoints obstructed by balls just spawned are manually ignored.
            ResetBall(playerBall, SpawnPoints, true, i);

            // Disables the movement component of every ball that isn't the first on the list.
            BallMovement playerBallMovement = playerBall.GetComponent<BallMovement>();
            playerBallMovement.enabled = playerBallState.BallID == 0;

            // Adds the new ball to the _balls list.
            _balls.Add(playerBall);
        }
    }

    /// <summary>
    /// Handles ending the current player's turn and beginning the next.
    /// Currently disables the movement of the ball ending its turn and enables that of the ball beginning its own.
    /// </summary>
    public void NextTurn()
    {
        // Disables the ball movement component of the ball controlled by the player whose turn is ending.
        BallMovement currentPlayerBallMovementComponent = _balls[_currentPlayerTurn].GetComponent<BallMovement>();
        currentPlayerBallMovementComponent.enabled = false;

        // Tracks whether any active ball was found in the balls list.
        bool anyBallsActive = false;

        // Loops through the balls list once to handle scenario where there is no active ball to switch turn to.
        for (int i = 0; i < _balls.Count; i++)
        {
            // Makes the next player on the list the one whose turn it is. Loops to 0 if end reached.
            _currentPlayerTurn = (_currentPlayerTurn + 1) % _players.Count;

            // Exits the loop if any active ball is found.
            if (_balls[_currentPlayerTurn].activeSelf)
            {
                anyBallsActive = true;
                break;
            }
        }

        // It should never be the case that NextTurn is called while no balls are active.
        if (!anyBallsActive)
            Debug.LogWarning("No balls are active.\nIf not all have reached goal, something is wrong.");


        // Enables the ball movement component of the ball controlled by the player whose turn is beginning.
        BallMovement nextPlayerBallMovementComponent = _balls[_currentPlayerTurn].GetComponent<BallMovement>();
        nextPlayerBallMovementComponent.enabled = true;
    }

    /// <summary>
    /// Called when a ball enters the hole.
    /// Gets relevant information to update its score and disables it.
    /// </summary>
    /// <param name="ballThatEnteredHole">The ball gameObject which just entered the hole.</param>
    public void HandleGoal(GameObject ballThatEnteredHole)
    {
        // Gets the ball's movement component for the launch count and state component for its ID.
        BallMovement ballThatEnteredHoleMovementComponent = ballThatEnteredHole.GetComponent<BallMovement>();
        BallState ballThatEnteredHoleStateComponent = ballThatEnteredHole.GetComponent<BallState>();

        // Calculates the player's score and assigns it to the appropriate player.
        _players[ballThatEnteredHoleStateComponent.BallID].Score = CalculateScore(ballThatEnteredHoleMovementComponent.LaunchCount);
        Debug.LogFormat("Score: {0}", _players[ballThatEnteredHoleStateComponent.BallID].Score);

        // Deactivates the ball now that it has reached the goal.
        ballThatEnteredHole.SetActive(false);

        // Changes turn to the next on the list if it was this ball's turn.
        if (ballThatEnteredHoleStateComponent.BallID == _currentPlayerTurn)
            NextTurn();
    }

    /// <summary>
    /// This function is called when the game object is instantiated, before Start() or any Update functions.
    /// Sets this instance as the singleton if there isn't one already, destroys this instance if there is.
    /// </summary>
    void Awake()
    {
        // Ensures that there will only be one instance of the GameMaster at any time.
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    /// <summary>
    /// Start is called once before the first execution of Update after the MonoBehaviour is created.
    /// Currently just spawns each player's ball.
    /// </summary>
    void Start()
    {
        SpawnBalls();
    }
}