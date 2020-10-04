using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;

public class CamMovement : MonoBehaviour
{

    private float YSensitivity = 8.0f;
    private float XSensitivity = 8.0f;
    private float yaw    = 0.0f;
    private float pitch  = 0.0f;
    private float maxPitch = 80;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        yaw += XSensitivity * Input.GetAxis("Mouse X");
        pitch += YSensitivity * Input.GetAxis("Mouse Y");
        if (pitch > maxPitch) pitch = maxPitch;
        if (-pitch > maxPitch) pitch = -maxPitch;
        transform.eulerAngles = new Vector3(-pitch, yaw, 0f);
// transform.parent.Find("FirePoint").transform.eulerAngles = new Vector3(-pitch, yaw, 0f);
    }

    void FixedUpdate()
    {

    }

}


