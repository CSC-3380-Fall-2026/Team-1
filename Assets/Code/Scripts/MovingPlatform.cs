using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Defines the behavior of moving platforms.
/// </summary>
public class MovingPlatform : NetworkBehaviour
{
    // A list of information about the points the platform will move to in sequence.
    [SerializeField, Tooltip("The ball will move to the same position/rotation of each object on the list sequentially.")]
    TargetPoint[] _targetPoints;

    // The index of the point on the target points list the platform is currently moving to.
    int _currentTargetPointIndex = 0;

    // A shortcut to reference the current target point.
    TargetPoint _currentTargetPoint {get => _targetPoints[_currentTargetPointIndex];}

    // Stops the platform from moving if false.
    bool _shouldMove = true;

    // Tracks how long the platform should remain still until it moves to the next point.
    float _timeToWaitTillNextPointLeft = 0.0f;

    // Used to calculate the amount of distance towards the current point's position to move each update.
    float _initialPositionDistanceFromTarget = 0.0f;

    // Used to calculate the angle distance towards the current point's rotation to move each update.
    float _initialRotationDistanceFromTarget = 0.0f;

    /// <summary>
    /// Called when the object is created on the client.
    /// Initializes the platform to the first target point and tells it to move to the next.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        // Derrived function, so base function called in case it does anything important.
        base.OnNetworkSpawn();

        // Should remain stationary if there are no target points.
        if (_targetPoints.Length == 0)
            return;

        // Sets the initial position and rotation to that of the first point on the list.
        transform.position = _currentTargetPoint.PointObjectTransform.position;
        transform.rotation = _currentTargetPoint.PointObjectTransform.rotation;

        // Performs necessary setup to change the target to the next one on the list.
        SwitchTargetToNextPoint();

        // Now that the server and the platform have been initialized, it should be allowed to move.
        _shouldMove = true;
    }

    /// <summary>
    /// Update is called once per frame.
    /// Handles moving the platform to the current target, determining if the target has been reached
    /// and what to do once it has.
    /// </summary>
    void Update()
    {
        // Should remain stationary if there are no targets in the list or told to.
        if (_targetPoints.Length == 0 || !_shouldMove)
            return;

        // Checks if the platform has reached the target position.
        if (transform.position == _currentTargetPoint.PointObjectTransform.position)
        {
            // Decides what to do when the platform has rotated to the target direction.
            if (transform.rotation == _currentTargetPoint.PointObjectTransform.rotation)
            {
                // Waits until the specified duration has passed then changes target to next point on the list.
                if (_timeToWaitTillNextPointLeft <= 0.0f)
                    SwitchTargetToNextPoint();
                else
                    _timeToWaitTillNextPointLeft -= Time.deltaTime;
            } else if (_currentTargetPoint.RotationMode == TargetPoint.TargetPointRotationMode.RotateAtDestination)
            {
                // Rotates the platform towards the target point's rotation over the given duration if told to rotate at the end of the path.
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    _currentTargetPoint.PointObjectTransform.rotation,
                    _initialRotationDistanceFromTarget / (_currentTargetPoint.DurationOfRotationAtDestination / Time.deltaTime));
            } else
                Debug.LogWarning("Platform reached the target position, but not rotation. It should reach both at the same time.");
        }

        // Moves a bit towards the target point's position over the specified duration based on the time elapsed since the last frame.
        transform.position = Vector2.MoveTowards(transform.position,
            _currentTargetPoint.PointObjectTransform.position,
            _initialPositionDistanceFromTarget / (_currentTargetPoint.DurationOfTravelToPoint / Time.deltaTime));

        // Rotates a bit towards the target point's rotation over the specified duration based on the time elapsed since the last frame.
        if (_currentTargetPoint.RotationMode == TargetPoint.TargetPointRotationMode.RotateDuringTravel)
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                _currentTargetPoint.PointObjectTransform.rotation,
                _initialRotationDistanceFromTarget / (_currentTargetPoint.DurationOfTravelToPoint / Time.deltaTime));
    }

    /// <summary>
    /// Sets up the data needed to perform the motion towards the next target point.
    /// </summary>
    void SwitchTargetToNextPoint()
    {
        // Increments the index showing the target on the list the platform should move to. Loops back to 0 if at the end.
        _currentTargetPointIndex = (_currentTargetPointIndex + 1) % _targetPoints.Length;

        // Updates the amount of time to wait once the destination has been reached with the new value.
        _timeToWaitTillNextPointLeft = _currentTargetPoint.DurationToRestAtPoint;

        // Calculates difference in position and angle between the platform and its target so that the movement per frame can be calculated.
        _initialPositionDistanceFromTarget = Vector2.Distance(transform.position, _currentTargetPoint.PointObjectTransform.position);
        _initialRotationDistanceFromTarget = Quaternion.Angle(transform.rotation, _currentTargetPoint.PointObjectTransform.rotation);
    }
}

/// <summary>
/// Contains data about each point a moving platform should go to.
/// </summary>
[Serializable]
public struct TargetPoint
{
    // Defines when in its motion towards this target the platform should rotate to match it.
    public enum TargetPointRotationMode
    {
        RotateDuringTravel,
        RotateAtDestination
    }

    // The transform component on the GameObject used as a target point. The platform will aim to match its position and rotation.
    [Tooltip("The platform will move towards this object's position and rotation.")]
    public Transform PointObjectTransform;

    // Defines how long the platform should take to reach the position (and rotation if 'during' rotation mode).
    [Tooltip("The platform will take this amount of time to reach the target location.")]
    public float DurationOfTravelToPoint;

    // Defines how long the platform should wait when it reaches the target position and rotation.
    [Tooltip("The platform will wait for this amount of time (in seconds) once it has reached this target's position and rotation.")]
    public float DurationToRestAtPoint;

    // Defines when the platform should rotate: constantly while moving towards the target position or only once it has been reached.
    [Tooltip("When in its motion towards this target the platform should rotate to match it.")]
    public TargetPointRotationMode RotationMode;

    // Defines how long the rotation to match the target's should take if the rotation mode states to rotate at the end.
    // If the rotation mode is set to 'during travel,' this value is ignored. Instead, it finishes rotating at the same time it does moving.
    [Tooltip("How long the platform should take to rotate if the mode specified was to 'rotate at the destination.'")]
    public float DurationOfRotationAtDestination;
}