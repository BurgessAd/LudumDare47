using UnityEngine;
// imagine this were a separate component, that controled general movement. you could reference it from the UFO "manager" class, and use it in states, like below.
public class MovementHandling : MonoBehaviour { }

// main class, stores state machine and public data instances that all states can access.
public class UFO : MonoBehaviour
{
    public GameObject targetCow;

    private MovementHandling m_MovementHandling;

    // setting up and adding states to state machine
    private StateMachine m_StateMachine;
    public void Start()
    {
        m_StateMachine = new StateMachine(new UFOFindState(this));
        m_StateMachine.AddState(new UFOSwoopState(this, m_MovementHandling));
        m_StateMachine.AddState(new UFOWrangleState(this));
    }

    // method called by something external, like the lasso, to begin wrangling, for example
    public void OnUfoWrangled() 
    {
        m_StateMachine.RequestTransition(typeof(UFOFindState));
    }

    // state machine update
    public void Update()
    {
        m_StateMachine.Tick();
    }
}

public class UFOWrangleState : IState 
{
    private UFO m_Ufo;
    public UFOWrangleState(UFO ufo)
    {
        m_Ufo = ufo;
    }
}

public class UFOFindState : IState
{
    private UFO m_Ufo;
    public UFOFindState(UFO ufo)
    {
        m_Ufo = ufo;
    }
    // here we cache a gameobject in the parent class and change states
    public override void Tick()
    {
        GameObject targetObject = FindGameObject();
        if (targetObject)
        {
            m_Ufo.targetCow = targetObject;
            RequestTransition<UFOSwoopState>();
        }
    }

    private GameObject FindGameObject() { return null; }
}

public class UFOSwoopState : IState
{
    private UFO m_Ufo;
    private MovementHandling m_MovementHandling;
    public UFOSwoopState(UFO ufo, MovementHandling movementHandling)
    {
        m_Ufo = ufo;
        m_MovementHandling = movementHandling;
    }

    public override void Tick()
    {
        MoveTowards(m_Ufo.targetCow);
    }
    // here we might reference m_MovementHandling to move the UFO
    private void MoveTowards(GameObject gobject) { }
}
