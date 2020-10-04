using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UfoMain : MonoBehaviour
{
    private GameObject targetCow;

    public Boolean inSwoopDown = false;
    public Boolean inSwoopUp = false;

    private float speed = 15f;
    private float lowestY = 20;
    private float roamingY = 20;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0f, 20f, 0f));

        if (inSwoopDown)
        {
            Vector3 targetPosition = new Vector3(targetCow.transform.position.x, lowestY, targetCow.transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed);
            if (transform.position == targetPosition)
            {
                inSwoopDown = false;
            }
        }

        if (inSwoopUp)
        {
            Vector3 targetPosition = new Vector3(transform.position.x, roamingY, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed);
            if (transform.position == targetPosition)
            {
                inSwoopUp = false;
            }
        }
    }

    public GameObject FindCow()
    {
        return null;
    }

    public void SwoopTo(GameObject target)
    {
        targetCow = target;
        inSwoopDown = true;
    }
    public void Leave()
    {
        inSwoopUp = true;

    }
}
