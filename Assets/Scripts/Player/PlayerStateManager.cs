using System.Security.Cryptography;
using UnityEngine;

// main class, stores state machine and public data instances that all states can access.
public class PlayerStateManager : MonoBehaviour
{

    // setting up and adding states to state machine
    private StateMachine m_StateMachine;
    public Transform camTransform;
    public LineRenderer lineRenderer;
    public PlayerMovement playerMovement;
    public ProjectileRenderer projectileRenderer;
    public void Start()
    {
        camTransform = transform.GetChild(0).gameObject.transform;
        playerMovement = gameObject.AddComponent(typeof(PlayerMovement)) as PlayerMovement;
        projectileRenderer = gameObject.AddComponent(typeof(ProjectileRenderer)) as ProjectileRenderer;


        m_StateMachine = new StateMachine(new PlayerMoving(this));
        //m_StateMachine = new StateMachine(new PlayerLassoAiming(this, transform.gameObject, 1.0f, 0.5f));
        m_StateMachine.AddState(new PlayerLassoReturning());
        m_StateMachine.AddState(new PlayerLassoWithObject());
    }


    // state machine update
    public void Update()
    {
        m_StateMachine.Tick();
    }
}

public class PlayerMoving : IState
{
    private float forwardSpeed = 0f;
    private float sideSpeed = 0f;
    private float slowFactor = 10f;
    private PlayerStateManager stateManager;

    public PlayerMoving(PlayerStateManager stateManager)
    {
        this.stateManager = stateManager;
    }

    public override void Tick()
    {
        stateManager.playerMovement.Tick();
    }

}


public class PlayerLassoAiming : IState
{
    private PlayerStateManager stateManager;
    private GameObject gameObject;
    private float mass;
    private float drag;

    public PlayerLassoAiming(PlayerStateManager stateManager, GameObject gameObject, float mass, float drag)
    {
        this.gameObject = gameObject;
        this.mass = mass;
        this.drag = drag;
        this.stateManager = stateManager;

    }
    // here we cache a gameobject in the parent class and change states
    public override void Tick()
    {
        stateManager.projectileRenderer.SimulatePath(gameObject, (stateManager.camTransform.right + stateManager.camTransform.up + stateManager.camTransform.forward), mass, drag, stateManager.lineRenderer);

        stateManager.playerMovement.Tick();
    }

}
    

public class PlayerLassoReturning : IState
{
    public PlayerLassoReturning()
    {

    }

    public override void Tick()
    {
    }


}

public class PlayerLassoWithObject : IState
{
    public PlayerLassoWithObject()
    {
        ;
    }

    public override void Tick()
    {
    }

}