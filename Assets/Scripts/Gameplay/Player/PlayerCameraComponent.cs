using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraComponent : MonoBehaviour
{
    [SerializeField]
    private float m_fMouseSensitivity = 100.0f;

    [SerializeField]
    private Transform m_tBodyTransform;

    [SerializeField]
    private Camera m_PlayerCamera;

    private float m_fCamPoint;
    private float m_fCurrentFOVPercentage = 0.0f;
    private float m_fTargetFOVPercentage;
    [Header("FOV Animation Params")]
    [SerializeField]
    private float m_fFOVAnimateTime = 1.0f;

    [SerializeField]
    private float m_fDefaultFOV;

    [SerializeField]
    private float m_fMaxWranglingFOV;

    private Transform m_tCamTransform;
    private Transform m_tFocusTransform;
    private StateMachine m_CameraStateMachine;

    public void SetFocusedTransform(Transform focusTransform) 
    {
        m_tFocusTransform = focusTransform;
        m_CameraStateMachine.RequestTransition(typeof(ObjectFocusLook));
    }

    public void ClearFocusedTransform() 
    {
        m_tFocusTransform = null;
        m_CameraStateMachine.RequestTransition(typeof(PlayerControlledLook));
    }

    void Start()
    {
        m_CameraStateMachine = new StateMachine();
        m_CameraStateMachine.AddState(new PlayerControlledLook(this));
        m_CameraStateMachine.AddState(new ObjectFocusLook(this));
        m_CameraStateMachine.SetInitialState(typeof(PlayerControlledLook));
        m_tCamTransform = transform;
    }

    public void ProcessTargetFOV() 
    {
        m_fCurrentFOVPercentage += Mathf.Clamp(m_fTargetFOVPercentage - m_fCurrentFOVPercentage, -Time.deltaTime / m_fFOVAnimateTime, Time.deltaTime / m_fFOVAnimateTime);
        m_PlayerCamera.fieldOfView = m_fCurrentFOVPercentage * m_fMaxWranglingFOV + (1 - m_fCurrentFOVPercentage) * m_fDefaultFOV;
    }

    public void SetFOVTargetStrength(in float targetFOV) 
    {
        m_fTargetFOVPercentage = targetFOV;
    }

    public void ProcessMouseInput() 
    {
        float mouseX = Input.GetAxis("Mouse X") * m_fMouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * m_fMouseSensitivity * Time.deltaTime;

        m_fCamPoint -= mouseY;
        m_fCamPoint = Mathf.Clamp(m_fCamPoint, -90f, 90f);

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

public class PlayerControlledLook : IState
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

public class ObjectFocusLook : IState 
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
