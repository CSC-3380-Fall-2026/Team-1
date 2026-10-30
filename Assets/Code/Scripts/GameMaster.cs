using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

/// <summary>
/// A general controller of game state.
/// Performs general game functions that are not specific enough to be delegated to specialized objects and components.
/// </summary>
public class GameMaster : NetworkBehaviour
{
    // A GameMaster singleton. Can only be set from within this script.
    // Any references to GameMaster.instance from anywhere will refer to one singular GameMaster, no need to search for it.
    public static GameMaster Instance
    {
        get;
        private set;
    }

    // For testing purposes, the current course being played.
    [SerializeField]
    Course _currentCourse;

    // The index of the hole currently being played on the current course's hole list.
    int _currentHoleID = 0;

    // The hole referenced by the current hole ID.
    Hole _currentHole {get => _currentCourse.Holes[_currentHoleID];}

    // The players in the current game.
    // The list is synchronized across all clients, but only the server can change it.
    [Tooltip("A list of the players in the current game.")]
    NetworkList<Player> _players = new NetworkList<Player>(writePerm: NetworkVariableWritePermission.Server);

    // The index of the player whose turn it is to perform actions in the players list.
    // When changing turns, all the server does is change this value. Clients handle resulting behavior.
    [HideInInspector]
    public NetworkVariable<int> CurrentPlayerTurn = new NetworkVariable<int>(0);

    // The balls in the current map. Ball index should match player index.
    // Each client maintains a local copy of this list, as entire GameObjects are too large to synchronize.
    List<GameObject> _balls = new List<GameObject>();

     // The ball object to be created at the beginning of the round.
    [SerializeField, Tooltip("The ball object to be created at the beginning of the round.")]
    GameObject _ballPrefab;

    // A list of all reset points whose locations will be used as reset points.
    // Anything that handles resets will choose a subset of these objects (via index) to determine potential locations.
    [SerializeField, Tooltip("A list of potential reset points, including spawn points."), Header("Ball Reset")]
    GameObject[] _resetLocationObjects;

    // A list containing the indices of reset location objects in the general list that are used as spawn points.
    [SerializeField, Tooltip("The indices of the reset location objects denoting spawn points.")]
    int[] _spawnPointIDs;

    // Objects in these layers will be considered to be obstructing the reset point.
    [Tooltip("Objects in these layers will be considered to be obstructing the reset point.")]
    public LayerMask ResetObstructionLayers;

    [SerializeField, Tooltip("The normal rate of passage of time.\n1.0 = regular\n0.0 = frozen"), Header("Time")]
    float _normalTimeRate = 1.0f;

    // This stores the normal physics timestep so the physics updates remain consistent during time dilation
    float _normalFixedDeltaTime;

