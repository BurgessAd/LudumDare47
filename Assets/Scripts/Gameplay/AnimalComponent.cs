using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalComponent : MonoBehaviour
{
    private AnimalStateHandler ash;
    [SerializeField]
    private CowGameManager cowGameManager;

    [SerializeField]
    private GameObject animal;
    private static GameObject player;
    private int speed = 3;
    private int evadeRadius = 10;
    private float evadeSpeedMultiplier = 2;
    private float jumpHeight = 2;
    [SerializeField]
    private GameObject body;
    float modifier = 1;
    float size = 1;
    Vector3 yeetDir = new Vector3(1, 1, 0);
    
    private Vector2 rotateDirXZ = new Vector2(10f, 10f);
    private float idleTimeout = 5;
    private float idleTimer = 0;
    private Vector3 moveDir = Vector3.zero;
    private Vector2 dir = Vector2.zero;
    private Vector3 surfaceNorm = new Vector3(0, 1, 0);

    // Start is called before the first frame update
    void Start()
    {
        cowGameManager.cows.Add(gameObject);
        ash = gameObject.AddComponent<AnimalStateHandler>();
        ash.animalComponent = this;

        if (player == null)
        {
            player = GameObject.Find("Player");
        }
        size = Random.value / 2.0f + 0.5f;
        animal.transform.localScale = new Vector3(size, size, size);
    }

    // Update is called once per frame
    void Update()
    {

        animal.GetComponent<Rigidbody>().AddForce(new Vector3(0, -9, 0));

    }

    void Move()
    {

        //Raytrace to keep the cows perp to surface
        RaycastHit hit;
        if (Physics.Raycast(body.transform.position, new Vector3(0, -1, 0), out hit, Mathf.Infinity))
        {
            surfaceNorm = hit.normal;
        }

        float mag = moveDir.magnitude;
		
        
        //Keeps cow movement in plane of surface 
        moveDir = Vector3.ProjectOnPlane(moveDir, surfaceNorm);

        moveDir = moveDir.normalized * mag * size;
        //Raycast to stop hitting into objects
        if (Physics.Raycast(body.transform.position + moveDir.normalized, moveDir, out hit, 3))
        {
            moveDir *= 0;
            //Debug.DrawRay(body.transform.position, transform.forward*3, Color.yellow);
        }
        //Raycast to stop it jumping of cliffs
        if (Physics.Raycast(body.transform.position + moveDir * 2, new Vector3(0, -1, 0), out hit, 5))
        {

            //Debug.DrawRay(body.transform.position + transform.forward * 3, new Vector3(0, -1, 0), Color.red);
        }
        else
        {
            moveDir *= 0;
        }



        //Adds slight movement to body of cow when moving
        if (moveDir != Vector3.zero)
        {
            if (body.transform.rotation.eulerAngles.x >= 92 || body.transform.rotation.eulerAngles.x <= 88)
            {
                rotateDirXZ = new Vector2(-rotateDirXZ.x, rotateDirXZ.y);
            }

            if (body.transform.rotation.eulerAngles.y >= 2 || body.transform.rotation.eulerAngles.y <= -2)
            {
                rotateDirXZ = new Vector2(rotateDirXZ.x, -rotateDirXZ.y);
            }

            body.transform.Rotate(rotateDirXZ.x * Time.deltaTime, 0f, rotateDirXZ.y * Time.deltaTime);

        }

        //Debug.DrawRay(body.transform.position, moveDir, Color.black);
        //Rotates body towards direction it will head
        animal.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(animal.transform.forward, moveDir, Time.deltaTime * modifier, 0));
        //Sets the velocity 
        animal.GetComponent<Rigidbody>().velocity = new Vector3(moveDir.x * modifier, animal.GetComponent<Rigidbody>().velocity.y, moveDir.z * modifier);
		if (mag > 0)
		{
            if(Random.value>0.95)
                animal.GetComponent<Rigidbody>().AddForce(Vector3.up, ForceMode.Impulse);

        }

    }

    void OnDestroy()
	{
        //if wrangled cow is destroyed destroy lasso
		if (ash.m_StateMachine.m_CurrentState.GetType()==typeof(AnimalWrangledState)){
            player.GetComponent<Lasso>().Kill();
        }        
        cowGameManager.cows.Remove(gameObject);
	}

    public void OnLasso()
    {
        ash.m_StateMachine.RequestTransition(typeof(AnimalWrangledState));
    }
    public void OffLasso()
	{
        ash.m_StateMachine.RequestTransition(typeof(AnimalIdleState));
    }



    //Called on state change
    public void EnterEvade()
    {
        //increase speed when evading
        modifier *= evadeSpeedMultiplier;
    }

    public void Evading()
    {
        //Checks to transition to Idle state if further than 1.5 times evade radius
        if ((animal.transform.position - player.transform.position).magnitude > 1.5 * evadeRadius)
        {
            ash.m_StateMachine.RequestTransition(typeof(AnimalIdleState));
            return;
        }
        //choose direction opposite to player to move away
        moveDir = (animal.transform.position - player.transform.position);
        moveDir = moveDir.normalized;
        moveDir *= speed;
        Move();
    }

    //Called when switch to idle state
    public void EnterIdle()
    {
        modifier = 1;
    }

    public void Idle()
    {
        //Checks if needs to change state
        if ((animal.transform.position - player.transform.position).magnitude < evadeRadius)
        {
            ash.m_StateMachine.RequestTransition(typeof(AnimalEvadingState));
            return;
        }

        int jump = 0;
        //checks if it needs to assign new direction or stop moving based on assigned timer
        if (Time.time - idleTimer > idleTimeout)
        {

            idleTimeout = 2 + Random.Range(0, 5);
            idleTimer = Time.time;
            //if wasn't moving, will move
            if (dir == Vector2.zero)
            {
                dir = Random.insideUnitCircle;
                
            }
            else
            {
                dir = Vector2.zero;
            }

            dir *= 3;


        }
        moveDir = new Vector3(dir.x, jump, dir.y);
        Move();

    }

    public void OnWrangled()
	{
        //make the leash visible when wrangled
        gameObject.transform.Find("Leash").gameObject.GetComponent<MeshRenderer>().enabled = true;
	}
    public void OffWrangled()
    {
        //make the leash invisible when unwrangled
        gameObject.transform.Find("Leash").gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    public void Wrangled()
	{

	}

   
    //called when entering Yeeted state
    public void EnterYeet()
	{

        int force = 15;
        GetComponent<Rigidbody>().AddForce(yeetDir * force, ForceMode.Impulse);
    }


    public void Yeeted()
	{
      ash.m_StateMachine.RequestTransition(typeof(AnimalWrangledState));
    }


}
