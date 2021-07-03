using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using EZCameraShake;

public class AnimalAnimationComponent : MonoBehaviour
{

    [Header("Animation Curves")]
    [SerializeField] public AnimationCurve m_HopAnimationCurve;
    [SerializeField] public AnimationCurve m_TiltAnimationCurve;
    [SerializeField] public AnimationCurve m_YawAnimationCurve;
    [SerializeField] public AnimationCurve m_ForwardBackwardAnimationCurve;
    [SerializeField] public AnimationCurve m_WalkHorizontalAnimationCurve;
    [SerializeField] public AnimationCurve m_StepSoundCurve;
    [SerializeField] public AnimationCurve m_DamagedHopAnimationCurve;
    [SerializeField] private AnimationCurve m_DamagedVisualsAnimationCurve;

    [Header("Movement Animation Durations")]
    [Range(0f, 2f)] [SerializeField] private float m_RunAnimationTime;
    [Range(0f, 2f)] [SerializeField] private float m_WalkAnimationTime;
    [Range(0f, 2f)] [SerializeField] private float m_EscapingAnimationTime;
    [Range(0f, 2f)] [SerializeField] private float m_WalkWindupTime = 1.0f;
    [Range(0f, 1f)] [SerializeField] private float m_Phase = 1.0f;
    [Range(0f, 720f)] [SerializeField] private float m_AnimRotationSpeed;
    [Range(0f, 60f)] [SerializeField] private float m_AnimMoveSpeed;

    [Header("Movement Animation Sizes")]
    [SerializeField] private float m_TiltSizeMultiplier = 1.0f;
    [SerializeField] private float m_HopHeightMultiplier = 1.0f;
    [SerializeField] private float m_YawSizeMultiplier = 1.0f;
    [SerializeField] private float m_HorizontalMovementMultiplier = 1.0f;
    [SerializeField] private float m_ForwardBackwardMovementMultiplier = 1.0f;


    [Header("Miscellaneous Params")]
    [SerializeField] private float m_AttackAnimationDuration = 1.0f;
    [SerializeField] private float m_DamagedAnimationDuration = 1.0f;
    [Range(0f, 0.5f)][SerializeField] private float m_AnimationSpeedRandom;
    [SerializeField] private float m_fAnimationSizeScalar;
    [SerializeField] AnimationCurve m_ImpactMagnitudeByImpactMomentum;

    [SerializeField] private float m_ConfusionAnimWindupTime;
    [SerializeField] private float m_ConfusionRotationSpeed;
    [ColorUsage(true, true)] [SerializeField] private Color m_DamagedColor;

    [Header("Object references")]
    [SerializeField] private Transform m_tAnimationTransform;
    [SerializeField] private Transform m_tParentObjectTransform;
    [SerializeField] private Rigidbody m_vCowRigidBody;
    [SerializeField] private NavMeshAgent m_Agent;
    [SerializeField] private Transform m_ConfusionEffectTiltTransform;
    [SerializeField] private Transform m_ConfusionEffectRotationTransform;
    [SerializeField] private Transform m_DraggingParticlesTransform;
    [SerializeField] private GameObject m_GroundImpactEffectsPrefab;

    [SerializeField] private ParticleEffectsController m_AlertEffectsController;
    [SerializeField] private ParticleEffectsController m_DraggingParticleController;
    [SerializeField] private ParticleEffectsController m_FreeFallingParticleController;
    [SerializeField] private ParticleEffectsController m_DamagedParticleController;
    [SerializeField] private ParticleEffectsController m_BashedParticleController;

    [SerializeField] private AudioManager m_AudioManager;
    [SerializeField] private List<MeshRenderer> m_DamagedMeshRenderers;
    [SerializeField] private CowGameManager m_Manager;
    [Header("Audio Identifiers")]
    [SerializeField] private string m_AnimalCallSoundIdentifier;
    [SerializeField] private string m_AnimalStepSoundIdentifier;
    [SerializeField] private string m_AnimalImpactSoundIdentifier;
    [Header("Breeding References")]


    [HideInInspector] public float m_TimeBeingPulled;
    [HideInInspector] public Vector3 m_vTargetForward;

    private AnimalComponent m_AnimalComponent;
    private float m_TotalAnimationTime = 1.0f;
    private float m_CurrentAnimationTime;
    private float m_fCurrentConfusionAnimTime;
    private float m_fAnimationSpeedRandomMult;

