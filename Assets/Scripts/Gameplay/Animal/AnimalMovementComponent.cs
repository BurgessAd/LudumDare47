﻿using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public class AnimalMovementComponent : MonoBehaviour
{
    [SerializeField]
    private float m_fMaximumWanderDistance;

    [SerializeField]
    private float m_fStuckTime;

    [SerializeField]
    private float m_fStuckSpeed;

    [SerializeField]
    private float m_RunSpeed;

    [SerializeField]
    private float m_IdleSpeed;

    [SerializeField]
    private float m_Acceleration;

    [SerializeField]
    private Rigidbody m_AnimalRigidBody;

    public float TimeOnGround { get; private set; }

    private Vector3 m_vDestination;
    private float m_fCurrentTimeStuck = 0.0f;
    private Vector3 m_vPositionLastFrame;
    private Transform m_tObjectTransform;
    private NavMeshAgent m_NavMeshAgent;
    private int m_iLayerMask;

    private StateMachine m_MovementStateMachine;


    //////////////////////////////////////////////////////////////////////////////////////////////
    void Awake()
    {
        m_iLayerMask = 1 << NavMesh.GetNavMeshLayerFromName("Default");
        m_tObjectTransform = GetComponent<Transform>();
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        m_vPositionLastFrame = m_tObjectTransform.position;
        m_vDestination = m_tObjectTransform.position;
        m_fCurrentTimeStuck = 0.0f;
        enabled = false;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        if (Vector3.Distance(m_vPositionLastFrame, m_tObjectTransform.position)/Time.deltaTime < m_fStuckSpeed && !HasReachedDestination())
        {
            m_fCurrentTimeStuck += Time.deltaTime;
        }
        else 
        {
            m_fCurrentTimeStuck = 0;
        }

        m_vPositionLastFrame = m_tObjectTransform.position;
        Debug.DrawLine(m_vDestination, m_tObjectTransform.position, Color.red);

        float m_fStuckPercentage = m_fCurrentTimeStuck / m_fStuckTime;
        Vector3 stuck1 = m_tObjectTransform.up * 3 + m_tObjectTransform.right;
        Vector3 stuck2 = m_tObjectTransform.up * 3 + m_tObjectTransform.right - m_tObjectTransform.right * 2 * m_fStuckPercentage;
        Debug.DrawLine(stuck1, stuck2, Color.red * (1 - m_fStuckPercentage) + Color.green * m_fStuckPercentage);
    }

    //////////////////////////////////////////////////////////////////////////////////////////////
    // function chooses a random destination within range m_fMaximumWanderDistance on the navmesh
    public bool ChooseRandomDestination() 
    {
        enabled = true;
        m_fCurrentTimeStuck = 0.0f;
        var randomDirection = Random.insideUnitSphere * m_fMaximumWanderDistance;

        randomDirection += m_tObjectTransform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 30, m_iLayerMask)) 
        {
            if (m_NavMeshAgent.SetDestination(hit.position)) 
            {
                m_vDestination = hit.position;
                return true;
            }
        }
        return false;
    }

    public void ClearDestination() 
    {
        m_NavMeshAgent.ResetPath();
    }

    public void RunInDirection(Vector3 dir) 
    {

        Vector3 targetUp;
        Vector3 targetForward = dir;
        if (Physics.Raycast(m_AnimalRigidBody.position + 0.5f * Vector3.up, -Vector3.up, out RaycastHit hit, 1, layerMask: (1 << 8)))
        {
            targetUp = hit.normal;
            targetForward = Vector3.ProjectOnPlane(targetForward, targetUp);
        }
        Vector3 currentVelocity = m_AnimalRigidBody.velocity;
        m_AnimalRigidBody.velocity = currentVelocity + targetForward * m_Acceleration * Time.deltaTime - currentVelocity.normalized * m_Acceleration * (currentVelocity.magnitude/m_RunSpeed) * Time.deltaTime;
    }



    //////////////////////////////////////////////////////////////////////////////////////////////
    public void Idle() 
    {
        enabled = false;
    }
    public void SetWalking() 
    {
        m_NavMeshAgent.speed = m_IdleSpeed;
    }

    public void SetRunning() 
    {
        m_NavMeshAgent.speed = m_RunSpeed;
    }
    //////////////////////////////////////////////////////////////////////////////////////////////
    public float GetDistanceToTarget() 
    { 
        return Vector3.Distance(m_vDestination, m_tObjectTransform.position); 
    }

    public bool HasReachedDestination() 
    {
        return Vector3.Distance(m_vDestination, m_tObjectTransform.position) < 0.5f;
    }

    public bool IsAtDestination() 
    {
        return Vector3.Distance(m_vDestination, m_tObjectTransform.position) < 1.0f || IsStuck();
    }

    public float GetDistanceToTransform(in Transform m_tTransform) 
    {
        return Vector3.Distance(m_tObjectTransform.position, m_tTransform.position);
    }

    public bool IsStanding() 
    {
        return true;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////
    public bool IsStuck() 
    { 
        return m_fCurrentTimeStuck > m_fStuckTime; 
    }

    //////////////////////////////////////////////////////////////////////////////////////////////
    // function chooses a destination within range m_fMaximumRunDistance directly away from objectTransform on the navmesh
    public bool RunAwayFromObject(Transform tRunAwayTransform, float runDistance)
    {
        enabled = true;
        m_fCurrentTimeStuck = 0.0f;
        Vector3 displacement = m_tObjectTransform.position - tRunAwayTransform.position;
        float distance = displacement.magnitude;
        Vector3 direction = displacement / distance;

        float distanceToRun = runDistance - distance;
        Vector3 runTo = direction * distanceToRun + m_tObjectTransform.position;

        if (NavMesh.SamplePosition(runTo, out NavMeshHit hit, distanceToRun, m_iLayerMask))
        {
            if (m_NavMeshAgent.SetDestination(hit.position)) 
            {
                m_vDestination = hit.position;
                return true;
            }
        }
        return false;
    }
}
