using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Applies a continuous wind force to golf balls while they are 
/// inside the wind zone
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WindZone : MonoBehaviour
{
    
    [Header("Wind Zone Settings")]

    // Controls the direction the wind pushes the ball and particles
    [SerializeField, Tooltip("The direction in which the wind pushes the ball and particles.")]
    Vector2 _windDirection = Vector2.right;

    // Controls the strength of the wind force
    [SerializeField, Min(0f), Tooltip("The strength of the wind force applied to the ball.")]
    float _windStrength = 5f;

    // Controls how many wind particles are emitted per second
    [SerializeField, Min(0f), Tooltip("The number of wind particles emitted per second.")]
    float _particleRate = 20f;

    // Controls the width and height of the wind zone
    [SerializeField, Tooltip("The width and height of the wind zone, including its collider, particle area, and mask.")]
    Vector2 _windZoneSize = new Vector2(5f, 3f);

    [Header("Component References")]

    // Stores a reference to the wind particle system
    [SerializeField, Tooltip("The particle system used to display the wind.")]
    ParticleSystem _windParticles;

    // Stores a reference to the sprite mask that contains the particles
    [SerializeField, Tooltip("The sprite mask used to keep particles inside the wind zone.")]
    SpriteMask _windMask;

    // Stores a reference to the wind zone's box collider
    [SerializeField, Tooltip("The box collider that determines the wind zone's boundaries.")]
    BoxCollider2D _windCollider;

    /// <summary>
    /// Configures the wind zone collider as a trigger
    /// </summary>
    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    /// <summary>
    /// Updates the wind zone's collider, particle emission, particle area, and sprite mask
    /// whenever settings are changed in the Unity Inspector
    /// </summary>
    private void OnValidate()
    {
        // Prevents the wind zone from having a negative or zero size
        _windZoneSize.x = Mathf.Max(0.1f, _windZoneSize.x);
        _windZoneSize.y = Mathf.Max(0.1f, _windZoneSize.y);

        // Finds the wind zone's box collider if it is not assigned
        if (_windCollider == null)
        {
            _windCollider = GetComponent<BoxCollider2D>();
        }

        // Finds the wind particle system if it is not assigned
        if (_windParticles == null)
        {
            Transform particles = transform.Find("WindParticles");

            if (particles != null)
            {
                _windParticles = particles.GetComponent<ParticleSystem>();
            }
        }

        // Finds the sprite mask if it is not assigned
        if (_windMask == null)
        {
            Transform mask = transform.Find("WindMask");

            if (mask != null)
            {
                _windMask = mask.GetComponent<SpriteMask>();
            }
        }

        // Updates the wind zone collider size
        if (_windCollider != null)
        {
            _windCollider.size = _windZoneSize;
            _windCollider.isTrigger = true;
        }

        // Updates the particle emission rate and area
        if (_windParticles != null)
        {
            var emission = _windParticles.emission;
            emission.rateOverTime = _particleRate;

            var shape = _windParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(
                _windZoneSize.x,
                _windZoneSize.y,
                0.1f
            );
        }

        // Updates the sprite mask size to match the wind zone
        if (_windMask != null)
        {
            _windMask.transform.localScale = new Vector3(
                _windZoneSize.x,
                _windZoneSize.y,
                1f
            );
        }
    }

    /// <summary>
    /// Updates the wind particle direction and speed to match the configured
    /// wind force
    /// </summary>
    private void Update()
    {
        if (_windParticles == null)
            return;

        var velocity = _windParticles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;

        Vector2 direction = _windDirection.normalized;
        Vector2 particleVelocity = direction * _windStrength * 0.4f;

        velocity.x = new ParticleSystem.MinMaxCurve(particleVelocity.x);
        velocity.y = new ParticleSystem.MinMaxCurve(particleVelocity.y);
        velocity.z = new ParticleSystem.MinMaxCurve(0f);

        var main = _windParticles.main;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        main.startRotation = -angle * Mathf.Deg2Rad;

    }

    /// <summary>
    /// Applies wind force to a ball while it remains inside the wind zone and temporarily
    /// ignores ground friction
    /// </summary>
    /// <param name="other"> The collider inside the wind zone </param>
    private void OnTriggerStay2D(Collider2D other)
    {
        // Ensures only the ball's owner applies wind physics
        if (!other.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
        {
            return;
        }

        if (!networkObject.IsSpawned || !networkObject.IsOwner)
        {
            return;
        }

        if (!other.CompareTag("Ball"))
        {
            return;
        }

        if (!other.TryGetComponent<Rigidbody2D>(out Rigidbody2D ballRigidbody))
        {
            return;
        }

        Vector2 direction = _windDirection.normalized;

        // Prevent ground friction from opposing upward wind
        if (direction.y > 0 &&
            other.TryGetComponent<BallFriction>(out BallFriction ballFriction))
        {
            ballFriction.IgnoreFriction(1);
        }

        // Apply continuous wind force
        ballRigidbody.AddForce(
            direction * _windStrength,
            ForceMode2D.Force
        );
    }
}
