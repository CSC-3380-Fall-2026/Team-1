using System;
using Unity.Netcode;
using Unity.Collections;

/// <summary>
/// A struct containing information about an individual player.
/// </summary>
/// <remarks>
/// The list of players in the game needs to be synchronized across all clients and must therefore be
/// serialized for network transmission.
/// A struct that implements INetworkSerializable can be used to make a NetworkVariable or NetworkList.
/// To ensure only updates are shared, IEquatable must be implemented so the system can tell if the
/// object differs from its previous state.
/// Will contain the player's customization choices.
/// </remarks>
public struct Player : INetworkSerializable, IEquatable<Player>
{
    // The player's name.
    // Strings cannot easily be serialized for network communication, but FixedString64Bytes can.
    public FixedString64Bytes PlayerName;

    // The NetworkClientID associated with the device that owns this player.
    public ulong PlayerClientID;

    // The player's GUID, used to maintain a player reference if a player disconnects.
    // The ClientID could change when reconnecting, while the GUID will be constant for the game's duration.
    public FixedString64Bytes PlayerGUID;

    // The player's score on the current course.
    public float PlayerScore;

    // Later, a reference to a ScriptableObject with cosmetic choices.
    // Maybe a reference to a settings object for player-specific settings.

    /// <summary>
    /// Handles serialization/deserialization of the player's data for network transmission.
    /// </summary>
    /// <typeparam name="T">The implementation of IReaderWriter that performs (de)serialization.</typeparam>
    /// <param name="bufferSerializer">The object that contains and uses a serializer and deserializer.</param>
    public void NetworkSerialize<T>(BufferSerializer<T> bufferSerializer) where T : IReaderWriter
    {
        bufferSerializer.SerializeValue(ref PlayerName);
        bufferSerializer.SerializeValue(ref PlayerClientID);
        bufferSerializer.SerializeValue(ref PlayerGUID);
        bufferSerializer.SerializeValue(ref PlayerScore);
    }

    /// <summary>
    /// Determines whether a player struct is equal to another player struct.
    /// </summary>
    /// <param name="otherPlayer">The player struct this player struct is compared to.</param>
    /// <returns>True if both are equal, false otherwise.</returns>
    public bool Equals(Player otherPlayer)
    {
        // Returns false if any property on both are unequal.
        if (PlayerName != otherPlayer.PlayerName) return false;
        if (PlayerClientID != otherPlayer.PlayerClientID) return false;
        if (PlayerGUID != otherPlayer.PlayerGUID) return false;
        if (PlayerScore != otherPlayer.PlayerScore) return false;

        // Returns true if the function survived all the previous comparisons.
        return true;
    }
}