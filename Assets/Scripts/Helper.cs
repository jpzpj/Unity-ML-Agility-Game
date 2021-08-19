using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helper
{
    #region Helpers
    /// <summary>
    /// Check if the current rotation is beyond a certain clipping value
    /// </summary>
    /// <param name="rotation"></param>
    /// <returns></returns>
    static public bool IsRotationClipped(Quaternion rotation)
    {
        Vector3 currentRotation = rotation.eulerAngles;
        // Limit the rotation to a specific value
        if (currentRotation.x != AllowedRotation(currentRotation.x)) return true;
        if (currentRotation.z != AllowedRotation(currentRotation.z)) return true;
        return false;
    }

    /// <summary>
    /// Clip the amount of ration so the board will not turn
    /// </summary>
    /// <param name="angle">The suggested angle of ratation</param>
    /// <returns>The clipped angle of rotation</returns>
    static public float AllowedRotation(float angle)
    {
        float maxAngle = 45.0f;
        // remember angles are from 0..360!
        if (angle < maxAngle) return angle;
        if (angle > 360 - maxAngle) return angle;

        if (angle <= 180.0f) return maxAngle;
        return 360.0f - maxAngle;
    }

    /// <summary>
    /// Generate a random board position
    /// </summary>
    /// <returns>A rondom position on the board</returns>
    static public Vector3 RandomBallPosition()
    {
        Vector2 ballPos = new Vector2();
        do
        {
            ballPos.x = UnityEngine.Random.Range(-8.5f, 8.5f);
            ballPos.y = UnityEngine.Random.Range(-8.5f, 8.5f);
        } while (ballPos.magnitude < 6.5f); // Do not accept a position inside the arc

        return new Vector3(ballPos.x, 3.0f, ballPos.y);
    }

    /// <summary>
    /// Normalize the position (for ML) to [0 - +1]
    /// </summary>
    /// <param name="position"></param>
    /// <param name="magnitude">The maximum value</param>
    /// <returns></returns>
    static public Vector3 NormalizePosition(Vector3 position, float magnitude)
    {
        return new Vector3((position.x - -magnitude) / (2 * magnitude),
            (position.y + magnitude) / (2 * magnitude),
            (position.z + magnitude) / (2 * magnitude));
    }

    /// <summary>
    /// Rename the agents index number between parenthesis in a consistent way (always 2 digits)
    /// </summary>
    /// <param name="name"></param>
    /// <returns>The new name</returns>
    static public string PrettyName(string name)
    {
        for (int i = 0; i < 100; i++)
        {
            if (name.EndsWith($"({i})"))
            {
                return name.Replace($"({i})", $"({i,2:D2})");
            }
        }

        return name + " (00)";
    }
    #endregion
}