    public float GetCurrentHopHeight => m_HopHeightMultiplier * m_HopAnimationCurve.Evaluate((m_CurrentAnimationTime + m_Phase) % 1);
    public float GetCurrentTilt => m_TiltSizeMultiplier * m_TiltAnimationCurve.Evaluate(m_CurrentAnimationTime);
    public float GetCurrentForwardBackward => m_ForwardBackwardMovementMultiplier * m_ForwardBackwardAnimationCurve.Evaluate(m_CurrentAnimationTime);
    public float GetCurrentYaw => m_YawSizeMultiplier * m_YawAnimationCurve.Evaluate(m_CurrentAnimationTime);
    public float GetCurrentStepSound => m_StepSoundCurve.Evaluate((m_CurrentAnimationTime + m_Phase) % 1);
    public float GetCurrentHorizontalMovement => m_HorizontalMovementMultiplier * m_WalkHorizontalAnimationCurve.Evaluate((m_CurrentAnimationTime + m_Phase) % 1);
    public AudioManager GetAudioManager => m_AudioManager;



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

    public void TriggerDamageParticles(Vector3 damageDirection) 
    {
        m_DamagedParticleController.transform.rotation = Quaternion.LookRotation(damageDirection, Vector3.up);
        m_DamagedParticleController.IterateParticleSystems((ParticleSystem particleSystem) => particleSystem.Play());
    }

    public void EnableDraggingParticles(bool enabled) 
    {
        if (enabled) 
        {
            m_DraggingParticleController.IterateParticleSystems((ParticleSystem particleSystem) => particleSystem.Play(false));
        }
        else 
        {
            m_DraggingParticleController.IterateParticleSystems((ParticleSystem particleSystem) => particleSystem.Stop(false));
        }
        
    }

    public void OnHitGround(Vector3 position, Quaternion rotation, float momentum) 
    {
        GameObject resultObject = Instantiate(m_GroundImpactEffectsPrefab, position, rotation);
        float shakeStrength = Mathf.Clamp(momentum / Mathf.Sqrt((m_vCowRigidBody.position - m_Manager.GetPlayer.transform.position).magnitude) / 10, 3, 200);
        CameraShaker.Instance.ShakeOnce(shakeStrength, shakeStrength, 0.1f, 1.0f);
        resultObject.GetComponent<ImpactEffectStrengthManager>().SetParamsOfObject(m_ImpactMagnitudeByImpactMomentum.Evaluate(momentum));
    }

    private StateMachine m_AnimatorStateMachine;
    public void SetTargetDirection(Vector3 target) 
    {
        m_AnimatorStateMachine.SetParam("targetDirection", target.normalized);
    }

    private void SetParams()
    {
        if (m_AnimatorStateMachine != null) 
        {
            m_AnimatorStateMachine.SetParam("damagedAnimationDuration", m_DamagedAnimationDuration);

            m_AnimatorStateMachine.SetCallback("deathAnimationFinished", () => DeathAnimationFinished());

            m_AnimatorStateMachine.SetParam("walkWindupTime", m_WalkWindupTime);
            m_AnimatorStateMachine.SetParam("animationRotationSpeed", m_AnimRotationSpeed);
            m_AnimatorStateMachine.SetParam("animationLinearSpeed", m_AnimMoveSpeed);

            m_AnimatorStateMachine.SetParam("animationTransform", m_tAnimationTransform);
            m_AnimatorStateMachine.SetParam("mainTransform", m_tParentObjectTransform);

            m_AnimatorStateMachine.SetParam("animalBody", m_vCowRigidBody);
            m_AnimatorStateMachine.SetParam("animalAgent", m_Agent);

            m_AnimatorStateMachine.SetParam("damagedHopCurve", m_DamagedHopAnimationCurve);

            m_AnimatorStateMachine.SetParam("animationSizeScalar", m_fAnimationSizeScalar);

            m_AnimatorStateMachine.SetParam("damagedVisualsCurve", m_DamagedVisualsAnimationCurve);
            m_AnimatorStateMachine.SetParam("damagedMeshRenderers", m_DamagedMeshRenderers);
            m_AnimatorStateMachine.SetParam("damagedFlashColour", m_DamagedColor);
        }  
    }

