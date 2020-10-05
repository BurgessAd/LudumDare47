using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lasso : MonoBehaviour
{
    public GameObject lassoColliderPrefab;
    private GameObject lassoCollider;
    public GameObject lassoLoop;
    private GameObject cowProjectile;

    private AnimalStateHandler ash;
    private PlayerStateManager psm;
    
    public LineRenderer lineRenderer;
    private Transform playerCam;
    public Transform firePoint;
    private Transform lassoEnd;

    private float force = 1f;
    private float cowForce = 10f;
    private float time = 0.1f;
    private float rotationSpeed = 500f;
    public Vector3 gravity = new Vector3(0,-9f,0);
    private Vector3 windUpCentre = new Vector3(0,0,0);
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
        psm = GameObject.Find("Player").GetComponent<PlayerStateManager>();
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
                rotationSpeed = 500;

                //stop rendering the winding up loop to avoid a trail
                if (windingUp)
                {
                    lassoLoop.GetComponentInChildren<TrailRenderer>().enabled = false;
                }
                windingUp = false;
            }
        }
        //this handles rendering the lasso attatched to the cow
        //RenderLasso();
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
            
            if (Input.GetMouseButtonUp(0) && ash.m_StateMachine.m_CurrentState.GetType() == typeof(AnimalThrowingState))
            {
                windingUp = false;
                Kill();
                ash.m_StateMachine.RequestTransition(typeof(AnimalThrownState));
                FireCow(cowForce);
                psm.m_StateMachine.RequestTransition(typeof(PlayerMoving));
                ash = null;
                cowForce = 10f;
                rotationSpeed = 500;
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
                rotationSpeed += 10;
            }
        }

        //would need the the cows state to be found here so its not confused with the wrangled state
        if (Input.GetMouseButton(0) && cowProjectile != null && ash.m_StateMachine.m_CurrentState.GetType() == typeof(AnimalThrowingState))
        {
            ash.m_StateMachine.RequestTransition(typeof(AnimalThrowingState));
            WindUpCow();
            cowForce += 1;
            rotationSpeed += 10;
        }
    }

    public void WindUp(){
        //moving the drawing position of the loop so the player can see my great rendering skills
        float drawPoint = 1;

        float r = loopRadius;
        //check to see if the centre has moved since last tick
        if(windUpCentre != firePoint.position + new Vector3(0,1.5f,0) + playerCam.forward.normalized*drawPoint){
            windUpCentre = firePoint.position + new Vector3(0,1.5f,0) + playerCam.forward.normalized*drawPoint;
            if(!windingUp){
                lassoLoop.GetComponentInChildren<TrailRenderer>().enabled = true;
                windingUp = true;
                lassoLoop.GetComponent<Transform>().position = windUpCentre + new Vector3(0,0,r);
            }
            //reposition the loop position by moving the loop back to 1 radius away from the centre
            Vector3 pos = lassoLoop.GetComponent<Transform>().position;
            pos.y = windUpCentre.y;
            pos = (pos - windUpCentre).normalized * r + windUpCentre;
            //(lassoLoop.GetComponent<Transform>().position - windUpCentre).normalized*loopRadius + windUpCentre;
            lassoLoop.GetComponent<Transform>().position = pos;
        }
        lassoLoop.GetComponent<Transform>().RotateAround(windUpCentre, new Vector3(0,1,0), -rotationSpeed*Time.deltaTime);
        lassoEnd = lassoLoop.GetComponent<Transform>();
        RenderLasso();
    }
    
    void WindUpCow(){
        float drawPoint = 1;
        float r = loopRadius + 3;
        windUpCentre = firePoint.position + new Vector3(0, 1.5f, 0) + playerCam.forward.normalized*drawPoint;
        if (!windingUp)
        {
            windingUp = true;
            cowProjectile.GetComponent<Transform>().position = windUpCentre + new Vector3(0, 0, r);
        }
        Vector3 pos = cowProjectile.GetComponent<Transform>().position;
        pos.y = windUpCentre.y;
        pos = (pos - windUpCentre).normalized * r + windUpCentre;
        cowProjectile.GetComponent<Transform>().position = pos;
        cowProjectile.GetComponent<Transform>().RotateAround(windUpCentre, new Vector3(0,1,0), -rotationSpeed*Time.deltaTime);
        lassoEnd = cowProjectile.GetComponent<Transform>();
        RenderLasso();
    }

    void FireCow(float force){
        cowProjectile.GetComponent<Transform>().position = firePoint.position;
        Rigidbody collider = cowProjectile.GetComponent<Rigidbody>();
        collider.AddForce(playerCam.forward*force, ForceMode.Impulse);
    }

    public void callToFireCow(float force)
    {
        Detach();
        FireCow(force);
        windingUp = false;
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
