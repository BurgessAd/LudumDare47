using UnityEngine;
using UnityEngine.AI;
using System;
using EZCameraShake;
using System.Collections.Generic;
using System.Collections;


[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(ThrowableObjectComponent))]
[RequireComponent(typeof(FreeFallTrajectoryComponent))]
public class AnimalComponent : MonoBehaviour
{
    [Header("Object references")]
    [SerializeField] protected CowGameManager m_Manager = default;

    [Header("Transforms")]
    [SerializeField] protected Transform m_AnimalMainTransform = default;
    [SerializeField] protected Transform m_AnimalLeashTransform = default;
    [SerializeField] protected Transform m_AnimalBodyTransform = default;
    [SerializeField] protected Collider m_AnimalBodyCollider = default;

    [Header("Variation Parameters")]
    [Range(0f, 0.3f)][SerializeField] protected float m_SizeVariation;

    [Header("Stagger parameters")]
    [SerializeField] AnimationCurve m_StaggerTimeByImpactMomentum;
    [SerializeField] float m_StaggerCooldown = 0.2f;

    [Header("Breeding Parameters")]
    [SerializeField] private float m_fBreedingChaseStartRange = default;
    [SerializeField] private float m_fBreedingChaseEndRange = default;
    [SerializeField] private float m_fBreedingStartRange = default;
    [SerializeField] private float m_fBreedingCooldownTime = default;
    [SerializeField] private float m_fBreedingHungerUsage = default;
    [SerializeField] private float m_fBreedingDuration = default;
    [SerializeField] private float m_fMaximumFullness = default;

    [Header("Idle parameters")]
    [SerializeField] protected float m_LowIdleTimer = default;
    [SerializeField] protected float m_HighIdleTimer = default;

    [Header("Fleeing parameters")]
    [SerializeField] protected float m_ScaredDistance = default;
    [SerializeField] protected float m_EvadedDistance = default;
    [SerializeField] protected float m_FleeCheckInterval = default;
    [SerializeField] protected float m_AnimalResistanceToPull = default;

    [Header("Hunting parameters")]
    [SerializeField] private float m_HuntBeginDistance = default;
    [SerializeField] private float m_HuntEndDistance = default;
    [SerializeField] private float m_fHuntCheckInterval = default;

    [SerializeField] private AttackBase m_DamageAttackType;
    [SerializeField] private AttackTypeDamage m_EatAttackType;

    private AttackBase m_CurrentAttackComponent;

    private bool m_bShouldStagger = false;
    private float m_fFullness = 0.0f;
    protected float m_TimeGrounded = 0.0f;
    private float m_CurrentStaggerCooldown;
    private float m_CurrentBreedingCooldown;

    private readonly Type[] m_CanStaggerStates = new Type[] {typeof(AnimalFreeFallState), typeof(AnimalLassoThrownState)};

    protected StateMachine m_StateMachine = default;
    protected AnimalMovementComponent m_AnimalMovement = default;
    protected FreeFallTrajectoryComponent m_FreeFallComponent = default;
    protected Rigidbody m_AnimalRigidBody = default;
    protected NavMeshAgent m_AnimalAgent = default;
    protected AnimalAnimationComponent m_AnimalAnimator = default;
    protected EntityTypeComponent m_EntityInformation = default;
    protected AbductableComponent m_AbductableComponent = default;
    protected HealthComponent m_AnimalHealthComponent = default;
    protected AttackBase m_AttackableComponent = default;
    protected ThrowableObjectComponent m_ThrowableComponent = default;

    #region Component Event Handlers

    public void OnPulledByLasso()
    {
        m_AnimalAnimator.WasPulled();
    }

    private void OnBeginAbducted()
    {
        IsInTractorBeam = true;
        m_EntityInformation.RemoveFromTrackable();
    }

    private void OnFinishedAbducted()
    {
        IsInTractorBeam = false;
        m_EntityInformation.AddToTrackable();
    }

    private void OnWrangledByLasso() 
    {
        m_StateMachine.SetParam("evadingTransform", m_Manager.GetPlayer.transform);
        IsWrangled = true;
    }

    private bool CanImpactHard() 
    {
        for (int i = 0; i< m_CanStaggerStates.Length; i++) 
        {
            if (m_StateMachine.GetCurrentState() == m_CanStaggerStates[i]) 
            {
                return true;
            }
        }      

        return false;
    }

    private void OnReleasedByLasso()
    {
        m_AnimalBodyCollider.enabled = true;
        IsWrangled = false;
    }

    protected bool ShouldEnterIdleFromWrangled() 
    {
        return (Vector3.Angle(GetGroundDir(), Vector3.up) < 20.0f);
    }

    private void OnThrownByLasso(ProjectileParams projectileParams)
    {
        m_AnimalBodyCollider.enabled = true;
        IsWrangled = false;
        m_StateMachine.RequestTransition(typeof(AnimalLassoThrownState));
    }

    private void OnTakeDamage(GameObject source, GameObject target, DamageType damageType, float currentHealthPercentage)
    {
        if (damageType == DamageType.PredatorDamage)
        {
            Vector3 damageDirection = target.transform.position - source.transform.position;
            m_AnimalAnimator.TriggerDamageParticles(damageDirection);
        }
        m_StateMachine.RequestTransition(typeof(AnimalDamagedState));
    }

    private void OnDead(GameObject source, GameObject target, DamageType damageType)
    {
		switch (damageType) 
        {
            case (DamageType.UFODamage):
                Destroy(gameObject);
                return;
            case (DamageType.PredatorDamage):
                Vector3 damageDirection = target.transform.position - source.transform.position;
                m_AnimalAnimator.TriggerDamageParticles(damageDirection);
                break;
            case (DamageType.FallDamage):
                StartCoroutine(DelayedDeathDestroy());
                break;
            default:
                Destroy(gameObject);
                break;
        }
        m_StateMachine.RequestTransition(typeof(AnimalDamagedState));
        IsDead = true;
        m_EntityInformation.RemoveFromTrackable();
    }

