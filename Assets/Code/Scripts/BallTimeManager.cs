using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Contains properties and defines behavior involving time measurement of the ball object.
/// </summary>
public class BallTimeManager : NetworkBehaviour
{
    // The amount of time that has passed during this ball's turn. Not realtime, affected by multipliers.
    // All clients need access to each ball's time spent to display it accurately on their UI, but only
    // the owner needs write access to prevent inteference/conflicting changes.
    public NetworkVariable<float> TimeSpentInPlay = new NetworkVariable<float>(0, writePerm: NetworkVariableWritePermission.Owner);

    // When this is true, time-taken-modifying calculations will be performed each frame.
    bool _measureTime = false;

    // The modifier applied to the time passed each frame before it is added to total.
    float _timeRateModifier = 1.0f;

    // The default time modifier. When the ball is not under any
    // effect, the time taken calculation will be modified by this factor.
    [SerializeField, Tooltip("The default modifier on the amount of time that should be added to the counter each second.")]
    float _regularTimeRateModifier = 1.0f;

    // The time taken modifier while the ball is being aimed.
    [SerializeField, Tooltip("The modifier on the amount of time added to the counter while the ball is being aimed.")]
    float _aimingTimeRateModifier = 1.0f;

    /// <summary>
    /// Sets the time rate modifier to the aiming amount.
    /// </summary>
    void SetTimeRateModifierToAimingSpeed()
    {
        SetTimeRateModifier(_aimingTimeRateModifier);
    }

    /// <summary>
    /// Sets the flag required to start time-taken calculations.
    /// </summary>
    void StartMeasuringTime()
    {
        _measureTime = true;
    }

    /// <summary>
    /// Unsets the flag required to start time-taken calculations.
    /// </summary>
    void StopMeasuringTime()
    {
        _measureTime = false;
    }

    /// <summary>
    /// Sets the time rate modifier to the specified value.
    /// The amount of time passed every frame will be multiplied by this amount before it is
    /// added to the total.
    /// </summary>
    /// <param name="newTimeRateModifier">The new modifier amount. If left unset (or set to -1.0f), resets to default.</param>
    void SetTimeRateModifier(float newTimeRateModifier = -1.0f)
    {
        _timeRateModifier = (newTimeRateModifier == -1.0f) ? _regularTimeRateModifier : newTimeRateModifier;
    }

    /// <summary>
    /// Callback function for the event called with the ball stops aiming.
    /// Resets the time rate modifier to the default value.
    /// </summary>
    /// <param name="successfullyLaunched">Indicates whether or not the ball stopped being aimed because it was launched.</param>
    void OnBallStoppedAiming(bool successfullyLaunched)
    {
        SetTimeRateModifier();
    }

    /// <summary>
    /// This function is called when the NetworkObject has been spawned and its NetworkBehaviors have been initialized.
    /// Assigns the relevant callback functions to the relevant events on the BallMovement component.
    /// </summary>
    protected override void OnNetworkPostSpawn()
    {
        // In case the parent function does something useful, as it is an override.
        base.OnNetworkPostSpawn();

        // Subscribes the relevant functions in this component to the ball's events.
        BallMovement ballMovementComponent = GetComponent<BallMovement>();
        ballMovementComponent.BallTurnBegan += StartMeasuringTime;
        ballMovementComponent.BallTurnEnded += StopMeasuringTime;
        ballMovementComponent.BallBeganAiming += SetTimeRateModifierToAimingSpeed;
        ballMovementComponent.BallStoppedAiming += OnBallStoppedAiming;
    }

    /// <summary>
    /// Update is called once per frame.
    /// Determines the new total of time spent since the last frame, post-modifier.
    /// </summary>
    void Update()
    {
        // Only the NetworkObject's owner should be able to change to time spent value, as it has the most accurate information.
        if (_measureTime && IsOwner)
            TimeSpentInPlay.Value += Time.deltaTime * _timeRateModifier;
    }
}
