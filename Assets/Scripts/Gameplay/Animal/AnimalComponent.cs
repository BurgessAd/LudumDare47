using UnityEngine;
using System;
using UnityEngine.AI;
using EZCameraShake;
public class AnimalComponent : MonoBehaviour
{
    [SerializeField]
    private CowGameManager m_Manager;
    [SerializeField]
    private AnimalMovementComponent m_AnimalMovement;
    [SerializeField]
    private LassoThrowableComponent m_LassoThrowableComponent;
    [SerializeField]
    private Rigidbody m_AnimalRigidBody;
    [SerializeField]
    private NavMeshAgent m_AnimalAgent;
    [SerializeField]
    private AnimalAnimationComponent m_AnimalAnimator;

    [SerializeField]
    private Transform m_CowMainTransform;
    [SerializeField]
    private Transform m_CowLeashTransform;
    [SerializeField]
    private Transform m_CowBodyTransform;

    [SerializeField]
    private float m_fStaggeredSpeed;

    [SerializeField]
    private float m_RunDistance;
    public event Action OnDestroy;
    private Transform m_CurrentEvadingTransform;
    [SerializeField]
    float m_fTimeGrounded = 0.0f;
    public bool IsWrangled { get; private set; }
    public bool IsInTractorBeam { get; private set; }

    private Camera m_PlayerCam;

    // free fall handled by spline which applies force
    // maybe have the spline dictate how much it tries to stick to it?
    // so for free-fall spline, stick to it quite a bit
    // and dropping
    // but for abduction, have it vaguely push towards the centre

    public Transform GetCurrentEvadingTransform => m_CurrentEvadingTransform;
    public Transform GetMainTansform => m_CowMainTransform;

    public Transform GetBodyTransform => m_CowBodyTransform;

    public Rigidbody GetCowRigidBody => m_AnimalRigidBody;

    public void OnPulledByLasso()
    {
        m_AnimalAnimator.WasPulled();
    }


    public void OnWrangledByLasso(in Transform thisTransform) 
    {
        m_CurrentEvadingTransform = thisTransform;
        IsWrangled = true;
    }


    private void OnCollisionEnter(Collision collision)
    {
        m_fTimeGrounded = Time.time;
        m_vFastestFallingVelocitySqrd = Mathf.Max(m_vFastestFallingVelocitySqrd, GetCowRigidBody.velocity.sqrMagnitude);
    }
    float m_vFastestFallingVelocitySqrd = 0.0f;
    private void CowHitGround(Collision collision) 
    {
        float shakeStrength = GetCowRigidBody.mass * GetCowRigidBody.velocity.magnitude/(GetCowRigidBody.position - m_PlayerCam.transform.position).magnitude/ 100;
        m_AnimalAnimator.OnHitGround(collision.GetContact(0).point, Quaternion.LookRotation(Vector3.forward, collision.GetContact(0).normal));
        CameraShaker.Instance.ShakeOnce(shakeStrength, shakeStrength, 0.1f, 1.0f);
        m_StateMachine.RequestTransition(typeof(AnimalFreeFallState));
    }

    private void OnCollisionExit(Collision collision)
    {
        m_fTimeGrounded = Mathf.Infinity;
    }

    public bool IsGrounded => Time.time - m_fTimeGrounded > 0;

    public float GetGroundedTime => Mathf.Max(Time.time - m_fTimeGrounded, 0);

    public Transform GetLeashTransform => m_CowLeashTransform;

    public float GetScaredDistance() 
    {
        return m_RunDistance;
    }

    public void OnReleasedByLasso() 
    {
        IsWrangled = false;
    }

    public void OnStartedLassoSpinning() 
    {
        IsWrangled = true;
        GetMainTansform.rotation = Quaternion.identity;
        m_StateMachine.RequestTransition(typeof(AnimalThrowingState));
    }

    public float GetResistiveForce() 
    {
        return 200.0f;
    }

    public void OnThrownByLasso() 
    {
        IsWrangled = false;
        m_StateMachine.RequestTransition(typeof(AnimalLassoThrownState));
    }

    public void OnEnterTractorBeam()
    {
        IsInTractorBeam = true;
        m_StateMachine.ActivateStateTransition(typeof(AnimalWrangledState), typeof(AnimalAbductedAndWrangledState), true);
    }

    public void OnLeaveTractorBeam() 
    {
        m_StateMachine.ActivateStateTransition(typeof(AnimalWrangledState), typeof(AnimalAbductedAndWrangledState), false);
        IsInTractorBeam = false;
    }

