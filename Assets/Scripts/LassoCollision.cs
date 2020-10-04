using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LassoCollision : MonoBehaviour
{
    private GameObject Entity;
    private Transform playerTransform;
    
    void Awake(){
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>(); 
    }

    void OnCollisionEnter(Collision collision){
        //we can then find the gameObject that we collide with which will come in handy
        Entity = collision.gameObject;
        Debug.Log("collision");
        //these statements find the tag of the gameObject we collide with
        if(Entity.tag == "Cow"){
            Debug.Log("We hit Cow");
            Debug.Log(Entity.transform.position - playerTransform.position);
        }
        if(Entity.tag == "Floor"){
            Debug.Log("We hit Floor");
            Debug.Log(Entity.transform.position - playerTransform.position);
        }
        Destroy(gameObject);
    }
}
