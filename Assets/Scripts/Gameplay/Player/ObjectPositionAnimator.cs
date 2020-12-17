using System.Collections;
using UnityEngine;
using System;


public class ObjectPositionAnimator : MonoBehaviour
{
    private Transform m_Transform;
    private void Awake()
    {
        m_Transform = GetComponent<Transform>();
    }

    public Action OnAnimComplete;

    private PositionAnimation[] positions = new PositionAnimation[3];
    private RotationAnimation rotation = new RotationAnimation();
    public void SetPositionCurve(in int i, in AnimationCurve animationCurve, in float lowVal, in float highVal) 
    {
        positions[i].animationCurve = animationCurve;
        positions[i].lowVal = lowVal;
        positions[i].highVal = highVal;
    }

    public void SetPositionCurves(in AnimationCurve curve, in Vector3 lowVals, in Vector3 highVals)
    {
        for (int i = 0; i < 3; i++) 
        {
            positions[i].animationCurve = curve;
            positions[i].lowVal = lowVals[i];
            positions[i].highVal = highVals[i];
        }
    }

    public void SetRotationCurve(in AnimationCurve curve, in Quaternion lowVal, in Quaternion highVal) 
    {
        rotation.animationCurve = curve;
        rotation.lowQuat = lowVal;
        rotation.highQuat = highVal;
    }

    public void StartAnimatingPosition(in float duration) 
    {
        StartAnimating(duration);
        StartCoroutine(AnimatePosition());
    }

    public void StartAnimatingRotation(in float duration) 
    {
        StartAnimating(duration);
        StartCoroutine(AnimateRotation());
    }

    private void StartAnimating(in float duration) 
    {
        timePassed = 0.0f;
        totalTime = duration;
        StartCoroutine(Animate());
    }

    public void StartAnimatingPositionAndRotation(in float duration) 
    {
        StartAnimating(duration);
        StartCoroutine(AnimatePosition());
        StartCoroutine(AnimateRotation());
    }

    // Update is called once per frame
    private float timePassed;

    private float totalTime;

    private IEnumerator Animate() 
    {
        while (timePassed < totalTime) 
        {
            timePassed += Time.deltaTime;
            yield return null;
        }
        OnAnimComplete?.Invoke();
    }

    private IEnumerator AnimatePosition() 
    {
        while (timePassed < totalTime)
        {
            float time = timePassed / totalTime;
            Vector3 position = Vector3.zero;
            position.x = positions[0].Evaluate(time);
            position.y = positions[1].Evaluate(time);
            position.z = positions[2].Evaluate(time);
            m_Transform.position = position;
            yield return null;
        }
    }
    private IEnumerator AnimateRotation()
    {
        while (timePassed < totalTime)
        {
            m_Transform.rotation = rotation.Evaluate(timePassed / totalTime);
            yield return null;
        }
    }
}

public struct PositionAnimation 
{
    public AnimationCurve animationCurve;
    public float lowVal;
    public float highVal;
    public float Evaluate(in float time) { return Mathf.LerpUnclamped(lowVal, highVal, animationCurve.Evaluate(time)); }
}

public struct RotationAnimation 
{
    public AnimationCurve animationCurve;
    public Quaternion lowQuat;
    public Quaternion highQuat;

    public Quaternion Evaluate(in float time) { return Quaternion.LerpUnclamped(lowQuat, highQuat, animationCurve.Evaluate(time)); }
}
