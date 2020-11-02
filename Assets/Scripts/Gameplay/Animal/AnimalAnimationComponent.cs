using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class AnimalAnimationComponent : MonoBehaviour
{

    [Header("Animation Curves")]
    [SerializeField]
    public AnimationCurve m_HopAnimationCurve;
    [SerializeField]
    public AnimationCurve m_TiltAnimationCurve;

    [Header("Animation Params")]
    [SerializeField]
    public float m_RunAnimationTime;
    [SerializeField]
    public float m_WalkAnimationTime;
    [SerializeField]
    public float m_EscapingAnimationTime;
    [SerializeField]
    public float m_TiltSizeMultiplier = 1.0f;
    [SerializeField]
    public float m_HopHeightMultiplier = 1.0f;
    [SerializeField]
    public float m_HorizontalMovementMultiplier = 1.0f;
    [SerializeField]
    public float m_WindupTime = 1.0f;
    [SerializeField]
    public float m_Phase = 1.0f;
    [SerializeField]
    public float m_fRotationSpeed;
    [Header("Object references")]
    [SerializeField]
    public Transform m_tAnimationTransform;
    [SerializeField]
    public Transform m_tParentObjectTransform;
    [SerializeField]
    public Rigidbody m_vCowRigidBody;
    [SerializeField]
    public NavMeshAgent m_Agent;


    [HideInInspector]
    public float m_TimeBeingPulled;
    [HideInInspector]
    public Vector3 m_vTargetForward;

    private float m_AnimationTime = 0.0f;
    private float m_TotalAnimationTime = 1.0f;
    private float m_CurrentAnimationTime;

    public float GetCurrentHopHeight => m_HopAnimationCurve.Evaluate((m_AnimationTime + m_Phase) % 1);
    public float GetCurrentTilt => m_TiltAnimationCurve.Evaluate(m_AnimationTime);


    private StateMachine m_AnimatorStateMachine;

    void Start()
    {
        m_AnimatorStateMachine = new StateMachine();
        m_AnimatorStateMachine.AddState(new AnimalStaggeredAnimationState(this));
        m_AnimatorStateMachine.AddState(new AnimalWalkingAnimationState(this));
        m_AnimatorStateMachine.AddState(new AnimalCapturedAnimationState(this));
        m_AnimatorStateMachine.AddState(new AnimalIdleAnimationState());
        m_AnimatorStateMachine.SetInitialState(typeof(AnimalIdleAnimationState));

        m_CurrentAnimationTime = UnityEngine.Random.Range(0.0f, m_TotalAnimationTime);
    }
    public void SetIdleAnimation() 
    {
        m_AnimatorStateMachine.RequestTransition(typeof(AnimalIdleAnimationState));
    }

    public void SetWalkAnimation() 
    {
        m_TotalAnimationTime = m_WalkAnimationTime;
        m_AnimatorStateMachine.RequestTransition(typeof(AnimalWalkingAnimationState));
    }

    public void SetRunAnimation() 
    {
        m_TotalAnimationTime = m_RunAnimationTime;
        m_AnimatorStateMachine.RequestTransition(typeof(AnimalWalkingAnimationState));
    }

    public void SetEscapingAnimation() 
    {
        m_TotalAnimationTime = m_EscapingAnimationTime;
        m_AnimatorStateMachine.RequestTransition(typeof(AnimalCapturedAnimationState));
    }

    public void SetStaggeredAnimation() 
    {
        m_TotalAnimationTime = m_WalkAnimationTime;
        m_AnimatorStateMachine.RequestTransition(typeof(AnimalStaggeredAnimationState));
    }

    public void SetDesiredLookDirection(Vector3 dir) 
    {
        m_vTargetForward = dir;
    }

    void FixedUpdate()
    {
        m_AnimatorStateMachine.Tick();

        m_CurrentAnimationTime = (m_CurrentAnimationTime + Time.deltaTime) % m_TotalAnimationTime;

        m_AnimationTime = m_CurrentAnimationTime / m_TotalAnimationTime;

    }

    public void WasPulled() 
    {
        m_TimeBeingPulled = 1.0f;
    }
}

public class AnimalIdleAnimationState : IState { }

public class AnimalStaggeredAnimationState : IState
{
    private AnimalAnimationComponent animator;

    private float m_TimeStaggered;
    public AnimalStaggeredAnimationState(AnimalAnimationComponent animator)
    {
        this.animator = animator;
    }
    public override void Tick()
    {
        Quaternion targetQuat = Quaternion.Euler(0, 0, 180 + animator.m_TiltSizeMultiplier * animator.GetCurrentTilt);
        animator.m_tAnimationTransform.localRotation = Quaternion.RotateTowards(animator.m_tAnimationTransform.localRotation, targetQuat, animator.m_fRotationSpeed * Time.fixedDeltaTime);

    }
    public override void OnEnter()
    {
        Vector3 targetUp = Vector3.up;
        if (Physics.Raycast(animator.m_tParentObjectTransform.position + 0.5f * Vector3.up, -2 * Vector3.up, out RaycastHit hit, 1, layerMask: (1 << 8)))
        {
            targetUp = hit.normal;
        }
        Quaternion currentAnimRotation = animator.m_tAnimationTransform.rotation;
        animator.m_vCowRigidBody.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(animator.m_vCowRigidBody.transform.forward, targetUp), targetUp);
        animator.m_tAnimationTransform.rotation = currentAnimRotation;
    }
}