    /// <summary>
    /// Resets a given ball to a chosen location, possibly also resetting status effects.
    /// </summary>
    /// <remarks>
    /// No status effects currently, but assuming a BallStatus component is added with said behavior, will call its reset function.
    /// </remarks>
    /// <param name="ballToBeResetID">The ID of the ball we would like to reset in the local balls list.</param>
    /// <param name="resetLocationObjectsIndices">A list of indices reset location objects whose locations are candidates for the reset.</param>
    /// <param name="removeStatusEffects">Whether or not we will remove any status effects currently affecting the ball.</param>
    /// <param name="startFromIndex">Only spawn points on or after this index on the list will be considered.</param>
    [Rpc(SendTo.Owner)]
    public void ResetBallRPC(int ballToBeResetID, int[] resetLocationObjectsIndices, bool removeStatusEffects = true, int startFromIndex = 0)
    {
        // When we add a ball status component, reset its status effects.
        if (removeStatusEffects);

        // This will be set to the location of the first free reset location provided.
        Vector2 resetLocation = Vector2.zero;

        // Tracks whether or not any reset location is unoccupied so that the ball can be placed there.
        bool resetLocationClear = false;

        // Initializes an list that will contain the reset location GameObjects stored in the indices provided.
        List<GameObject> consideredResetLocationObjects = new List<GameObject>();

        // Adds the reset location GameObject at each given index to the list of considered objects.
        // To account for situations where multiple balls are reset in the same physics update (and thus cannot check for obstruction),
        // can manually skip a number of indices (if it is known a ball will be there).
        for (int i = startFromIndex; i < resetLocationObjectsIndices.Length; i++)
            consideredResetLocationObjects.Add(_resetLocationObjects[resetLocationObjectsIndices[i]]);

        // Checks if each reset location is unobstructed.
        foreach (GameObject resetLocationObject in consideredResetLocationObjects)
        {
            // Needed to determine if the collider is touching anything on the layers we're checking.
            CircleCollider2D resetLocationObjectCollider = resetLocationObject.GetComponent<CircleCollider2D>();

            // It is possible that the ball's radius is not static (powerups, obstruction effects, etc).
            // Therefore, we must ensure the area we check is that of the ball's area.
            resetLocationObjectCollider.radius = _balls[ballToBeResetID].GetComponent<CircleCollider2D>().radius;

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
        Rigidbody2D ballToBeResetRigidbody = _balls[ballToBeResetID].GetComponent<Rigidbody2D>();

        // Removes any added forces on the ball and any linear or angular velocity, ensuring it will be stationary on the next physics update.
        ballToBeResetRigidbody.totalForce = Vector2.zero;
        ballToBeResetRigidbody.linearVelocity = Vector2.zero;
        ballToBeResetRigidbody.angularVelocity = 0;

        // Sets the ball's position to that of the first unobstructed reset point found.
        _balls[ballToBeResetID].transform.position = resetLocation;

        // Resets ChronoShot so no time dilation carries over after the ball is reset.
        ResetChronoShot();
    }
    
    /// <summary>
    /// Resets ChronoShot to its default inactive state.
    /// </summary>
    public void ResetChronoShot()
    {
        SetTimeRateRPC();
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
    [Rpc(SendTo.ClientsAndHost)]
    public void SetTimeRateRPC(float newTimeRate = -1.0f)
    {
        // Uses the normal time rate when no specifc rate is provided
        if (newTimeRate == -1.0f)
            newTimeRate = _normalTimeRate;

        Time.timeScale = newTimeRate;

        // This will adjust the physics timestep with the time scale to keep the physics updates smooth during time dilaton
        Time.fixedDeltaTime = _normalFixedDeltaTime * newTimeRate;
    }

    /// <summary>
    /// Calculates a ball's resulting score based on the number of shots made, the
    /// time spent and the current hole's score modifiers.
    /// </summary>
    /// <param name="shotCount">The number of shots made before reaching the goal.</param>
    /// <returns>The score calculated given the number of shots and the current hole's values.</returns>
    float CalculateScore(int shotCount, float timeSpent)
    {
        return (float)(_currentHole.BaseScore *
            Mathf.Pow(_currentHole.ShotCountScoreFactor, shotCount) *
            Mathf.Pow(_currentHole.TimeSpentScoreFactor, timeSpent));
    }

    /// <summary>
    /// Goes through the list of players and spawns a ball for each one at the given spawn points.
    /// Uses the first unobstructed one found in the list.
    /// </summary>
    /// <remarks>
    /// The function is only called on the server
    /// </remarks>
    [Rpc(SendTo.ClientsAndHost)]
    void SpawnBallsRPC()
    {
        // Despawns and destroys all ball objects for all clients if called on the host.
        if (IsHost)
        {
            foreach(GameObject ball in _balls)
            {
                // Creates a reference to the NetworkObject component on the ball GameObject, needed to despawn it.
                NetworkObject ballNetworkObject = ball.GetComponent<NetworkObject>();

                // Despawns the ball from all clients, destroying it in the process.
                ballNetworkObject.Despawn();
            }
        }

        // The client's balls list now holds invalid/null references to the now-deleted balls, so clear them.
        _balls.Clear();

        // Creates a ball for each player and changes ownership to the player whose turn it is.
        if (IsHost)
        {
            // Since spawning all the balls only happens at the beginning of the hole, start at the first player's turn.
            CurrentPlayerTurn.Value = 0;

            // The following operations occur for each player in the _players list.
            for (int i = 0; i < _players.Count; i++)
            {
                // To prevent constant re-referencing.
                Player player = _players[i];

                // Creates a ball at the origin.
                GameObject playerBall = Instantiate(_ballPrefab);

                // Configures the ball's initial state.
                // Its ID should be the position of the owning player on the players list.
                BallState playerBallState = playerBall.GetComponent<BallState>();
                playerBallState.BallID.Value = i;
                playerBallState.PlayerName.Value = player.PlayerName.ToString();

                // Gets a reference to the new ball's NetworkObject component and spawns it on each client.
                NetworkObject ballNetworkObjectComponent = playerBall.GetComponent<NetworkObject>();
                ballNetworkObjectComponent.Spawn();

                // Moves the new ball to an unobstructed spawnpoint by the reset function.
                // Every ball will be spawned before the next physics update, so reset cannot check for obstruction.
                // To get around this, spawnpoints obstructed by balls just spawned are manually ignored.
                ResetBallRPC(i, _spawnPointIDs, true, i);

                // Every ball should be owned by the player whose turn it is, so that their physics simulation is accurate.
                ballNetworkObjectComponent.ChangeOwnership(_players[CurrentPlayerTurn.Value].PlayerClientID);
            }
        }
    }

    /// <summary>
    /// Adds an existing ball to the local client's balls list in the appropriate order.
    /// </summary>
    /// <param name="ballGameObject">The ball GameObject to be added.</param>
    public void AddPlayerBall(GameObject ballGameObject)
    {
        // Adds the ball to the list in the last position. Likely out of order.
        _balls.Add(ballGameObject);

        // A list that will store the balls newly reordered according to their IDs.
        List<GameObject> temporaryListForReorganization = new List<GameObject>();

        // Extends the temporary list to match the length of the balls list.
        while (temporaryListForReorganization.Count < _balls.Count)
            temporaryListForReorganization.Add(null);

        // The following occurs for every ball in the balls list.
        for (int i = 0; i < _balls.Count; i++)
        {
            // Makes a reference to the ball's state component in order to retrieve its ID.
            BallState ballStateComponent = _balls[i].GetComponent<BallState>();

            // Inserts the ball into the correct position on the temporary list.
            temporaryListForReorganization.Insert(ballStateComponent.BallID.Value, _balls[i]);
        }

        // Replaces the balls list with the temporary list now that the temporary list contains the ordered balls.
        _balls = temporaryListForReorganization;
    }

    /// <summary>
    /// Handles ending the current player's turn and beginning the next. It is only run on the server/host.
    /// Changes the current player turn variable to the ID of the player whose turn it is now.
    /// Moves to next hole if all balls have entered the cup.
    /// </summary>
    [Rpc(SendTo.Server)]
    public void NextTurnRPC()
    {
        // Tracks whether any active ball was found in the balls list.
        bool anyBallsActive = false;

        // Used to avoid triggering functions subscribed to the value's change while deciding whose turn it is.
        int nextTurnIndexTemporary = CurrentPlayerTurn.Value;

        // Loops through the balls list once to handle scenario where there is no active ball to switch turn to.
        for (int i = 0; i < _balls.Count; i++)
        {
            // Makes the next player on the list the one whose turn it is. Loops to 0 if end reached.
            nextTurnIndexTemporary = (nextTurnIndexTemporary + 1) % _players.Count;

            // Exits the loop if any active ball is found.
            if (_balls[nextTurnIndexTemporary].activeSelf)
            {
                anyBallsActive = true;
                break;
            }
        }

        // If no balls are active, then all should have entered the cup.
        // Call function that handles moving to the next hole.
        if (!anyBallsActive)
        {
            LoadNextHole();
            return;
        }

        // Makes the client owning the player whose turn it is the new owner of all the ball NetworkObjects.
        foreach (GameObject ball in _balls)
        {
            // Only need to change owner if the ball is active.
            if (ball.activeSelf)
            {
                // Gets a reference to the ball's NetworkObject component in order to change its owner.
                NetworkObject ballNetworkObjectComponent = ball.GetComponent<NetworkObject>();

                // Changes the owner of the ball to the client that contains the player whose turn it now is.
                ballNetworkObjectComponent.ChangeOwnership(_players[nextTurnIndexTemporary].PlayerClientID);
            }
        }

        // Actually changes the player turn value, triggering functions subscribed to the value's change event.
        CurrentPlayerTurn.Value = nextTurnIndexTemporary;
    }

    /// <summary>
    /// Advances to the next hole in the current cocurse and laods its scene
    /// </summary>
    void LoadNextHole()
    {
        // Stops progression if no course has been assigned
        if (_currentCourse == null)
        {
            Debug.LogWarning("No course is assigned. Unable to load the next hole.");
            return;
        }

        //Advances the current hole index
        _currentHoleID++;

        // Stops progression if the current hole was the final hole in the course
        if (_currentHoleID >= _currentCourse.Holes.Length)
        {
            Debug.Log("All players have completed the course.");
            return;
        }

        // Loads the scene associated with the next hole
        SceneManager.LoadScene(_currentHole.Map.name);
    }

    /// <summary>
    /// Called when a ball enters the hole. Executed on every client (including the host).
    /// Gets relevant information to update its score and disables it.
    /// </summary>
    /// <param name="ballThatEnteredHoleID">The ID of the ball gameObject in the balls list which just entered the hole.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void HandleGoalRPC(int ballThatEnteredHoleID)
    {
        // Only the host needs to calculate the score and update the value for all the clients.
        if (IsHost)
        {
            // Gets the ball's movement component for the launch count and state component for its ID.
            BallMovement ballThatEnteredHoleMovementComponent = _balls[ballThatEnteredHoleID].GetComponent<BallMovement>();
            BallTimeManager ballThatEnteredHoleTimeManagerComponent = _balls[ballThatEnteredHoleID].GetComponent<BallTimeManager>();

            // Calculates the player's new score and assigns it to the appropriate player.
            int launchCount = ballThatEnteredHoleMovementComponent.LaunchCount.Value;
            float timeSpent = ballThatEnteredHoleTimeManagerComponent.TimeSpentInPlay.Value;

            // Players structs in the players NetworkList cannot be changed directly, but can be set to a new Player.
            // Creates a new player struct based on the old player, sets the new score and replaces the old one with it.
            Player playerWhoOwnsBall = _players[ballThatEnteredHoleID];
            playerWhoOwnsBall.PlayerScore += CalculateScore(launchCount, timeSpent);
            _players[ballThatEnteredHoleID] = playerWhoOwnsBall;

            // To be replaced with UI visuals when we get there.
            Debug.LogFormat("Player: {0}, Launches: {1}, Time Spent: {2}, Score: {3}", ballThatEnteredHoleID, launchCount, timeSpent, _players[ballThatEnteredHoleID].PlayerScore);
        }

        // Deactivates the ball now that it has reached the goal.
        _balls[ballThatEnteredHoleID].SetActive(false);

        // Changes turn to the next on the list if it was this ball's turn.
        // Host check needed to prevent the next turn function being called again for every client.
        if (ballThatEnteredHoleID == CurrentPlayerTurn.Value && IsHost)
            NextTurnRPC();
    }

    /// <summary>
    /// Creates a new player struct based on the given information and adds it to the synchronized list.
    /// Should only every be called on the server/host.
    /// </summary>
    /// <param name="playerName">The name of the newly added player.</param>
    /// <param name="playerClientID">The NetworkClientID that owns the newly added player.</param>
    /// <param name="playerGUID">The GUID of the newly added player. Used to determine if reconnecting.</param>
    public void AddPlayer(string playerName, ulong playerClientID, string playerGUID)
    {
        // Throws a warning and exits the function if it is not executing on the host.
        if (!IsHost)
        {
            Debug.LogWarning("AddPlayer called while not the host. Unexpected and improper.");
            return;
        }

        // Updates the player's ClientID if the player was already in the list based on GUID.
        for(int i = 0; i < _players.Count; i++)
        {
            // GUID will be determined and stored when launching the game (probably PlayerPrefs).
            if (_players[i].PlayerGUID == playerGUID)
            {
                // Players in the NetworkList cannot be edited directly, so makes a clone with the new ID change
                // then sets the player to the new clone.
                Player playerThatExists = _players[i];
                playerThatExists.PlayerClientID = playerClientID;
                _players[i] = playerThatExists;

                Debug.Log("Player already exists. Updated client ID");
                return;
            }
        }

        // Makes a new player struct with the given information.
        Player newPlayer = new Player();
        newPlayer.PlayerName = playerName;
        newPlayer.PlayerClientID = playerClientID;
        newPlayer.PlayerGUID = playerGUID;

        // Adds the player to the players list, automatically synchronizing across connected clients.
        _players.Add(newPlayer);

        Debug.Log($"Added {newPlayer.PlayerName} with client ID {newPlayer.PlayerClientID}.");
    }

    /// <summary>
    /// This function is called when the game object is instantiated, before Start() or any Update functions.
    /// Sets this instance as the singleton if there isn't one already, destroys this instance if there is.
    /// </summary>
    void Awake()
    {
        // This stores Unity's default physics timestep before any time dilation is applied
        _normalFixedDeltaTime = Time.fixedDeltaTime;

        // Ensures that there will only be one instance of the GameMaster at any time.
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }
}