using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Collection of async animation functions, unless otherwise stated animations last duration of the animation curve and start from whatever state object is given in
/// </summary>
public static class AsyncAnimation
{


    #region Transform Animations

    #region Local Transfomations

    public static async Task LocalScale(Transform target, AnimationCurve animationCurve, Vector3 newScale)
    {
        float duration = animationCurve.keys[animationCurve.length - 1].time;
        float startTime = Time.time;
        Vector3 startScale = target.localScale;
        while(Time.time - startTime < duration)
        {
            var t = animationCurve.Evaluate(Time.time - startTime);
            target.localScale = Vector3.LerpUnclamped(startScale, newScale, t);
            await Awaiters.NextFrame;
        }
        target.localScale = Vector3.LerpUnclamped(startScale, newScale, animationCurve.Evaluate(duration));
    }
    public static async Task LocalRotate(Transform target, AnimationCurve animationCurve, Vector3 newEuler)
    {
        float duration = animationCurve.keys[animationCurve.length - 1].time;
        float startTime = Time.time;
        Quaternion startEuler = target.localRotation;
        while(Time.time - startTime < duration)
        {
            var t = animationCurve.Evaluate(Time.time - startTime);
            target.localRotation = Quaternion.SlerpUnclamped(startEuler, Quaternion.Euler(newEuler), t);
            await Awaiters.NextFrame;
        }
        target.localRotation = Quaternion.Euler(newEuler);
    }

    public static async Task LocalTranslate(Transform target, AnimationCurve animationCurve, Vector3 newPosition)
    {
        float duration = animationCurve.keys[animationCurve.length - 1].time;
        float startTime = Time.time;
        Vector3 startPosition = target.localPosition;
        while(Time.time - startTime < duration)
        {
            var t = animationCurve.Evaluate(Time.time - startTime);
            target.localPosition = Vector3.LerpUnclamped(startPosition, newPosition, t);
            await Awaiters.NextFrame;
        }
        target.localPosition = Vector3.LerpUnclamped(startPosition, newPosition, animationCurve.Evaluate(duration));
    }

    #endregion Local Transfomations

    #region Global Transformations

    public static async Task GlobalRotate(Transform target, AnimationCurve animationCurve, Vector3 newEuler)
    {
        float duration = animationCurve.keys[animationCurve.length - 1].time;
        float startTime = Time.time;
        Quaternion startEuler = target.rotation;
        while(Time.time - startTime < duration)
        {
            var t = animationCurve.Evaluate(Time.time - startTime);
            target.rotation = Quaternion.SlerpUnclamped(startEuler, Quaternion.Euler(newEuler), t);
            await Awaiters.NextFrame;
        }
        target.rotation = Quaternion.Euler(newEuler);
    }

    public static async Task GlobalTranslate(Transform target, Vector3 newPosition, AnimationCurve animationCurve)
    {
        float duration = animationCurve.keys[animationCurve.length - 1].time;
        float startTime = Time.time;
        Vector3 startPosition = target.position;
        while(Time.time - startTime < duration)
        {
            var t = animationCurve.Evaluate(Time.time - startTime);
            target.position = Vector3.LerpUnclamped(startPosition, newPosition, t);
            await Awaiters.NextFrame;
        }
        target.position = Vector3.LerpUnclamped(startPosition, newPosition, animationCurve.Evaluate(duration));
    }
    public static async Task GlobalTranslate(Transform target, Vector3 newPosition, AnimationCurve animationCurve, float duration)
    {
        float startTime = Time.time;
        Vector3 startPosition = target.position;
        while(Time.time - startTime < duration)
        {
            var t = animationCurve.Evaluate((Time.time - startTime)/duration);
            target.position = Vector3.LerpUnclamped(startPosition, newPosition, t);
            await Awaiters.NextFrame;
        }
        target.position = Vector3.LerpUnclamped(startPosition, newPosition, animationCurve.Evaluate(1));
    }

    #endregion Global Transformations

    #endregion Transform Animations

    public static async Task TwoPointTranslate(Transform target, AnimationCurve animationCurve, Vector3 finalPosition, Vector3 curvePoint, float curveStrength)
    {
        float duration = animationCurve.keys[animationCurve.length - 1].time;
        float startTime = Time.time;
        Vector3 startPosition = target.position;
        while(Time.time - startTime < duration)
        {
            var t = animationCurve.Evaluate(Time.time - startTime);
            var LegOne = Vector3.LerpUnclamped(startPosition, curvePoint, Mathf.Pow(t, 2));
            var Legtwo = Vector3.LerpUnclamped(Vector3.zero, finalPosition - curvePoint, Mathf.Sqrt(t));
            target.position = LegOne + Legtwo;
            await Awaiters.NextFrame;
        }
    }

    public static async Task ColorLerp(SpriteRenderer target, AnimationCurve animationCurve, Color newColor)
    {
        float duration = animationCurve.keys[animationCurve.length - 1].time;
        float startTime = Time.time;
        Color startColor = target.color;
        while(Time.time - startTime < duration)
        {
            var t = animationCurve.Evaluate(Time.time - startTime);
            target.color = Color.LerpUnclamped(startColor, newColor, t);
            await Awaiters.NextFrame;
        }
        target.color = newColor;
    }

    /// <summary>
    /// Waits for animator to reach animation state of string, kinda dangerous as it relys on proper naming
    /// </summary>
    /// <param name="Animation"></param>
    /// <param name="animator"></param>
    /// <returns></returns>
    public static async Task WaitForAnimation(string Animation, Animator animator)
    {
        while(!animator.GetCurrentAnimatorStateInfo(0).IsName(Animation))
        {
            await Awaiters.NextFrame;
        }
    }

    /// <summary>
    /// Waits for animator to reach animation state of string, kinda dangerous as it relys on proper naming, little safer due to timeout
    /// </summary>
    /// <param name="Animation"></param>
    /// <param name="animator"></param>
    /// <returns></returns>
    public static async Task WaitForAnimation(string Animation, Animator animator, float timeOutDuration)
    {
        float startTime = Time.time;
        while(!animator.GetCurrentAnimatorStateInfo(0).IsName(Animation) && timeOutDuration <= Time.time - startTime)
        {
            await Awaiters.NextFrame;
        }
    }

}
