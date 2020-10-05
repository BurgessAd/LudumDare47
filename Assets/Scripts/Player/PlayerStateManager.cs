using System.Security.Cryptography;
using UnityEngine;

// main class, stores state machine and public data instances that all states can access.
public class PlayerStateManager : MonoBehaviour
{

    // setting up and adding states to state machine
    [HideInInspector]
    public StateMachine m_StateMachine;
    [HideInInspector]

    public Transform camTransform;

    public LineRenderer lineRenderer;
    public GameObject firePoint;

    [HideInInspector]
    public PlayerMovement playerMovement;

    [HideInInspector]
    public ProjectileRenderer projectileRenderer;

    [HideInInspector]
    public GameObject cow =null;

    [HideInInspector]
    public Transform playerTransform;


    public void Start()
    {

        //FindObjectOfType<AudioManager>().Play("Background_Boingy");
        

        this.playerTransform = transform;
        playerMovement = gameObject.AddComponent(typeof(PlayerMovement)) as PlayerMovement;
        projectileRenderer = gameObject.AddComponent(typeof(ProjectileRenderer)) as ProjectileRenderer;

        this.m_StateMachine = new StateMachine(new PlayerMoving(this));
        m_StateMachine.AddState(new PlayerLassoAiming(this, firePoint, 1.0f, 0.5f));
        m_StateMachine.AddState(new PlayerLassoReturning());

        m_StateMachine.AddState(new PlayerLassoWithObject(this));

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
        Camera.FindObjectOfType<AudioManager>().Play("woosh");
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
//State for lassoing Cow
public class PlayerLassoWithObject : IState
{
    //strength of throw
    private float strength = 20;
    private PlayerStateManager stateManager;
    //lassoed cow
    private GameObject cow;

    public PlayerLassoWithObject(PlayerStateManager stateManager)
    {

        this.stateManager =stateManager;

    }

    public override void Tick()
    {
        //if no cow leave state
		if (cow == null)
		{
            stateManager.playerTransform.gameObject.GetComponent<Lasso>().Kill();
            RequestTransition<PlayerMoving>();
            return;
		}
        
        stateManager.playerMovement.Tick();
        //removes lasso with c key
		if (Input.GetKeyDown("c")){
            cow.GetComponent<AnimalComponent>().OffLasso();
            stateManager.playerTransform.gameObject.GetComponent<Lasso>().Kill();
            RequestTransition<PlayerMoving>();
        }
        //fling cow with left click towards player and up
		if (Input.GetMouseButtonDown(0))
		{
            Vector3 dir = stateManager.gameObject.transform.position - cow.transform.position;
            dir = dir.normalized;
            dir += Vector3.up;
            cow.GetComponent<Rigidbody>().AddForce(strength*dir,ForceMode.Impulse);
		}

    }
    public override void OnEnter(){
        cow = stateManager.cow;
	}

}