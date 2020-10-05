using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lasso : MonoBehaviour
{
    public GameObject lassoColliderPrefab;
    private GameObject lassoCollider;
    public GameObject lassoLoop;
    private AnimalStateHandler ash;
    private GameObject cowProjectile;

    public LineRenderer lineRenderer;

    private Transform playerCam;
    public Transform firePoint;
    private Transform lassoEnd;

    private float force = 1f;
    private float cowForce = 100f;
    private float time = 0.1f;
    public Vector3 gravity = new Vector3(0,-9f,0);
    private Vector3 windUpCentre;
    private Vector3 windUpStart;
    private float loopRadius = 2;
    private Vector3 offset = new Vector3(0,0,0);
    //private float offsetScale = 5;
    
    private float despawnTime = 5f;
    private bool madeLasso = false;
    private bool attatched = false;
    private bool windingUp = false;

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
        //make sure the we have not wrangled a cow
        if(ash == null || ash.m_StateMachine.m_CurrentState.GetType() != typeof(AnimalWrangledState) && ash.m_StateMachine.m_CurrentState.GetType() != typeof(AnimalThrowingState) && !Input.GetKeyDown("f") && ash.m_StateMachine.m_CurrentState.GetType() != typeof(AnimalThrownState) && ash.m_StateMachine.m_CurrentState.GetType() != typeof(AnimalYeetedState)){
            //if there is a collider and the rope isnt attached to anything else set the end of the lasso to the collider
            if (lassoCollider != null && !attatched)
            {
                lassoEnd = lassoCollider.GetComponent<Transform>();
            }
            //render the lasso if one has been made
            if (madeLasso && lassoCollider != null)
            {
                RenderLasso();
                MakeLoop();
            }
            if (madeLasso && attatched)
            {
                RenderLasso();
            }
            if (lassoCollider != null)
            {
                lassoCollider.GetComponent<Rigidbody>().AddForce(gravity);
            }
            if (Input.GetMouseButtonUp(0))
            {
                Detach();
                madeLasso = true;
                lineRenderer.enabled = true;
                force = 1f;
                time = 0.01f;

                //stop rendering the winding up loop to avoid a trail
                if (windingUp)
                {
                    lassoLoop.GetComponentInChildren<TrailRenderer>().enabled = false;
                }
                windingUp = false;
            }
        }
        //this handles rendering the lasso attatched to the cow
        RenderLasso();
        if (attatched){
            RenderLasso();
        }
        if (ash != null)
        {
            //transition from wrangled state to throwing state
            if (Input.GetKeyDown("f") && ash.m_StateMachine.m_CurrentState.GetType() == typeof(AnimalWrangledState))
            {
                ash.m_StateMachine.RequestTransition(typeof(AnimalThrowingState));
            }
            //leave throwing state
            if (Input.GetKeyDown("g") && ash.m_StateMachine.m_CurrentState.GetType() == typeof(AnimalThrowingState))
            {
                ash.m_StateMachine.RequestTransition(typeof(AnimalWrangledState));
            }
            //would need the the cows state to be found here so its not confused with the wrangled state
            if (Input.GetMouseButton(0) && cowProjectile != null && ash.m_StateMachine.m_CurrentState.GetType() == typeof(AnimalThrowingState))
            {
                Debug.Log(ash.m_StateMachine.m_CurrentState.GetType());
                WindUpCow();
            }
            if (Input.GetMouseButtonUp(0) && ash.m_StateMachine.m_CurrentState.GetType() == typeof(AnimalThrowingState))
            {
                windingUp = false;
                ash.m_StateMachine.RequestTransition(typeof(AnimalThrownState));
                FireCow();
            }
        }
        
    }

    void FixedUpdate(){

        if(ash == null || ash.m_StateMachine.m_CurrentState.GetType() != typeof(AnimalWrangledState) && ash.m_StateMachine.m_CurrentState.GetType() != typeof(AnimalThrowingState) && ash.m_StateMachine.m_CurrentState.GetType() != typeof(AnimalThrownState ) && ash.m_StateMachine.m_CurrentState.GetType() != typeof(AnimalYeetedState)){
            //throw lasso on mouse1 if there is no collider out there and if it is not attached to anything
            if (Input.GetMouseButton(0) && !attatched)
            {
                WindUp();
                RemoveLoop();
                if (!windingUp)
                {
                    Detach();
                }
                if (lassoCollider != null)
                {
                    Destroy(lassoCollider);
                }
                time += 0.01f;
                force += 1;
            }
        }
    }


    public void WindUp(){
        //Vector3 pos = firePoint.position;
        //pos.y = playerCam.position.y + 2;
        windUpCentre = firePoint.position + new Vector3(0,1.5f,0);
        if(!windingUp){
            lassoLoop.GetComponent<Transform>().position = windUpCentre + new Vector3(0,0,loopRadius);
            lassoLoop.GetComponentInChildren<TrailRenderer>().enabled = true;
            windingUp = true;
        }
        lassoLoop.GetComponent<Transform>().RotateAround(windUpCentre, new Vector3(0,1,0), -1000*Time.deltaTime);
        lassoEnd = lassoLoop.GetComponent<Transform>();
        //Debug.Log(firePoint.position);
        //Debug.Log((lassoEnd.position - windUpCentre).magnitude);
        RenderLasso();
    }
    
    void WindUpCow(){
        windUpCentre = firePoint.position + new Vector3(0,2f,0);
        //giving a bigger radius for looks
        if(!windingUp){
            cowProjectile.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
            cowProjectile.GetComponent<Transform>().position = windUpCentre + new Vector3(0,0,loopRadius + 2);
        }
        windingUp = true;
        cowProjectile.GetComponent<Transform>().RotateAround(windUpCentre, new Vector3(0,1,0), -1000*Time.deltaTime);
        lassoEnd = cowProjectile.GetComponent<Transform>();
        //lassoEnd = cowProjectile.gameObject.GetType
        RenderLasso();
    }

    void FireCow(){
        cowProjectile.GetComponent<Transform>().position = firePoint.position;
        Rigidbody collider = cowProjectile.GetComponent<Rigidbody>();
        collider.AddForce(playerCam.forward*cowForce, ForceMode.Impulse);
    }

    void MakeLoop(){
        lassoLoop.GetComponentInChildren<LineRenderer>().enabled = true;
        float r = loopRadius;
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

    public void AttachToCow(GameObject leash, GameObject cow){
        lassoEnd = leash.GetComponent<Transform>();
        attatched = true;
        ash = cow.GetComponent<AnimalStateHandler>();
        Debug.Log(ash.m_StateMachine.m_CurrentState.GetType());
        RemoveLoop();
        AttachToCowToThrow(cow);
    }

    //for now will call this in the attachtocow method
    void AttachToCowToThrow(GameObject cow){
        cowProjectile = cow;
    }

    void RemoveLoop(){
        lassoLoop.GetComponentInChildren<LineRenderer>().enabled = false;
    }

    public void Detach(){
        madeLasso = false;
        attatched = false;
        lineRenderer.enabled = false;
    }

    void FireLasso(float force){
        lassoCollider = Instantiate(lassoColliderPrefab, firePoint.position, Quaternion.Euler(0,0,0), transform);
        Rigidbody collider = lassoCollider.GetComponent<Rigidbody>();
        collider.AddForce(playerCam.forward*force, ForceMode.Impulse);
        offset.x = playerCam.forward.normalized.x*loopRadius;
        offset.z = playerCam.forward.normalized.z*loopRadius;
    }

    public void callToFireLasso(float force)
    {
        Detach();
        Destroy(lassoCollider);
        FireLasso(force);
        lassoCollider.GetComponent<TrailRenderer>().time = time;
        lassoLoop.GetComponentInChildren<TrailRenderer>().enabled = false;
        windingUp = false;
        MakeLoop();
        madeLasso = true;
        lineRenderer.enabled = true;
    }

    void RenderLasso(){
        if(lassoEnd != null){
            lineRenderer.SetPosition(0, firePoint.position);
            lineRenderer.SetPosition(1, lassoEnd.position);
            lineRenderer.enabled = true;
        }
        
    }

}
