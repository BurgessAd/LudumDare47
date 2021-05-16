using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerCameraComponent : MonoBehaviour
{
    [SerializeField] private float m_fMouseSensitivity = 100.0f;
    [SerializeField] private Transform m_tBodyTransform;
    [SerializeField] private Camera m_PlayerCamera;
    [SerializeField] private CowGameManager m_Manager;
    [SerializeField] private EZCameraShake.CameraShaker m_CameraShaker;
    [SerializeField] private float m_fMaxFOVChangePerSecond = 1.0f;
    [SerializeField] private float m_fDefaultFOV;
    [SerializeField] private AnimationCurve m_FOVTugAnimator;
    [SerializeField] private AnimationCurve m_FOVForceAnimator;
    [SerializeField] private AnimationCurve m_GroundImpactSpeedSize;

    private float m_fCamPoint;
    float m_fTargetFOV;
    float m_fCurrentFOV;
    private Transform m_tCamTransform;
    private Transform m_tFocusTransform;
    private StateMachine m_CameraStateMachine;
    private Type m_CachedType;

    [Header("FOV Animation Params")]
    [SerializeField] private Animator m_CameraAnimator;
    [SerializeField] private PlayerMovement m_PlayerMovement;
    [SerializeField] private LassoStartComponent m_LassoStart;
    [SerializeField] private string m_JumpString;
    [SerializeField] private string m_GroundedAnimString;
    [SerializeField] private string m_MovementSpeedAnimString;

    private void OnSetPullStrength(float force, float yankSize) 
    {
        m_fTargetFOV = m_FOVTugAnimator.Evaluate(yankSize) * force + m_FOVForceAnimator.Evaluate(force) + m_fDefaultFOV;
    }

    private void OnSetPullingObject(ThrowableObjectComponent pullingObject) 
    {
        SetFocusedTransform(pullingObject.GetCameraFocusTransform);
    }

    private void OnStoppedPullingObject() 
    {
        ClearFocusedTransform();
    }

    public void SetFocusedTransform(Transform focusTransform) 
    {
        m_tFocusTransform = focusTransform;
        m_CameraStateMachine.RequestTransition(typeof(ObjectFocusLook));
        m_CachedType = typeof(ObjectFocusLook);
    }

    private void OnJumped() 
    {
        m_CameraAnimator.SetBool(m_JumpString, true);
    }

    private void OnLeftGround() 
    {
        m_CameraAnimator.SetBool(m_GroundedAnimString, false);
    }

    private void OnHitGround(float impactSpeed) 
    {
        m_CameraAnimator.SetBool(m_JumpString, false);
        float animationSize = m_GroundImpactSpeedSize.Evaluate(Mathf.Abs(impactSpeed));
        m_CameraAnimator.SetBool(m_GroundedAnimString, true);
        m_CameraShaker.ShakeOnce(animationSize, animationSize / 2, 0.15f, 0.45f);
    }

    public void OnSetMovementSpeed(float speed) 
    {
        m_CameraAnimator.SetFloat(m_MovementSpeedAnimString, speed);
    }

    public void ClearFocusedTransform() 
    {
        m_tFocusTransform = null;
        m_fTargetFOV = m_fDefaultFOV;
        m_CameraStateMachine.RequestTransition(typeof(PlayerControlledLook));
        m_CachedType = typeof(PlayerControlledLook);
    }

    public void SetCameraIdle() 
    {
        m_CameraStateMachine.RequestTransition(typeof(CameraIdleState));
    }

    public void UnsetCameraIdle() 
    {
        m_CameraStateMachine.RequestTransition(m_CachedType);
    }

    void Start()
    {
        m_CameraStateMachine = new StateMachine(new PlayerControlledLook(this));
        m_CameraStateMachine.AddState(new ObjectFocusLook(this));
        m_CameraStateMachine.AddState(new CameraIdleState());
        m_tCamTransform = transform;
        m_fTargetFOV = m_fDefaultFOV;
        m_fCurrentFOV = m_fDefaultFOV;
        m_LassoStart.OnSetPullingStrength += OnSetPullStrength;
        m_LassoStart.OnSetPullingObject += OnSetPullingObject;
        m_LassoStart.OnStoppedPullingObject += OnStoppedPullingObject;

        m_Manager.AddToPauseUnpause(() => enabled = false, () => enabled = true);
        m_PlayerMovement.OnHitGround += OnHitGround;
        m_PlayerMovement.OnSuccessfulJump += OnJumped;
        m_PlayerMovement.OnSetMovementSpeed += OnSetMovementSpeed;
        m_PlayerMovement.OnNotHitGround += OnLeftGround;
    }
     
    public void ProcessTargetFOV() 
    {
        m_fCurrentFOV += Mathf.Clamp(m_fTargetFOV - m_fCurrentFOV, -Time.deltaTime * m_fMaxFOVChangePerSecond, Time.deltaTime * m_fMaxFOVChangePerSecond);
        m_PlayerCamera.fieldOfView = m_fCurrentFOV;
    }

    public void ProcessMouseInput() 
    {
        float mouseX = Input.GetAxis("Mouse X") * m_fMouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * m_fMouseSensitivity * Time.deltaTime;

        m_fCamPoint -= mouseY;
        m_fCamPoint = Mathf.Clamp(m_fCamPoint, -80f, 80f);

        m_tCamTransform.localRotation = Quaternion.Euler(m_fCamPoint, 0.0f, 0.0f);
        m_tBodyTransform.Rotate(Vector3.up * mouseX);
    }

    public void ProcessLookTowardsTransform() 
    {
        // rotate body by z in plane towards object
        // rotate cam around x towards object
        Vector3 lookDir = m_tFocusTransform.position - m_tCamTransform.position;

        Quaternion targetCamQuat = Quaternion.FromToRotation(m_tCamTransform.forward, Vector3.ProjectOnPlane(lookDir, m_tCamTransform.right)) * m_tCamTransform.rotation;
        m_tCamTransform.rotation = Quaternion.RotateTowards(m_tCamTransform.rotation, targetCamQuat, 60.0f * Time.deltaTime);

        Quaternion targetBodyQuat = Quaternion.FromToRotation(m_tBodyTransform.forward, Vector3.ProjectOnPlane(lookDir, Vector3.up)) * m_tBodyTransform.rotation;
        m_tBodyTransform.rotation = Quaternion.RotateTowards(m_tBodyTransform.rotation, targetBodyQuat, 60.0f * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        m_CameraStateMachine.Tick();
    }
}

public class PlayerControlledLook : AStateBase
{
    private readonly PlayerCameraComponent mouseLook;
    public PlayerControlledLook(PlayerCameraComponent mouseLook) 
    {
        this.mouseLook = mouseLook;
    }

    public override void Tick()
    {
        mouseLook.ProcessMouseInput();
        mouseLook.ProcessTargetFOV();
    }
}

public class ObjectFocusLook : AStateBase 
{
    private readonly PlayerCameraComponent mouseLook;
    public ObjectFocusLook(PlayerCameraComponent mouseLook)
    {
        this.mouseLook = mouseLook;
    }
    public override void Tick()
    {
        mouseLook.ProcessLookTowardsTransform();
        mouseLook.ProcessTargetFOV();
    }
}

public class CameraIdleState : AStateBase 
{
    
}
