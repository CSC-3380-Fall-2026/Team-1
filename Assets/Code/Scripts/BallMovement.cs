using System;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This component translates player (click/touch) and drag input to a force on the ball.
/// Only mouse input handled currently, need to know how to get touch input screen coordinates.
/// Needs visualization, like a line to signal active dragging.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(StrokeCounter))]
public class BallMovement : MonoBehaviour
{
    // Defines states describing whether the ball is in contact with an object on the "Ground" layer or not.
    public enum GroundState
    {
        Airborne,
        Grounded
    }

    // Defines states describing the ball's motion (enum rather than bool, as likely to add more states later).
    public enum MotionState
    {
        Stationary,
        Moving
    }

    // Describes whether the ball is in contact with an object on the "Ground" layer or not.
    public GroundState CurrentGroundState;

    // Describes information regarding the ball's motion.
    public MotionState CurrentMotionState;

    // Only objects in these layers will apply friction to the ball.
    [Tooltip("Only objects in these layers will apply friction to the ball."), Header("Friction")]
    public LayerMask FrictionCheckLayers;

    // The number of times the ball has been launched. Reset every hole.
    [Tooltip("The number of times the ball has been launched."), Header ("Launching")]
    public int LaunchCount = 0;

    // The minimum velocity at which the ball will be considered to be moving.
    [SerializeField, Tooltip("The minimum velocity at which the ball will be considered to be moving.")]
    float _movementThreshold = 0.2f;

    // The ball's rigidbody component handles its physics interactions.
    Rigidbody2D _ballRigidBody;

    // Tracks the number valid strokes performed by this ball.
    StrokeCounter _strokeCounter; 

    // A reference to the ball's child object's component, that handles drawing the aiming indicator.
    AimingIndicator _aimingIndicator;

    // A reference to the "launch" action set in the "Input Actions" settings window.
    InputAction _clickTouchAction;

    // The radius around the ball's center that can be clicked on to interact with it.
    [SerializeField, Tooltip("The radius around the ball's center that can be clicked on to interact with it.")]
    float _ballInteractionRadius = 1;

    // The force multiplier applied after distance from the ball is calculated.
    [SerializeField, Tooltip("Multiplied with dragging distance (capped at 5 units) to calculate the final force applied.")] 
    float _forceMultiplier = 50;

    // Tracks whether the launch has been initiated and the direction/strength is being decided.
    bool _aimingLaunch = false;

    // Tracks the current position of the mouse/finger when aiming the ball.
    Vector2 _indicatorPosition;

    // Determines the amount of force applied to the ball. Based on indicator distance from ball's center.
    float _launchStrength;

    // Tracks whether the ball has been in contact with an object on the ground layer since it was last stationary+grounded.
    // Used to avoid scenario where the ball can be launched after hitting the ground, but falling of a ledge while rolling.
    bool _hasHitGroundDuringCurrentLaunch = false;

    // Tracks whether the ball has been launched since it was last stationary+grounded.
    // The ball will only deny relaunching after ground contact if this value is true.
    bool _hasLaunched = false;

    // Ignores friction if greater than 0. Used to prevent friction from cancelling launches for a chosen number of physics updates.
    // Works generally to delay checking ground contact for a given duration to allow movement to be applied beforehand.
    int _ignoreFriction = 0;

    [SerializeField, Tooltip("The rate of passage of time when aiming the ball.\n1.0 = regular\n0.0 = frozen")]
    float _aimingTimeRate = 0.2f;

    /// <summary>
    /// Awake is called when the object is instantiated, before Start() and Update().
    /// Stores the rigidbody2D component, aiming indicator component and the click action from the Input Actions window for future use.
    /// </summary>
    void Awake()
    {
        // Initializes component references.
        _ballRigidBody = GetComponent<Rigidbody2D>();
        _aimingIndicator = GetComponentInChildren<AimingIndicator>();
        _strokeCounter = GetComponent<StrokeCounter>();

        // Sets reference to the Input Action assigned to a mouse click or one finger touch.
        _clickTouchAction = InputSystem.actions.FindAction("ClickTouch");
    }

