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
    [SerializeField]
    public AnimationCurve m_StepSoundCurve;

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
    [SerializeField]
    private float m_fConfusionAnimWindupTime;
    [SerializeField]
    private float m_fConfusionRotationSpeed;
    [Header("Object references")]
    [SerializeField]
    public Transform m_tAnimationTransform;
    [SerializeField]
    public Transform m_tParentObjectTransform;
    [SerializeField]
    public Rigidbody m_vCowRigidBody;
    [SerializeField]
    public NavMeshAgent m_Agent;
    [SerializeField]
    private Transform m_ConfusionEffectTiltTransform;
    [SerializeField]
    private Transform m_ConfusionEffectRotationTransform;
    [SerializeField]
    private Transform m_DraggingParticlesTransform;

    [SerializeField]
    private GameObject m_GroundImpactEffectsPrefab;

    [SerializeField]
    private ParticleSystem m_DraggingParticleSystem;
    [SerializeField]
    private AudioManager m_AudioManager;

    [HideInInspector]
    public float m_TimeBeingPulled;
    [HideInInspector]
    public Vector3 m_vTargetForward;

    [SerializeField]
    private string m_AnimalCallSoundIdentifier;

    [SerializeField]
    private string m_AnimalStepSoundIdentifier;

    [SerializeField]
    private string m_AnimalImpactSoundIdentifier;

    public void PlayAnimalStepSound(float strength) 
    {
        m_AudioManager.Play(m_AnimalStepSoundIdentifier);
    }

    public void PlayAnimalCall() 
    {
        m_AudioManager.Play(m_AnimalCallSoundIdentifier);
    }

    public void PlayAnimalImpactSound() 
    {
        m_AudioManager.Play(m_AnimalImpactSoundIdentifier);
    }

    public void EnableDraggingParticles(bool enabled) 
    {
        if (enabled) 
        {
            m_DraggingParticleSystem.Play(true);
            
        }
        else 
        {
            m_DraggingParticleSystem.Stop(true);
        }
        
    }

    public void OrientDraggingParticles(Quaternion rotation, Vector3 position) 
    {
        m_DraggingParticlesTransform.rotation = rotation;
        m_DraggingParticlesTransform.position = position;
    }

    public void OnHitGround(Vector3 position, Quaternion rotation) 
    {
        Instantiate(m_GroundImpactEffectsPrefab, position, rotation);
    }

    private float m_AnimationTime = 0.0f;
    private float m_TotalAnimationTime = 1.0f;
    private float m_CurrentAnimationTime;
    private float m_fCurrentConfusionAnimTime;

    public float GetCurrentHopHeight => m_HopAnimationCurve.Evaluate((m_AnimationTime + m_Phase) % 1);
    public float GetCurrentTilt => m_TiltAnimationCurve.Evaluate(m_AnimationTime);

    public float GetCurrentStepSound => m_StepSoundCurve.Evaluate((m_AnimationTime + m_Phase) % 1);

    public AudioManager GetAudioManager => m_AudioManager;


    private StateMachine m_AnimatorStateMachine;

    void Start()
    {
        m_AnimatorStateMachine = new StateMachine();
        m_AnimatorStateMachine.AddState(new AnimalStaggeredAnimationState(this));
        m_AnimatorStateMachine.AddState(new AnimalWalkingAnimationState(this));
        m_AnimatorStateMachine.AddState(new AnimalCapturedAnimationState(this));
        m_AnimatorStateMachine.AddState(new AnimalIdleAnimationState());
        m_AnimatorStateMachine.SetInitialState(typeof(AnimalIdleAnimationState));
        m_DraggingParticleSystem.Stop();
        m_CurrentAnimationTime = UnityEngine.Random.Range(0.0f, m_TotalAnimationTime);

        AnimCoroutine = ConfusionRotationCoroutine();
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

    public void TriggerConfuseAnim() 
    {
        m_fConfusionAnimMultiplier = 1.0f;
        StartCoroutine(ConfusionRotationCoroutine());
    }

    public void RemoveConfuseAnim() 
    {
        m_fConfusionAnimMultiplier = -1.0f;
    }

    private IEnumerator AnimCoroutine;
    float m_fConfusionAnimMultiplier = 1.0f;

    public void SetDesiredLookDirection(Vector3 dir) 
    {
        m_vTargetForward = dir;
    }

    void FixedUpdate()
    {
        m_AnimatorStateMachine.Tick();

        m_CurrentAnimationTime = (m_CurrentAnimationTime + Time.fixedDeltaTime) % m_TotalAnimationTime;

        m_AnimationTime = m_CurrentAnimationTime / m_TotalAnimationTime;

    }

    public void WasPulled() 
    {
        m_TimeBeingPulled = 1.0f;
    }

    private IEnumerator ConfusionRotationCoroutine() 
    {
        m_fCurrentConfusionAnimTime = Mathf.Clamp(m_fCurrentConfusionAnimTime + m_fConfusionAnimMultiplier * Time.deltaTime, 0, m_fConfusionAnimWindupTime);
        while (m_fCurrentConfusionAnimTime != 0.0f) 
        {        
            float time = m_fCurrentConfusionAnimTime / m_fConfusionAnimWindupTime;
            m_ConfusionEffectRotationTransform.localScale = Vector3.one * time;
            m_ConfusionEffectRotationTransform.localRotation = Quaternion.AngleAxis(Time.deltaTime * m_fConfusionRotationSpeed, Vector3.up) * m_ConfusionEffectRotationTransform.localRotation;
            m_ConfusionEffectTiltTransform.localRotation = Quaternion.AngleAxis(Time.deltaTime * m_fConfusionRotationSpeed/2, -Vector3.up) * m_ConfusionEffectTiltTransform.localRotation;
            m_fCurrentConfusionAnimTime = Mathf.Clamp(m_fCurrentConfusionAnimTime + m_fConfusionAnimMultiplier * Time.deltaTime, 0, m_fConfusionAnimWindupTime);
            yield return null;
        }
    }
}

