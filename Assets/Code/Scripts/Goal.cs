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
        // Gets a reference to the entering ball's state component to get its ID.
        BallState enteringBallStateComponent = otherObjectCollider.GetComponent<BallState>();

        // Informs the GameMaster that the ball with the given ID has entered the goal.
        GameMaster.Instance.HandleGoalRPC(enteringBallStateComponent.BallID.Value);
    }
}
