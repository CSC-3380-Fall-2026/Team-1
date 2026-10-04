using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Holds state and logic needed to dynamically draw an aiming indicator (arrow in this case).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class AimingIndicator : MonoBehaviour
{
    // The sprite renderer holds the sprite and information relevant to render it, like textures.
    SpriteRenderer _spriteRenderer;

    // The sprite whose vertices and triangles we will set dynamically to create the indicator
    Sprite _indicatorSprite;

    // The texture of the indicator sprite that will be drawn.
    public Texture2D IndicatorTexture;

    // Stores the number of pixels that amounts to one unit in unity distance.
    float _indicatorTexturePixelsPerUnitDistance = 100f;

    // A multiplier on the length of the launch strength indicator applied after it is mulitplied by the launch strength.
    [SerializeField, Tooltip("Influences the length of the launch strength arrow.\nMultiplied with launch strength.")]
    float _launchLengthMultiplier = 2f;

    // A cap on the length of the pointer portion of the indicator in world units
    [SerializeField, Tooltip("The maximum the indicator can stretch to the pointer in world units.")]
    float _maxIndicatorStretch = 3f;

    // A multiplier on the width of the inner part of the launch strength indicator's arrowhead's wings applied after it is mulitplied by the launch strength.
    [SerializeField, Tooltip("Influences the width of the inner part of the launch strength arrowhead's wings.\nMultiplied with launch strength."), Header("Arrowhead Wings")]
    float _arrowheadWingsInnerWidthMultiplier = 0.3f;

    // A multiplier that determines what percentage of the distance from the arrow indicator's center to the inner wing part the distance from the inner to outer part should be.
    [SerializeField, Tooltip("Influences the width of the outer part of the launch strength arrowhead's wings.\nMultiplied with launch strength.")]
    float _arrowheadWingsOuterWidthMultiplier = 1.5f;


    // Determines how far up the arrow's length the arrowhead wings will be applied. 1 = at the arrowhead, 0 = at the center.
    [SerializeField, Tooltip("Influences how far up the arrow's length the wings will be.\n1 = at the arrowhead, 0 = at the center.")]
    float _arrowheadWingsDistance = 0.7f;

    /// <summary>
    /// This function is called when the object is first instantiated, before Start() and Update()
    /// Gets a reference to the sprite renderer component on this object, creates a sprite and sets the renderer's sprite reference to the object we will modify.
    /// </summary>
    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        Rect indicatorTextureRect = new Rect(0, 0, IndicatorTexture.width, IndicatorTexture.height);
        Vector2 indicatorTextureCenter = new Vector2(0.5f, 0.5f);
        _indicatorSprite = Sprite.Create(IndicatorTexture, indicatorTextureRect, indicatorTextureCenter, _indicatorTexturePixelsPerUnitDistance);

        _spriteRenderer.sprite = _indicatorSprite;
        _spriteRenderer.enabled = false;
    }

    /// <summary>
    /// This function is called every frame.
    /// EXTERMELY INEFFICIENT: we don't want the child indicator object to rotate with the parent ball object, so we need to lock its rotation.
    /// Can't find other way but to set it every frame.
    /// </summary>
    void Update()
    {
        transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// Creates an arrow indicator sprite with vertices at the appropriate positions.
    /// </summary>
    /// <param name="screenSpaceCoordinates"></param> The screen coordinates of the mouse pointer or the finger when aiming.
    /// <param name="launchStrength"></param> The amount of force to be applied to the ball.
    /// <param name="showIndicator"></param> Determines whether or not the indicator will be visible.
    public void UpdateIndicator(bool showIndicator, Vector2 aimWorldCoordinates = default(Vector2), float launchStrength = 0)
    {
        // Hides the arrow by disabling the sprite renderer if showIndicator is true, shows it by enabling if not.
        if (!showIndicator)
        {
            _spriteRenderer.enabled = false;
            return;
        } else
            _spriteRenderer.enabled = true;
        
        // Offset to account for the fact that the center of the sprite is (width/2, height/2), not (0, 0).
        Vector2 textureOffset = new Vector2(IndicatorTexture.width / 2, IndicatorTexture.height / 2);

        // Calculates the position of the mouse pointer in the sprite's rect space.
        Vector2 aimLocalCoordinates = (aimWorldCoordinates - (Vector2)transform.parent.position) * _indicatorTexturePixelsPerUnitDistance;

        // Calculates the position of the tip of the arrow indicator's head by multiplying the direction of the vector opposing the pointer
        // by the magnitude of the force and our set modifier.
        Vector2 arrowheadLocation = -aimLocalCoordinates.normalized * launchStrength * _launchLengthMultiplier;

        // Limit the pointer portion of the indicator to our maximum value in world units. Recalculate texture units to do so.
        aimLocalCoordinates = Vector2.ClampMagnitude(aimWorldCoordinates - (Vector2)transform.parent.position, _maxIndicatorStretch) * _indicatorTexturePixelsPerUnitDistance;

        // Calculates the point along the arrow where the wings will branch out left and right.
        Vector2 arrowheadWingsOrigin = arrowheadLocation * _arrowheadWingsDistance;
        // Calculates the direction of the vector perpendicular to the arrow's length, then gives this vector the magnitude of the force on the
        // ball with our set multiplier to determine the length of the inner corners of the arrowhead.
        Vector2 arrowheadWingsPerpendicularVector = Vector2.Perpendicular(arrowheadWingsOrigin.normalized) * launchStrength * _arrowheadWingsInnerWidthMultiplier;

        // Calculates the position of the left and right inner corners of the arrowhead by adding the perpendicular vector to the branching point.
        Vector2 arrowheadWingsInnerLeft = arrowheadWingsOrigin + arrowheadWingsPerpendicularVector;
        Vector2 arrowheadWingsInnerRight = arrowheadWingsOrigin - arrowheadWingsPerpendicularVector;

        // Calculates the position of the left and right outer corners of the arrowhead by adding the perpendicular vector
        // again to the inner corners, multiplied by our set modifier to determine the length.
        Vector2 arrowHeadWingsOuterLeft = arrowheadWingsInnerLeft + (arrowheadWingsPerpendicularVector * _arrowheadWingsOuterWidthMultiplier);
        Vector2 arrowHeadWingsOuterRight = arrowheadWingsInnerRight - (arrowheadWingsPerpendicularVector * _arrowheadWingsOuterWidthMultiplier);

        // Applies offsets to the vertices' coordinates to account for displaced origin.
        aimLocalCoordinates += textureOffset;
        arrowheadLocation += textureOffset;
        arrowheadWingsInnerLeft += textureOffset;
        arrowheadWingsInnerRight += textureOffset;
        arrowHeadWingsOuterLeft += textureOffset;
        arrowHeadWingsOuterRight += textureOffset;

        // An array of the calculated vertices to be combined into triangles.
        Vector2[] aimingIndicatorArrowVertices = {
            aimLocalCoordinates,
            arrowheadLocation,
            arrowheadWingsInnerLeft,
            arrowheadWingsInnerRight,
            arrowHeadWingsOuterLeft,
            arrowHeadWingsOuterRight
        };

        /*
        Debug.LogFormat("0: ({0}, {1})\n1: ({2}, {3})\n2: ({4}, {5})\n3: ({6}, {7})\n4: ({8}, {9})\n5: ({10}, {11})",
        aimLocalCoordinates.x, aimLocalCoordinates.y,
        arrowheadLocation.x, arrowheadLocation.y,
        arrowheadWingsInnerLeft.x, arrowheadWingsInnerLeft.y,
        arrowheadWingsInnerRight.x, arrowheadWingsInnerRight.y,
        arrowHeadWingsOuterLeft.x, arrowHeadWingsOuterLeft.y,
        arrowHeadWingsOuterRight.x, arrowHeadWingsOuterRight.y);
        */

        // Creates the triangles from the given vertex indices.
        // Every three digits is a triangle made from the given vertex indices.
        ushort[] aimingIndicatorArrowTriangles = {0, 2, 3, 4, 5, 1};

        // Applies the triangles to the sprite.
        _indicatorSprite.OverrideGeometry(aimingIndicatorArrowVertices, aimingIndicatorArrowTriangles);
    }
}
