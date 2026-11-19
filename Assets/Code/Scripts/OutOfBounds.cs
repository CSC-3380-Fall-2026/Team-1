using UnityEngine;

/// <summary>
/// Handles objects passing through its trigger area, as they are considered out of bounds.
/// </summary>
public class OutOfBounds : MonoBehaviour
{
    // A list of indices of possible locations to send any ball that enters this object's area.
    [SerializeField, Tooltip("Indices of reset location objects whose locations are candidates for the reset location.")]
    int[] _resetPointsIDs;

    /// <summary>
    /// Called when an object on any of the callback layers enters this object's trigger area (the collider).
    /// Currently resets any balls it encounters.
    /// </summary>
    /// <param name="objectCollider">The collider component of the object that made contact with this object's trigger area.</param>
    void OnTriggerEnter2D(Collider2D objectCollider)
    {
        // Only balls will be reset in this way, so can check for tag more specifically, not any object on a given range of layers.
        if (objectCollider.tag == "Ball")
        {
            // Gets a reference to the state component of the entering ball, to retrieve its ID.
            BallState enteringBallStateComponent = objectCollider.GetComponent<BallState>();

            // Resets the ball to the location of the first of the available given reset points.
            GameMaster.Instance.ResetBallRPC(enteringBallStateComponent.BallID.Value, _resetPointsIDs);
        }
    }
}
