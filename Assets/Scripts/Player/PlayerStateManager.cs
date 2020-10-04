using System.Security.Cryptography;
using UnityEngine;

// main class, stores state machine and public data instances that all states can access.
public class PlayerStateManager : MonoBehaviour
{

    // setting up and adding states to state machine
    public StateMachine m_StateMachine;
    public Transform camTransform;
    public LineRenderer lineRenderer;
    public PlayerMovement playerMovement;
    public ProjectileRenderer projectileRenderer;
    public GameObject cow =null;

    public void Start()
    {
        camTransform = transform.GetChild(0).gameObject.transform;
        playerMovement = gameObject.AddComponent(typeof(PlayerMovement)) as PlayerMovement;
        projectileRenderer = gameObject.AddComponent(typeof(ProjectileRenderer)) as ProjectileRenderer;


        m_StateMachine = new StateMachine(new PlayerMoving(this));
        m_StateMachine.AddState(new PlayerLassoAiming(this, gameObject, 1.0f, 0.5f));
        m_StateMachine.AddState(new PlayerLassoReturning());
        m_StateMachine.AddState(new PlayerLassoWithObject(this));
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
    private GameObject gameObject;
    private float mass;
    private float drag;
    private float force;
    public float minForce = 0.5f;
    public float maxForce = 10f;
    public GameObject cow;

    public PlayerLassoAiming(PlayerStateManager stateManager, GameObject gameObject, float mass, float drag)
    {
        this.gameObject = gameObject;
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
            force += 0.5f* Time.deltaTime;
            
        }

        stateManager.projectileRenderer.SimulatePath(gameObject, stateManager.camTransform.forward * force, mass, drag, stateManager.lineRenderer);
        stateManager.playerMovement.Tick();
        if (Input.GetMouseButtonUp(0))
        {
            RequestTransition<PlayerLassoReturning>();
        }
    }
    public override void OnExit() {
        this.force = minForce;
        stateManager.projectileRenderer.clear();
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
    private float strength = 5;
    private PlayerStateManager stateManager;
    private GameObject cow;

    public PlayerLassoWithObject(PlayerStateManager stateManager)
    {
        this.stateManager =stateManager;
    }

    public override void Tick()
    { 

        
        stateManager.playerMovement.Tick();

		if (Input.GetMouseButton(0))
		{
            Vector3 dir = stateManager.gameObject.transform.position - cow.transform.position;
            dir = dir.normalized;

            cow.GetComponent<Rigidbody>().AddForce(strength*dir);
		}

    }
    public override void OnEnter(){
        cow = stateManager.cow;
	}

}