    private IEnumerator DelayedDeathDestroy() 
    {
        yield return new WaitForSeconds(2.0f);
        Destroy(gameObject);
    }

    private void OnStartedLassoSpinning()
    {
        m_AnimalBodyCollider.enabled = false;
        IsWrangled = true;
        DisablePhysics();
        m_AnimalAnimator.SetIdleAnimation();
        m_AnimalMainTransform.rotation = Quaternion.identity;
        m_StateMachine.RequestTransition(typeof(AnimalThrowingState));
    }

    private void OnHitGroundFromThrown(Collision collision)
    {
        OnHitGround(collision);
        m_StateMachine.RequestTransition(typeof(AnimalFreeFallState));
    }

    private void OnHitGround(Collision collision) 
    {
        Vector3 momentum = m_AnimalRigidBody.mass * m_AnimalRigidBody.velocity;
        float momentumInNormalDirection = -Vector3.Dot(collision.contacts[0].normal, momentum);
        if (momentumInNormalDirection > m_StaggerTimeByImpactMomentum.keys[0].time && CanImpactHard() && m_CurrentStaggerCooldown == 0) 
        {
            m_bShouldStagger = true;
            m_CurrentStaggerCooldown = m_StaggerCooldown;
            m_StateMachine.SetParam("staggerTime", m_StaggerTimeByImpactMomentum.Evaluate(momentumInNormalDirection));
            m_AnimalAnimator.OnHitGround(collision.GetContact(0).point, Quaternion.LookRotation(Vector3.forward, collision.GetContact(0).normal), momentumInNormalDirection);
        }
    }
    #endregion

	#region State Machine Callbacks
	private void RequestIdleState()
    {
        m_StateMachine.RequestTransition(typeof(AnimalIdleState));
    }
    private void SetManagedByAgent(bool enable) 
    {
        m_AnimalAgent.enabled = enable;
        if (m_AnimalAgent.isOnNavMesh)
            m_AnimalAgent.isStopped = false;
        m_AnimalAgent.updatePosition = enable;
        m_AnimalAgent.updateUpAxis = false;
        m_AnimalAgent.updateRotation = false;
    }

    private void DisablePhysics() 
    {
        m_AnimalRigidBody.isKinematic = true;
        m_AnimalRigidBody.useGravity = false;
    }

    private void SetGeneralPhysics() 
    {
        m_AnimalRigidBody.isKinematic = false;
        m_AnimalRigidBody.useGravity = true;
    }

    private void OnStaggered() 
    {
        m_bShouldStagger = false;
    }

    private void OnBreedingCompleted() 
    {
        OnSuccessfullyBred();
        Debug.Log("Successful breeding!");
        m_TargetEntity.GetComponent<AnimalComponent>().OnSuccessfullyBred();
        InitiateCancelBreedingAttempt();
    }

    private void SetAbductionPhysics() 
    {
        m_AnimalRigidBody.isKinematic = false;
        m_AnimalRigidBody.useGravity = false;
    }
    private void DamagedAnimationComplete()
    {
        m_StateMachine.RequestTransition(typeof(AnimalIdleState));
    }
    #endregion

    #region State Machine Transitions
    protected bool IsWrangled { get; private set; }
    protected bool IsInTractorBeam { get; private set; }
    protected bool IsDead { get; private set; }

    public Vector3 GetGroundDir() 
    {
        if (Physics.Raycast(m_AnimalMainTransform.position + 0.5f * Vector3.up, -Vector3.up, out RaycastHit hit, 1, layerMask: (1 << 8)))
        {
            return hit.normal;
        }
        return Vector3.up;
    }


    public void OnStruckByObject(in Vector3 velocity, in float mass) 
    {
        Vector3 momentum = velocity * mass;
        if (momentum.sqrMagnitude > m_StaggerTimeByImpactMomentum.keys[0].time * m_StaggerTimeByImpactMomentum.keys[0].time) 
        {
            m_StateMachine.SetParam("staggerTime", m_StaggerTimeByImpactMomentum.Evaluate(momentum.magnitude));
            m_StateMachine.RequestTransition(typeof(AnimalStaggeredState));
        }
        m_AnimalRigidBody.velocity += GetGroundDir() * momentum.magnitude / m_AnimalRigidBody.mass;
    }

    protected bool ShouldStopActionToEvadeNext() 
    {
        if (m_Manager.GetClosestTransformMatchingList(m_AnimalMainTransform.position, m_EntityInformation.GetEntityInformation.GetScaredOf, out EntityToken objToken, null))
        {
            float distSq = Vector3.SqrMagnitude(objToken.GetEntityType.GetTrackingTransform.transform.position - m_AnimalMainTransform.position);
            float distToEscSq = m_ScaredDistance * m_ScaredDistance * 1.0f;
            return distSq < distToEscSq;
        }
        return false;
    }

    protected bool ShouldEvade()
    {
        if (m_Manager.GetClosestTransformMatchingList(m_AnimalMainTransform.position, m_EntityInformation.GetEntityInformation.GetScaredOf, out EntityToken objToken, null))
        {
            float distSq = Vector3.SqrMagnitude(objToken.GetEntityType.GetTrackingTransform.position - m_AnimalMainTransform.position);
            float distToEscSq = m_ScaredDistance * m_ScaredDistance * 1.0f;
            if (distSq < distToEscSq)
            {
                SetEvadingAnimal(objToken.GetEntityType);
                m_StateMachine.SetParam("evadingTransform", objToken.GetEntityTransform);
                return true;
            }
        }
        return false;
    }

