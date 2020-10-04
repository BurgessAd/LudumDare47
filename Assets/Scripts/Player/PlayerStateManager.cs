using System.Security.Cryptography;
using UnityEngine;

// main class, stores state machine and public data instances that all states can access.
public class PlayerStateManager : MonoBehaviour
{

    // setting up and adding states to state machine
    private StateMachine m_StateMachine;
    public LineRenderer lineRenderer;
    public GameObject firePoint;
    public PlayerMovement playerMovement;
    public ProjectileRenderer projectileRenderer;
    public Transform playerTransform;

    public void Start()
    {
        this.playerTransform = transform;
        playerMovement = gameObject.AddComponent(typeof(PlayerMovement)) as PlayerMovement;
        projectileRenderer = gameObject.AddComponent(typeof(ProjectileRenderer)) as ProjectileRenderer;

        this.m_StateMachine = new StateMachine(new PlayerMoving(this));
        m_StateMachine.AddState(new PlayerLassoAiming(this, firePoint, 1.0f, 0.5f));
        m_StateMachine.AddState(new PlayerLassoReturning());
        m_StateMachine.AddState(new PlayerLassoWithObject());
        m_StateMachine.AddState(new PlayerLassoThrown(this));

    }



    // state machine update
    public void Update()
    {
        m_StateMachine.Tick();

    }
}

public class PlayerMoving : IState
{
    private PlayerStateManager stateManager;

    public PlayerMoving(PlayerStateManager stateManager)
    {
        this.stateManager = stateManager;
    }

    public override void Tick()
    {
        stateManager.playerMovement.Tick();
        if (Input.GetMouseButtonDown(0))
        {
            RequestTransition<PlayerLassoAiming>();
        }
    }


}


public class PlayerLassoAiming : IState
{
    private PlayerStateManager stateManager;
    private GameObject firePoint;
    private float mass;
    private float drag;
    private float force;
    public float minForce = 10f;
    public float maxForce = 100f;
    public PlayerLassoAiming(PlayerStateManager stateManager, GameObject firePoint, float mass, float drag)
    {
        this.firePoint = firePoint;
        this.mass = mass;
        this.drag = drag;
        this.stateManager = stateManager;
        this.force = minForce;
    }
    // here we cache a gameobject in the parent class and change states
    public override void Tick()
    {

        if(force < maxForce)
        {
            force += 5f* Time.deltaTime;
            
        }

        stateManager.projectileRenderer.SimulatePath(firePoint,this.force, mass, drag, stateManager.lineRenderer);
        stateManager.playerMovement.Tick();
        if (Input.GetMouseButtonUp(0))
        {

            RequestTransition<PlayerLassoThrown>();
        }
    }
    public override void OnExit() {
        stateManager.playerTransform.gameObject.GetComponent<Lasso>().callToFireLasso(this.force);
        this.force = minForce;
        stateManager.projectileRenderer.clear();
    }
}


public class PlayerLassoThrown : IState
{
    private PlayerStateManager stateManager;

    public PlayerLassoThrown(PlayerStateManager stateManager)
    {
        this.stateManager = stateManager;
    }
    // here we cache a gameobject in the parent class and change states
    public override void Tick()
    {

        stateManager.playerMovement.Tick();
        RequestTransition<PlayerMoving>();
    }
}


public class PlayerLassoReturning : IState
{
    public PlayerLassoReturning()
    {

    }

    public override void Tick()
    {
        RequestTransition<PlayerMoving>();
    }


}

public class PlayerLassoWithObject : IState
{
    public PlayerLassoWithObject()
    {
        
    }

    public override void Tick()
    {
    }

}