using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lasso : MonoBehaviour
{
    public GameObject lassoColliderPrefab;
    private GameObject lassoCollider;

    //PlayerCam will be the fire point for the lasso
    private Transform playerCam;
    private float force = 100f;
    private bool fired = false;
    private Vector3 firePoint;

    void Awake(){
        playerCam = GameObject.Find("Main Camera").GetComponent<Transform>();
    }

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
       fired = false;
    }

    void FireLasso(){
        fired = true;
        lassoCollider = Instantiate(lassoColliderPrefab, playerCam.position, Quaternion.Euler(0,0,0), transform);
        Rigidbody collider = lassoCollider.GetComponent<Rigidbody>();
        collider.AddForce(playerCam.forward*force, ForceMode.Impulse);
    }
}
