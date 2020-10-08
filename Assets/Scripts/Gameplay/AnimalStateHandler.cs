using UnityEngine;
using System;
// imagine this were a separate component, that controled general movement. you could reference it from the UFO "manager" class, and use it in states, like below.


// main class, stores state machine and public data instances that all states can access.
public class AnimalStateHandler : MonoBehaviour
{

    public AnimalComponent animalComponent;
    public AnimalMovement animalMovement;

    public event Action OnDestroy;

    public bool IsWrangled { get; private set; }
    public bool IsInTractorBeam { get; private set; }
    
    public void OnWrangledByLasso(Lasso lasso) 
    {
        IsWrangled = true;
    }

    public void OnEnterTractorBeam()
    {
        IsInTractorBeam = true;
    }

    public void OnLeaveTractorBeam() 
    {
        IsInTractorBeam = false;
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

        m_StateMachine.AddAnyTransition(typeof(AnimalDeathState), () => );

        // if both abducted and wrangled, special state for these

        m_StateMachine.AddTransition(typeof(AnimalWrangledState), typeof(AnimalAbductedAndWrangledState), () => IsInTractorBeam);
        m_StateMachine.AddTransition(typeof(AnimalAbductedState), typeof(AnimalAbductedAndWrangledState), () => IsWrangled);

        // cow evasion behaviour; runs from player when close; idles when not.
        m_StateMachine.AddTransition(typeof(AnimalIdleState), typeof(AnimalEvadingState), () => animalMovement.GetDistanceToTarget() < 20.0f);
        m_StateMachine.AddTransition(typeof(AnimalEvadingState), typeof(AnimalIdleState), () => animalMovement.GetDistanceToTarget() > 20.0f);

        // when the cow hits the ground, it staggers for a bit
        m_StateMachine.AddTransition(typeof(AnimalFreeFallState), typeof(AnimalStaggeredState), () => animalMovement.IsStanding());

        // when the cow stos being abducted, it falls
        m_StateMachine.AddTransition(typeof(AnimalAbductedState), typeof(AnimalFreeFallState), () => !IsInTractorBeam);

        // when the rope becomes too far away, or we let it go, the animal has escaped
        m_StateMachine.AddTransition(typeof(AnimalWrangledState), typeof(AnimalEvadingState), () => );

        // when the animal gets close enough to the player when wrangled, we can start swinging
        m_StateMachine.AddTransition(typeof(AnimalWrangledState), typeof(AnimalThrowingState), () => animalMovement.GetDistanceToTarget() < 3.0f);

        // when we let go of the mouse, we can start throwing the cow
        m_StateMachine.AddTransition(typeof(AnimalThrowingState), typeof(AnimalFreeFallState), () => );

        // we can also move from these states by dropping the cow's leash
        m_StateMachine.AddTransition(typeof(AnimalThrowingState), typeof(AnimalEvadingState), () =>);
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
        if (animalStateHandler.IsTouchingGround()) 
        {
            RequestTransition<AnimalStaggeredState>();
        }
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
        ufo.cowEscaped();
    }
}

public class AnimalAbductedAndWrangledState : IState 
{
    AnimalStateHandler animalStateHandler;
    public override void Tick()
    {
        
    }
}