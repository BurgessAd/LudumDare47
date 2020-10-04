using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lasso : MonoBehaviour
{
    public GameObject lassoColliderPrefab;
    private GameObject lassoCollider;
    public GameObject lassoLoop;

    public LineRenderer lineRenderer;

    private Transform playerCam;
    public Transform firePoint;
    private Transform lassoEnd;

    private float force = 1f;
    private float time = 0.1f;
    public Vector3 gravity = new Vector3(0,-9f,0);
    private Vector3 windUpCentre;
    private Vector3 windUpStart;
    private Vector3 offset;
    private float offsetScale = 5;
    
    private float despawnTime = 5f;
    private bool madeLasso = false;
    private bool attatched = false;
    private bool windingUp = false;
    private bool renderingLoop = false;

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
    //added to remove lasso publicly
    public void Kill()
	{
        attatched = false;
        madeLasso = false;
        lineRenderer.enabled = false;
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
            MakeLoop();
        }
        if(madeLasso && attatched){
            RenderLasso();
        }
        if(Input.GetKeyDown("f")){
            madeLasso = false;
            lineRenderer.enabled = false;
            lassoLoop.GetComponentInChildren<LineRenderer>().enabled = false;
        }
        //add gravity if the collider exists
        if(lassoCollider != null){
            lassoCollider.GetComponent<Rigidbody>().AddForce(gravity); 
        }
        if(Input.GetMouseButtonUp(0)){
            Detatch();
            FireLasso();
            lassoCollider.GetComponent<TrailRenderer>().time = time;
            madeLasso = true;
            lassoLoop.GetComponentInChildren<TrailRenderer>().enabled = false;
            windingUp = false;
            lineRenderer.enabled = true;
            force = 1f;
            time = 0.01f;
            MakeLoop();
        }
    }

    void FixedUpdate(){
        //throw lasso on mouse1 if there is no collider out there and if it is not attached to anything
        if(Input.GetMouseButton(0)){
            WindUp();
            RemoveLoop();
            if(!windingUp){
                Detatch();
            }
            if(lassoCollider != null){
                Destroy(lassoCollider);
            }
            time += 0.01f;
            force += 1;
        }
    }

    void WindUp(){
        if(!windingUp){
            windUpCentre = firePoint.position + new Vector3(0,1.5f,0);
            Vector3 windUpStart = windUpCentre + new Vector3(0,0,3);
            lassoLoop.GetComponent<Transform>().position = windUpStart;
            lassoLoop.GetComponentInChildren<TrailRenderer>().enabled = true;
        }
        windingUp = true;
        lassoLoop.GetComponent<Transform>().RotateAround(windUpCentre, new Vector3(0,1,0), -1000*Time.deltaTime);
        lassoEnd = lassoLoop.GetComponent<Transform>();
        RenderLasso();
    }

    void MakeLoop(){
        lassoLoop.GetComponentInChildren<LineRenderer>().enabled = true;
        float r = (windUpStart - windUpCentre).magnitude;
        //make number of parts
        int parts = 100;
        float angleSeg = 2*Mathf.PI/parts;
        float theta = 0f;
        lassoLoop.GetComponentInChildren<LineRenderer>().positionCount = parts;
        for(int i = 0; i < parts; i++){
            float x = r*Mathf.Cos(theta);
            float z = r*Mathf.Sin(theta);
            Vector3 pos = new Vector3(lassoCollider.GetComponent<Transform>().position.x + x, lassoCollider.GetComponent<Transform>().position.y, lassoCollider.GetComponent<Transform>().position.z + z);
            lassoLoop.GetComponentInChildren<LineRenderer>().SetPosition(i, pos + offset);
            theta += angleSeg;
        }
    }

    public void AttatchToCow(GameObject cow){
        //Debug.Log("attatching to cow");
        lassoEnd = cow.GetComponent<Transform>();
        attatched = true;
        RemoveLoop();
    }

    void RemoveLoop(){
        lassoLoop.GetComponentInChildren<LineRenderer>().enabled = false;
    }

    public void Detatch(){
        
        attatched = false;
        lineRenderer.enabled = false;
    }

    void FireLasso(float force){
        lassoCollider = Instantiate(lassoColliderPrefab, firePoint.position, Quaternion.Euler(0,0,0), transform);
        Rigidbody collider = lassoCollider.GetComponent<Rigidbody>();
        collider.AddForce(playerCam.forward*force, ForceMode.Impulse);
        offset.x = playerCam.forward.x*offsetScale;
        offset.y = playerCam.forward.y;
        offset.z = playerCam.forward.z*offsetScale;
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
        lineRenderer.enabled = true;
        if(renderingLoop){
            lassoLoop.GetComponentInChildren<TrailRenderer>().enabled = false;
        }
    }

    IEnumerator Despawn(float despawnTime){
        //I think I'm calling this then destroying a lasso at a later time (idealy this would stop when the collider is null, not check for it after waiting)
        yield return new WaitForSeconds(despawnTime);
        if(lassoCollider != null){
            Destroy(lassoCollider);
        }
    }
}
