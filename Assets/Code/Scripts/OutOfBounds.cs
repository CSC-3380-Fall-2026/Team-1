using UnityEngine;

/// <summary>
/// Handles objects passing through its trigger area, as they are considered out of bounds.
/// </summary>
public class OutOfBounds : MonoBehaviour
{
    // A list of possible locations to send any ball that enters this object's area.
    [Tooltip("Objects whose locations are candidates for the reset location.")]
    public GameObject[] ResetPoints;

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
            // Resets the ball to the location of the first of the available given reset points.
            GameMaster.Instance.ResetBall(objectCollider.gameObject, ResetPoints);
        }
    }
}
