using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;
using System;
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float m_fGravity;
    [SerializeField] private Transform m_tGroundTransform = null;
    [SerializeField] private float m_fGroundDistance = 0.1f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private CharacterController m_CharacterController = null;
    [SerializeField] private Transform m_tBodyTransform = null;
    [SerializeField] private float m_fMaxSpeed = 4;
    [SerializeField] private PlayerCameraComponent m_CameraComponent;
    [SerializeField] private CowGameManager m_Manager;
 
    [SerializeField] private float m_fJumpHeight = 3.0f;
    [SerializeField] private float m_fImpactSpeedReductionPerSecondGrounded;
    [SerializeField] private AnimationCurve m_SpinningStrengthSlowCurve;
    [SerializeField] private ThrowableObjectNoRigidComponent m_throwableObjectComponent;

    [SerializeField] private AnimationCurve m_SpinningMassSlowCurve;
    [SerializeField] private LassoStartComponent m_LassoComponent;

    [SerializeField] private GameObject m_GroundImpactEffectsPrefab;
    [SerializeField] private AnimationCurve m_ImpactStrengthByImpactSpeed;

    public event Action OnSuccessfulJump;
    public event Action<float> OnHitGround;
    public event Action OnNotHitGround;
    public event Action<float> OnSetMovementSpeed;
    private Vector3 m_vVelocity;
    float m_fCurrentSpinningMassSpeedDecrease = 1.0f;
    float m_fCurrentSpinningStrengthSpeedDecrease = 1.0f;
    float m_fCurrentDraggingSpeedDecrease = 1.0f;
    private float m_fSpeed;

    bool m_bHasJumped = false;

    bool m_bIsGrounded;

    // Start is called before the first frame update
    void Start()
    {
        m_LassoComponent.OnSetPullingObject += OnIsPullingObject;
        m_LassoComponent.OnSetSwingingObject += OnIsSpinningObject;
        m_LassoComponent.OnSetSwingingStrength += OnSetSpinningStrength;

        m_LassoComponent.OnStoppedPullingObject += OnStoppedPulling;
        m_LassoComponent.OnStoppedSwingingObject += OnStoppedSpinning;

        m_throwableObjectComponent.OnThrown += OnThrown;

        OnHitGround += OnPlayerHitGround;
        m_Manager.AddToPauseUnpause(() => enabled = false, () => enabled = true);
    }

    void OnIsSpinningObject(ThrowableObjectComponent throwableObject) 
    {
        m_fCurrentSpinningStrengthSpeedDecrease = m_SpinningStrengthSlowCurve.Evaluate(throwableObject.GetMass());
    }

    void OnSetSpinningStrength(float spinningStrength) 
    {
        m_fCurrentSpinningMassSpeedDecrease = m_SpinningMassSlowCurve.Evaluate(spinningStrength);
    }

    void OnPlayerHitGround(float speed) 
    {
        if (speed > m_ImpactStrengthByImpactSpeed.keys[0].time) 
        {
            GameObject resultObject = Instantiate(m_GroundImpactEffectsPrefab, m_tGroundTransform.position, m_tGroundTransform.rotation);
            resultObject.GetComponent<ImpactEffectStrengthManager>().SetParamsOfObject(m_ImpactStrengthByImpactSpeed.Evaluate(speed));
        }

    }


    void OnStoppedSpinning() 
    {
        m_fCurrentSpinningMassSpeedDecrease = 1.0f;
        m_fCurrentSpinningStrengthSpeedDecrease = 1.0f;
    }

    void OnIsPullingObject(ThrowableObjectComponent throwableObject) 
    {
        m_fCurrentDraggingSpeedDecrease = 0.0f;
    }

    void OnStoppedPulling() 
    {
        m_fCurrentDraggingSpeedDecrease = 1.0f;
    }
    bool m_bWasGroundedLastFrame = false;
    Vector3 positionLastFrame;
    // Update is called once per frame

    void OnThrown(ProjectileParams throwDetails) 
    {
        m_vVelocity = throwDetails.EvaluateVelocityAtTime(0);
    }

    Vector3 m_MoveAcceleration = Vector3.zero;
    Vector3 m_CurrentMoving = Vector3.zero;

    void FixedUpdate()
    {
        float currentMultiplier = m_fCurrentSpinningMassSpeedDecrease * m_fCurrentSpinningStrengthSpeedDecrease * m_fCurrentDraggingSpeedDecrease;
        m_fSpeed = m_fMaxSpeed * currentMultiplier;
        m_bIsGrounded = Physics.CheckSphere(m_tGroundTransform.position, m_fGroundDistance, groundMask);
        //movement for the player
        float forwardSpeed = Input.GetAxis("Vertical");
        float sideSpeed = Input.GetAxis("Horizontal");

        Vector3 playerInputMoveDir = (m_tBodyTransform.right * sideSpeed + m_tBodyTransform.forward * forwardSpeed).normalized;


        if (m_CharacterController.isGrounded)
        {
			Vector3 horizontalVelocity = Vector3.ProjectOnPlane(m_vVelocity, Vector3.up) / 1.1f;
			m_vVelocity.x = horizontalVelocity.x;
			m_vVelocity.z = horizontalVelocity.z;
            m_CurrentMoving = Vector3.SmoothDamp(m_CurrentMoving, playerInputMoveDir * m_fSpeed * currentMultiplier, ref m_MoveAcceleration, 0.1f);
            if (!m_bWasGroundedLastFrame) 
            {
                OnHitGround?.Invoke(m_CharacterController.velocity.y);
            }
        }
        else 
        {
            if (m_bWasGroundedLastFrame) 
            {
                OnNotHitGround?.Invoke();
            }
            m_CurrentMoving = Vector3.SmoothDamp(m_CurrentMoving, playerInputMoveDir * m_fSpeed * currentMultiplier, ref m_MoveAcceleration, 0.5f);
            m_vVelocity.y += m_fGravity * Time.fixedDeltaTime;
        }

        m_bWasGroundedLastFrame = m_CharacterController.isGrounded;



        if (Input.GetButtonDown("Jump") && m_CharacterController.isGrounded) 
        {
            OnSuccessfulJump?.Invoke();
            m_vVelocity.y = Mathf.Sqrt(m_fJumpHeight * -2f * currentMultiplier * m_fGravity);
        }    

        m_CharacterController.Move(m_CurrentMoving * Time.fixedDeltaTime + m_vVelocity * Time.fixedDeltaTime);

        Vector3 posThisFrame = m_tBodyTransform.position;
        Vector3 movementThisFrame = posThisFrame - positionLastFrame;
        positionLastFrame = posThisFrame;

        OnSetMovementSpeed?.Invoke(Mathf.Clamp01(movementThisFrame.magnitude / (Time.deltaTime * m_fSpeed)));
    }
}
