using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class FlightComponent : MonoBehaviour
{
    [SerializeField]
    private float m_MaximumAcceleration;
    [SerializeField]
    private float m_CruiseSpeed;
    [SerializeField]
    private float m_DistanceTolerance;
    [SerializeField]
    private float m_FinalSpeed;

    private Transform m_Transform;
    private Rigidbody m_Body;
    private Vector3 m_Destination;
    public IEnumerator m_FlightCoroutine;

    private void Awake()
    {
        m_Body = GetComponent<Rigidbody>();
        m_Transform = transform;
        UpdateAction = HasReachedDestination;
    }

    public void UpdateLinearDestination(in Vector3 destination) 
    {
        m_Destination = destination;
    }

    public void SetLinearDestination(in Vector3 destination) 
    {
        OnAutopilotEventCancelled?.Invoke();
        m_Destination = destination;
        UpdateAction = MovingToDestination;
    }

    public void SetTargetSpeed(in float finalVelocity) 
    {
        m_FinalSpeed = finalVelocity;
    }

    public void SetCruiseSpeed(in float cruiseVelocity) 
    {
        m_CruiseSpeed = cruiseVelocity;
    }

    public void StopFlight() 
    {
        UpdateAction = HasReachedDestination;
        accelDirection = Vector3.zero;
    }

    public void ResetFlightCallback()
    {
        OnAutopilotEventCompleted = null;
    }

    Vector3 accelDirection = Vector3.zero;

    private void MovingToDestination() 
    {
        Vector3 offsetFromDestination = m_Destination - m_Transform.position;
        // we need to both decellerate perpendicular velocity and accelerate linear velocity.
        // linear velocity can be accelerated/decellerate depending on wheth

        Vector3 normalizedTargetDirection = offsetFromDestination.normalized;



        Vector3 velParallel = normalizedTargetDirection * Vector3.Dot(normalizedTargetDirection, m_Body.velocity);
        Vector3 velPerpendicular = m_Body.velocity - velParallel;
        Vector3 acceleration = Vector3.zero;



        // if there's a perpendicular component we dont want
        if (velPerpendicular.sqrMagnitude > 1.0f) 
        {
            // slow down in that direction
            acceleration -= velPerpendicular.normalized * m_MaximumAcceleration * Time.fixedDeltaTime;
        }

        // if we need to slow down to reach target
        float distanceToAccelerate = (m_FinalSpeed * m_FinalSpeed - velParallel.sqrMagnitude) / (2 * m_MaximumAcceleration);
        Debug.Log(distanceToAccelerate);
        if (distanceToAccelerate * distanceToAccelerate > offsetFromDestination.sqrMagnitude) 
        {
            // slow down
            acceleration -= normalizedTargetDirection * m_MaximumAcceleration * Time.fixedDeltaTime;
        }
        // if we're over max speed
        else if (velParallel.sqrMagnitude > m_CruiseSpeed * m_CruiseSpeed) 
        {
            // slow down
            acceleration -= normalizedTargetDirection * m_MaximumAcceleration * Time.fixedDeltaTime;
        }
        // if we're under max speed
        else if (velParallel.sqrMagnitude < m_CruiseSpeed * m_CruiseSpeed) 
        {
            // speed up
            acceleration += normalizedTargetDirection * m_MaximumAcceleration * Time.fixedDeltaTime;
        }

        accelDirection = acceleration.normalized;

        m_Body.velocity += acceleration;

        if (offsetFromDestination.sqrMagnitude < m_DistanceTolerance * m_DistanceTolerance) 
        {
            accelDirection = Vector3.zero;
            UpdateAction = HasReachedDestination;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + accelDirection * 10);
    }

    private void HasReachedDestination() 
    {
        // get current velocity, slow down to zero.
        Vector3 currentVelocity = m_Body.velocity;

        float velocityChange = m_MaximumAcceleration * Time.fixedDeltaTime;

        float maximumVelocityChange = currentVelocity.magnitude;

        Vector3 acceleration = -currentVelocity.normalized * Mathf.Min(velocityChange, maximumVelocityChange);

        m_Body.velocity += acceleration;
    }

    private void FixedUpdate()
    {
        UpdateAction();
    }

    private Action UpdateAction;

    public event Action OnAutopilotEventCompleted;

    public event Action OnAutopilotEventCancelled;
}
