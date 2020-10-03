using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lasso : MonoBehaviour
{

    public GameObject lasso;
    public GameObject lassoCollider;

    //firepoint will be the players position
    public Transform firePoint;
    public Transform playerCam;
    private float force = 100f;
    private bool fired = false;

    void Update()
    {

        if(Input.GetButtonDown("Fire1") && !fired){
            //can add a hold here to increase the distance of the throw
            FireLasso();
        }
        if(lassoCollider != null){
            lassoCollider.GetComponent<Rigidbody>().AddForce(new Vector3(0,-9,0));
        }
        HandleCollision();
    }

    void HandleCollision(){
        //on collision with an entity (ground or otherwise)
        fired = false;
    }

    void FireLasso(){
        fired = true;
        lassoCollider = Instantiate(lassoCollider, transform.position, Quaternion.Euler(0,0,0), transform);
        Rigidbody collider = lassoCollider.GetComponent<Rigidbody>();
        collider.AddForce(playerCam.forward*force, ForceMode.Impulse);
    }

}
