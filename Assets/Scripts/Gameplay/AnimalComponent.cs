using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalComponent : MonoBehaviour
{
    private AnimalStateHandler ash;

    [SerializeField]
    private GameObject animal;
    private static GameObject player;
    private int speed = 3;
    private int evadeRadius = 10;
    private float evadeSpeedMultiplier = 2;
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


        RaycastHit hit;
        if (Physics.Raycast(body.transform.position, new Vector3(0, -1, 0), out hit, Mathf.Infinity))
        {
            surfaceNorm = hit.normal;
        }

        float mag = moveDir.magnitude;

        moveDir = Vector3.ProjectOnPlane(moveDir, surfaceNorm);

        moveDir = moveDir.normalized * mag * size;

        if (Physics.Raycast(body.transform.position + moveDir.normalized, moveDir, out hit, 3))
        {
            moveDir *= 0;
            //Debug.DrawRay(body.transform.position, transform.forward*3, Color.yellow);
        }

        if (Physics.Raycast(body.transform.position + moveDir * 2, new Vector3(0, -1, 0), out hit, 5))
        {

            //Debug.DrawRay(body.transform.position + transform.forward * 3, new Vector3(0, -1, 0), Color.red);
        }
        else
        {
            moveDir *= 0;
        }




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

        animal.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(animal.transform.forward, moveDir, Time.deltaTime * modifier, 0));
        animal.GetComponent<Rigidbody>().velocity = new Vector3(moveDir.x * modifier, animal.GetComponent<Rigidbody>().velocity.y, moveDir.z * modifier);
        

    }

    public void EnterEvade()
    {
        modifier *= evadeSpeedMultiplier;
    }

    public void Evading()
    {
        if ((animal.transform.position - player.transform.position).magnitude > 1.5 * evadeRadius)
        {
            ash.m_StateMachine.RequestTransition(typeof(AnimalIdleState));
            return;
        }
        moveDir = (animal.transform.position - player.transform.position);
        moveDir = moveDir.normalized;
        moveDir *= speed;
        Move();
    }


    public void EnterIdle()
    {
        modifier = 1;
    }

    public void Idle()
    {
        if ((animal.transform.position - player.transform.position).magnitude < evadeRadius)
        {
            ash.m_StateMachine.RequestTransition(typeof(AnimalEvadingState));
            return;
        }

        int jump = 0;
        if (Time.time - idleTimer > idleTimeout)
        {

            idleTimeout = 2 + Random.Range(0, 5);
            idleTimer = Time.time;
            if (dir == Vector2.zero)
            {
                dir = Random.insideUnitCircle;
                if (Random.value > 0.9f)
                {
                    jump = 10;
                }
                else
                {
                    jump = 0;
                }
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