    /// <summary>
    /// Update is called once every frame.
    /// Polls touch/tap input and stores its coordinates.
    /// Updates the ball's movement state.
    /// </summary>
    void Update()
    {
        // Checks if the "ClickTouch" (Input Actions window) buttons were pressed/released, calls handler if so.
        if (_clickTouchAction.WasPressedThisFrame())
            HandleClick();
        
        if (_clickTouchAction.WasReleasedThisFrame())
            HandleRelease();

        // Checks if the ball is currently being aimed.
        // If so, translates the current mouse coordinates (touch TODO) from screen to world space coordinates and stores,
        // determines force amount based on drag distance and a multiplier, then updates the aiming indicator.
        if (_aimingLaunch && CanLaunch())
        {
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            _indicatorPosition = ScreenHelper.ScreenToWorldCoordinates(mousePosition, Camera.main);

            _launchStrength = CalculateLaunchStrength(
                Vector2.Distance(transform.position, _indicatorPosition)
            );

            _aimingIndicator.UpdateIndicator(true, _indicatorPosition, _launchStrength);
        } else if (_aimingLaunch && !CanLaunch())
        {
            // Assume that if the ability to launch is lost, the ball should no longer be aimed.
            _aimingLaunch = false;

            // Disables the indicator if the player is currently aiming, but loses the ability to launch.
            _aimingIndicator.UpdateIndicator(false);

            // Restores the rate of passage of time to the default value.
            // Maybe move time-rate adjustment to UpdateIndicator function to simplify.
            GameMaster.Instance.SetTimeRate();
        }

        // Sets the ball's motion state to moving if its velocity is greater than our set threshold, stationary otherwise.
        // If more movement states are added, will need more checks than just this binary one.
        CurrentMotionState = (_ballRigidBody.linearVelocity.magnitude > _movementThreshold) ? MotionState.Moving : MotionState.Stationary;

        // Asserts that we have not launched the ball since it last became stationary and grounded.
        if (CurrentMotionState == MotionState.Stationary &&
            CurrentGroundState == GroundState.Grounded &&
            _hasLaunched &&
            _ignoreFriction <= 0)
        {
            // The ball is allowed to be launched if this value is false.
            _hasHitGroundDuringCurrentLaunch = false;

            // _hasHitGroundDuringCurrentLaunch is only set to true on ground collision if this value is true as well.
            // Done this way to prevent redundant operations and premature setting of _hasHitGroundDuringCurrentLaunch.
            _hasLaunched = false;

            // Now that the ball has become stationary and grounded after its launch, end the player's turn, start the next.
            GameMaster.Instance.NextTurn();
        }
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
    /// Handler for click/touch (TODO) input, intended to be run when the user begins pressing.
    /// Checks if the current mouse position is within the ball's interaction radius, sets aiming status active if so.
    /// </summary>
    void HandleClick()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();

        if (Vector2.Distance((Vector2)transform.position, ScreenHelper.ScreenToWorldCoordinates(mousePosition, Camera.main)) < _ballInteractionRadius && CanLaunch())
        {
            // Sets this value to indicate that we are currently aiming. Enables aiming indicator updates.
            _aimingLaunch = true;

            // Tells the GameMaster slow time to the ball's set value.
            GameMaster.Instance.SetTimeRate(_aimingTimeRate);
        }
    }
    
    /// <summary>
    /// Handler for click/touch (TODO) input, intended to be run when the user releases.
    /// Checks if currently aiming and the ball is allowed to launch; if so, unflags, hides the aiming indicator, then applies force.
    /// </summary>
    void HandleRelease()
    {
        if (_aimingLaunch && CanLaunch())
        {
            // Sets this value to indicate that we are no longer aiming. Disables aiming indicator updates
            _aimingLaunch = false;

            // Hides the aiming indicator.
            _aimingIndicator.UpdateIndicator(false);

            // If we don't ignore friction for a number of FixedUpdates, it will greatly oppose the launch force. Disabled next FixedUpdate call.
            _ignoreFriction += 3;

            // Resets Chronoshot when a new shot begins.
            GameMaster.Instance.ResetChronoShot();

            // Provides a clean movement/force slate for the new launch.
            _ballRigidBody.totalForce = Vector2.zero;
            _ballRigidBody.linearVelocity = Vector2.zero;
            _ballRigidBody.angularVelocity = 0;

            // Applies a force to the ball in the direction opposite the pointer and with the magnitude of the launch strength.
            _ballRigidBody.AddForce(Vector2.Normalize((Vector2)transform.position - _indicatorPosition) * _launchStrength);

            // Allows for checks for ground contact to deny launching until stationary+grounded to begin.
            _hasLaunched = true;

            // Increments the ball's launch count.
            LaunchCount++;

            // Records a valid shot as a stroke.
            _strokeCounter.AddStroke();
        }
    }