    public void SetCurrentAttackAnimation(in AttackBase m_NewAttack) 
    {
        m_AnimatorStateMachine.SetParam("attackAnimationDuration", m_NewAttack.GetAttackDuration);
        m_AnimatorStateMachine.SetParam("attackForwardCurve", m_NewAttack.GetForwardCurve());
        m_AnimatorStateMachine.SetParam("attackHopCurve", m_NewAttack.GetHopCurve());
        m_AnimatorStateMachine.SetParam("attackPitchCurve", m_NewAttack.GetPitchCurve());
        m_AnimatorStateMachine.SetParam("attackTiltCurve", m_NewAttack.GetTiltCurve());
    }

    private void Awake()
    {
        m_fAnimationSpeedRandomMult = 1 + UnityEngine.Random.Range(-m_AnimationSpeedRandom, m_AnimationSpeedRandom);

        m_AnimatorStateMachine = new StateMachine(new AnimalIdleAnimationState());
        m_AnimatorStateMachine.AddState(new AnimalStaggeredAnimationState(this));
        m_AnimatorStateMachine.AddState(new AnimalWalkingAnimationState(this));
        m_AnimatorStateMachine.AddState(new AnimalCapturedAnimationState(this));
        m_AnimatorStateMachine.AddState(new AnimalFreeFallAnimationState(this, m_FreeFallingParticleController));
        m_AnimatorStateMachine.AddState(new AnimalAttackAnimationState());
        m_AnimatorStateMachine.AddState(new AnimalDamagedAnimationState());
        m_AnimalComponent = GetComponent<AnimalComponent>();

        SetParams();
    }

    void FixedUpdate()
    {
        m_AnimatorStateMachine.Tick();

        m_CurrentAnimationTime = (m_CurrentAnimationTime + (Time.fixedDeltaTime * m_fAnimationSpeedRandomMult) / m_TotalAnimationTime) % 1;
    }


    private void OnValidate()
	{
        SetParams();    
	}

	void Start()
    {
        m_AnimatorStateMachine.InitializeStateMachine();
        m_CurrentAnimationTime = UnityEngine.Random.Range(0.0f, 1.0f);
    }
    public void SetIdleAnimation() 
    {
        m_AnimatorStateMachine.RequestTransition(typeof(AnimalIdleAnimationState));
    }

    public void IsScared() 
    {
        m_AlertEffectsController.PlayOneShot();
    }

    public void HasSeenEnemy() 
    {
        m_AlertEffectsController.PlayOneShot();
    }

    public void HasSeenFood() 
    {
        m_AlertEffectsController.PlayOneShot();
    }

    public void SetWalkAnimation() 
    {
        m_TotalAnimationTime = m_WalkAnimationTime;
        m_AnimatorStateMachine.SetParam("runningAnimationDuration", m_WalkAnimationTime);
        m_AnimatorStateMachine.RequestTransition(typeof(AnimalWalkingAnimationState));
    }

    private void DeathAnimationFinished()   
    {
        Destroy(m_tParentObjectTransform.gameObject);
    }

    public void SetRunAnimation() 
    {
        m_TotalAnimationTime = m_RunAnimationTime;
        m_AnimatorStateMachine.SetParam("runningAnimationDuration", m_RunAnimationTime);
        m_AnimatorStateMachine.RequestTransition(typeof(AnimalWalkingAnimationState));
    }

    public void SetFreeFallAnimation() 
    {
        m_AnimatorStateMachine.RequestTransition(typeof(AnimalFreeFallAnimationState));
    }

    public void SetEscapingAnimation() 
    {
        m_TotalAnimationTime = m_RunAnimationTime;
        m_AnimatorStateMachine.SetParam("runningAnimationDuration", m_EscapingAnimationTime);
        m_AnimatorStateMachine.RequestTransition(typeof(AnimalCapturedAnimationState));
    }

    public void SetStaggeredAnimation() 
    {
        m_AnimatorStateMachine.RequestTransition(typeof(AnimalStaggeredAnimationState));
    }
    
    public void TriggerAttackAnimation(Action OnAnimationComplete, Action OnTriggerDamage) 
    {
        m_AnimatorStateMachine.SetCallback("attackCompleteCallback", OnAnimationComplete);
        m_AnimatorStateMachine.SetCallback("attackDamageCallback", OnTriggerDamage);
        m_AnimatorStateMachine.RequestTransition(typeof(AnimalAttackAnimationState));
    }