public class AnimalIdleAnimationState : IState { }

public class AnimalStaggeredAnimationState : IState
{
    private readonly AnimalAnimationComponent animator;
    public AnimalStaggeredAnimationState(AnimalAnimationComponent animator)
    {
        this.animator = animator;
    }

    public override void OnEnter()
    {
        animator.TriggerConfuseAnim();
    }
    public override void OnExit()
    {
        animator.RemoveConfuseAnim();
    }
}

public class AnimalWalkingAnimationState : IState
{
    private readonly AnimalAnimationComponent animator;
    Vector3 m_vPositionLastFrame;

    float lastStepSound = 0.0f;
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

        float stepSound = animator.GetCurrentStepSound;

        float hopHeight = animator.GetCurrentHopHeight;

        float tiltSize = animator.GetCurrentTilt;

        float speed = animator.m_Agent.velocity.magnitude;
        float multiplier = Mathf.Sign(speed - 1.0f);
        m_fCurrentWindup = Mathf.Clamp(m_fCurrentWindup + multiplier * Time.deltaTime, 0.0f, animator.m_WindupTime);
        if (stepSound > 0.5f && lastStepSound < 0.5f)
        {
            animator.PlayAnimalStepSound(m_fCurrentWindup / 2 + 0.5f);

        }
        lastStepSound = stepSound;

        float bounceMult = m_fCurrentWindup / animator.m_WindupTime;
        animator.m_tAnimationTransform.rotation = currentQuat * Quaternion.Euler(0, 0, bounceMult * animator.m_TiltSizeMultiplier * tiltSize);
        animator.m_tAnimationTransform.localPosition = animator.m_tAnimationTransform.right * bounceMult * animator.m_HorizontalMovementMultiplier * tiltSize + animator.m_tAnimationTransform.up * bounceMult * animator.m_HopHeightMultiplier * hopHeight;
    }
    public override void OnEnter()
    {
        m_vPositionLastFrame = animator.m_tParentObjectTransform.position;
        Quaternion savedrot = animator.m_tAnimationTransform.rotation;
        animator.m_vCowRigidBody.transform.rotation = Quaternion.identity;
        animator.m_tAnimationTransform.rotation = savedrot;
        m_fCurrentWindup = 0.0f;
    }
}

public class AnimalCapturedAnimationState : IState
{
    private readonly AnimalAnimationComponent m_Animator;
    bool particlesEnabled = false;
    public AnimalCapturedAnimationState(AnimalAnimationComponent animator)
    {
        m_Animator = animator;
    }
    public override void Tick()
    {
        float dirAlignment = Vector3.Dot(m_Animator.m_vTargetForward, m_Animator.m_vCowRigidBody.velocity.normalized);
        float velAlignment = Mathf.Min(1.0f, m_Animator.m_vCowRigidBody.velocity.magnitude / 2.0f);
        float walkMult = Mathf.Max(0.0f, dirAlignment * velAlignment);
        float tiltSize = m_Animator.GetCurrentTilt * walkMult;
        float hopHeight = m_Animator.GetCurrentHopHeight * walkMult;


        Vector3 targetUp = m_Animator.m_tAnimationTransform.up;
        Vector3 targetForward = m_Animator.m_vTargetForward;
        bool hashit = false;
        if (Physics.Raycast(m_Animator.m_tParentObjectTransform.position + 0.5f * Vector3.up, -Vector3.up, out RaycastHit hit, 1, layerMask: (1 << 8)))
        {
            hashit = true;
            targetUp = hit.normal;
            targetForward = Vector3.ProjectOnPlane(targetForward, targetUp);
        }
        Quaternion escapeDirection = Quaternion.LookRotation(targetForward, targetUp);
        if (m_Animator.m_TimeBeingPulled > 0.01f && hashit) 
        {
            if (!particlesEnabled) 
            {
                particlesEnabled = true;
                m_Animator.EnableDraggingParticles(true);
            }
            m_Animator.OrientDraggingParticles(escapeDirection, hit.point);
        }
        else if (particlesEnabled)
        {
            m_Animator.EnableDraggingParticles(false);
            particlesEnabled = false;
        }
        m_Animator.m_TimeBeingPulled = Mathf.Max(m_Animator.m_TimeBeingPulled - Time.fixedDeltaTime / 0.3f, 0.0f);
        m_Animator.m_tAnimationTransform.localPosition = new Vector3(0, hopHeight * m_Animator.m_HopHeightMultiplier * 0.5f);
        m_Animator.m_tAnimationTransform.localRotation = Quaternion.RotateTowards(m_Animator.m_tAnimationTransform.localRotation, Quaternion.Euler(m_Animator.m_TimeBeingPulled * 60.0f, 0, m_Animator.m_TiltSizeMultiplier * tiltSize), m_Animator.m_fRotationSpeed * Time.fixedDeltaTime);
        m_Animator.m_vCowRigidBody.rotation = Quaternion.RotateTowards(m_Animator.m_vCowRigidBody.rotation, escapeDirection, m_Animator.m_fRotationSpeed * Time.fixedDeltaTime);
        m_Animator.m_vCowRigidBody.angularVelocity *= (1 - 0.05f);
    }

    public override void OnExit()
    {
        particlesEnabled = false;
        m_Animator.EnableDraggingParticles(false);
    }
}
