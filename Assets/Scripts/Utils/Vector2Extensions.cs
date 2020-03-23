using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector2Extensions
{
    /// <summary>
    /// Rotates Counter clockwise for positive inputs
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="rad"></param>
    /// <returns></returns>
    public static Vector2 RotateRadians(Vector2 vec, float rad) {
        float ca = Mathf.Cos(rad);
        float sa = Mathf.Sin(rad);
        return new Vector2(ca * vec.x - sa * vec.y, sa * vec.x + ca * vec.y);
    }
    /// <summary>
    /// Rotates Counter clockwise for positive inputs
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="deg"></param>
    /// <returns></returns>
    public static Vector2 RotateDegree(Vector2 vec, float deg)
    {
        float ca = Mathf.Cos(deg * Mathf.Deg2Rad);
        float sa = Mathf.Sin(deg * Mathf.Deg2Rad);
        return new Vector2(ca * vec.x - sa * vec.y, sa * vec.x + ca * vec.y);
    }
}
