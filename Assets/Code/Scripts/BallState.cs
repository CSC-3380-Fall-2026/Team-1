using System;
using UnityEngine;

/// <summary>
/// Contains information about the ball's state and handles changes to it.
/// </summary>
/// <remarks>
/// Will have information about status effects as well later.
/// Movement state information should be in the BallMovement component.
/// </remarks>
public class BallState : MonoBehaviour
{
    // The position of this ball in the GameMaster's ball list (therefore, also player list).
    [Tooltip("The position of this ball in the GameMaster's balls list.")]
    public int ballID;

    // The name of the player who controls this ball.
    public String playerName;
}
