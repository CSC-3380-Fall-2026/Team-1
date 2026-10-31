using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// Contains information about the ball's state and handles changes to it.
/// </summary>
/// <remarks>
/// Will have information about status effects as well later.
/// Movement state information should be in the BallMovement component.
/// </remarks>
public class BallState : NetworkBehaviour
{
    // The position of this ball in the GameMaster's ball list (therefore, also player list).
    // The ID is set by the server pre-spawn on the clients, so that it can be used to place
    // the ball in the correct possition on the client's local balls list.
    [Tooltip("The position of this ball in the GameMaster's balls list.")]
    public NetworkVariable<int> BallID;

    // The name of the player who controls this ball.
    // Strings cannot easily be serialized for network transmission, but FixedString64Bytes can.
    public NetworkVariable<FixedString64Bytes> PlayerName;

    /// <summary>
    /// This function is called when the NetworkObject has been spawned and all attached NetworkBehaviors have been initialized.
    /// Handles enabling/disabling the movement component on turn change.
    /// </summary>
    protected override void OnNetworkPostSpawn()
    {
        // In case the base class' call was essential, as it is an override.
        base.OnNetworkPostSpawn();

        // GameObjects are too large to be serialized for network transmission, but each client needs active links to
        // the ball objects in the scene in player order. To get around this, the client's GameMaster will construct
        // the list locally.
        GameMaster.Instance.AddPlayerBall(gameObject);

        // A reference to the ball movement component on this object.
        BallMovement ballMovementComponent = GetComponent<BallMovement>();

        // There is no shared list of ball objects, only one local to each client, so the server cannot change their
        // states for all clients. In this case, changing turns involves disabling/enabling movement components.
        // Instead, the server changes the current player turn (which is a shared network variable) and the client
        // will disable/enable the BallMovement component of each ball on their local list based on if it is the
        // ball's turn and the client is the owner (networking-wise) of the ball's NetworkObject.
        GameMaster.Instance.CurrentPlayerTurn.OnValueChanged += (previousTurn, newTurn) =>
        {
            // Disables this ball's movement component if its ID is the previous turn's value.
            if (BallID.Value == previousTurn)
            {
                ballMovementComponent.BallTurnEnded?.Invoke();
                ballMovementComponent.enabled = false;
            }

            // Enabes this ball's movement component if its ID is the now current turn's value.
            // Additionally, only the NetworkObject's owner should be able to launch it.
            if (BallID.Value == newTurn && IsOwner)
            {
                ballMovementComponent.enabled = true;
                ballMovementComponent.BallTurnBegan?.Invoke();
            }
        };

        // The server cannot enable/disable the movement component on spawn, so the client
        // should do so by enabling/disabling the movement component if it is the first ball on the list
        // and the client owns it.
        if (BallID.Value == 0 && IsOwner)
        {
            ballMovementComponent.enabled = true;
            ballMovementComponent.BallTurnBegan?.Invoke();
        }
        else
            ballMovementComponent.enabled = false;
    }
}