    public void TriggerDamagedAnimation(Action OnAnimationComplete) 
    {
        m_AnimatorStateMachine.SetCallback("damagedCompleteCallback", OnAnimationComplete);
        m_AnimatorStateMachine.RequestTransition(typeof(AnimalDamagedAnimationState));
    }

    public void TriggerBashedAnimation() 
    {

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

    float m_fConfusionAnimMultiplier = 1.0f;

    public void SetDesiredLookDirection(Vector3 dir) 
    {
        m_vTargetForward = dir;
    }

    public bool IsGrounded() 
    {
        return m_AnimalComponent.IsGrounded();
    }

    public ref Vector3 GetImpactPos() 
    {
        return ref m_AnimalComponent.GetLastContactPoint();
    }

    public ref Vector3 GetImpactNormal()   
    {
        return ref m_AnimalComponent.GetLastContactNormal();
    }

    public void WasPulled() 
    {
        m_TimeBeingPulled = 1.5f;
    }

    public void OnDead() 
    {
        m_AnimatorStateMachine.RequestTransition(typeof(AnimalIdleAnimationState));
    }

    private IEnumerator ConfusionRotationCoroutine() 
    {
         m_fCurrentConfusionAnimTime = Mathf.Clamp(m_fCurrentConfusionAnimTime + m_fConfusionAnimMultiplier * Time.deltaTime, 0, m_ConfusionAnimWindupTime);
        while (m_fCurrentConfusionAnimTime != 0.0f) 
        {        
            float time = m_fCurrentConfusionAnimTime / m_ConfusionAnimWindupTime;
            m_ConfusionEffectRotationTransform.localScale = Vector3.one * time;
            m_ConfusionEffectRotationTransform.localRotation = Quaternion.AngleAxis(Time.deltaTime * m_ConfusionRotationSpeed, Vector3.up) * m_ConfusionEffectRotationTransform.localRotation;
            m_ConfusionEffectTiltTransform.localRotation = Quaternion.AngleAxis(Time.deltaTime * m_ConfusionRotationSpeed/2, -Vector3.up) * m_ConfusionEffectTiltTransform.localRotation;
            m_fCurrentConfusionAnimTime = Mathf.Clamp(m_fCurrentConfusionAnimTime + m_fConfusionAnimMultiplier * Time.deltaTime, 0, m_ConfusionAnimWindupTime);
            yield return null;
        }
        m_ConfusionEffectRotationTransform.localScale = Vector3.zero;
    }
}

public class AnimalIdleAnimationState : AStateBase { }

public class AnimalBreedingAnimationState : AStateBase 
{
    private readonly AnimalAnimationComponent m_animator;
    private readonly ParticleEffectsController m_heartParticleController;
    public AnimalBreedingAnimationState(AnimalAnimationComponent animator, ParticleEffectsController heartController) 
    {
        this.m_heartParticleController = heartController;
        this.m_animator = animator;
    }

	public override void OnEnter()
	{
        m_heartParticleController.TurnOnAllSystems();
	}

	public override void OnExit()
	{
        m_heartParticleController.TurnOffAllSystems();
	}
}

public class AnimalFreeFallAnimationState : AStateBase 
{
    private readonly AnimalAnimationComponent animator;
    private readonly ParticleEffectsController dragController;
    bool m_bGroundedStateLastFrame;
    public AnimalFreeFallAnimationState(AnimalAnimationComponent animator, ParticleEffectsController dragController)
    {
        this.animator = animator;
        this.dragController = dragController;
    }

	public override void OnEnter()
	{
        m_bGroundedStateLastFrame = animator.IsGrounded();
        if (m_bGroundedStateLastFrame) dragController.TurnOnAllSystems();
    }
	public override void Tick()
	{
		if (animator.IsGrounded() != m_bGroundedStateLastFrame) 
        {
            m_bGroundedStateLastFrame = animator.IsGrounded();
            if (m_bGroundedStateLastFrame) dragController.TurnOnAllSystems();
            else dragController.TurnOffAllSystems();
        }

        if (animator.IsGrounded()) 
        {
            dragController.SetWorldPos(animator.GetImpactPos());
            dragController.SetLookDirection(animator.GetImpactNormal());
        }
	}


	public override void OnExit()
	{
        dragController.TurnOffAllSystems();
    }
}

public class AnimalStaggeredAnimationState : AStateBase
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

public class AnimalWalkingAnimationState : AStateBase
{
    private readonly AnimalAnimationComponent animator;
    Vector3 m_vPositionLastFrame;