    public void OnKillCow() 
    {
        m_StateMachine.RequestTransition(typeof(AnimalDeathState));
        enabled = false;
    }

    public void SetManagedByAgent(bool enable) 
    {
        m_AnimalAgent.enabled = enable;
        m_AnimalAgent.updatePosition = enable;
        m_AnimalAgent.updateUpAxis = false;
        m_AnimalAgent.updateRotation = false;

    }

    public void SetPhysicsActive(bool enable) 
    {
        m_AnimalRigidBody.isKinematic = !enable;
        m_AnimalRigidBody.useGravity = enable;
    }

    private bool ShouldEvade() 
    {
        if (m_Manager.GetClosestHostileTransform(EntityType.Prey, GetMainTansform.position, out Transform objTransform)) 
        {
            float distSq = Vector3.SqrMagnitude(objTransform.position - GetMainTansform.position);
            float distToEscSq = m_RunDistance * m_RunDistance * 1.0f;
            if (distSq < distToEscSq) 
            {
                m_CurrentEvadingTransform = objTransform;
                return true;
            }
        }
        return false;

    }

    private bool ShouldStagger() 
    {
        Debug.Log(m_vFastestFallingVelocitySqrd);
        return m_vFastestFallingVelocitySqrd > m_fStaggeredSpeed * m_fStaggeredSpeed; 
    }

    public LassoThrowableComponent GetThrowable => m_LassoThrowableComponent;

    public void SetManualEvading(in Transform evadeTransform) 
    {
        m_CurrentEvadingTransform = evadeTransform;
    }

    private bool HasEvaded() 
    {
        float distSq = Vector3.SqrMagnitude(m_CurrentEvadingTransform.position - GetMainTansform.position);
        float distToEscSq = m_RunDistance * m_RunDistance * 1.0f;
        if (distSq > distToEscSq) 
        {
             return true;
        }
        return false;
    }
    bool m_bIsSpinning = false;

    // setting up and adding states to state machine
    public StateMachine m_StateMachine;
    public void Start()
    {
        m_PlayerCam = Camera.main;
        m_LassoThrowableComponent.OnObjectHitGround += CowHitGround;

        m_StateMachine = new StateMachine();
        m_StateMachine.AddState(new AnimalEvadingState(this, m_AnimalMovement, m_AnimalAnimator));
        m_StateMachine.AddState(new AnimalWrangledState(m_AnimalMovement, m_AnimalAnimator, this));
        m_StateMachine.AddState(new AnimalAbductedState(this));
        m_StateMachine.AddState(new AnimalThrowingState(this, m_AnimalAnimator));
        m_StateMachine.AddState(new AnimalFreeFallState(this, m_AnimalAnimator));
        m_StateMachine.AddState(new AnimalStaggeredState(this, m_AnimalAnimator));
        m_StateMachine.AddState(new AnimalIdleState(m_AnimalMovement, m_AnimalAnimator, this));
        m_StateMachine.AddState(new AnimalLassoThrownState(this, m_AnimalAnimator));



        // need to always be able to abduct if we're in a tractor beam and not wrangled
        m_StateMachine.AddAnyTransition(typeof(AnimalAbductedState), () => !IsWrangled && IsInTractorBeam && !m_bIsSpinning);
        // we can always wrangle a cow if it's not in a tractor beam
        m_StateMachine.AddAnyTransition(typeof(AnimalWrangledState), () => IsWrangled && !IsInTractorBeam && !m_bIsSpinning);

        m_StateMachine.AddTransition(typeof(AnimalThrowingState), typeof(AnimalFreeFallState), () => !IsWrangled);
        // if both abducted and wrangled, special state for these

        m_StateMachine.AddTransition(typeof(AnimalWrangledState), typeof(AnimalAbductedAndWrangledState), () => IsInTractorBeam);
        m_StateMachine.AddTransition(typeof(AnimalAbductedState), typeof(AnimalAbductedAndWrangledState), () => IsWrangled);

        // when the cow stops being wrangled, it idles
        m_StateMachine.AddTransition(typeof(AnimalWrangledState), typeof(AnimalFreeFallState), () => !IsWrangled);
        // when the cow stos being abducted, it falls
        m_StateMachine.AddTransition(typeof(AnimalAbductedState), typeof(AnimalFreeFallState), () => !IsInTractorBeam);

        // cow evasion behaviour; runs from player when close; idles when not.
        m_StateMachine.AddTransition(typeof(AnimalIdleState), typeof(AnimalEvadingState), () => ShouldEvade());
        m_StateMachine.AddTransition(typeof(AnimalEvadingState), typeof(AnimalIdleState), () => HasEvaded());

        // when the cow hits the ground, it staggers for a bit
        m_StateMachine.AddTransition(typeof(AnimalFreeFallState), typeof(AnimalStaggeredState), () => ShouldEnterStaggeredFromFreeFall());
        m_StateMachine.AddTransition(typeof(AnimalFreeFallState), typeof(AnimalIdleState), () => ShouldEnterIdleFromFreeFall());

        // and then gets back up in a second
        m_StateMachine.AddTransition(typeof(AnimalStaggeredState), typeof(AnimalIdleState), () => ShouldEnterIdleFromStaggered());
        m_StateMachine.SetInitialState(typeof(AnimalIdleState));
    }

