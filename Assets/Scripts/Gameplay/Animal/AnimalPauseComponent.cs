using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimalPauseComponent : PauseComponent
{
    [SerializeField] private AnimalAnimationComponent m_Animator;
    [SerializeField] private AnimalMovementComponent m_AnimalMovement;
    [SerializeField] private AnimalComponent m_AnimalStateHandler;
    [SerializeField] private NavMeshAgent m_NavMeshAgent;
    [SerializeField] private Rigidbody m_RigidBody;

    private Vector3 m_BodyVelocity = Vector3.zero;
    private Vector3 m_BodyAngularVelocity = Vector3.zero;
    private bool m_bWasUsingNavmeshAgent = false;
    private bool m_bWasUsingRigidBody = false;

    public override void Pause()
    {
        if (m_NavMeshAgent.enabled) 
        {
            m_bWasUsingNavmeshAgent = true;
            m_BodyVelocity = m_NavMeshAgent.velocity;
        }
        if (!m_RigidBody.isKinematic) 
        {
            m_BodyVelocity = Vector3.zero;
            m_BodyAngularVelocity = Vector3.zero;
            m_RigidBody.velocity = Vector3.zero;
            m_RigidBody.angularVelocity = Vector3.zero;
            m_bWasUsingRigidBody = true;
            m_RigidBody.isKinematic = true;
            
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
        if (m_bWasUsingRigidBody) 
        {
            m_bWasUsingRigidBody = false;
            m_RigidBody.velocity = m_BodyVelocity;
            m_RigidBody.angularVelocity = m_BodyAngularVelocity;
        }

        m_Animator.enabled = true;
        m_AnimalMovement.enabled = true;
        m_AnimalStateHandler.enabled = true;
    }
}