    float lastStepSound = 0.0f;
    public AnimalWalkingAnimationState(AnimalAnimationComponent animator)
    {
        this.animator = animator;
    }

    private Transform m_AnimTransform;
    private NavMeshAgent m_Agent;
    private Rigidbody m_Body;
    private Transform m_MainTransform;
    private float m_WalkWindupTime;
    private float m_RotationSpeed;
    float m_fCurrentWindup = 0.0f;
    public override void Tick()
    {
        Vector3 velocity = m_MainTransform.position - m_vPositionLastFrame;
        m_vPositionLastFrame = m_MainTransform.position;

    
        Vector3 targetForward = m_AnimTransform.forward;
        Vector3 targetUp = m_AnimTransform.up;
        if (Physics.Raycast(m_MainTransform.position + 0.5f * Vector3.up, -Vector3.up, out RaycastHit hit, 1, layerMask: (1 << 8)))
        {
            targetUp = hit.normal;
            targetForward = Vector3.ProjectOnPlane(targetForward, targetUp).normalized;
        }
        Quaternion currentBodyQuat = m_AnimTransform.rotation;
        if (velocity.sqrMagnitude > 0.0001f)
        {
            targetForward = velocity.normalized;
        }
        Quaternion targetBodyQuat = Quaternion.LookRotation(targetForward, targetUp);
        float moveMult = Mathf.Clamp(Quaternion.Angle(targetBodyQuat, currentBodyQuat) / 20.0f, 0.01f, 1.0f);
        Quaternion currentQuat = Quaternion.RotateTowards(currentBodyQuat, targetBodyQuat, m_RotationSpeed * Time.deltaTime * moveMult);

        float stepSound = animator.GetCurrentStepSound;

        float hopHeight = animator.GetCurrentHopHeight;

        float tiltSize = animator.GetCurrentTilt;

        float horizontalMovement = 0;// animator.GetCurrentHorizontalMovement;

        float forwardBackwardMovement = animator.GetCurrentForwardBackward;

        float yawSize = animator.GetCurrentYaw;

        float speed = m_Agent.velocity.magnitude;
        float multiplier = Mathf.Sign(speed - 1.0f);
        m_fCurrentWindup = Mathf.Clamp(m_fCurrentWindup + multiplier * Time.deltaTime, 0.0f, m_WalkWindupTime);
        if (stepSound > 0.5f && lastStepSound < 0.5f)
        {
            animator.PlayAnimalStepSound(m_fCurrentWindup / 2 + 0.5f);

        }
        lastStepSound = stepSound;

        float bounceMult = m_fCurrentWindup / m_WalkWindupTime;
        m_AnimTransform.rotation = currentQuat * Quaternion.Euler(yawSize * bounceMult, 0, bounceMult * tiltSize);
        m_AnimTransform.localPosition = m_AnimTransform.forward * bounceMult * forwardBackwardMovement + m_AnimTransform.right * bounceMult * horizontalMovement + m_AnimTransform.up * bounceMult * hopHeight;
    }
    public override void OnEnter()
    {
        m_AnimTransform = GetParam<Transform>("animationTransform");
        m_MainTransform = GetParam<Transform>("mainTransform");
        m_WalkWindupTime = GetParam<float>("walkWindupTime");
        m_RotationSpeed = GetParam<float>("animationRotationSpeed");
        m_Agent = GetParam<NavMeshAgent>("animalAgent");
        m_Body = GetParam<Rigidbody>("animalBody");
        m_vPositionLastFrame = m_MainTransform.position;
        Quaternion savedrot = m_AnimTransform.rotation;
        m_Body.transform.rotation = Quaternion.identity;
        m_AnimTransform.rotation = savedrot;
        m_fCurrentWindup = 0.0f;
    }
}

public class AnimalDamagedAnimationState : AStateBase 
{
    private float m_CurrentAnimTime = 0.0f;

    private float m_TotalAnimationDuration;

