using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This will control the gameplay camera and follows the ball belonging to
/// the player whose turn is currently active
/// </summary>
public class BallCamera : MonoBehaviour
{
    // The ball that the camera is currently following
    Transform _targetBall;

    // Controls how smoothly the camera moves towards the active ball
    [SerializeField, Tooltip("The amount of time used to smoothly move the camera towards the active ball.")]
    float _smoothTime = 0.3f;

    // Stores the current velocity used to SmoothDamp
    Vector3 _cameraVelocity = Vector3.zero;

    // Minimum and maximum zoom levels for the camera
    [SerializeField, Tooltip("The minimum zoom level allowed for the camera.")]
    float _minZoom = 5f;

    [SerializeField, Tooltip("The maximum zoom level allowed for the camera.")]
    float _maxZoom = 12f;

    // This controls how fast the camera can zoom in and out
    [SerializeField, Tooltip("The speed at which the camera zooms in and out.")]
    float _zoomSpeed = 2f;

    // Stores the desired camera zoom level
    float _targetZoom;

    /// <summary>
    /// This will find the ball belonging to the player whose turn is currently 
    /// active and makes it the camera's target
    /// </summary>
    void UpdateCameraTarget()
    {
        if (GameMaster.Instance == null)
        {
            return;
        } 

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
            if (i >= GameMaster.Instance.Players.Count)
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

    /// <summary>
    /// Subscribes to the client connection event when the camera begins.
    /// </summary>
    void Start()
    {
        _targetZoom = 5f;
        GetComponent<Camera>().orthographicSize = _targetZoom;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    /// <summary>
    /// Subscribes to turn changes after the local client connects to the server.
    /// </summary>
    /// <param name="clientID">The ID of the client that connected to the server.</param>
    void HandleClientConnected(ulong clientID)
    {
        if (clientID == NetworkManager.Singleton.LocalClientId)
        {
            GameMaster.Instance.CurrentPlayerTurn.OnValueChanged += HandleTurnChanged;
            _targetBall = null;
            UpdateCameraTarget();
        }
    }
    /// <summary>
    /// Haldes the camera zoom using the mouse scroll wheel
    /// </summary>
    void Update()
    {
        float scroll = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;

        // Updates zoom and keep it between 5 and 12
        _targetZoom = Mathf.Clamp(_targetZoom - scroll * _zoomSpeed * 0.01f, _minZoom, _maxZoom);

        // Smoothly adjusts the camera's zoom
        Camera camera = GetComponent<Camera>();
        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, _targetZoom, Time.deltaTime * 5f);
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
            _targetBall.position.x, _targetBall.position.y, transform.position.z
        );

        transform.position = Vector3.SmoothDamp(
            transform.position, targetPosition, ref _cameraVelocity, _smoothTime
        );
    }

    /// <summary>
    /// Updates the camera target when the active player's turn changes
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
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }

        if (GameMaster.Instance != null)
        {
            GameMaster.Instance.CurrentPlayerTurn.OnValueChanged -= HandleTurnChanged;
        }
    }
}