using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LassoCollision : MonoBehaviour
{
    public GameObject Entity;
    private GameObject player;
    private Lasso lasso;
    private Rigidbody m_Body;
    private Transform playerTransform;
    
    void Awake(){
        
        player =  GameObject.FindGameObjectWithTag("Player");
        lasso = player.GetComponent<Lasso>();
    }

    void OnCollisionEnter(Collision collision){
        //we can then find the gameObject that we collide with which will come in handy
        Entity = collision.gameObject;

        //these statements find the tag of the gameObject we collide with

        if(Entity.tag == "Cow"){
            AnimalComponent hitCow = Entity.GetComponentInParent<AnimalComponent>();
            //Activates cows lasso on traits
            hitCow.OnLasso();
            //sets local copy of player's cow to wrangled cow
            Debug.Log("hit cow");
            player.GetComponent<PlayerStateManager>().cow = collision.gameObject;
            player.GetComponent<PlayerStateManager>().m_StateMachine.RequestTransition(typeof(PlayerLassoWithObject));
            //attatches to Leash of Cow
            lasso.AttachToCow(Entity.transform.Find("Leash").gameObject, Entity);
            Destroy(gameObject);
        }
        if(Entity.tag == "Floor"){
            //Debug.Log(Entity.transform.position - playerTransform.position);
            Destroy(gameObject);
        }
    }
}
