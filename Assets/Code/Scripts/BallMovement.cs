using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Rigidbody2D))]
public class BallMovement : MonoBehaviour
{
    // defines whether the ball is touching the ground
    public enum GroundState
    {
        Airborne,
        Grounded
    }

    // defines whether the ball is moving or stationary
    public enum MotionState
    {
        Stationary,
        Moving
    }

    public delegate void BallTurnBeganEvent(); // // notifies subscribed functions when the ball's turn begins
    public BallTurnBeganEvent BallTurnBegan;

    public delegate void BallTurnEndedEvent();
    public BallTurnEndedEvent BallTurnEnded;

    
    public delegate void BallBeganAimingEvent();
    public BallBeganAimingEvent BallBeganAiming;

  
    public delegate void BallMotionStateChangedEvent(GameObject thisBallObject, MotionState newMotionState);
    public BallMotionStateChangedEvent BallMotionStateChanged;

    
    public delegate void BallGroundedStateChangedEvent(GameObject thisBallObject, GroundState newGroundState);
    public BallGroundedStateChangedEvent BallGroundedStateChanged;

    
    public delegate void BallCurrentlyAimingEvent(Vector2 pointerWorldCoordinates = default(Vector2), float launchStrength = 0.0f);
    public BallCurrentlyAimingEvent BallCurrentlyAiming;

    // notifies subscribed functions when aiming stops
    public delegate void BallStoppedAimingEvent(bool successfullyLaunched = false);
    public BallStoppedAimingEvent BallStoppedAiming;

    public GroundState CurrentGroundState; 
    GroundState _previousGroundState; // stores the previous ground state

    public MotionState CurrentMotionState; 
    MotionState _previousMotionState; // stores the previous motion state

    [Tooltip("The number of times the ball has been launched"), Header("Launching")]
    public int LaunchCount = 0; // tracks the total number of launches

    [SerializeField, Tooltip("The minimum velocity at which the ball will be considered to be moving")]
    float _movementThreshold = 0.2f; // sets the minimum velocity needed to count as moving

    Rigidbody2D _ballRigidBody; // stores the ball's physics component
    BallFriction _ballFriction; 

    InputAction _clickTouchAction; 

    Camera _mainCamera; // stores the main camera used for mouse position calculations

    [SerializeField, Tooltip("The radius around the ball's center that can be clicked on to interact with it")]
    float _ballInteractionRadius = 1; // sets the area where the player can begin aiming

    [SerializeField, Tooltip("Multiplied with dragging distance capped at 5 units to calculate the final force applied")]
    float _forceMultiplier = 50; // controls the strength of the ball launch

    bool _aimingLaunch = false; 

    Vector2 _indicatorPosition; // stores the current mouse position in world coordinates

    float _launchStrength; 

    bool _hasHitGroundDuringCurrentLaunch = false; // prevents relaunching after the ball touches the ground

    bool _hasLaunched = false; // tracks whether the ball has been launched during the current turn

    [SerializeField, Tooltip("The rate of passage of time when aiming the ball\n1.0 = regular\n0.0 = frozen")]
    float _aimingTimeRate = 0.2f; // controls how quickly time passes while aiming

   
    void Awake()
    {
        // gets the components attached to the ball
        _ballRigidBody = GetComponent<Rigidbody2D>();
        _ballFriction = GetComponent<BallFriction>();

        
        _mainCamera = Camera.main;

        // finds the input action assigned in the input actions window
        if (InputSystem.actions != null)
        {
            _clickTouchAction = InputSystem.actions.FindAction("ClickTouch");
        }

        // checks whether the input action was successfully found
        if (_clickTouchAction == null)
        {
            Debug.LogError("clicktouch input action could not be found");
        }
    }

    void Update()
    {
        // stops updates if the required physics component is missing
        if (_ballRigidBody == null)
            return;

        // refreshes the camera reference if it is unavailable
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_clickTouchAction != null && _clickTouchAction.WasPressedThisFrame())
            HandleClick();

        if (_clickTouchAction != null && _clickTouchAction.WasReleasedThisFrame())
            HandleRelease();

        
        if (_aimingLaunch && CanLaunch())
        {
            if (Mouse.current != null && _mainCamera != null)
            {
                Vector3 mousePosition = Mouse.current.position.ReadValue(); // gets the current mouse position

                _indicatorPosition = ScreenHelper.ScreenToWorldCoordinates(mousePosition, _mainCamera); // converts the mouse position into world coordinates

                _launchStrength = CalculateLaunchStrength(
                    Vector2.Distance(transform.position, _indicatorPosition)
                );

                
                BallCurrentlyAiming?.Invoke(_indicatorPosition, _launchStrength);
            }
        }
        else if (_aimingLaunch && !CanLaunch())
        {
           
            _aimingLaunch = false; // stops aiming when launching is no longer allowed

            
            if (GameMaster.Instance != null) // restores the normal passage of time
                GameMaster.Instance.SetTimeRate();

            // notifies subscribed functions that aiming has stopped
            BallStoppedAiming?.Invoke(false);
        }

        
        _previousMotionState = CurrentMotionState; // stores the previous motion state before checking for changes

        
        CurrentMotionState = (_ballRigidBody.linearVelocity.magnitude > _movementThreshold) // // determines whether the ball is moving or stationary
            ? MotionState.Moving
            : MotionState.Stationary;

