using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This will handle the friction forces that is applied to the ball while
/// it is in contact with objects on the configured friction layers
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BallFriction : MonoBehaviour
{
    // Only objects in these layers will apply friction to the ball
    [Tooltip("Only objects in these layers will apply friction to the ball."), Header("Friction")]
    public LayerMask FrictionCheckLayers;

    // The ball's Rigidbody2D component
    Rigidbody2D _ballRigidBody;

    // The number of physics updates during which friction should be ignored
    int _ignoreFriction = 0;

    // Determines the number of physics updates on which friction will be ignored to prevent opposition to launch force.
    [SerializeField, Tooltip("The number of physics updates during which friction is ignored.\nNeeded, as friction will oppose launch force otherwise.")]
    int _framesToIgnoreFrictionOnLaunch = 3;

    /// <summary>
    /// This is called when the object is instantiaed, before Start() and Update()
    /// It will also store the ball's Rigidbody2D component so it can be used when calculating
    /// and applying friction
    /// </summary>
    void Awake()
    {
        _ballRigidBody = GetComponent<Rigidbody2D>();

        // Subscribes to the event called when the ball stops being aimed to ignore friction
        // for the required frames to stop it from opposing launch force.
        BallMovement ballMovement = GetComponent<BallMovement>();
        ballMovement.BallStoppedAiming += OnBallStoppedAiming;
    }

    /// <summary>
    /// Called every time the physics engine updates.
    /// Used to apply the friction of every object in contact if they in one of the appropriate layers.
    /// </summary>
    void FixedUpdate()
    {
        // In case we decide we don't want friction applied this physics engine update.
        if (_ignoreFriction > 0)
        {
            _ignoreFriction--;
            return;
        }

        // This object configures the request for all objects in contact. Currently filters for those in our chosen layers.
        ContactFilter2D ballCollisionFilter = new ContactFilter2D();
        ballCollisionFilter.useLayerMask = true;
        ballCollisionFilter.layerMask = FrictionCheckLayers;

        // Stores information about the contact points of all collisions that pass the filter in the list.
        List<ContactPoint2D> ballCollisionContacts = new List<ContactPoint2D>();
        _ballRigidBody.GetContacts(ballCollisionFilter, ballCollisionContacts);

        foreach(ContactPoint2D contactPoint in ballCollisionContacts)
        {
            // The force applied to the ball at the point of contact. Remains at zero if the ball is stationary.
            Vector2 frictionForce = Vector2.zero;

            // Stores the velocity of the point on the rigidbody where the collision occured. Accounts for linear and angular velocity.
            Vector2 contactPointVelocity = _ballRigidBody.GetPointVelocity(contactPoint.point);

            // Avoids needless computation, as no friction needed if the ball is stationary.
            if (contactPointVelocity.magnitude > 0)
            {
                // Friction opposes motion, so its direction should be opposite that of the velocity of the rigidbody at the point of contact.
                Vector2 frictionForceDirection = -contactPointVelocity.normalized;

                // The magnitude of the vector projection of the net forces acting on the ball onto the normal vector.
                // The rigidbody's totalForce attribute only accounts for those created with AddForce and its family of functions, not gravity.
                float normalForceMagnitude = ((_ballRigidBody.totalForce + Physics2D.gravity) * contactPoint.normal).magnitude;

                // Calculates the dynamic friction in the direction opposing motion.
                // The colliding object's friction value is the coefficient of dynamic friction.
                frictionForce = contactPoint.collider.friction * normalForceMagnitude * frictionForceDirection;
            }
            
            // Applies the friction force on the ball's rigidbody at the point of contact.
            _ballRigidBody.AddForceAtPosition(frictionForce, contactPoint.point);
        }
    }

    /// <summary>
    /// This sets the number of upcoming physics updates during which friction should be ignored
    /// Used to prevent friction from immediately opposing a newly applied launch force
    /// </summary>
    /// <param name="physicsUpdates"> The number of physics updates to ignore friction for </param>
    public void IgnoreFriction(int physicsUpdates)
    {
        _ignoreFriction += physicsUpdates;
    }

    /// <summary>
    /// Determines whether friction is being ignored
    /// </summary>
    /// <returns> True if friction is being ignored for one or more physics updates; false otherwise </returns>
    public bool IsIgnoringFriction()
    {
        return _ignoreFriction > 0;
    }

    /// <summary>
    /// This function is subscribed to the event that occurs when the ball stops being aimed.
    /// It ignores friction for the default number of physics updates when the ball is launched to prevent friction
    /// from opposing motion.
    /// </summary>
    /// <param name="successfullyLaunched">Reflects whether the ball stopped being aimed becaused it was launched or not.</param>
    void OnBallStoppedAiming(bool successfullyLaunched)
    {
        if (successfullyLaunched)
            IgnoreFriction(_framesToIgnoreFrictionOnLaunch);
    }
}