public class AnimalWalkingAnimationState : IState
{
    private AnimalAnimationComponent animator;
    Vector3 m_vPositionLastFrame;
    public AnimalWalkingAnimationState(AnimalAnimationComponent animator)
    {
        this.animator = animator;
    }
    float m_fCurrentWindup = 0.0f;
    public override void Tick()
    {
        Vector3 velocity = animator.m_tParentObjectTransform.position - m_vPositionLastFrame;
        m_vPositionLastFrame = animator.m_tParentObjectTransform.position;

        Vector3 targetUp = animator.m_tAnimationTransform.up;
        Vector3 targetForward = animator.m_tAnimationTransform.forward;
        if (Physics.Raycast(animator.m_tParentObjectTransform.position + 0.5f * Vector3.up, -Vector3.up, out RaycastHit hit, 1, layerMask: (1 << 8)))
        {
            targetUp = hit.normal;
        }
        Quaternion currentBodyQuat = animator.m_tAnimationTransform.rotation;
        if (velocity.sqrMagnitude > 0.0001f)
        {
            targetForward = velocity.normalized;
        }
        Quaternion targetBodyQuat = Quaternion.LookRotation(targetForward, targetUp);
        float moveMult = Mathf.Clamp(Quaternion.Angle(targetBodyQuat, currentBodyQuat) / 20.0f, 0.01f, 1.0f);
        Quaternion currentQuat = Quaternion.RotateTowards(currentBodyQuat, targetBodyQuat, animator.m_fRotationSpeed * Time.deltaTime * moveMult);

        float hopHeight = animator.GetCurrentHopHeight;
        float tiltSize = animator.GetCurrentTilt;
        float speed = animator.m_Agent.velocity.magnitude;
        float multiplier = Mathf.Sign(speed - 1.0f);
        m_fCurrentWindup = Mathf.Clamp(m_fCurrentWindup + multiplier * Time.deltaTime, 0.0f, animator.m_WindupTime);
        float bounceMult = m_fCurrentWindup / animator.m_WindupTime;
        animator.m_tAnimationTransform.rotation = currentQuat * Quaternion.Euler(0, 0, bounceMult * animator.m_TiltSizeMultiplier * tiltSize);
        animator.m_tAnimationTransform.localPosition = animator.m_tAnimationTransform.right * bounceMult * animator.m_HorizontalMovementMultiplier * tiltSize + animator.m_tAnimationTransform.up * bounceMult * animator.m_HopHeightMultiplier * hopHeight;
    }
    public override void OnEnter()
    {
        m_vPositionLastFrame = animator.m_tParentObjectTransform.position;
        animator.m_vCowRigidBody.rotation = Quaternion.identity;
        m_fCurrentWindup = 0.0f;
    }
}

public class AnimalCapturedAnimationState : IState
{
    private AnimalAnimationComponent animator;
    private Vector3 m_vVelocityLastFrame;
    public AnimalCapturedAnimationState(AnimalAnimationComponent animator)
    {
        this.animator = animator;
    }
    public override void Tick()
    {
        float dirAlignment = Vector3.Dot(animator.m_vTargetForward, animator.m_vCowRigidBody.velocity.normalized);
        float velAlignment = Mathf.Min(1.0f, animator.m_vCowRigidBody.velocity.magnitude / 2.0f);
        float walkMult = Mathf.Max(0.0f, dirAlignment * velAlignment);
        float tiltSize = animator.GetCurrentTilt * walkMult;
        float hopHeight = animator.GetCurrentHopHeight * walkMult;


        Vector3 targetUp = animator.m_tAnimationTransform.up;
        Vector3 targetForward = animator.m_vTargetForward;
        if (Physics.Raycast(animator.m_tParentObjectTransform.position + 0.5f * Vector3.up, -Vector3.up, out RaycastHit hit, 1, layerMask: (1 << 8)))
        {
            targetUp = hit.normal;
            targetForward = Vector3.ProjectOnPlane(targetForward, targetUp);
        }

        animator.m_TimeBeingPulled = Mathf.Max(animator.m_TimeBeingPulled - Time.fixedDeltaTime / 0.3f, 0.0f);
        m_vVelocityLastFrame = animator.m_vCowRigidBody.velocity;
        animator.m_tAnimationTransform.localPosition = new Vector3(0, hopHeight * animator.m_HopHeightMultiplier * 0.5f);
        animator.m_tAnimationTransform.localRotation = Quaternion.RotateTowards(animator.m_tAnimationTransform.localRotation, Quaternion.Euler(animator.m_TimeBeingPulled * 60.0f, 0, animator.m_TiltSizeMultiplier * tiltSize), animator.m_fRotationSpeed * Time.fixedDeltaTime);
        animator.m_vCowRigidBody.rotation = Quaternion.RotateTowards(animator.m_vCowRigidBody.rotation, Quaternion.LookRotation(targetForward, targetUp), animator.m_fRotationSpeed * Time.fixedDeltaTime);
        animator.m_vCowRigidBody.angularVelocity *= (1 - 0.05f);
    }
}
