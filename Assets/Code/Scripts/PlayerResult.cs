using System;
using Unity.Collections;
using Unity.Netcode;

/// <summary>
/// This is a container for only the player information relevant to a player's performance in a course/hole
/// The GameMaster will maintain a list of PlayerResults for each hole in a course and the course in total.
/// </summary>
[Serializable]
public struct PlayerResult : INetworkSerializable
{
    // The name of the player whose results are recorded here.
    // Strings can't be serialized easily for network transmission, so byte structure used instead.
    public FixedString64Bytes PlayerName;

    // The score the player attained in the context this result applies to.
    // This class could be used to record the score for a singular hole or the cumulative course total.
    public float PlayerScore;

    // The number of hits made in the context this result applies to.
    // This class could be used to record the launch count for a singular hole or the cumulative course total.
    public int PlayerHits;

    // The amount of time spent in the context this result applies to.
    // This class could be used to record the time spent for a singular hole or the cumulative course total.
    public float PlayerTimeSpent;

    /// <summary>
    /// Handles serialization/deserialization of the player's results data for network transmission.
    /// </summary>
    /// <typeparam name="T">The implementation of IReaderWriter that performs (de)serialization.</typeparam>
    /// <param name="bufferSerializer">The object that contains and uses a serializer and deserializer.</param>
    public void NetworkSerialize<T>(BufferSerializer<T> bufferSerializer) where T : IReaderWriter
    {
        bufferSerializer.SerializeValue(ref PlayerName);
        bufferSerializer.SerializeValue(ref PlayerScore);
        bufferSerializer.SerializeValue(ref PlayerHits);
        bufferSerializer.SerializeValue(ref PlayerTimeSpent);
    }
}