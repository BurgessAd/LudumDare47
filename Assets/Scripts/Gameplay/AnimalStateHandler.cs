using UnityEngine;
using System;
// imagine this were a separate component, that controled general movement. you could reference it from the UFO "manager" class, and use it in states, like below.


// main class, stores state machine and public data instances that all states can access.
public class AnimalStateHandler : MonoBehaviour
{

    private AnimalComponent animalComponent;
    private AnimalMovement animalMovement;
    private GameObject leashPointObject;
    private Rigidbody cowRigidBody;

    public event Action OnDestroy;

    public bool IsWrangled { get; private set; }
    public bool IsInTractorBeam { get; private set; }

    // free fall handled by spline which applies force
    // maybe have the spline dictate how much it tries to stick to it?
    // so for free-fall spline, stick to it quite a bit
    // and dropping
    // but for abduction, have it vaguely push towards the centre

    public GameObject GetLeashPointObject => leashPointObject;
    public Transform GetMainTansform => cowRigidBody.transform;
    public Rigidbody GetCowRigidBody => cowRigidBody;

    public void OnWrangledByLasso() 
    {
        IsWrangled = true;
    }

    public Transform GetLeashTransform { get; private set; }

    public void OnReleasedByLasso() 
    {
        IsWrangled = false;
    }

    public void OnStartedLassoSpinning() 
    {
        IsWrangled = true;
        m_StateMachine.RequestTransition(typeof(AnimalThrowingState));
    }

    public void OnThrownByLasso() 
    {
        IsWrangled = false;
        m_StateMachine.RequestTransition(typeof(AnimalFreeFallState));
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

    // setting up and adding states to state machine
    public StateMachine m_StateMachine;
    public void Start()
    {
        m_StateMachine = new StateMachine(new AnimalIdleState(this));
        m_StateMachine.AddState(new AnimalEvadingState(this));
        m_StateMachine.AddState(new AnimalWrangledState(this));
        m_StateMachine.AddState(new AnimalAbductedState(this));
        m_StateMachine.AddState(new AnimalThrowingState(this));
        m_StateMachine.AddState(new AnimalFreeFallState(this));


        // need to always be able to abduct if we're in a tractor beam and not wrangled
        m_StateMachine.AddAnyTransition(typeof(AnimalAbductedState), () => !IsWrangled && IsInTractorBeam);
        // we can always wrangle a cow if it's not in a tractor beam
        m_StateMachine.AddAnyTransition(typeof(AnimalWrangledState), () => IsWrangled && !IsInTractorBeam);

        // if both abducted and wrangled, special state for these

        m_StateMachine.AddTransition(typeof(AnimalWrangledState), typeof(AnimalAbductedAndWrangledState), () => IsInTractorBeam);
        m_StateMachine.AddTransition(typeof(AnimalAbductedState), typeof(AnimalAbductedAndWrangledState), () => IsWrangled);

        // when the cow stops being wrangled, it idles
        m_StateMachine.AddTransition(typeof(AnimalWrangledState), typeof(AnimalIdleState), () => !IsWrangled);
        // when the cow stos being abducted, it falls
        m_StateMachine.AddTransition(typeof(AnimalAbductedState), typeof(AnimalFreeFallState), () => !IsInTractorBeam);

        // cow evasion behaviour; runs from player when close; idles when not.
        m_StateMachine.AddTransition(typeof(AnimalIdleState), typeof(AnimalEvadingState), () => animalMovement.GetDistanceToTarget() < 20.0f);
        m_StateMachine.AddTransition(typeof(AnimalEvadingState), typeof(AnimalIdleState), () => animalMovement.GetDistanceToTarget() > 20.0f);

        // when the cow hits the ground, it staggers for a bit
        m_StateMachine.AddTransition(typeof(AnimalFreeFallState), typeof(AnimalStaggeredState), () => animalMovement.IsStanding());

        // and then gets back up in a second
        m_StateMachine.AddTransition(typeof(AnimalStaggeredState), typeof(AnimalIdleState), () => animalMovement.TimeOnGround > 2.0f);


    }

    public bool IsTouchingGround() { return false; }

    // state machine update
    public void Update()
    {
        
        m_StateMachine.Tick();
    }
}


public class AnimalEvadingState : IState
{
    AnimalStateHandler animalStateHandler;
    AnimalMovement animalMovement;
    Transform playerTransform;
    public AnimalEvadingState(AnimalStateHandler animalStateHandler)
    {
        this.animalStateHandler = animalStateHandler;
    }
    public override void Tick()
    {
        animalMovement.RunAwayFromObject(playerTransform);
        if (animalMovement.IsStuck()) 
        {
            
        }
        if (animalMovement.GetDistanceToTarget() > 20.0f)
        {
            RequestTransition<AnimalIdleState>();
        }
    }
}


public class AnimalIdleState : IState 
{
    AnimalStateHandler animalStateHandler;

