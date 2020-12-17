using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;
using System;
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float m_fGravity;

    [SerializeField]
    private Transform m_tGroundTransform = null;

    [SerializeField]
    private float m_fGroundDistance = 0.1f;

    [SerializeField]
    private LayerMask groundMask;

    [SerializeField]
    private CharacterController m_CharacterController = null;

    [SerializeField]
    private Transform m_tBodyTransform = null;

    [SerializeField]
    private float m_fMaxSpeed = 4;

    [SerializeField]
    private PlayerCameraComponent m_CameraComponent;

    [SerializeField]
    private float m_fSpeed;

    private float m_fCurrentMultiplier = 1.0f;

    [SerializeField]
    private float m_fJumpHeight = 3.0f;

    private Vector3 m_vVelocity;

    bool m_bHasJumped = false;

    bool m_bIsGrounded;

    // Start is called before the first frame update
    void Start()
    {
        m_fCurrentMultiplier = 1.0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        m_fSpeed = m_fMaxSpeed * m_fCurrentMultiplier;
        m_bIsGrounded = Physics.CheckSphere(m_tGroundTransform.position, m_fGroundDistance, groundMask);
        //movement for the player

        if (m_CharacterController.isGrounded)
        {
            if (m_bHasJumped) 
            {
                m_CameraComponent.OnImpactAnimation();
                m_bHasJumped = false;
            }

            m_vVelocity.y = -20;
   
        }
        else 
        {
            m_vVelocity.y += m_fGravity * Time.fixedDeltaTime;
        }


        float forwardSpeed = Input.GetAxis("Vertical");
        float sideSpeed = Input.GetAxis("Horizontal");

        Vector3 move = m_tBodyTransform.right * sideSpeed + m_tBodyTransform.forward * forwardSpeed;

        if (Input.GetButtonDown("Jump") && m_CharacterController.isGrounded) 
        {
            m_vVelocity.y = Mathf.Sqrt(m_fJumpHeight * -2f * m_fCurrentMultiplier * m_fGravity);
            m_bHasJumped = true;
        }

        

        m_CharacterController.Move(move * m_fSpeed * Time.fixedDeltaTime + m_vVelocity * Time.fixedDeltaTime);
    }

    public void SetMovementSpeedMult(float mult)
    {
        m_fCurrentMultiplier = mult;
    }
}
