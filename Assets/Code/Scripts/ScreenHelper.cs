using UnityEngine;

/// <summary>
/// Holds methods and properties to assist with screen related tasks.
/// </summary>
public class ScreenHelper
{
    /// <summary>
    /// A replacement for Unity's Camera.ScreenToWorldPoint function, as the original produced inaccurate results near edges.
    /// </summary>
    /// <param name="screenCoordinates">The distance of the point from the bottom-left of the screen in pixels.</param>
    /// <param name="camera">The camera whose viewport coordinates we are translating to world coordinates.</param>
    /// <returns>The coordinates of the point in world space.</returns>
    public static Vector2 ScreenToWorldCoordinates(Vector2 screenCoordinates, Camera camera)
    {
        // Finds the distance from the center of the camera's view to the edges in world units.
        // orthographicSize is half of the height of the viewport in world units.
        Vector2 cameraWorldCoordinatesCenterOffset = new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize);

        // Finds the percentage distance of the point from the edges, where 1.0 is the top/right, 0 is the center and -1.0 is the bottom/left.
        float screenPointXRelativeCenter = (screenCoordinates.x - Screen.width / 2) / (Screen.width / 2);
        float screenPointYRelativeCenter = (screenCoordinates.y - Screen.height / 2) / (Screen.height / 2);
        Vector2 screenCoordinatesOffset = new Vector2(screenPointXRelativeCenter, screenPointYRelativeCenter);

        // Left addition operand is the displacement from the origin. Camera might not be there, so offset is added to camera's position.
        return (screenCoordinatesOffset * cameraWorldCoordinatesCenterOffset) + (Vector2)camera.transform.position;
    }
}