    public bool IsTouchingGround() { return false; }

    public void SetIsSpinning(bool Val) { m_bIsSpinning = Val; }

    public void ResetFallingSpeed() 
    {
        m_vFastestFallingVelocitySqrd = 0.0f;
    }

    private bool ShouldEnterStaggeredFromFreeFall() 
    {
        return CanLeaveFreeFall() && ShouldStagger();
    }

    private bool ShouldEnterIdleFromFreeFall() 
    {
        return CanLeaveFreeFall() && !ShouldStagger();
    }

    private bool CanLeaveFreeFall() { return IsGrounded && m_AnimalRigidBody.velocity.magnitude < 1.0f && m_AnimalMovement.IsNearNavMesh(); }

    private bool ShouldEnterIdleFromStaggered() 
    {
        return Time.time - m_fTimeGrounded > 2.0f;
    }

    // state machine update
    public void FixedUpdate()
    {
        
        m_StateMachine.Tick();
    }
}


public class AnimalEvadingState : IState
{
    private readonly AnimalComponent animalStateHandler;
    private readonly AnimalAnimationComponent animalAnimator;
    private readonly AnimalMovementComponent animalMovement;
    float retrialTime = 0.3f;
    public AnimalEvadingState(AnimalComponent animalStateHandler, AnimalMovementComponent animalMovement, AnimalAnimationComponent animalAnimator)
    {
        this.animalStateHandler = animalStateHandler;
        this.animalMovement = animalMovement;
        this.animalAnimator = animalAnimator;
    }
    public override void OnEnter()
    {
        currentRunTime = 0.0f;
        animalMovement.enabled = true;
        animalMovement.RunAwayFromObject(animalStateHandler.GetCurrentEvadingTransform, animalStateHandler.GetScaredDistance());
        animalAnimator.SetRunAnimation();
        animalMovement.SetRunning();
        animalStateHandler.SetManagedByAgent(true);
        animalStateHandler.SetPhysicsActive(false);
    }
    float currentRunTime = 0.0f;
    public override void Tick()
    {
        currentRunTime += Time.deltaTime;
        if (currentRunTime > retrialTime) 
        {
            animalMovement.RunAwayFromObject(animalStateHandler.GetCurrentEvadingTransform, animalStateHandler.GetScaredDistance());
            currentRunTime = 0.0f;
        }
        Color color = Color.green;
        if (animalMovement.IsStuck()) 
        {
            animalMovement.RunAwayFromObject(animalStateHandler.GetCurrentEvadingTransform, animalStateHandler.GetScaredDistance());
        }
        Debug.DrawLine(animalStateHandler.GetCurrentEvadingTransform.position, animalStateHandler.GetMainTansform.position, color);
    }
}


public class AnimalIdleState : IState 
{
    private readonly AnimalMovementComponent animalMovement;
    private readonly AnimalAnimationComponent animalMovementAnimator;
    private readonly AnimalComponent animalStateHandler;
    public AnimalIdleState(AnimalMovementComponent animalMovement, AnimalAnimationComponent animalAnimator, AnimalComponent animalStateHandler)
	{
        this.animalMovement = animalMovement;
        this.animalStateHandler = animalStateHandler;
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
                timeTot = UnityEngine.Random.Range(3.0f, 5.0f);
            }
        }
    }
    public override void OnEnter()
    {
        Debug.Log("Idle");
        animalStateHandler.SetManagedByAgent(true);
        animalStateHandler.SetPhysicsActive(false);
        timeTot = UnityEngine.Random.Range(3.0f, 5.0f);
        animalMovementAnimator.SetWalkAnimation();
        animalMovement.SetWalking();
        animalMovement.ClearDestination();
    }
}