    private float m_AnimLinearSpeed;
    private float m_AnimRotationSpeed;
    private List<MeshRenderer> m_VisualMeshRenderers;
    private Color m_FlashColour;
    private Transform m_AnimTransform;
    private AnimationCurve m_HopAnimationCurve;
    private AnimationCurve m_VisualAnimationCurve;
    public override void OnEnter()
    {
        m_CurrentAnimTime = 0.0f;
        m_TotalAnimationDuration = GetParam<float>("damagedAnimationDuration");
        m_HopAnimationCurve = GetParam<AnimationCurve>("damagedHopCurve");
        m_VisualAnimationCurve = GetParam<AnimationCurve>("damagedVisualsCurve");
        m_VisualMeshRenderers = GetParam<List<MeshRenderer>>("damagedMeshRenderers");
        m_FlashColour = GetParam<Color>("damagedFlashColour");
        m_AnimTransform = GetParam<Transform>("animationTransform");
        m_AnimLinearSpeed = GetParam<float>("animationLinearSpeed");
        m_AnimRotationSpeed = GetParam<float>("animationRotationSpeed");

        ForEachMeshRenderer(m_VisualMeshRenderers, (MeshRenderer meshRenderer) =>
        {
            meshRenderer.enabled = true;
        });

    }
    private void ForEachMeshRenderer(List<MeshRenderer> renderers, Action<MeshRenderer> action)
    {
        foreach (MeshRenderer renderer in renderers)
        {
            action.Invoke(renderer);
        }
    }
    public override void Tick()
    {
        if (m_CurrentAnimTime < 1.0f)
        {
            m_CurrentAnimTime += Time.deltaTime / m_TotalAnimationDuration;
            float colorSlider = m_VisualAnimationCurve.Evaluate(m_CurrentAnimTime);
            ForEachMeshRenderer(m_VisualMeshRenderers, (MeshRenderer renderer) =>
            {
                List<Material> rendererMats = new List<Material>();
                renderer.GetSharedMaterials(rendererMats);
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                for (int i = 0; i < rendererMats.Count; i++)
                {
                    renderer.GetPropertyBlock(block, i);
                    block.SetColor("_EmissionColor", m_FlashColour * colorSlider);
                    renderer.SetPropertyBlock(block, i);
                }
            });

  
            Vector3 desiredPosition = new Vector3(0, m_HopAnimationCurve.Evaluate(m_CurrentAnimTime), 0);
            m_AnimTransform.localPosition = Vector3.MoveTowards(m_AnimTransform.localPosition, desiredPosition, m_AnimLinearSpeed);



            if (m_CurrentAnimTime > 1.0f)
            {
                TriggerCallback("damagedCompleteCallback");
            }
        }
    }

	public override void OnExit()
	{
        ForEachMeshRenderer(m_VisualMeshRenderers, (MeshRenderer meshRenderer) =>
        {
            meshRenderer.enabled = false;
        });
    }
}

public class AnimalAttackAnimationState : AStateBase 
{
    private float m_TotalAnimationDuration;
    private float m_CurrentAnimTime = 0.0f;

    private Transform m_AnimTransform;
    bool m_HasDamaged = false;

    private float m_AnimLinearSpeed;
    private float m_AnimRotationSpeed;

    private AnimationCurve m_AttackHopCurve;
    private AnimationCurve m_AttackPitchCurve;
    private AnimationCurve m_AttackTiltCurve;
    private AnimationCurve m_AttackForwardCurve;
    private Quaternion m_startQuat;
    public override void OnEnter()
    {
        m_CurrentAnimTime = 0.0f;
        m_HasDamaged = false;
        m_TotalAnimationDuration = GetParam<float>("attackAnimationDuration");
        m_AnimTransform = GetParam<Transform>("animationTransform");
        m_AttackHopCurve = GetParam<AnimationCurve>("attackHopCurve");
        m_AttackPitchCurve = GetParam<AnimationCurve>("attackPitchCurve");
        m_AttackTiltCurve = GetParam<AnimationCurve>("attackTiltCurve");
        m_AttackForwardCurve = GetParam<AnimationCurve>("attackForwardCurve");
        m_AnimLinearSpeed = GetParam<float>("animationLinearSpeed");
        m_AnimRotationSpeed = GetParam<float>("animationRotationSpeed");
        m_startQuat = m_AnimTransform.localRotation;
    }

