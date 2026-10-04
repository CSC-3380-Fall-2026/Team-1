using UnityEngine;

/// <summary>
/// A general controller of game state.
/// Performs general game functions that are not specific enough to be delegated to specialized objects and components.
/// </summary>
public class GameMaster : MonoBehaviour
{
    [SerializeField, Tooltip("The ball object to be created at the beginning of runtime.")]
    GameObject _ballPrefab;

    /// <summary>
    /// Start is called once before the first execution of Update after the MonoBehaviour is created.
    /// Currently just instantiates the ball at the origin.
    /// </summary>
    void Start()
    {
        Instantiate(_ballPrefab, Vector3.zero, Quaternion.identity);
    }
}