public class AnimalWrangledState : IState
{
    private readonly AnimalMovementComponent animalMovement;
    private readonly AnimalAnimationComponent animalMovementAnimator;
    private readonly AnimalComponent animalStateHandler;
    public AnimalWrangledState(AnimalMovementComponent animalMovement, AnimalAnimationComponent animalAnimator, AnimalComponent animalStateHandler)
    {
        this.animalMovement = animalMovement;
        this.animalStateHandler = animalStateHandler;
        this.animalMovementAnimator = animalAnimator;
    }
    // here we cache a gameobject in the parent class and change states
    public override void Tick()
    {
        Vector3 dir = (animalStateHandler.GetCowRigidBody.transform.position - animalStateHandler.GetCurrentEvadingTransform.position).normalized;

        animalMovement.RunInDirection(dir);
        animalMovementAnimator.SetDesiredLookDirection(dir);
    }
    public override void OnEnter()
    {
        animalMovement.enabled = false;
        animalStateHandler.SetManagedByAgent(false);
        animalStateHandler.SetPhysicsActive(true);
        animalMovementAnimator.SetEscapingAnimation();
    }
    public override void OnExit()
	{

    }


}

public class AnimalThrowingState : IState
{
    AnimalComponent animalStateHandler;
    AnimalAnimationComponent animalAnimator;
    public AnimalThrowingState(AnimalComponent animalStateHandler, AnimalAnimationComponent animalAnimator)
    {
        this.animalStateHandler = animalStateHandler;
        this.animalAnimator = animalAnimator;
    }
    // here we cache a gameobject in the parent class and change states
    public override void Tick()
    {
        //animalStateHandler.animalComponent.addGravity();

    }
    public override void OnEnter()
    {
        animalStateHandler.SetPhysicsActive(false);
        animalStateHandler.SetIsSpinning(true);
        animalAnimator.SetIdleAnimation();
    }
    public override void OnExit()
    {
        animalStateHandler.SetIsSpinning(false);
    }
}

public class AnimalStaggeredState : IState 
{
    AnimalComponent animalStateHandler;
    AnimalAnimationComponent animalAnimator;

    private float m_TimeStaggered;

    public AnimalStaggeredState(AnimalComponent stateHandler, AnimalAnimationComponent animalAnimator)
    {       
        this.animalAnimator = animalAnimator;
        animalStateHandler = stateHandler;
    }

    public override void OnEnter()
    {
        Debug.Log("Staggered");
        m_TimeStaggered = 0.0f;
        animalAnimator.SetStaggeredAnimation();
        animalStateHandler.SetPhysicsActive(true);
    }

    public override void Tick()
    {
        m_TimeStaggered += Time.deltaTime;
        if (m_TimeStaggered > 3.0f) 
        {
            RequestTransition<AnimalIdleState>();
        }
    }
}

public class AnimalLassoThrownState : IState 
{
    private readonly AnimalComponent animalStateHandler;
    private readonly AnimalAnimationComponent animalAnimator;
    public AnimalLassoThrownState(AnimalComponent stateHandler, AnimalAnimationComponent animalAnimator)
    {
        this.animalAnimator = animalAnimator;
        animalStateHandler = stateHandler;
    }

    public override void OnEnter()
    {
        Debug.Log("LassoThrownState");
        animalStateHandler.SetPhysicsActive(true);
        animalStateHandler.SetManagedByAgent(false);
        animalAnimator.SetIdleAnimation();
    }
}
public class AnimalFreeFallState : IState 
{
    private readonly AnimalComponent animalStateHandler;
    private readonly AnimalAnimationComponent animalAnimator;
    public AnimalFreeFallState(AnimalComponent stateHandler, AnimalAnimationComponent animalAnimator) 
    {
        this.animalAnimator = animalAnimator;
        animalStateHandler = stateHandler;
    }

    public override void OnEnter()
    {
        Debug.Log("FreeFall");
        animalStateHandler.SetPhysicsActive(true);
        animalStateHandler.SetManagedByAgent(false);
        animalAnimator.SetIdleAnimation();
        animalStateHandler.ResetFallingSpeed();
    }
}

public class AnimalDeathState : IState 
{
}
    
public class AnimalAbductedState : IState
{
    AnimalComponent animalStateHandler;
    private UfoMain ufo;

    public AnimalAbductedState(AnimalComponent animalStateHandler)
    {
       this.animalStateHandler = animalStateHandler;
}

    public override void OnEnter()
    {

    }

    public override void Tick()
    {

    }
    public override void OnExit()
    {

    }
}

public class AnimalAbductedAndWrangledState : IState 
{
    AnimalComponent animalStateHandler;
    public override void Tick()
    {
        
    }
}