using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This component translates player (click/touch) and drag input to a force on the ball.
/// Only mouse input handled currently, need to know how to get touch input screen coordinates.
/// Needs visualization, like a line to signal active dragging.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BallMovement : MonoBehaviour
{
    // Defines states describing whether the ball is in contact with an object on the "Ground" layer or not.
    public enum GroundState
    {
        Airborne,
        Grounded
    }

    // Describes whether the ball is in contact with an object on the "Ground" layer or not.
    public GroundState CurrentGroundState;

    // The ball's rigidbody component handles its physics interactions.
    Rigidbody2D _ballRigidBody;

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

    /// <summary>
    /// Awake is called when the object is instantiated, before Start() and Update().
    /// Stores the rigidbody2D component, aiming indicator component and the click action from the Input Actions window for future use.
    /// </summary>
    void Awake()
    {
        _ballRigidBody = GetComponent<Rigidbody2D>();
        _aimingIndicator = GetComponentInChildren<AimingIndicator>();

        _clickTouchAction = InputSystem.actions.FindAction("ClickTouch");
    }

    /// <summary>
    /// Update is called once every frame.
    /// Polls touch/tap input and stores its coordinates.
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
        if (_aimingLaunch)
        {
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            mousePosition.z = Math.Abs(Camera.main.transform.position.z); // Needed because camera is not at the same z-coordinate as the rest of the scene.

            _indicatorPosition = Camera.current.ScreenToWorldPoint(mousePosition);
            _launchStrength = Math.Clamp(Vector2.Distance(transform.position, _indicatorPosition), 0, 5) * _forceMultiplier;
            _aimingIndicator.UpdateIndicator(true, _indicatorPosition, _launchStrength);
        }
    }

    /// <summary>
    /// Handler for click/touch (TODO) input, intended to be run when the user begins pressing.
    /// Checks if the current mouse position is within the ball's interaction radius, sets aiming status active if so.
    /// </summary>
    void HandleClick()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        mousePosition.z = Math.Abs(Camera.main.transform.position.z); // Needed because camera is not at the same z-coordinate as the rest of the scene.

        if (Vector2.Distance((Vector2)transform.position, (Vector2)Camera.main.ScreenToWorldPoint(mousePosition)) < _ballInteractionRadius)
            _aimingLaunch = true;
    }
    
    /// <summary>
    /// Handler for click/touch (TODO) input, intended to be run when the user releases.
    /// Checks if currently aiming; if so, unflags, hides the aiming indicator, then applies force.
    /// </summary>
    void HandleRelease()
    {
        if (_aimingLaunch)
        {
            _aimingLaunch = false;
            _aimingIndicator.UpdateIndicator(false);
            _ballRigidBody.AddForce(Vector2.Normalize((Vector2)transform.position - _indicatorPosition) * _launchStrength);
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
        CurrentGroundState = (collisionInfo.collider.gameObject.layer == LayerMask.NameToLayer("Ground")) ? GroundState.Grounded : CurrentGroundState;
    }

    /// <summary>
    /// This function is called whenever the ball object's collider (CircleCollider2D in this case) exits collision with another object in its "callbackLayers" list.
    /// Checks whether the object that the ball just left collision with is on the "Ground" layer and sets the ball's state to "Grounded" if so, maintains current
    /// state otherwise.
    /// </summary>
    /// <param name="collisionInfo">Holds details about the collision that the ball just exited.</param>
    void OnCollisionExit2D(Collision2D collisionInfo)
    {
        // Only reference for now (maybe in Start() if entry callback doesn't happen on frame 1) no need to store.
        CircleCollider2D ballCollider = GetComponent<CircleCollider2D>();
        CurrentGroundState = (!ballCollider.IsTouchingLayers(LayerMask.NameToLayer("Ground"))) ? GroundState.Airborne : CurrentGroundState;
    }
}
