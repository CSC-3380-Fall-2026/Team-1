using UnityEngine;

/// <summary>
/// Contains behavior for the goal area.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Goal : MonoBehaviour
{
    /// <summary>
    /// Called when an object on the callback layer enters this object's trigger area.
    /// The only layer active is the ball layer, the object entering the trigger area will be a ball.
    /// Currently just passes the ball object to the GameMaster to handle.
    /// </summary>
    /// <param name="otherObjectCollider">The collider component on the object entering the goal area.</param>
    void OnTriggerEnter2D(Collider2D otherObjectCollider)
    {
        GameMaster.Instance.HandleGoal(otherObjectCollider.gameObject);
    }
}
