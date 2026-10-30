using UnityEngine;

/// <summary>
/// Contains properties and behaviors of water volumes.
/// </summary>
public class Water : MonoBehaviour
{
    // The amount of drag a ball will experience when it enters this object's trigger area.
    [SerializeField, Tooltip("The amount of drag a ball will experience if it passes through the water.")]
    float _dragOnBall = 1.0f;

    // The list of indices on the GameMaster's reset objects list showing potential reset locations.
    [SerializeField, Tooltip("A list of indices on the reset objects list where balls that stay too long could be sent to.")]
    int[] _resetPointsIDs;

    /// <summary>
    /// Called automatically when an object on the callback layer enters the trigger area.
    /// Changes drag (linear/angular damping) to the diminished value.
    /// </summary>
    /// <param name="enteringObjectCollider">The collider of the object entering this trigger area.</param>
    void OnTriggerEnter2D(Collider2D enteringObjectCollider)
    {
        // Just in case other objects eventually reside on the ball layer (cosmetics, etc).
        if (enteringObjectCollider.tag == "Ball")
        {
            // A reference to the rigid body on the same gameobject as the collider.
            Rigidbody2D _ballRigidBodyComponent = enteringObjectCollider.gameObject.GetComponent<Rigidbody2D>();

            // Linear damping is essentially drag, but doesn't affect rotation.
            _ballRigidBodyComponent.linearDamping = _dragOnBall;

            // Angular damping slows down rotational velocity.
            _ballRigidBodyComponent.angularDamping = _dragOnBall;

            // A reference to the BallMovement component on the same gameobject as the collider.
            BallMovement _ballMovementComponent = enteringObjectCollider.gameObject.GetComponent<BallMovement>();

            // Subscribes to watch for when the ball becomes stationary to reset it.
            _ballMovementComponent.BallMotionStateChanged += HandleBallMotionStateChanged;
        }
    }

    /// <summary>
    /// Called automatically when an object on the callback layer exits the trigger area.
    /// Resets drag (linear/angular damping) to normal (0).
    /// </summary>
    /// <remarks>
    /// If there is ever a situation where two water triggers overlap, leaving one while
    /// remaining in the other will still reset drag to 0, rather than remaining at a higher
    /// level until the ball has exited the area of all of them.
    /// To keep things simple, just don't overlap any water volumes.
    /// </remarks>
    /// <param name="exitingObjectCollider">The collider of the object leaving this trigger area.</param>
    void OnTriggerExit2D(Collider2D exitingObjectCollider)
    {
        // Just in case other objects eventually reside on the ball layer (cosmetics, etc).
        if (exitingObjectCollider.tag == "Ball")
        {
            // A reference to the rigid body on the same gameobject as the collider.
            Rigidbody2D _ballRigidBody = exitingObjectCollider.gameObject.GetComponent<Rigidbody2D>();

            // Linear damping is essentially drag, but doesn't affect rotation.
            _ballRigidBody.linearDamping = 0.0f;

            // Angular damping slows down rotational velocity.
            _ballRigidBody.angularDamping = 0.05f;

            // A reference to the BallMovement component on the same gameobject as the collider.
            BallMovement _ballMovementComponent = exitingObjectCollider.gameObject.GetComponent<BallMovement>();

            // Unsubscribes, as there is no need to watch for motion state changes anymore.
            _ballMovementComponent.BallMotionStateChanged -= HandleBallMotionStateChanged;
        }
    }

    /// <summary>
    /// A handler for the event raised when a ball changes its motion state.
    /// Resets the ball when it becomes stationary.
    /// </summary>
    /// <param name="ballObject">The ball GameObject whose state just changed.</param>
    /// <param name="newMotionState">The new motion state of the ball.</param>
    void HandleBallMotionStateChanged(GameObject ballObject, BallMovement.MotionState newMotionState)
    {
        // Resets the ball if it is now stationary.
        if (newMotionState == BallMovement.MotionState.Stationary)
        {
            // Gets a reference to the state component on the ball object to retrieve its ID.
            BallState ballObjectState = ballObject.GetComponent<BallState>();

            // Resets the ball to the first available indicated location.
            GameMaster.Instance.ResetBallRPC(ballObjectState.BallID.Value, _resetPointsIDs);
        }
    }
}
