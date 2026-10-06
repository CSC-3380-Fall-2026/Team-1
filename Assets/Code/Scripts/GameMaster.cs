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

    // A list of game objects whose locations will be used as reset points.
    [Tooltip("A list of game objects whose locations will be used as reset points."), Header("Ball Reset")]
    public GameObject[] ResetPoints;

    // Objects in these layers will be considered to be obstructing the reset point.
    [Tooltip("Objects in these layers will be considered to be obstructing the reset point.")]
    public LayerMask ResetObstructionLayers;

    // The ball object to be created at the beginning of runtime.
    [SerializeField, Tooltip("The ball object to be created at the beginning of runtime."), Header("Testing")]
    GameObject _ballPrefab;

    /// <summary>
    /// Resets a given ball to a chosen location, possibly also resetting status effects.
    /// </summary>
    /// <remarks>
    /// No status effects currently, but assuming a BallStatus component is added with said behavior, will call its reset function.
    /// </remarks>
    /// <param name="ballToBeReset">A reference to the ball we would like to reset.</param>
    /// <param name="resetLocation">The coordinates of the place we would like to reset the ball to.</param>
    /// <param name="removeStatusEffects">Whether or not we will remove any status effects currently affecting the ball.</param>
    public void ResetBall(GameObject ballToBeReset, GameObject[] resetLocationObjects, bool removeStatusEffects = true)
    {
        // When we add a ball status component, reset its status effects.
        if (removeStatusEffects);

        // This will be set to the location of the first free reset location provided.
        Vector2 resetLocation = Vector2.zero;

        // Tracks whether or not any reset location is unoccupied so that the ball can be placed there.
        bool resetLocationClear = false;

        // Checks if each reset location is unobstructed.
        foreach (GameObject resetLocationObject in resetLocationObjects)
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
    /// Currently just instantiates the ball at the origin.
    /// </summary>
    void Start()
    {
        Instantiate(_ballPrefab, Vector3.zero, Quaternion.identity);
    }
}
