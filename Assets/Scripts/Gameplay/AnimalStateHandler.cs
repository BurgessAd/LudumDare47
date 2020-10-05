using UnityEngine;
// imagine this were a separate component, that controled general movement. you could reference it from the UFO "manager" class, and use it in states, like below.


// main class, stores state machine and public data instances that all states can access.
public class AnimalStateHandler : MonoBehaviour
{

    public AnimalComponent animalComponent;
    

    // setting up and adding states to state machine
    public StateMachine m_StateMachine;
    public void Start()
    {
        m_StateMachine = new StateMachine(new AnimalIdleState(this));
        m_StateMachine.AddState(new AnimalEvadingState(this));
        m_StateMachine.AddState(new AnimalInPenState(this));
        m_StateMachine.AddState(new AnimalWrangledState(this));
        m_StateMachine.AddState(new AnimalAbductedState(this));
        m_StateMachine.AddState(new AnimalYeetedState(this));
        m_StateMachine.AddState(new AnimalThrowingState(this));
        m_StateMachine.AddState(new AnimalThrownState(this));

    }

    // state machine update
    public void Update()
    {
        
        m_StateMachine.Tick();
    }
}


public class AnimalEvadingState : IState
{
    AnimalStateHandler animalStateHandler;

    public AnimalEvadingState(AnimalStateHandler animalStateHandler)
    {
        this.animalStateHandler = animalStateHandler;
    }
    public override void Tick()
    {
        animalStateHandler.animalComponent.Evading();
        animalStateHandler.animalComponent.addGravity();
    }
    public override void OnEnter()
    {
        animalStateHandler.animalComponent.EnterEvade();
        animalStateHandler.animalComponent.Moo();
    }
}

public class AnimalInPenState : IState
{

    AnimalStateHandler animalStateHandler;
    public AnimalInPenState(AnimalStateHandler animalStateHandler)
    {
        this.animalStateHandler = animalStateHandler;
    }
    public override void Tick()
    {
        animalStateHandler.animalComponent.addGravity();
    }
    public override void OnEnter()
	{
        animalStateHandler.animalComponent.Moo();

    }

}

public class AnimalIdleState : IState 
{
    AnimalStateHandler animalStateHandler;
    public AnimalIdleState(AnimalStateHandler animalStateHandler)
	{
    this.animalStateHandler = animalStateHandler;
	}
    public override void Tick()
    {
        animalStateHandler.animalComponent.Idle();
        animalStateHandler.animalComponent.addGravity();

    }
    public override void OnEnter()
    {
        animalStateHandler.animalComponent.Moo();
        animalStateHandler.animalComponent.EnterIdle();
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
        animalStateHandler.animalComponent.Wrangled();
        animalStateHandler.animalComponent.addGravity();

    }
    public override void OnEnter()
    {
        animalStateHandler.animalComponent.Moo();
        animalStateHandler.animalComponent.OnWrangled();
    }
    public override void OnExit()
	{
        animalStateHandler.animalComponent.OffWrangled();

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

public class AnimalThrownState : IState
{
    AnimalStateHandler animalStateHandler;
    public AnimalThrownState(AnimalStateHandler animalStateHandler)
    {
        this.animalStateHandler = animalStateHandler;
    }
    // here we cache a gameobject in the parent class and change states
    public override void Tick()
    {
        animalStateHandler.animalComponent.addGravity();
    }
    public override void OnEnter()
    {
        animalStateHandler.animalComponent.Moo();
        int i = (int)Random.Range(0, 4);
        if (i == 0) { Camera.FindObjectOfType<AudioManager>().Play("GoneGet_1"); }
        else if (i == 1) { Camera.FindObjectOfType<AudioManager>().Play("GoneGet_2"); }
        else if (i == 2) { Camera.FindObjectOfType<AudioManager>().Play("GoneGet_3"); }
        else { Camera.FindObjectOfType<AudioManager>().Play("GoneGet_4"); }   

    }
    public override void OnExit()
    {

    }


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
        this.ufo = ufo as UfoMain;
        animalStateHandler.animalComponent.Moo();
    }

    public override void Tick()
    {
        animalStateHandler.animalComponent.spinAndScream();
    }
    public override void OnExit()
    {
        ufo.cowEscaped();
    }




}
public class AnimalYeetedState : IState
{
    AnimalStateHandler animalStateHandler;

    public AnimalYeetedState(AnimalStateHandler animalStateHandler)
    {
    this.animalStateHandler = animalStateHandler;
    }

    public override void Tick()
    {
        animalStateHandler.animalComponent.Yeeted();
    }
    public override void OnEnter()
    {
        animalStateHandler.animalComponent.EnterYeet();
        animalStateHandler.animalComponent.Moo();

    }


}
