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
        float x = ca * vec.x - sa * vec.y;
        float y = sa * vec.x + ca * vec.y;
        return new Vector2(Mathf.Round(x * 100f) / 100f, Mathf.Round(y * 100f) / 100f);
    }

    /// <summary>
    /// Rotates Counter clockwise for positive inputs
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="deg"></param>
    /// <returns></returns>
    public static Vector2 RotateDegree(Vector2 vec, float deg)
    {
        return RotateRadians(vec, deg * Mathf.Deg2Rad);
    }

    public static float GetRadian(Vector2 vec)
    {
        vec = RotateDegree(vec.normalized, -90);
        float angle = Mathf.Atan2(vec.y, vec.x);
        if(vec.y < 0)
        {
            angle = 2* Mathf.PI + angle;
        }
        return angle;
    }
    public static float GetDegree(Vector2 vec)
    {
        return GetRadian(vec) * Mathf.Rad2Deg;
    }
}
