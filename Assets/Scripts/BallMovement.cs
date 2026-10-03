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
    // The ball's rigidbody component handles its physics interactions.
    Rigidbody2D _ballRigidBody;

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
    /// Start is called once before the first execution of Update after the MonoBehaviour is created.
    /// Stores the rigidbody2D component and the click action from the Input Actions window for future use.
    /// </summary>
    void Start()
    {
        _ballRigidBody = GetComponent<Rigidbody2D>();

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
        // If so, translates the current mouse coordinates (touch TODO) from screen to world space coordinates and stores.
        if (_aimingLaunch)
        {
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            mousePosition.z = Math.Abs(Camera.main.transform.position.z); // Needed because camera is not at the same z-coordinate as the rest of the scene.

            _indicatorPosition = Camera.current.ScreenToWorldPoint(mousePosition);
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
    /// Checks if currently aiming; if so, unflags, determines force amount based on drag distance and a multiplier, then applies force.
    /// </summary>
    void HandleRelease()
    {
        if (_aimingLaunch)
        {
            _aimingLaunch = false;
            
            _launchStrength = Math.Clamp(Vector2.Distance(transform.position, _indicatorPosition), 0, 5) * _forceMultiplier;
            _ballRigidBody.AddForce(Vector2.Normalize((Vector2)transform.position - _indicatorPosition) * _launchStrength);
        }
    }
}
