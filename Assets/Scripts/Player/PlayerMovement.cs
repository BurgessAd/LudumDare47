using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float forwardSpeed = 0f;
    private float sideSpeed = 0f;

    private float speed = 8;

    private float camOffset = 1f;

    private float slowFactor = 10f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Tick()

    {
 
        forwardSpeed = Input.GetAxis("Vertical");
        sideSpeed = Input.GetAxis("Horizontal");


        Transform camTransform = Camera.main.gameObject.transform;

        Vector3 velocity = camTransform.forward * forwardSpeed + camTransform.right * sideSpeed;
        velocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
        velocity *= speed;
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
        rb.AddForce(-9 * Vector3.up);


        Camera.main.transform.position = gameObject.transform.position+ Vector3.up*camOffset;



    }

}
