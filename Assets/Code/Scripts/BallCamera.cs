using UnityEngine; 

/// <summary>
/// This will control the gameplay camrea and follows the ball belonging to
/// the player whose turn is currently active
/// </summary>
public class BallCamera : MonoBehaviour
{
    // The ball that the camrea is currently following
    Transform _targetBall;

    // Controls how smoothly the camrea moves towards the active ball
    [SerializeField, Tooltip("The amount of time used to smoothly move the camrea towards the active ball.")]
    float _smoothTime = 0.3f;

    // Stores the current velocity used to SmoothDamp
    Vector3 _cameraVelocity = Vector3.zero;

    /// <summary>
    /// This will find the ball belonging to the player whose turn is currently 
    /// active and makes it the camrea's target
    /// </summary>
    void UpdateCameraTarget()
    {
        BallState[] balls = FindObjectsByType<BallState>(FindObjectsSortMode.None);

        // Assume all players on this device have finished until an active local ball is found.
        bool allLocalPlayersFinished = true;

        foreach (BallState ball in balls)
        {
            int ballID = ball.BallID.Value;

            // Checks whether this ball belongs to a player on this device.
            if (GameMaster.Instance.Players[ballID].PlayerClientID ==
                Unity.Netcode.NetworkManager.Singleton.LocalClientId)
            {
                // If a local ball is still active, this device still has a player in the hole.
                if (ball.gameObject.activeSelf)
                {
                    allLocalPlayersFinished = false;
                    break;
                }
            }
        }

        foreach (BallState ball in balls)
        {
            int ballID = ball.BallID.Value;

            if (ballID == GameMaster.Instance.CurrentPlayerTurn.Value)
            {
                bool ballBelongsToLocalPlayer =
                    GameMaster.Instance.Players[ballID].PlayerClientID ==
                    Unity.Netcode.NetworkManager.Singleton.LocalClientId;

                /*Follow local players normally. If all local players have finished,
                 allow the camera to spectate the player whose turn is active.
                 */
                if (ballBelongsToLocalPlayer || allLocalPlayersFinished)
                {
                    _targetBall = ball.transform;
                }

                return;
            }
        }
    }

    /// <summary>
    /// Sets the initial camrea target when the gameplay begins
    /// </summary>
    void Start()
    {
        GameMaster.Instance.CurrentPlayerTurn.OnValueChanged += HandleTurnChanged;
        UpdateCameraTarget();
    }

    /// <summary>
    /// Smoothly moves the camrea toward the ball it is currently following
    /// </summary>
    void LateUpdate()
    {
        if (_targetBall == null)
        {
            UpdateCameraTarget();
            if (_targetBall == null)
                return;
        }

        
        Vector3 targetPosition = new Vector3(
            _targetBall.position.x, transform.position.y, transform.position.z
        );

        transform.position = Vector3.SmoothDamp(
            transform.position, targetPosition, ref _cameraVelocity, _smoothTime
        );
    }

    /// <summary>
    /// Updates the camrea target when the active player's turn changes
    /// </summary>
    /// <param name="previousTurn"> The ID of the player whose turn just ended. </param>
    /// <param name="newTurn"> The ID of the player whose turn is beginning. </param>
    void HandleTurnChanged(int previousTurn, int newTurn)
    {
        UpdateCameraTarget();
    }

    /// <summary>
    /// Stops listening for turn changes when this camera is destroyed
    /// </summary>
    void OnDestroy()
    {
        if (GameMaster.Instance != null)
        {
            GameMaster.Instance.CurrentPlayerTurn.OnValueChanged -= HandleTurnChanged;
        }
    }
}