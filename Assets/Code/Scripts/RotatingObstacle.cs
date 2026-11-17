
using UnityEngine;

/// <summary>
/// Rotates an obstacle continuously around its visual center
/// or a custom rotation point
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class RotatingObstacle : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 90f;
    [SerializeField] bool clockwise = true;
    [SerializeField] bool rotateAroundCenter = true;
    [SerializeField] Transform customPivot;

    Rigidbody2D rigidbody2D;
    Vector2 rotationPoint;

    /// <summary>
    /// Initializes the Rigidbody2D and determines the rotation point
    /// </summary>
    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.bodyType = RigidbodyType2D.Kinematic;

        if (rotateAroundCenter)
        {
            Renderer obstacleRenderer = GetComponentInChildren<Renderer>();

            rotationPoint = obstacleRenderer != null
                ? obstacleRenderer.bounds.center
                : rigidbody2D.position;
        }
        else
        {
            rotationPoint = customPivot != null
                ? customPivot.position
                : rigidbody2D.position;
        }
    }

    /// <summary>
    /// Rotates the obstacle around the selected point
    /// </summary>
    private void FixedUpdate()
    {
        float direction = clockwise ? -1f : 1f;
        float angle = direction * rotationSpeed * Time.fixedDeltaTime;

        Vector2 offset = rigidbody2D.position - rotationPoint;
        Vector2 rotatedOffset = Quaternion.Euler(0f, 0f, angle) * offset;

        rigidbody2D.MovePosition(rotationPoint + rotatedOffset);
        rigidbody2D.MoveRotation(rigidbody2D.rotation + angle);
    }
}
