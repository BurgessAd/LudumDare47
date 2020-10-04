using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lasso : MonoBehaviour
{
    public GameObject lassoColliderPrefab;
    private GameObject lassoCollider;

    public LineRenderer lineRenderer;

    private Transform playerCam;
    public Transform firePoint;
    private Transform lassoEnd;

    private float force = 100f;
    public Vector3 gravity = new Vector3(0,-9f,0);
    
    private float despawnTime = 5f;
    private bool madeLasso = false;
    private bool attatched = false;

    //only change the lasso length to avoid problems
    //private List<RopeSegment> ropeSegments = new List<RopeSegment>();
    private float ropeSegLen = 0.25f;
    private int lassoLength = 100;
    private float lineWidth = 0.1f;
    private Vector3 ropePos;

    void Awake(){
        playerCam = Camera.main.GetComponent<Transform>();
    }
   
    // Use this for initialization
    void Start()
    {
        this.lineRenderer = this.GetComponent<LineRenderer>();
    }
    
    void Update()
    {   
        //throw lasso on mouse1 if there is no collider out there and if it is not attached to anything
/*        if(Input.GetButtonDown("Fire1")){
            Detatch();
            Destroy(lassoCollider);
            FireLasso();
            madeLasso = true;
            lineRenderer.enabled = true;
        }*/
        //if there is a collider and the rope isnt attached to anything else set the end of the lasso to the collider
        if(lassoCollider != null && !attatched){
            lassoEnd = lassoCollider.GetComponent<Transform>();
        }
        //render the lasso if one has been made
        if(madeLasso && lassoCollider != null){
            RenderLasso();
        }
        if(madeLasso && attatched){
            RenderLasso();
        }
        if(Input.GetKeyDown("f")){
            madeLasso = false;
            lineRenderer.enabled = false;
        }
        //add gravity if the collider exists
        if(lassoCollider != null){
            lassoCollider.GetComponent<Rigidbody>().AddForce(gravity); 
        }
    }

    public void AttatchToCow(GameObject cow){
        Debug.Log("attatching to cow");
        lassoEnd = cow.GetComponent<Transform>();
        attatched = true;
    }

    void Detatch(){
        attatched = false;
    }

    void FireLasso(float force){
        lassoCollider = Instantiate(lassoColliderPrefab, firePoint.position, Quaternion.Euler(0,0,0), transform);
        Rigidbody collider = lassoCollider.GetComponent<Rigidbody>();
        collider.AddForce(playerCam.forward*force, ForceMode.Impulse);
        //StartCoroutine(Despawn(despawnTime));
    }

    public void callToFireLasso(float force)
    {
        Detatch();
        Destroy(lassoCollider);
        FireLasso(force);
        madeLasso = true;
        lineRenderer.enabled = true;
    }

    void RenderLasso(){
        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, lassoEnd.position);
    }

    IEnumerator Despawn(float despawnTime){
        //I think I'm calling this then destroying a lasso at a later time (idealy this would stop when the collider is null, not check for it after waiting)
        yield return new WaitForSeconds(despawnTime);
        if(lassoCollider != null){
            Destroy(lassoCollider);
        }
    }
}
