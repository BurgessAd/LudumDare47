using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public class AnimalMovement : MonoBehaviour
{
    [SerializeField]
    private float m_fMaximumWanderDistance;

    [SerializeField]
    private float m_fMaximumRunDistance;

    [SerializeField]
    private float m_fStuckTime;

    [SerializeField]
    private float m_fStuckSpeed;

    [SerializeField]
    private int m_iLayerMask;

    private Vector3 m_vDestination;
    private float m_fCurrentTimeStuck = 0.0f;
    private Vector3 m_vPositionLastFrame;
    private Transform m_tObjectTransform;
    private NavMeshAgent m_NavMeshAgent;

    //////////////////////////////////////////////////////////////////////////////////////////////
    void Awake()
    {
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
        if (Vector3.Distance(m_vPositionLastFrame, m_tObjectTransform.position)/Time.deltaTime < m_fStuckSpeed)
        {
            m_fCurrentTimeStuck += Time.deltaTime;
        }
        else 
        {
            m_fCurrentTimeStuck = 0.0f;
        }

        m_vPositionLastFrame = m_tObjectTransform.position;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////
    // function chooses a random destination within range m_fMaximumWanderDistance on the navmesh
    public bool ChooseRandomDestination() 
    {
        enabled = true;

        var randomDirection = Random.insideUnitSphere * m_fMaximumWanderDistance;

        randomDirection += m_tObjectTransform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, m_fMaximumWanderDistance, m_iLayerMask)) 
        {
            m_NavMeshAgent.SetDestination(hit.position);

            m_vDestination = hit.position;
            return true;
        }
        return false;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////
    public void Idle() 
    {
        enabled = false;
    }
    //////////////////////////////////////////////////////////////////////////////////////////////
    public float GetDistanceToTarget() 
    { 
        return Vector3.Distance(m_vDestination, m_tObjectTransform.position); 
    }
    //////////////////////////////////////////////////////////////////////////////////////////////
    public bool IsStuck() 
    { 
        return m_fCurrentTimeStuck > m_fStuckTime; 
    }

    //////////////////////////////////////////////////////////////////////////////////////////////
    // function chooses a destination within range m_fMaximumRunDistance directly away from objectTransform on the navmesh
    public bool RunAwayFromObject(Transform objectTransform)
    {
        enabled = true;

        Vector3 direction = Vector3.Normalize(m_tObjectTransform.position - objectTransform.position);

        Vector3 runTo = direction * m_fMaximumRunDistance + m_tObjectTransform.position;

        if (NavMesh.SamplePosition(runTo, out NavMeshHit hit, m_fMaximumRunDistance, m_iLayerMask))
        {
            m_NavMeshAgent.SetDestination(hit.position);

            m_vDestination = hit.position;
            return true;
        }
        return false;
    }
}
