using UnityEngine;

public class HoleData : MonoBehaviour
{
    // A singleton. Only one of this component should exist per hole.
    public static HoleData Instance {private set; get;}

    // A list of gameObjects whose positions and rotations will determine where the balls are reset to.
    [Tooltip("GameObjects referenced by ID to tell out-of-bounds areas where to reset the ball to.")]
    public GameObject[] ResetLocationObjects;

    [Tooltip("A list of indices of resetLocationObjects on the list that determine where the spawn points are.")]
    public int[] SpawnPointIndices;

    /// <summary>
    /// This function is called when the object is first instantiated.
    /// Performs singleton initialization.
    /// </summary>
    void Awake()
    {
        // Singleton handling.
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    /// <summary>
    /// This function is called when all Awake() calls have been completed on other objects.
    /// Tells the GameMaster to spawn player balls.
    /// </summary>
    void Start()
    {
        // Difficult to ensure this singleton has been instantiated from the start course function in the GameMaster.
        // Easier to just call it here, since it is certain.
        GameMaster.Instance.SpawnBallsRPC();
    }
}