    AnimalMovement animalMovement;

    Transform playerTransform;
    public AnimalIdleState(AnimalStateHandler animalStateHandler)
	{
        this.animalStateHandler = animalStateHandler;
	}

    float timeTot = 0.0f;
    public override void Tick()
    {
        timeTot += Time.deltaTime;
        if (UnityEngine.Random.Range(3.0f, 5.0f) - timeTot < 0.0f) 
        {
            timeTot = 0.0f;
            animalMovement.ChooseRandomDestination();
        }
        if (!animalMovement.HasEvadedPlayer(playerTransform)) 
        {
            RequestTransition<AnimalEvadingState>();
        }
    }
    public override void OnEnter()
    {
        timeTot = 0.0f;
    }
}

public class AnimalWrangledState : IState
{
    AnimalStateHandler animalStateHandler;
    public AnimalWrangledState(AnimalStateHandler animalStateHandler)
    {
       this.animalStateHandler = animalStateHandler;
    }
    // here we cache a gameobject in the parent class and change states
    public override void Tick()
    {

    }
    public override void OnEnter()
    {

    }
    public override void OnExit()
	{


    }


}

public class AnimalThrowingState : IState
{
    AnimalStateHandler animalStateHandler;
    public AnimalThrowingState(AnimalStateHandler animalStateHandler)
    {
        this.animalStateHandler = animalStateHandler;
    }
    // here we cache a gameobject in the parent class and change states
    public override void Tick()
    {
        //animalStateHandler.animalComponent.addGravity();
    }
    public override void OnEnter()
    {

    }
    public override void OnExit()
    {

    }
}

public class AnimalStaggeredState : IState 
{
    AnimalStateHandler animalStateHandler;
    AnimalMovementAnimator animalAnimator;

    private float m_TimeStaggered;

    public AnimalStaggeredState(AnimalStateHandler stateHandler) 
    {

    }

    public override void OnEnter()
    {
        m_TimeStaggered = 0.0f;
    }

    public override void Tick()
    {
        m_TimeStaggered += Time.deltaTime;
        if (m_TimeStaggered > 2.0f) 
        {
            RequestTransition<AnimalIdleState>();
        }
    }
}

public class AnimalFreeFallState : IState 
{
    AnimalStateHandler animalStateHandler;

    public AnimalFreeFallState(AnimalStateHandler stateHandler) 
    {
            
    }

    public override void Tick()
    {

    }
}

public class AnimalDeathState : IState 
{
}
    
public class AnimalAbductedState : IState
{
    AnimalStateHandler animalStateHandler;
    private UfoMain ufo;

    public AnimalAbductedState(AnimalStateHandler animalStateHandler)
    {
       this.animalStateHandler = animalStateHandler;
}

    public override void OnEnter(object ufo)
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
    AnimalStateHandler animalStateHandler;
    public override void Tick()
    {
        
    }
}