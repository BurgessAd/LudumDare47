using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float forwardSpeed = 0f;
    private float sideSpeed = 0f;

    private float slowFactor = 10f;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        forwardSpeed = Input.GetAxis("Vertical") / slowFactor;
        sideSpeed = Input.GetAxis("Horizontal") / slowFactor;


        Transform camTansform = transform.GetChild(0).gameObject.transform;

        Vector3 newPos = transform.position + (camTansform.forward * forwardSpeed) + (camTansform.right * sideSpeed); ;

        transform.position = new Vector3(newPos.x, 0f, newPos.z);
    }

    private void FixedUpdate()
    {

    }

}
