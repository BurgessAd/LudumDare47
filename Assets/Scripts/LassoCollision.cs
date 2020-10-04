﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LassoCollision : MonoBehaviour
{
    private GameObject Entity;
    private GameObject player;
    private Lasso lasso;
    private Transform playerTransform;
    
    void Awake(){
        player =  GameObject.FindGameObjectWithTag("Player");
        playerTransform = player.GetComponent<Transform>();
        lasso = player.GetComponent<Lasso>();
    }

    void OnCollisionEnter(Collision collision){
        //we can then find the gameObject that we collide with which will come in handy
        Entity = collision.gameObject;

        //these statements find the tag of the gameObject we collide with
        if(Entity.tag == "Cow"){
            Debug.Log("We hit Cow");
            //Debug.Log(Entity.transform.position - playerTransform.position);
            lasso.AttatchToCow(Entity);
            Destroy(gameObject);
        }
        if(Entity.tag == "Floor"){
            //Debug.Log(Entity.transform.position - playerTransform.position);
            Destroy(gameObject);
        }
    }
}