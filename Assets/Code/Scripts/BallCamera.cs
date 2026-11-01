using Unity.Netcode;
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
        // Wait until all player balls have been added to the GameMaster.
        if (GameMaster.Instance.Balls.Count == 0 ||
            GameMaster.Instance.Balls.Count != GameMaster.Instance.Players.Count)
        {
            return;
        }

        // Assume all players on this device have finished until an active local ball is found.
        bool allLocalPlayersFinished = true;

        for (int i = 0; i < GameMaster.Instance.Balls.Count; i++)
        {
            if (GameMaster.Instance.Balls[i] == null)
            {
                return;
            }
            // Checks whether this ball belongs to a player on this device.
            if (GameMaster.Instance.Players[i].PlayerClientID ==
                NetworkManager.Singleton.LocalClientId)
            {
                // If a local ball is still active, this device still has a player in the hole.
                if (GameMaster.Instance.Balls[i].activeSelf)
                {
                    allLocalPlayersFinished = false;
                    break;
                }
            }
        }

        int activeBallID = GameMaster.Instance.CurrentPlayerTurn.Value;

        // Gets the active player's ball directly using its ID.
        GameObject activeBall = GameMaster.Instance.Balls[activeBallID];

        bool ballBelongsToLocalPlayer =
            GameMaster.Instance.Players[activeBallID].PlayerClientID ==
            NetworkManager.Singleton.LocalClientId;

        /* Follow local players normally. If all local players have finished,
           allow the camera to spectate the player whose turn is active.
         */
        if (ballBelongsToLocalPlayer || allLocalPlayersFinished)
        {
            _targetBall = activeBall.transform;
        }
    }

    bool _subscribedToTurnChanges = false;

    void Update()
    {
        if (!_subscribedToTurnChanges &&
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening)
        {
            GameMaster.Instance.CurrentPlayerTurn.OnValueChanged += HandleTurnChanged;
            _subscribedToTurnChanges = true;

            _targetBall = null;
            UpdateCameraTarget();
        }
    }

    /// <summary>
    /// Smoothly moves the camera toward the ball it is currently following
    /// </summary>
    void LateUpdate()
    {
        int activeBallID = GameMaster.Instance.CurrentPlayerTurn.Value;

        // Updates the target if there is no target or the camera is following
        // a ball that no longer belongs to the active player.
        if (_targetBall == null ||
            activeBallID >= GameMaster.Instance.Balls.Count ||
            GameMaster.Instance.Balls[activeBallID] == null ||
            _targetBall != GameMaster.Instance.Balls[activeBallID].transform)
        {
            UpdateCameraTarget();
        }

        if (_targetBall == null)
            return;

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
        _targetBall = null;
        UpdateCameraTarget();
    }

    /// <summary>
    /// Stops listening for turn changes when this camera is destroyed
    /// </summary>
    void OnDestroy()
    {
        if (_subscribedToTurnChanges && GameMaster.Instance != null)
        {
            GameMaster.Instance.CurrentPlayerTurn.OnValueChanged -= HandleTurnChanged;
        }
    }
}