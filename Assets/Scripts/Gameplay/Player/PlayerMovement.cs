using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;

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

    private float m_fSpeed;

    private float m_fCurrentMultiplier = 1.0f;

    [SerializeField]
    private float m_fJumpHeight = 3.0f;

    private Vector3 m_vVelocity;

    bool m_bIsGrounded = false;

    // Start is called before the first frame update
    void Start()
    {
        m_fCurrentMultiplier = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        m_fSpeed = m_fMaxSpeed * m_fCurrentMultiplier;
        m_bIsGrounded = Physics.CheckSphere(m_tGroundTransform.position, m_fGroundDistance, groundMask);
        //movement for the player

        if (m_bIsGrounded && m_vVelocity.y < 0)
        {
            m_vVelocity.y = -2.0f;
        }


        float forwardSpeed = Input.GetAxis("Vertical");
        float sideSpeed = Input.GetAxis("Horizontal");

        Vector3 move = m_tBodyTransform.right * sideSpeed + m_tBodyTransform.forward * forwardSpeed;
        m_CharacterController.Move(move * m_fSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && m_bIsGrounded) 
        {
            m_vVelocity.y = Mathf.Sqrt(m_fJumpHeight * -2f * m_fCurrentMultiplier * m_fGravity);
        }

        m_vVelocity.y += m_fGravity * Time.deltaTime;

        m_CharacterController.Move(m_vVelocity * Time.deltaTime);
    }

    public void SetMovementSpeedMult(float mult)
    {
        m_fCurrentMultiplier = mult;
    }
}