    protected bool HasEvadedEnemy()
    { 
        if (!m_TargetEntity)
            return true;
        float distSq = Vector3.SqrMagnitude(m_TargetEntity.GetTrackingTransform.position - m_AnimalMainTransform.position);
        float distToEscSq = m_EvadedDistance * m_EvadedDistance * 1.0f;
        return distSq > distToEscSq;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Chasing Data
    private float m_fLastAttackTime = -Mathf.Infinity;

    private bool CanHuntEnemy()
    {
        if (m_Manager.GetClosestTransformMatchingList(m_AnimalMainTransform.position, m_EntityInformation.GetEntityInformation.GetHunts, out EntityToken objToken, null))
        {
            if (TryHuntObject(objToken)) 
            {
                m_CurrentAttackComponent = m_EatAttackType;
                m_AnimalAnimator.SetCurrentAttackAnimation(m_EatAttackType);
                m_StateMachine.SetParam("attackDistance", m_CurrentAttackComponent.GetAttackRange);
                return true;
            }
        }

        if (m_Manager.GetClosestTransformMatchingList(m_AnimalMainTransform.position, m_EntityInformation.GetEntityInformation.GetAttacks, out EntityToken objAtkToken, null))
        {
            if (TryHuntObject(objAtkToken))
            {
                m_CurrentAttackComponent = m_DamageAttackType;
                m_AnimalAnimator.SetCurrentAttackAnimation(m_DamageAttackType);
                m_StateMachine.SetParam("attackDistance", m_CurrentAttackComponent.GetAttackRange);
                return true;
            }
        }
        return false;
    }
    bool TryHuntObject(in EntityToken objToken)
    {
        float distSq = Vector3.SqrMagnitude(objToken.GetEntityType.GetTrackingTransform.position - m_AnimalMainTransform.position);
        float distToEscSq = m_HuntBeginDistance * m_HuntBeginDistance;
        if (distSq < distToEscSq)
        {
            m_TargetEntity = objToken.GetEntityTransform.GetComponent<EntityTypeComponent>();
            objToken.GetEntityType.BeginTrackingObject(OnTargetInvalidated);
            m_StateMachine.SetParam("evadingTransform", objToken.GetEntityTransform);
            return true;
        }
        return false;
    }
    private bool CanAttackEnemy()
    {
        float distSq = Vector3.SqrMagnitude(Vector3.ProjectOnPlane(m_TargetEntity.GetTrackingTransform.position - m_AnimalMainTransform.position, Vector3.up));
        float distToEscSq = m_CurrentAttackComponent.GetAttackRange * m_CurrentAttackComponent.GetAttackRange;
        if (distSq < distToEscSq )
        {
            m_AnimalAgent.isStopped = true;
            if (Time.time - m_fLastAttackTime > m_CurrentAttackComponent.GetAttackCooldownTime)
            {
                m_fLastAttackTime = Time.time;
                return true;
            }
        }
        return false;
    }

    private void AttackAnimationComplete()
    {
        m_StateMachine.RequestTransition(typeof(AnimalIdleState));
    }

    private void AttemptAttackTarget()
    {
        m_CurrentAttackComponent.AttackTarget(m_TargetEntity.gameObject, targetDirection);
        OnTargetInvalidated();
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// General Target Data
    private EntityTypeComponent m_TargetEntity;
    private void OnTargetInvalidated()
    {
        m_TargetEntity.EndTrackingObject(OnTargetInvalidated);
        m_TargetEntity = null;
    }
    private bool HasLostTarget(float targetLostDistance)
    {
        if (!m_TargetEntity)
            return true;
        float distSq = Vector3.SqrMagnitude(m_TargetEntity.GetTrackingTransform.position - m_AnimalMainTransform.position);
        float distToEscSq = targetLostDistance * targetLostDistance;
        return distSq > distToEscSq;
    }

    private void SendTargetPosition()
    {
        if (m_TargetEntity != null)
        {
            Vector3 targetPositionGroundPlane = Vector3.ProjectOnPlane(m_TargetEntity.GetTrackingTransform.position - m_AnimalMainTransform.position, GetGroundDir()).normalized;
            targetDirection = targetPositionGroundPlane;
            m_AnimalAnimator.SetTargetDirection(targetPositionGroundPlane);
        }
    }
    Vector3 targetDirection;

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Breeding Data

    private bool m_bIsMated = true;
    private float m_fLastBreedingTime = Mathf.NegativeInfinity;
    private bool FoundBreedingPartner()
    {
        if (IsReadyToBreed())
        {
            if (m_Manager.GetClosestTransformsMatchingList(m_AnimalMainTransform.position, m_EntityInformation.GetEntityInformation.GetScaredOf, out List<EntityToken> objTokens))
            {
                for (int i = 0; i < objTokens.Count; i++)
                {
                    if (Vector3.SqrMagnitude(objTokens[i].GetEntityType.GetTrackingTransform.transform.position - m_AnimalMainTransform.position) < m_fBreedingChaseStartRange * m_fBreedingChaseStartRange)
                    {
                        if (objTokens[i].GetEntityType.TryGetComponent(out AnimalComponent animal))
                        {
							if (animal.OnRequestedAsBreedingPartner(gameObject)) 
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool OnRequestedAsBreedingPartner(GameObject partner) 
    {
        if (IsReadyToBreed() && !IsMated()) 
        {
            partner.GetComponent<AnimalComponent>().CompleteStartBreeding(gameObject);
            CompleteStartBreeding(partner);
            return true;
        }
        return false;
    }

    private void CompleteStartBreeding(GameObject partner) 
    {
        m_TargetEntity = partner.GetComponent<EntityTypeComponent>();
        m_TargetEntity.BeginTrackingObject(InitiateCancelBreedingAttempt);
        m_bIsMated = true;
    }

    private void InitiateCancelBreedingAttempt() 
    {
        AnimalComponent otherAnimal = m_TargetEntity.GetComponent<AnimalComponent>();
        otherAnimal.CompleteCancelBreedingAttempt();
        CompleteCancelBreedingAttempt();
    }

    public void OnSuccessfullyBred() 
    {
        m_fFullness -= m_fBreedingHungerUsage;
        m_fLastBreedingTime = Time.time;
    }

    public void CompleteCancelBreedingAttempt() 
    {
        m_TargetEntity = null;
        m_TargetEntity.EndTrackingObject(InitiateCancelBreedingAttempt);
        m_bIsMated = false;
    }

    private bool CanBreedPartner()
    {
        float distSq = Vector3.SqrMagnitude(Vector3.ProjectOnPlane(m_TargetEntity.GetTrackingTransform.position - m_AnimalMainTransform.position, Vector3.up));
        float distToEscSq = m_fBreedingStartRange * m_fBreedingStartRange;
        return distSq < distToEscSq;
    }

    public bool IsReadyToBreed()
    {
        return m_fFullness > m_fBreedingHungerUsage && Time.time - m_fLastBreedingTime > m_fBreedingCooldownTime;
    }

    public bool IsMated()
    {
        return m_bIsMated;
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Staggered Data
    private bool IsStaggered()
    {
        return m_StateMachine.GetCurrentState() == typeof(AnimalStaggeredState);
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Free Fall Data
    protected bool CanLeaveFreeFall()
    {
        return m_AnimalRigidBody.velocity.magnitude < 0.2f && m_AnimalMovement.IsNearNavMesh();
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Evading Data

    protected void SetEvadingAnimal(EntityTypeComponent evadingAnimal) 
    {
        m_TargetEntity = evadingAnimal;
        m_TargetEntity.BeginTrackingObject(ClearEvadingAnimal);
    }

    protected void ClearEvadingAnimal() 
    {
        m_TargetEntity.EndTrackingObject(ClearEvadingAnimal);
        m_TargetEntity = null;
    }


    #endregion

    #region Unity Functions
    private readonly List<ContactPoint> m_Contacts = new List<ContactPoint>();
    private Vector3 m_LastGroundedPosition = Vector3.zero;
    private Vector3 m_LastGroundedNormal = Vector3.zero;
    public bool IsGrounded()
    {
        return Time.time >= m_TimeGrounded;
    }
    public ref Vector3 GetLastContactPoint()
    {
        return ref m_LastGroundedPosition;
    }
    public ref Vector3 GetLastContactNormal()
    {
        return ref m_LastGroundedNormal;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (m_Manager.IsGroundLayer(collision.gameObject.layer))
        {
            OnHitGround(collision);
        }
    }
	private void OnCollisionStay(Collision collision)
	{

        if (m_Manager.IsGroundLayer(collision.gameObject.layer)) 
        {
            m_LastGroundedPosition = Vector3.zero;
            m_LastGroundedNormal = Vector3.zero;
            collision.GetContacts(m_Contacts);

            m_LastGroundedPosition = m_Contacts[0].point;
            m_LastGroundedNormal = m_Contacts[0].normal;
            m_LastGroundedNormal.Normalize();

            m_TimeGrounded = Time.time;
        }
	}
	private void OnCollisionExit(Collision collision)
    {
        if (m_Manager.IsGroundLayer(collision.gameObject.layer))
        {
            m_TimeGrounded = Mathf.Infinity;
        }
    }

    private void OnDamageObject(float damageAmount, GameObject target) 
    {
        m_fFullness = Mathf.Min(damageAmount + m_fFullness, m_fMaximumFullness);
    }

    protected virtual void Awake()
    {
        m_AnimalMainTransform.localScale = Vector3.one * (1 + UnityEngine.Random.Range(-m_SizeVariation, m_SizeVariation));

        m_AbductableComponent = GetComponent<AbductableComponent>();
        m_AnimalMovement = GetComponent<AnimalMovementComponent>();
        m_FreeFallComponent = GetComponent<FreeFallTrajectoryComponent>();
        m_AnimalRigidBody = GetComponent<Rigidbody>();
        m_AnimalAgent = GetComponent<NavMeshAgent>();
        m_AnimalAnimator = GetComponent<AnimalAnimationComponent>();
        m_EntityInformation = GetComponent<EntityTypeComponent>();
        m_AnimalHealthComponent = GetComponent<HealthComponent>();
        m_ThrowableComponent = GetComponent<ThrowableObjectComponent>();
        m_AttackableComponent = GetComponent<AttackBase>();

        m_ThrowableComponent.OnTuggedByLasso += OnPulledByLasso;
        m_ThrowableComponent.OnStartSpinning += OnStartedLassoSpinning;
        m_ThrowableComponent.OnThrown += OnThrownByLasso;
        m_ThrowableComponent.OnReleased += OnReleasedByLasso;
        m_ThrowableComponent.OnWrangled += OnWrangledByLasso;

        m_EatAttackType.OnDamagedTarget += OnDamageObject;

        m_FreeFallComponent.OnObjectHitGround += OnHitGroundFromThrown;

        m_AbductableComponent.OnStartedAbducting += (UfoMain ufo, AbductableComponent abductable) => OnBeginAbducted();
        m_AbductableComponent.OnEndedAbducting += (UfoMain ufo, AbductableComponent abductable) => OnFinishedAbducted();
        m_AnimalHealthComponent.OnTakenDamageInstance += OnTakeDamage;
        m_AnimalHealthComponent.OnEntityDied += OnDead;

        m_StateMachine = new StateMachine(new AnimalIdleState(m_AnimalMovement, m_AnimalAnimator));

        m_StateMachine.AddState(new AnimalEvadingState(m_AnimalMovement, m_AnimalAnimator));
        m_StateMachine.AddState(new AnimalWrangledState(m_AnimalMovement, m_AnimalAnimator));
        m_StateMachine.AddState(new AnimalAbductedState(m_AnimalMovement));
        m_StateMachine.AddState(new AnimalThrowingState(m_AnimalAnimator));
        m_StateMachine.AddState(new AnimalFreeFallState(m_AnimalAnimator));
        m_StateMachine.AddState(new AnimalStaggeredState(m_AnimalAnimator));
        m_StateMachine.AddState(new AnimalDamagedState(m_AnimalAnimator));
        m_StateMachine.AddState(new AnimalLassoThrownState(m_AnimalAnimator));
        m_StateMachine.AddState(new AnimalDeathState(m_AnimalAnimator));
        m_StateMachine.AddState(new AnimalPredatorChaseState(m_AnimalMovement, m_AnimalAnimator));
        m_StateMachine.AddState(new AnimalAttackState(m_AnimalAnimator));
        m_StateMachine.AddState(new AnimalBreedingChaseState(m_AnimalMovement, m_AnimalAnimator));
        m_StateMachine.AddState(new AnimalBreedingState());

        m_StateMachine.AddStateGroup(StateGroup.CreateWithOnExit(InitiateCancelBreedingAttempt, typeof(AnimalBreedingChaseState), typeof(AnimalBreedingState)));
        m_StateMachine.AddStateGroup(StateGroup.CreateWithOnExit(OnTargetInvalidated, typeof(AnimalPredatorChaseState), typeof(AnimalAttackState)));

        m_StateMachine.SetParam("navmeshAgent", m_AnimalAgent);
        m_StateMachine.SetParam("animalAnimator", m_AnimalAnimator);
        m_StateMachine.SetParam("rigidBody", m_AnimalRigidBody);
        m_StateMachine.SetParam("mainTransform", m_AnimalMainTransform);
        m_StateMachine.SetParam("bodyTransform", m_AnimalBodyTransform);

        m_StateMachine.SetParam("fleeCheckInterval", m_FleeCheckInterval);
        m_StateMachine.SetParam("scaredDistance", m_ScaredDistance);
        m_StateMachine.SetParam("evadedDistance", m_EvadedDistance);
        m_StateMachine.SetParam("lowIdleTimer", m_LowIdleTimer);
        m_StateMachine.SetParam("highIdleTimer", m_HighIdleTimer);

        m_StateMachine.SetCallback("attackAnimationComplete", AttackAnimationComplete);
        m_StateMachine.SetCallback("triggerDamage", AttemptAttackTarget);

        m_StateMachine.SetParam("huntBeginDistance", m_HuntBeginDistance);
        m_StateMachine.SetParam("huntEndDistance", m_HuntEndDistance);
        m_StateMachine.SetParam("huntCheckInterval", m_fHuntCheckInterval);

        m_StateMachine.SetParam("breedingTime", m_fBreedingDuration);

        m_StateMachine.SetCallback("stopBeingThrown", () => { m_FreeFallComponent.StopThrowingObject(); });
        m_StateMachine.SetCallback("sendTargetPosition", () => SendTargetPosition());

        m_StateMachine.SetCallback("onStaggered", OnStaggered);
        m_StateMachine.SetCallback("damagedAnimationComplete", DamagedAnimationComplete);
        m_StateMachine.SetCallback("managedByAgent", () => SetManagedByAgent(true));
        m_StateMachine.SetCallback("unmanagedByAgent", () => SetManagedByAgent(false));
        m_StateMachine.SetCallback("setAbductionPhysics", SetAbductionPhysics);
        m_StateMachine.SetCallback("setGeneralPhysics", SetGeneralPhysics);
        m_StateMachine.SetCallback("disablePhysics", DisablePhysics);
        m_StateMachine.SetCallback("requestIdleState", RequestIdleState);
        m_StateMachine.SetCallback("onBreedingCompleted", OnBreedingCompleted);

        m_StateMachine.AddTransition(typeof(AnimalIdleState), typeof(AnimalDeathState), () => IsDead);
        m_StateMachine.AddAnyTransition(typeof(AnimalAbductedState), () => (!IsWrangled && IsInTractorBeam && !IsStaggered()));
        m_StateMachine.AddAnyTransition(typeof(AnimalWrangledState), () => (IsWrangled && !IsInTractorBeam && !IsStaggered()));
        m_StateMachine.AddTransition(typeof(AnimalThrowingState), typeof(AnimalFreeFallState), () => !IsWrangled);
        m_StateMachine.AddTransition(typeof(AnimalWrangledState), typeof(AnimalStaggeredState), () => m_bShouldStagger);
        m_StateMachine.AddTransition(typeof(AnimalWrangledState), typeof(AnimalAbductedAndWrangledState), () => IsInTractorBeam);
        m_StateMachine.AddTransition(typeof(AnimalAbductedState), typeof(AnimalAbductedAndWrangledState), () => IsWrangled);
        m_StateMachine.AddTransition(typeof(AnimalAbductedState), typeof(AnimalFreeFallState), () => !IsInTractorBeam);
        m_StateMachine.AddTransition(typeof(AnimalWrangledState), typeof(AnimalFreeFallState), () => !IsWrangled && CanLeaveFreeFall() && m_bShouldStagger);
        m_StateMachine.AddTransition(typeof(AnimalWrangledState), typeof(AnimalIdleState), () => !IsWrangled && CanLeaveFreeFall() && !m_bShouldStagger);
        m_StateMachine.AddTransition(typeof(AnimalAbductedState), typeof(AnimalFreeFallState), () => !IsInTractorBeam);

        // evading states
        m_StateMachine.AddTransition(typeof(AnimalIdleState), typeof(AnimalEvadingState), ShouldEvade);
        m_StateMachine.AddTransition(typeof(AnimalEvadingState), typeof(AnimalIdleState), HasEvadedEnemy);

        // breeding and breeding chase states
        m_StateMachine.AddTransition(typeof(AnimalIdleState), typeof(AnimalBreedingChaseState), FoundBreedingPartner);
        m_StateMachine.AddTransition(typeof(AnimalBreedingChaseState), typeof(AnimalBreedingState), CanBreedPartner);
        m_StateMachine.AddTransition(typeof(AnimalBreedingChaseState), typeof(AnimalIdleState), ShouldStopActionToEvadeNext);
        m_StateMachine.AddTransition(typeof(AnimalBreedingState), typeof(AnimalIdleState), ShouldStopActionToEvadeNext);
        m_StateMachine.AddTransition(typeof(AnimalBreedingState), typeof(AnimalIdleState), () => HasLostTarget(m_fBreedingChaseEndRange));
        m_StateMachine.AddTransition(typeof(AnimalBreedingChaseState), typeof(AnimalIdleState), () => HasLostTarget(m_fBreedingChaseEndRange));

        // free fall transitionary states
        m_StateMachine.AddTransition(typeof(AnimalFreeFallState), typeof(AnimalStaggeredState), () => CanLeaveFreeFall() && m_bShouldStagger);
        m_StateMachine.AddTransition(typeof(AnimalFreeFallState), typeof(AnimalIdleState), () => CanLeaveFreeFall() && !m_bShouldStagger);

        // attack and attack chase states
        m_StateMachine.AddTransition(typeof(AnimalPredatorChaseState), typeof(AnimalIdleState), ShouldStopActionToEvadeNext);
        m_StateMachine.AddTransition(typeof(AnimalIdleState), typeof(AnimalPredatorChaseState), CanHuntEnemy);
        m_StateMachine.AddTransition(typeof(AnimalPredatorChaseState), typeof(AnimalIdleState), () => HasLostTarget(m_HuntEndDistance));
        m_StateMachine.AddTransition(typeof(AnimalPredatorChaseState), typeof(AnimalAttackState), CanAttackEnemy);

        m_Manager.AddToPauseUnpause(Pause, Unpause);
    }

    private Vector3 m_BodyVelocity = Vector3.zero;
    private Vector3 m_BodyAngularVelocity = Vector3.zero;
    private bool m_bWasUsingNavmeshAgent = false;
    private bool m_bWasUsingRigidBody = false;

    public void Pause()
    {
        if (m_AnimalAgent.enabled)
        {
            m_bWasUsingNavmeshAgent = true;
            m_BodyVelocity = m_AnimalAgent.velocity;
        }
        if (!m_AnimalRigidBody.isKinematic)
        {
            m_BodyVelocity = Vector3.zero;
            m_BodyAngularVelocity = Vector3.zero;
            m_AnimalRigidBody.velocity = Vector3.zero;
            m_AnimalRigidBody.angularVelocity = Vector3.zero;
            m_bWasUsingRigidBody = true;
            m_AnimalRigidBody.isKinematic = true;

        }

        m_AnimalAgent.enabled = false;
        m_AnimalAnimator.enabled = false;
        m_AnimalMovement.enabled = false;
        enabled = false;
    }

    public void Unpause()
    {
        if (m_bWasUsingNavmeshAgent)
        {
            m_AnimalAgent.velocity = m_BodyVelocity;
            m_AnimalAgent.enabled = true;
            m_bWasUsingNavmeshAgent = false;
        }
        if (m_bWasUsingRigidBody)
        {
            m_bWasUsingRigidBody = false;
            m_AnimalRigidBody.velocity = m_BodyVelocity;
            m_AnimalRigidBody.angularVelocity = m_BodyAngularVelocity;
        }

        m_AnimalAnimator.enabled = true;
        m_AnimalMovement.enabled = true;
        enabled = true;
    }

    protected void Start()
    {
        m_StateMachine.InitializeStateMachine();
    }
    public void FixedUpdate()
    {
        m_CurrentStaggerCooldown = Mathf.Max(0, m_CurrentStaggerCooldown -= Time.deltaTime);
        m_CurrentBreedingCooldown = Mathf.Max(0, m_CurrentBreedingCooldown -= Time.deltaTime);
        m_StateMachine.Tick();
    }
    #endregion
}


public class AnimalIdleState : AStateBase
{
    private readonly AnimalMovementComponent animalMovement;
    private readonly AnimalAnimationComponent animalMovementAnimator;
    private float m_fLowIdleTime;
    private float m_fHighIdleTime;
    public AnimalIdleState(AnimalMovementComponent animalMovement, AnimalAnimationComponent animalAnimator)
    {
        this.animalMovement = animalMovement;
        this.animalMovementAnimator = animalAnimator;
    }

    private float timeTot = 0.0f;
    public override void Tick()
    {
        if (animalMovement.HasReachedDestination())
        {
            timeTot -= Time.deltaTime;
        }
        else if (animalMovement.IsStuck())
        {
            timeTot = 0.0f;
        }

        if (timeTot <= 0.0f)
        {
            if (animalMovement.ChooseRandomDestination())
            {
                timeTot = UnityEngine.Random.Range(m_fLowIdleTime, m_fHighIdleTime);
            }
        }
    }
    public override void OnEnter()
    {
        TriggerCallback("managedByAgent");
        TriggerCallback("disablePhysics");
        animalMovementAnimator.SetWalkAnimation();
        animalMovement.SetWalking();
        animalMovement.ClearDestination();
        m_fLowIdleTime = GetParam<float>("lowIdleTimer");
        m_fHighIdleTime = GetParam<float>("highIdleTimer");
        timeTot = 0.0f;
    }
}

public class AnimalEvadingState : AStateBase
{
    private readonly AnimalAnimationComponent animalAnimator;
    private readonly AnimalMovementComponent animalMovement;
    private Transform m_RunningTransform;
    private float m_fFleeCheckInterval;
    private float m_fEvadeDistance;
    float currentRunTime = 0.0f;
    public AnimalEvadingState(AnimalMovementComponent animalMovement, AnimalAnimationComponent animalAnimator)
    {
        this.animalMovement = animalMovement;
        this.animalAnimator = animalAnimator;
    }

    public override void OnEnter()
    {
        currentRunTime = 0.0f;
        m_RunningTransform = GetParam<Transform>("evadingTransform");
        m_fFleeCheckInterval = GetParam<float>("fleeCheckInterval");
        m_fEvadeDistance = GetParam<float>("evadedDistance");
        animalMovement.enabled = true;
        animalMovement.RunAwayFromObject(m_RunningTransform, m_fEvadeDistance);
        animalAnimator.SetRunAnimation();
        animalMovement.SetRunning();

        TriggerCallback("managedByAgent");
        TriggerCallback("disablePhysics");
    }
    public override void Tick()
    {
        currentRunTime += Time.deltaTime;
        if (currentRunTime > m_fFleeCheckInterval) 
        {
            animalMovement.RunAwayFromObject(m_RunningTransform, m_fEvadeDistance);
            currentRunTime = 0.0f;
        }
        if (animalMovement.IsStuck()) 
        {
            animalMovement.RunAwayFromObject(m_RunningTransform, m_fEvadeDistance);
            currentRunTime = m_fFleeCheckInterval / 2f; 
        }
    }

	public override void OnExit()
	{
		TriggerCallback("onLeaveEvadeState");

    }
}
public class AnimalWrangledState : AStateBase
{
    private readonly AnimalMovementComponent animalMovement;
    private readonly AnimalAnimationComponent animalMovementAnimator;
    private Transform m_BodyTransform;
    public AnimalWrangledState(AnimalMovementComponent animalMovement, AnimalAnimationComponent animalAnimator)
    {
        this.animalMovement = animalMovement;
        this.animalMovementAnimator = animalAnimator;
    }

    public override void Tick()
    {
        Vector3 dir = (m_BodyTransform.position - GetParam<Transform>("evadingTransform").position).normalized;
        animalMovement.RunInDirection(dir);
        animalMovementAnimator.SetDesiredLookDirection(dir);
    }
    public override void OnEnter()
    {
        TriggerCallback("unmanagedByAgent");
        TriggerCallback("setGeneralPhysics");
        m_BodyTransform = GetParam<Transform>("bodyTransform");
        animalMovement.enabled = false;
        animalMovementAnimator.SetEscapingAnimation();
    }
}

public class AnimalThrowingState : AStateBase
{
    private readonly AnimalAnimationComponent animalAnimator;
    public AnimalThrowingState(AnimalAnimationComponent animalAnimator)
    {
        this.animalAnimator = animalAnimator;
    }
}

public class AnimalDamagedState : AStateBase 
{
    private readonly AnimalAnimationComponent animalAnimator;
    public AnimalDamagedState(AnimalAnimationComponent animalAnimator)
    {
        this.animalAnimator = animalAnimator;
    }

    public override void OnEnter()
    {
        animalAnimator.TriggerDamagedAnimation(() => RequestTransition<AnimalIdleState>());
    }
}

public class AnimalStaggeredState : AStateBase 
{
    private readonly AnimalAnimationComponent animalAnimator;
    private float m_fTimeStaggered;
    private float m_fTotalStaggerTime;

    public AnimalStaggeredState(AnimalAnimationComponent animalAnimator)
    {       
        this.animalAnimator = animalAnimator;
    }

    public override void OnEnter()
    {
        Debug.Log("Staggered");
        m_fTimeStaggered = 0.0f;
        animalAnimator.SetIdleAnimation();
        m_fTotalStaggerTime = GetParam<float>("staggerTime");
        animalAnimator.SetStaggeredAnimation();
        TriggerCallback("setGeneralPhysics");
        TriggerCallback("onStaggered");
    }

    public override void Tick()
    {
        m_fTimeStaggered += Time.deltaTime;
        if (m_fTimeStaggered > m_fTotalStaggerTime) 
        {
            TriggerCallback("requestIdleState");
        }
    }

	public override void OnExit()
	{
        base.OnExit();
	}
}

public class AnimalLassoThrownState : AStateBase 
{
    private readonly AnimalAnimationComponent animalAnimator;
    public AnimalLassoThrownState(AnimalAnimationComponent animalAnimator)
    {
        this.animalAnimator = animalAnimator;
    }

    public override void OnEnter()
    {
        TriggerCallback("setGeneralPhysics");
        TriggerCallback("unmanagedByAgent");
        animalAnimator.SetIdleAnimation();
    }

	public override void OnExit()
	{
        TriggerCallback("stopBeingThrown");
	}
}

public class AnimalBreedingState : AStateBase 
{
    float currentBreedingTime = 0.0f;
    public override void OnEnter()
    {
        currentBreedingTime = GetParam<float>("breedingTime");
    }
    public override void Tick()
    {
        currentBreedingTime -= Time.deltaTime;
        if (currentBreedingTime < 0) 
        {
            TriggerCallback("onBreedingCompleted");
        }
    }
}

public class AnimalBreedingChaseState : AStateBase 
{
    private readonly AnimalAnimationComponent animalAnimator;
    private readonly AnimalMovementComponent animalMovement;
    private Transform m_RunningTransform;
    private float m_fHuntCheckInterval;
    private float m_fBreedingDistance;
    private float m_fHuntDistance;
    float currentRunTime = 0.0f;
    public AnimalBreedingChaseState(AnimalMovementComponent animalMovement, AnimalAnimationComponent animalAnimator)
    {
        this.animalMovement = animalMovement;
        this.animalAnimator = animalAnimator;
    }

    public override void OnEnter()
    {
        currentRunTime = 0.0f;
        m_RunningTransform = GetParam<Transform>("evadingTransform");
        m_fHuntCheckInterval = GetParam<float>("huntCheckInterval");
        m_fHuntDistance = GetParam<float>("breedingHuntBeginDistance");
        m_fBreedingDistance = GetParam<float>("breedingDistance");
        animalMovement.enabled = true;
        animalMovement.RunTowardsObject(m_RunningTransform, m_fHuntDistance);
        animalAnimator.SetRunAnimation();
        animalMovement.SetRunning();
        currentRunTime = m_fHuntCheckInterval;

        TriggerCallback("managedByAgent");
        TriggerCallback("disablePhysics");
    }
    public override void Tick()
    {
        currentRunTime += Time.deltaTime;
        if (currentRunTime > m_fHuntCheckInterval)
        {
            animalMovement.RunTowardsObject(m_RunningTransform, m_fHuntDistance, m_fBreedingDistance);
            currentRunTime = 0.0f;
        }
        if (animalMovement.IsStuck())
        {
            animalMovement.RunTowardsObject(m_RunningTransform, m_fHuntDistance, m_fBreedingDistance);
            currentRunTime = m_fHuntCheckInterval / 2f;
        }
    }
}


public class AnimalFreeFallState : AStateBase 
{
    private readonly AnimalAnimationComponent animalAnimator;
    public AnimalFreeFallState(AnimalAnimationComponent animalAnimator) 
    {
        this.animalAnimator = animalAnimator;
    }

    public override void OnEnter()
    {
        TriggerCallback("setGeneralPhysics");
        TriggerCallback("unmanagedByAgent");
        animalAnimator.SetFreeFallAnimation();
    }
}

public class AnimalDeathState : AStateBase 
{
        private readonly AnimalAnimationComponent animalAnimator;
    public AnimalDeathState(AnimalAnimationComponent animalAnimator) 
    {
        this.animalAnimator = animalAnimator;
    }

    public override void OnEnter()
    {
        TriggerCallback("setGeneralPhysics");
        TriggerCallback("unmanagedByAgent");
        animalAnimator.SetIdleAnimation();
    }
}
    
public class AnimalAbductedState : AStateBase
{
    private readonly AnimalMovementComponent animalMovement;

    public AnimalAbductedState(AnimalMovementComponent animalMovement)
    {
       this.animalMovement = animalMovement;
    }

    public override void OnEnter()
    {
        animalMovement.enabled = false;
        TriggerCallback("setAbductionPhysics");
        TriggerCallback("unmanagedByAgent");
    }
}

public class AnimalAbductedAndWrangledState : AStateBase 
{
    private readonly AnimalComponent animalStateHandler;

    public AnimalAbductedAndWrangledState(AnimalComponent animalStateHandler)
    {
        this.animalStateHandler = animalStateHandler;
    }
}

public class AnimalAttackState : AStateBase
{
    private readonly AnimalAnimationComponent animalAnimator;
    public AnimalAttackState(AnimalAnimationComponent animalAnimator)
    {
        this.animalAnimator = animalAnimator;
    }

    public override void OnEnter()
    {
        TriggerCallback("sendTargetPosition");
        animalAnimator.TriggerAttackAnimation(() => TriggerCallback("attackAnimationComplete"), () => TriggerCallback("triggerDamage"));
    }
}

public class AnimalPredatorChaseState : AStateBase
{
    private readonly AnimalAnimationComponent animalAnimator;
    private readonly AnimalMovementComponent animalMovement;
    private Transform m_RunningTransform;
    private float m_fHuntCheckInterval;
    private float m_fAttackDistance;
    private float m_fHuntDistance;
    float currentRunTime = 0.0f;
    public AnimalPredatorChaseState(AnimalMovementComponent animalMovement, AnimalAnimationComponent animalAnimator)
    {
        this.animalMovement = animalMovement;
        this.animalAnimator = animalAnimator;
    }

    public override void OnEnter()
    {
        currentRunTime = 0.0f;
        m_RunningTransform = GetParam<Transform>("evadingTransform");
        m_fHuntCheckInterval = GetParam<float>("huntCheckInterval");
        m_fHuntDistance = GetParam<float>("huntBeginDistance");
        m_fAttackDistance = GetParam<float>("attackDistance");
        animalMovement.enabled = true;
        animalMovement.RunTowardsObject(m_RunningTransform, m_fAttackDistance);
        animalAnimator.SetRunAnimation();
        animalMovement.SetRunning();
        currentRunTime = m_fHuntCheckInterval;

        TriggerCallback("managedByAgent");
        TriggerCallback("disablePhysics");
    }
    public override void Tick()
    {
        currentRunTime += Time.deltaTime;
        if (currentRunTime > m_fHuntCheckInterval)
        {
            animalMovement.RunTowardsObject(m_RunningTransform, m_fHuntDistance, m_fAttackDistance);
            currentRunTime = 0.0f;
        }
        else if (animalMovement.IsStuck())
        {
            animalMovement.RunTowardsObject(m_RunningTransform, m_fHuntDistance, m_fAttackDistance);
            currentRunTime = m_fHuntCheckInterval / 2f;
        }
		else 
        {
            animalMovement.CheckStoppingDistanceForChase(m_RunningTransform, m_fAttackDistance);
        }
    }
}
