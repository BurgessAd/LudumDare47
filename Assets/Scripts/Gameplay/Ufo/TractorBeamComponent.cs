using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TractorBeamComponent : MonoBehaviour
{
    [SerializeField]
    private Transform m_Transform;
    
    [SerializeField]
    private Collider m_BeamCollider;

    [SerializeField]
    private AnimationCurve m_HorizontalityCurve;

    [SerializeField]
    private float m_TargetAbductionVelocity;

    [SerializeField]
    private float m_AbductionAcceleration;

    [SerializeField]
    private float m_TractorBeamRadius;

    [SerializeField]
    private float m_TractorBeamLength;

    public event Action OnTractorBeamFinished;

    private readonly List<AbductableComponent> m_Abducting = new List<AbductableComponent>();

    private IEnumerator AbductingCoroutine;
    public float GetHeight => m_TractorBeamLength * 0.9f;

    private void Awake()
    {
        AbductingCoroutine = Abducting();
    }

    public void OnBeginTractorBeam() 
    {
        m_BeamCollider.enabled = true;
        StartCoroutine(AbductingCoroutine);
    }

    public void OnStopTractorBeam() 
    {
        m_BeamCollider.enabled = false;
        StopCoroutine(AbductingCoroutine);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out AbductableComponent abductable)) 
        {
            m_Abducting.Add(abductable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out AbductableComponent abductable))
        {
            m_Abducting.Remove(abductable);
        }
    }

    private float GetDistanceHorizontally(in Transform abductablePosition) 
    {
        Vector3 axis = -m_Transform.up;
        Vector3 offset = abductablePosition.position - m_Transform.position;
        return Vector3.Magnitude(offset - Vector3.Dot(axis, offset) * axis);
    }

    private float GetDistanceVertically(in Transform abductablePosition) 
    {
        Vector3 axis = -m_Transform.up;
        Vector3 offset = abductablePosition.position - m_Transform.position;
        return Vector3.Dot(axis, offset);
    }

    public IEnumerator Abducting() 
    {
        yield return new WaitForSecondsRealtime(1.0f);
        while (m_Abducting.Count > 0) 
        {
            for (int i = 0; i < m_Abducting.Count; i++) 
            {
                float angleAwayFromDirectlyUp = m_HorizontalityCurve.Evaluate( Mathf.Clamp(GetDistanceHorizontally(m_Abducting[i].GetTransform)/m_TractorBeamRadius , 0, 1)) * 90 * Mathf.Deg2Rad;
                Vector3 upDir = -m_Transform.up;
                Vector3 offset = m_Abducting[i].GetTransform.position - m_Transform.position;
                Vector3 outDir = offset - Vector3.Dot(upDir, offset) * upDir;
                Vector3 desiredVelocity = (upDir * Mathf.Cos(angleAwayFromDirectlyUp) + outDir * Mathf.Sin(angleAwayFromDirectlyUp)).normalized * m_TargetAbductionVelocity;
                Vector3 desiredVelocityDifference = desiredVelocity - m_Abducting[i].GetBody.velocity;
                // this line is incorrect - should minimise on each timestep
                float accelerationMagnitudeThisStep = Time.fixedDeltaTime * Mathf.Min(m_AbductionAcceleration, desiredVelocityDifference.magnitude);
                Vector3 accelerationThisStep = desiredVelocityDifference.normalized * accelerationMagnitudeThisStep;
                m_Abducting[i].GetBody.AddForce(accelerationThisStep, ForceMode.Acceleration);
            }
            yield return null;
        }
    }
}
