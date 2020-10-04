using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float forwardSpeed = 0f;
    private float sideSpeed = 0f;
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

        forwardSpeed = Input.GetAxis("Vertical") / slowFactor;
        sideSpeed = Input.GetAxis("Horizontal") / slowFactor;

        Transform camTansform = Camera.main.transform;
        Vector3 newPos = transform.position + (camTansform.forward * forwardSpeed) + (camTansform.right * sideSpeed); 

        transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);
        Camera.main.transform.position = new Vector3(newPos.x, transform.position.y + camOffset, newPos.z);


    }

}
