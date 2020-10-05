using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UfoMain : MonoBehaviour
{
    private GameObject targetCow;

    private UfoStateManager stateManager;

    private float speed = 15f;
    private float Tractorbeamspeed = 5f;
    private float lowestY = 20;
    private float roamingY = 50;
    private float abductDistance = 9.0f;
    public int health = 3;

    private Vector3 ExitPoint = new Vector3(50f, 50f, 50f);
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void swoopDownTick()
    {
        transform.Rotate(new Vector3(0f, 20f, 0f));

        Vector3 targetPosition = new Vector3(targetCow.transform.position.x, lowestY, targetCow.transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, (speed * Time.deltaTime));

        if (transform.position == targetPosition)
        {
            stateManager.m_StateMachine.RequestTransition(typeof(UfoStateManager.UfoAbduct));
        }
    }

    public void abductTick()
    {
        transform.Rotate(new Vector3(0f, 20f, 0f));

        if (targetCow != null)
        {
            targetCow.transform.position = Vector3.MoveTowards(targetCow.transform.position, transform.position, (Tractorbeamspeed * Time.deltaTime));
            //targetCow.transform.position = new Vector3(targetCow.transform.position.x, targetCow.transform.position.y + (Tractorbeamspeed * Time.deltaTime), targetCow.transform.position.z);
            if (Vector3.Distance(transform.position, targetCow.transform.position) <= abductDistance)
            {
                killCow();
                turnBeamOff();
                stateManager.m_StateMachine.RequestTransition(typeof(UfoStateManager.UfoReturnSweep));

            }
        }
        else
        {
            stateManager.m_StateMachine.RequestTransition(typeof(UfoStateManager.UfoReturnSweep));
        }

    }

    public void swoopUpTick()
    {
        transform.Rotate(new Vector3(0f, 20f, 0f));

        transform.position = Vector3.MoveTowards(transform.position, ExitPoint, (speed * Time.deltaTime));

        if (transform.position == ExitPoint)
        {
            stateManager.m_StateMachine.RequestTransition(typeof(UfoStateManager.UfoIdle));
        }
    }

    public void deathTick()
    {
        turnRed();
        flip();
    }
    void flip()
    {
        transform.Rotate(new Vector3(60f*Time.deltaTime, 0f, 0f));
    }
    public void turnRed()
    {
        increaseRed(transform);
        foreach (Transform child in transform)
        {
            increaseRed(child);
        }

    }
    public void increaseRed(Transform t)
    {
        Color c = t.GetComponent<Renderer>().material.color;
        float newRed = c.r + 0.333f * Time.deltaTime <= 1 ? c.r + 0.333f * Time.deltaTime : 1f;
        t.GetComponent<Renderer>().material.color = new Color(newRed, c.g, c.b, c.a);
    }

    public void setStateManager(UfoStateManager stateManager)
    {
        if (this.stateManager == null)
        {
            this.stateManager = stateManager;
        }
    }



    public void setTarget(GameObject target)
    {
        targetCow = target;

    }


    public void abductCow()
    {
        targetCow.GetComponent<AnimalComponent>().Abducted(this);
        turnBeamOn();
    }
    public void cowEscaped()
    {
        turnBeamOff();
        stateManager.m_StateMachine.RequestTransition(typeof(UfoStateManager.UfoReturnSweep));


    }
    public void killCow()
    {
        Destroy(targetCow);
    }

    public void turnInvisible()
    {
        transform.GetComponent<Renderer>().enabled = false;
    }
    public void turnVisible()
    {
        transform.GetComponent<Renderer>().enabled = true;

    }

    public void turnBeamOn()
    {
        transform.Find("tractorBeam").GetComponent<MeshRenderer>().enabled = true;
    } 
    public void turnBeamOff()
    {
        transform.Find("tractorBeam").GetComponent<MeshRenderer>().enabled = false;

    }
    public void wobble()
    {
        float angleDelta = 0.1f;

        // check if value not 0 and tease the rotation towards it using angleDelta
        if (transform.rotation.x > 0)
        {
            angleDelta = -0.2f;
        }
        else if (transform.rotation.x < 0)
        {
            angleDelta = 0.2f;
        }
        transform.Rotate(new Vector3(transform.rotation.x + angleDelta, transform.rotation.y, transform.rotation.z));

    }
    public void resetRotation()
    {
        transform.transform.eulerAngles = new Vector3(0f, 0f, 0f);

    }
    public void OnCollisionEnter(Collision collision)
    {
        hit();
    }
    public void hit()
    {
        health--;
        if (health == 0) { stateManager.m_StateMachine.RequestTransition(typeof(UfoStateManager.UfoDeath)); }
        else { stateManager.m_StateMachine.RequestTransition(typeof(UfoStateManager.UfoStaggered), stateManager.m_StateMachine.m_CurrentState.ToString()); }
    }
}
