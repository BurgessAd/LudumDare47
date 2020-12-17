using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimalPauseComponent : PauseComponent
{
    [SerializeField]
    private AnimalAnimationComponent m_Animator;
    [SerializeField]
    private AnimalMovementComponent m_AnimalMovement;
    [SerializeField]
    private AnimalComponent m_AnimalStateHandler;
    [SerializeField]
    private NavMeshAgent m_NavMeshAgent;
    [SerializeField]
    private CowGameManager m_Manager;

    private Vector3 m_BodyVelocity = Vector3.zero;
    private Vector3 m_BodyAngularVelocity = Vector3.zero;
    private Vector3 m_NavDestination = Vector3.zero;
    private bool m_BodyWasUsingGravity = false;
    private bool m_bWasUsingNavmeshAgent = false;

    private void Start()
    {
        m_Manager.OnEntitySpawned(gameObject, EntityType.Prey);
    }

    public override void Pause()
    {
        m_NavDestination = m_NavMeshAgent.destination;

        if (m_NavMeshAgent.enabled) 
        {
            m_bWasUsingNavmeshAgent = true;
            m_BodyVelocity = m_NavMeshAgent.velocity;
        }

        m_NavMeshAgent.enabled = false;
        m_Animator.enabled = false;
        m_AnimalMovement.enabled = false;
        m_AnimalStateHandler.enabled = false;
    }

    public override void Unpause()
    {
        if (m_bWasUsingNavmeshAgent) 
        {
            m_NavMeshAgent.velocity = m_BodyVelocity;
            m_NavMeshAgent.enabled = true;
            m_bWasUsingNavmeshAgent = false;
        }

        m_Animator.enabled = true;
        m_AnimalMovement.enabled = true;
        m_AnimalStateHandler.enabled = true;
    }
}
