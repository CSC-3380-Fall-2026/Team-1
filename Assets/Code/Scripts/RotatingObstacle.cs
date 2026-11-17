using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Rotates an obstacle continuously around its visual center
/// or a custom rotation point
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class RotatingObstacle : NetworkBehaviour
{
    [SerializeField] float _rotationSpeed = 90f;
    [SerializeField] bool _clockwise = true;
    [SerializeField] bool _rotateAroundCenter = true;
    [SerializeField] Transform _customPivot;

    private Rigidbody2D _rigidbody2D;
    private Vector2 _rotationPoint;

    /// <summary>
    /// Initializes the Rigidbody2D and determines the rotation point
    /// </summary>
    private void Awake()
    {
        _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody2D = GetComponent<Rigidbody2D>();

        if (_rotateAroundCenter)
        {
            _rotationPoint = _rigidbody2D.position;
        }
        else
        {
            _rotationPoint = _customPivot != null
                ? _customPivot.position
                : _rigidbody2D.position;
        }
    }

    /// <summary>
    /// Rotates the obstacle around the selected point
    /// </summary>
    private void FixedUpdate()
    {

        if (!IsSpawned || !IsOwner)
            return;

        float direction = _clockwise ? -1f : 1f;
        float angle = direction * _rotationSpeed * Time.fixedDeltaTime;

        Vector2 offset = _rigidbody2D.position - _rotationPoint;
        Vector2 rotatedOffset = Quaternion.Euler(0f, 0f, angle) * offset;

        _rigidbody2D.MovePosition(_rotationPoint + rotatedOffset);
        _rigidbody2D.MoveRotation(_rigidbody2D.rotation + angle);
    }
}
