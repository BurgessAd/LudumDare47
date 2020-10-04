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

    }

    public void Tick()
    {
        RaycastHit hit;
        float lastDistToFloor = 1f;
        if (Physics.Raycast(transform.position, new Vector3(0, -1, 0), out hit, Mathf.Infinity)){ lastDistToFloor = hit.distance; }
        

        forwardSpeed = Input.GetAxis("Vertical") / slowFactor;
        sideSpeed = Input.GetAxis("Horizontal") / slowFactor;


        Transform camTansform = transform.Find("Main Camera").gameObject.transform;

        Vector3 newPos = transform.position + (camTansform.forward * forwardSpeed) + (camTansform.right * sideSpeed); 

        transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);

        if (Physics.Raycast(transform.position, new Vector3(0, -1, 0), out hit, Mathf.Infinity) && lastDistToFloor != hit.distance) {
            transform.position = new Vector3(newPos.x, transform.position.y + (lastDistToFloor - hit.distance), newPos.z);
        }

        

    }

}