        // notifies subscribed functions when the motion state changes
        if (_previousMotionState != CurrentMotionState)
            BallMotionStateChanged?.Invoke(gameObject, CurrentMotionState);

        // checks whether the ball has stopped and is touching the ground
        bool ignoringFriction = _ballFriction != null && _ballFriction.IsIgnoringFriction();

        if (CurrentMotionState == MotionState.Stationary &&
            CurrentGroundState == GroundState.Grounded &&
            _hasLaunched &&
            !ignoringFriction)
        {
            // allows the player to launch the ball again
            _hasHitGroundDuringCurrentLaunch = false;

            // resets the launch tracking state
            _hasLaunched = false;

           d
            BallTurnEnded?.Invoke();

            // starts the next player's turn
            if (GameMaster.Instance != null)
                GameMaster.Instance.NextTurn();
        }
    }

    void HandleClick()
    {
        // checks whether the mouse and camera are available
        if (Mouse.current == null || _mainCamera == null)
            return;

        Vector3 mousePosition = Mouse.current.position.ReadValue(); // gets the current mouse position

        
        if (Vector2.Distance(
            (Vector2)transform.position,
            ScreenHelper.ScreenToWorldCoordinates(mousePosition, _mainCamera)
        ) < _ballInteractionRadius && CanLaunch())
        {
            // enables the aiming state
            _aimingLaunch = true;

            // slows down time while the player aims
            if (GameMaster.Instance != null)
                GameMaster.Instance.SetTimeRate(_aimingTimeRate);

        
            BallBeganAiming?.Invoke();
        }
    }

    void HandleRelease()
    {
        if (_aimingLaunch && CanLaunch())
        {
            // disables the aiming state
            _aimingLaunch = false;

            
            if (GameMaster.Instance != null)
                GameMaster.Instance.ResetChronoShot();

            // clears previous movement and force values
            _ballRigidBody.totalForce = Vector2.zero;
            _ballRigidBody.linearVelocity = Vector2.zero;
            _ballRigidBody.angularVelocity = 0;

            
            _ballRigidBody.AddForce(
                Vector2.Normalize((Vector2)transform.position - _indicatorPosition) * _launchStrength // // launches the ball in the direction opposite the pointer
            );

        
            _hasLaunched = true;

            
            LaunchCount++; // increases the total launch count

            
            BallStoppedAiming?.Invoke(true); // notifies subscribed functions that the ball was launched
        }
        else if (_aimingLaunch)
        {
            
            BallStoppedAiming?.Invoke(false);
        }
    }

    void OnCollisionEnter2D(Collision2D collisionInfo)
    {
        
        _previousGroundState = CurrentGroundState; // stores the previous ground state

        
        CurrentGroundState = (collisionInfo.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            ? GroundState.Grounded
            : CurrentGroundState;

        bool ignoringFriction = _ballFriction != null && _ballFriction.IsIgnoringFriction();

        
        if ((_previousGroundState != CurrentGroundState) && !ignoringFriction) // // notifies subscribed functions when the ground state changes
            BallGroundedStateChanged?.Invoke(gameObject, CurrentGroundState);

       
        if (!ignoringFriction)
            _hasHitGroundDuringCurrentLaunch = _hasLaunched;
    }

    void OnCollisionStay2D(Collision2D collisionInfo)
    {
        
        if (collisionInfo.collider.gameObject.layer == LayerMask.NameToLayer("Ground")) // // checks whether the ball is touching the ground layer
        {
            bool ignoringFriction = _ballFriction != null && _ballFriction.IsIgnoringFriction();

            
            if (!ignoringFriction) // prevents relaunching after ground contact
                _hasHitGroundDuringCurrentLaunch = _hasLaunched;
        }
    }

    void OnCollisionExit2D(Collision2D collisionInfo)
    {
        
        _previousGroundState = CurrentGroundState; // stores the previous ground state


        // checks whether the ball is still touching the ground
        CurrentGroundState = (!_ballRigidBody.IsTouchingLayers(LayerMask.GetMask("Ground")))
            ? GroundState.Airborne
            : CurrentGroundState;

        bool ignoringFriction = _ballFriction != null && _ballFriction.IsIgnoringFriction();

        // notifies subscribed functions when the ground state changes
        if ((_previousGroundState != CurrentGroundState) && !ignoringFriction)
            BallGroundedStateChanged?.Invoke(gameObject, CurrentGroundState);
    }

    public float CalculateLaunchStrength(float dragDistance)
    {
        return Mathf.Clamp(dragDistance, 0, 5) * _forceMultiplier;
    }

    bool CanLaunch()
    {
        // allows launching when stationary or airborne without ground contact
        return !_hasHitGroundDuringCurrentLaunch &&
               ((CurrentMotionState == MotionState.Stationary) ||
                (CurrentGroundState == GroundState.Airborne));
    }
}