    public override void Tick()
    {
        if (m_CurrentAnimTime < m_TotalAnimationDuration) 
        {
            Quaternion lookQuat = Quaternion.LookRotation(GetParam<Vector3>("targetDirection"), Vector3.up);
            m_CurrentAnimTime += Time.deltaTime;
            float animTime = m_CurrentAnimTime / m_TotalAnimationDuration;

            float pitchAng = m_AttackPitchCurve.Evaluate(animTime);
            float tiltAng = m_AttackTiltCurve.Evaluate(animTime);
            float forwardAmount = m_AttackForwardCurve.Evaluate(animTime);
            float hopAmount = m_AttackHopCurve.Evaluate(animTime);

            Quaternion targetQuat = lookQuat * Quaternion.Euler(pitchAng, 0, tiltAng);
            m_AnimTransform.localRotation = Quaternion.RotateTowards(m_AnimTransform.localRotation, targetQuat, m_AnimRotationSpeed);
            
            Vector3 targetPos = m_startQuat * (new Vector3(0, hopAmount, forwardAmount));
            m_AnimTransform.localPosition = Vector3.MoveTowards(m_AnimTransform.localPosition, targetPos, m_AnimLinearSpeed);

  

            if (!m_HasDamaged && animTime > 0.5f) 
            {
                m_HasDamaged = true;
                TriggerCallback("attackDamageCallback");
            }        

            if (animTime > 1.0f)
            {
                TriggerCallback("attackCompleteCallback");
            }
        }
    }
}

public class AnimalCapturedAnimationState : AStateBase
{
    private readonly AnimalAnimationComponent m_Animator;
    bool particlesEnabled = false;
    private Transform m_AnimTransform;
    private Transform m_MainTransform;
    private NavMeshAgent m_Agent;
    private Rigidbody m_Body;
    private float m_RotationSpeed;
    public AnimalCapturedAnimationState(AnimalAnimationComponent animator)
    {
        m_Animator = animator;
    }
    public override void OnEnter()
    {
        m_AnimTransform = GetParam<Transform>("animationTransform");
        m_MainTransform = GetParam<Transform>("mainTransform");
        m_Agent = GetParam<NavMeshAgent>("animalAgent");
        m_Body = GetParam<Rigidbody>("animalBody");
        m_RotationSpeed = GetParam<float>("animationRotationSpeed");
    }
    public override void Tick()
    {
        float dirAlignment = Vector3.Dot(m_Animator.m_vTargetForward, m_Body.velocity.normalized);
        float velAlignment = Mathf.Min(1.0f, m_Body.velocity.magnitude / 2.0f);
        float walkMult = Mathf.Max(0.0f, dirAlignment * velAlignment);
        float tiltSize = m_Animator.GetCurrentTilt * walkMult;
        float hopHeight = m_Animator.GetCurrentHopHeight * walkMult;


        Vector3 targetUp = m_AnimTransform.up;
        Vector3 targetForward = m_Animator.m_vTargetForward;
        bool hashit = false;
        if (Physics.Raycast(m_MainTransform.position + 0.5f * Vector3.up, -Vector3.up, out RaycastHit hit, 1, layerMask: (1 << 8)))
        {
            Debug.DrawRay(m_MainTransform.position + 0.5f * Vector3.up, hit.point - (m_MainTransform.position + 0.5f * Vector3.up));
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
        }
        else if (particlesEnabled)
        {
            m_Animator.EnableDraggingParticles(false);
            particlesEnabled = false;
        }
        m_Animator.m_TimeBeingPulled = Mathf.Max(m_Animator.m_TimeBeingPulled - Time.fixedDeltaTime / 0.3f, 0.0f);
        m_AnimTransform.localRotation = Quaternion.RotateTowards(m_AnimTransform.localRotation, Quaternion.Euler(m_Animator.m_TimeBeingPulled * 60.0f, 0, tiltSize), m_RotationSpeed * Time.fixedDeltaTime);
        m_AnimTransform.localPosition = hopHeight * targetUp;

        float offsetFromDesired = Quaternion.Angle(m_Body.rotation, escapeDirection);

        Quaternion currentToDesired = Quaternion.Inverse(m_Body.rotation) * escapeDirection;
        currentToDesired.ToAngleAxis(out float angle, out Vector3 axis);
        float maximumTorque = 10.0f;
        float torqueSize = angle / 180.0f * maximumTorque;
        Vector3 totalTorque = axis * torqueSize;
        // apply a torque to get us to our desired offset
        m_Body.AddTorque(totalTorque);
        m_Body.rotation = Quaternion.RotateTowards(m_Body.rotation, escapeDirection, m_RotationSpeed * Time.fixedDeltaTime);
        m_Body.angularVelocity *= (1 - 0.05f);
    }

    public override void OnExit()
    {
        particlesEnabled = false;
        m_Animator.EnableDraggingParticles(false);
    }
}