    /// <summary>
    /// This function is called whenever the ball object's collider (CircleCollider2D in this case) begins colliding with another object in its "callbackLayers" list.
    /// Checks whether the object that the ball collided with is on the "Ground" layer and sets the ball's state to "Grounded" if so, maintains current state otherwise.
    /// </summary>
    /// <remarks>
    /// If this isn't called on the starting frame, the scene could begin with the ball marked as airborne while it started in collision with the ground, but this function
    /// wasn't called. Gound set with IsTouchingLayers called from this object's collider in Start() to set appropriate value if so.
    /// </remarks>
    /// <param name="collisionInfo">Holds details about the collision that just occured.</param>
    void OnCollisionEnter2D(Collision2D collisionInfo)
    {
        // Changes the ball's ground state to grounded if the object it collided with was on the ground layer.
        CurrentGroundState = (collisionInfo.collider.gameObject.layer == LayerMask.NameToLayer("Ground")) ? GroundState.Grounded : CurrentGroundState;

        // Confirms that the ball is no longer allowed to be launched, as it hit the ground after having being launched.
        // This check does not occur for until the same number of physics updates have passed that are needed to
        // be sure the ball has had enough time to leave the ground post launch.
        if (_ignoreFriction == 0)
            _hasHitGroundDuringCurrentLaunch = _hasLaunched;
    }

    /// <summary>
    /// This function is called every frame the ball object's collider remains in contact with another object in its "callbackLayers" list.
    /// Currently checks for contact with the ground to prevent relaunching until the ball becomes stationary+grounded again.
    /// </summary>
    /// <remarks>
    /// This is needed to account for the scenario where someone launches the ball and it rolls along the ground without getting any airtime.
    /// Relaunching still needs to be prevented, to avoid scenario where the ball rolls off a ledge, but can still be launched.
    /// </remarks>
    /// <param name="collisionInfo">Holds details about the collision that just occured.</param>
    void OnCollisionStay2D(Collision2D collisionInfo)
    {
        // Checks if the colliding object is on the ground layer. Maybe editor-exposed layermask if more layers needed.
        if (collisionInfo.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Asserts that the ball has hit the ground during this launch if the chosen number of physics updates have passed to be sure movement has been applied.
            if (_ignoreFriction == 0)
                _hasHitGroundDuringCurrentLaunch = _hasLaunched;
        }
    }

    /// <summary>
    /// This function is called whenever the ball object's collider (CircleCollider2D in this case) exits collision with another object in its "callbackLayers" list.
    /// Checks whether the object that the ball just left collision with is on the "Ground" layer and sets the ball's state to "Grounded" if so, maintains current
    /// state otherwise.
    /// </summary>
    /// <param name="collisionInfo">Holds details about the collision that the ball just exited.</param>
    void OnCollisionExit2D(Collision2D collisionInfo)
    {
        CurrentGroundState = (!_ballRigidBody.IsTouchingLayers(LayerMask.GetMask("Ground"))) ? GroundState.Airborne : CurrentGroundState;
    }

    /// <summary>
    /// Calculates the launch strength based on the distance dragged from the ball
    /// </summary>
    /// <param name="dragDistance">The distance dragged from the ball</param>
    /// <returns>The calculated launch strength</returns>
    public float CalculateLaunchStrength(float dragDistance)
    {
        return Math.Clamp(dragDistance, 0, 5) * _forceMultiplier;
    }

    /// <summary>
    /// Determines whether or not launching the ball is allowed.
    /// </summary>
    /// <remarks>
    /// Assuming more conditions on launching the ball will be introduced later (paused? affected by obstacle/item? etc), using a function
    /// rather than checking for motion state on every statement related to ball launching will be easier: only have to change one function.
    /// </remarks>
    /// <returns>Whether or not the ball is allowed to be launched.</returns>
    bool CanLaunch()
    {
        // The ball can be launched if it is stationary or it is airborne and has not previously been in contact with the ground during this launch.
        return !_hasHitGroundDuringCurrentLaunch && ((CurrentMotionState == MotionState.Stationary) || (CurrentGroundState == GroundState.Airborne));
    }
}
