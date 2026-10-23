using UnityEditor.EditorTools;
using UnityEngine;

/// <summary>
/// Contains properties and defines behavior involving time measurement of the ball object.
/// </summary>
public class BallTimeManager : MonoBehaviour
{
    // The amount of time that has passed during this ball's turn.
    // Not realtime, affected by multipliers.
    public float TimeSpentInPlay {get; private set;}

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
    void SetTimeRateToAimingSpeed()
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
    /// Start is called once before the first execution of Update after the MonoBehaviour is created.
    /// Initializes the time spent in play to 0.
    /// </summary>
    void Start()
    {
        TimeSpentInPlay = 0;
    }

    /// <summary>
    /// Update is called once per frame.
    /// Determines the new total of time spent since the last frame, post-modifier.
    /// </summary>
    void Update()
    {
        if (_measureTime)
            TimeSpentInPlay += Time.deltaTime * _timeRateModifier;
    }
}
