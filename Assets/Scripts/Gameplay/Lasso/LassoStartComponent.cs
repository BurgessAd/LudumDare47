﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using EZCameraShake;

public class LassoStartComponent : MonoBehaviour
{
    private StateMachine m_StateMachine;
    private ThrowableObjectComponent m_Throwable;
    private Animator m_PowerBarAnimator;

    [SerializeField] private Transform m_LassoStartTransform;
    [SerializeField] private Transform m_LassoEndTransform;
    [SerializeField] private Transform m_SwingPointTransform;
    [SerializeField] private CanvasGroup m_CanGrabCanvasGroup;
    [SerializeField] private PlayerComponent playerComponent;
    [SerializeField] private Rigidbody m_LassoEndRigidBody;
    [SerializeField] private Transform m_ProjectionPoint;
    [SerializeField] private Transform m_LassoGrabPoint;
    [SerializeField] private Transform m_LassoNormalContainerTransform;
    [SerializeField] private IThrowableObjectComponent m_ThrowableComponent;
    [SerializeField] private LassoLoopComponent m_LassoLoop;
    [SerializeField] private PlayerMovement m_Player;
    [SerializeField] private PlayerCameraComponent m_PlayerCam;
    [SerializeField] private AudioManager m_AudioManager;
    [SerializeField] private LineRenderer m_HandRopeLineRenderer;
    [SerializeField] private LineRenderer m_LassoSpinningLoopLineRenderer;
    [SerializeField] private LineRenderer m_TrajectoryLineRenderer;

    [SerializeField] private UIObjectReference m_PowerBarObjectReference;
    [SerializeField] private CowGameManager m_Manager;

    [SerializeField] private LassoParams m_LassoParams;

    private float m_fThrowStrength;

    public AudioManager GetAudioManager => m_AudioManager;

    private Transform m_EndTransform;
    
    public ThrowableObjectComponent GetThrowableObject => m_Throwable;

    public Rigidbody GetLassoBody => m_LassoEndRigidBody;

    public Transform GetEndTransform => m_EndTransform;

    public Transform GetSwingingTransform => m_SwingPointTransform;

    public Transform GetLassoGrabPoint => m_LassoGrabPoint;

    public event Action<ThrowableObjectComponent> OnSetPullingObject;
    public event Action OnStoppedPullingObject;
    public event Action<float, float> OnSetPullingStrength;
    public event Action<float> OnSetSwingingStrength;
    public event Action<ThrowableObjectComponent> OnSetSwingingObject;
    public event Action OnStoppedSwingingObject;
    public event Action OnThrowObject;

    private bool m_bIsAttachedToObject = false;
    private bool m_bCanPickUpObject = false;
    private bool m_bIsShowingGrabbableUI = false;
    private bool m_bPlayerThrown = false;

    private void LateUpdate()
    {
        m_StateMachine.Tick();
    }

    private void Awake()
    {
        m_LassoLoop.OnHitGround += OnHitGround;
        m_LassoLoop.OnHitObject += OnHitObject;

        m_ThrowableComponent.OnThrown += (ProjectileParams pparams) => OnThrown();

        m_ThrowableComponent.OnLanded += OnNotThrown;

        m_Throwable = m_LassoEndTransform.GetComponent<ThrowableObjectComponent>();

        m_EndTransform = m_LassoEndTransform;
        m_StateMachine = new StateMachine(new LassoIdleState(this));
        m_StateMachine.AddState(new LassoReturnState(this, m_LassoParams));
        m_StateMachine.AddState(new LassoThrowingState(this));
        m_StateMachine.AddState(new LassoSpinningState(this, m_LassoParams));
        m_StateMachine.AddState(new LassoAnimalAttachedState(this, m_LassoParams));
        m_StateMachine.AddState(new LassoAnimalSpinningState(this, m_LassoParams));

        // for if we want to start spinning
        m_StateMachine.AddTransition(typeof(LassoIdleState), typeof(LassoSpinningState), () => Input.GetMouseButtonDown(0) && !m_bIsAttachedToObject && !m_bPlayerThrown);

        m_StateMachine.AddTransition(typeof(LassoReturnState), typeof(LassoIdleState), () => Vector3.SqrMagnitude(GetEndTransform.position - m_LassoGrabPoint.position) < 1.0f, () => SetLassoAsChildOfPlayer(true));
        // for if we're spinning and want to cancel 
        m_StateMachine.AddTransition(typeof(LassoSpinningState), typeof(LassoIdleState), () => Input.GetMouseButtonUp(1));
        // for if we're spinning and want to throw
        m_StateMachine.AddTransition(typeof(LassoSpinningState), typeof(LassoThrowingState), () => Input.GetMouseButtonUp(0), () => { ProjectLasso(); SetLassoAsChildOfPlayer(false);});
        // for if we're spinning an animal and want to cancel
        m_StateMachine.AddTransition(typeof(LassoAnimalSpinningState), typeof(LassoIdleState), () => Input.GetMouseButtonUp(1), () => DetachFromObject());
        // for if we're throwing and want to cancel
        m_StateMachine.AddTransition(typeof(LassoThrowingState), typeof(LassoReturnState), () => (Input.GetMouseButtonUp(1) || Vector3.SqrMagnitude(GetEndTransform.position - m_LassoGrabPoint.position) > m_LassoParams.m_LassoLength * m_LassoParams.m_LassoLength), () => { m_LassoEndTransform.GetComponent<FreeFallTrajectoryComponent>().enabled = false; } );
        // for if we've decided we want to unattach to our target
        m_StateMachine.AddTransition(typeof(LassoAnimalAttachedState), typeof(LassoReturnState), () => Input.GetMouseButtonUp(1) || m_bPlayerThrown, () => DetachFromObject());

        m_StateMachine.AddTransition(typeof(LassoAnimalAttachedState), typeof(LassoReturnState), () => !m_bIsAttachedToObject);
        // for if the cow has reached us
        m_StateMachine.AddTransition(typeof(LassoAnimalAttachedState), typeof(LassoAnimalSpinningState), () => m_bCanPickUpObject && Input.GetMouseButtonDown(0));
        // for if we want to throw the animal
        m_StateMachine.AddTransition(typeof(LassoAnimalSpinningState), typeof(LassoIdleState), () => (!Input.GetMouseButton(0) && !m_LassoParams.SpinningIsInitializing), () => { ProjectObject(); SetLassoAsChildOfPlayer(true); });
        // instant transition back to idle state
        m_StateMachine.InitializeStateMachine();

        m_Manager.AddToPauseUnpause(() => enabled = false, () => enabled = true);
    }

	private void Start()
	{
        m_PowerBarAnimator = m_Manager.GetUIElementFromReference(m_PowerBarObjectReference).GetComponent<Animator>();
	}

	public void StartSwingingObject() 
    {
        OnSetSwingingObject(GetThrowableObject);
    }

    public void StopSwingingObject() 
    {
        OnStoppedSwingingObject();
    }

    public void StartDraggingObject() 
    {
        OnSetPullingObject(GetThrowableObject);
    }

    public void StopDraggingObject() 
    {
        OnStoppedPullingObject();
    }

    public void OnThrown() 
    {
        m_bPlayerThrown = true;
    }

    public void OnNotThrown() 
    {
        m_bPlayerThrown = false;
    }

    public void TriggerPowerBarAnimIn() 
    {
        m_PowerBarAnimator.SetBool("AnimOut", false);
        m_PowerBarAnimator.Play("PowerBarInitAnimation", 0);
    }

    public void SetCanGrabEntity(bool canGrab) 
    {
        if (m_bIsShowingGrabbableUI != canGrab) 
        {
            m_bIsShowingGrabbableUI = canGrab;
            LeanTween.cancel(m_CanGrabCanvasGroup.gameObject);
            LeanTween.alphaCanvas(m_CanGrabCanvasGroup, canGrab ? 1 : 0, 0.20f);
        }
        m_bCanPickUpObject = canGrab;
    }

    public void TriggerPowerBarAnimOut() 
    {
        m_PowerBarAnimator.SetBool("AnimOut", true);
    }

    public void OnChangePowerBarValue(float strength) 
    {
        m_PowerBarAnimator.SetFloat("SliderLength", strength);
    }

    public void ActivateLassoCollider(bool activate) 
    {
        m_LassoLoop.EnableColliders(activate);
    }
    ProjectileParams m_projectileParams;
    private void ProjectLasso()
    {
        GetThrowableObject.ThrowObject(m_projectileParams);
    }
    private void OnHitObject(ThrowableObjectComponent throwableObject)
    {
        m_Throwable = throwableObject;
        m_Throwable.Wrangled();
        m_Throwable.OnDestroyed += OnThrowableObjectDestroyed;
        m_StateMachine.RequestTransition(typeof(LassoAnimalAttachedState));
        m_EndTransform = GetThrowableObject.GetAttachmentTransform;
        m_bIsAttachedToObject = true;
    }

    private void OnThrowableObjectDestroyed() 
    {
        m_LassoEndTransform.position = m_Throwable.GetAttachmentTransform.position;
        m_Throwable = m_LassoEndRigidBody.GetComponent<ThrowableObjectComponent>();
        m_LassoEndTransform.SetParent(m_LassoNormalContainerTransform);
        m_EndTransform = m_LassoEndTransform;
        m_bIsAttachedToObject = false;
    }

    private void OnHitGround()
    {
        m_StateMachine.RequestTransition(typeof(LassoReturnState));
    }
    private void ProjectObject()
    {
        m_projectileParams.SetAngularVelocity(360);
        GetThrowableObject.ThrowObject(m_projectileParams);
        float throwForce = m_projectileParams.m_fThrowSpeed / GetThrowableObject.GetMass();
        CameraShaker.Instance.ShakeOnce(throwForce/2, throwForce/2, 0.1f, 1.0f);
        OnThrowObject?.Invoke();
        DetachFromObject();
    }
    private void SetLassoAsChildOfPlayer(bool set)
    {
        if (set)
        {
            m_EndTransform.SetParent(m_LassoNormalContainerTransform);
        }
        else
        {
            m_EndTransform.SetParent(null);
        }
    }

    private void DetachFromObject()
    {
        m_Throwable.Released();
        m_Throwable.OnDestroyed -= OnThrowableObjectDestroyed;
        m_Throwable.GetMainTransform.SetParent(null);
        m_LassoEndTransform.position = m_Throwable.GetAttachmentTransform.position;
        m_Throwable = m_LassoEndRigidBody.GetComponent<ThrowableObjectComponent>();
        m_LassoEndTransform.SetParent(m_LassoNormalContainerTransform);
        m_EndTransform = m_LassoEndTransform;
        m_bIsAttachedToObject = false;
    }

    private float GetForceFromSwingTime() 
    {
        return m_LassoParams.m_ThrowForceCurve.Evaluate(m_fThrowStrength);
    }

    public void RenderThrownLoop()
    {
        Vector3 displacement = GetEndTransform.position - GetLassoGrabPoint.position;
        Vector3 midPoint = GetEndTransform.position + displacement.normalized * 0.8f;

        Quaternion colliderRotation = Quaternion.LookRotation(-displacement, Vector3.up);
        m_LassoLoop.transform.rotation = colliderRotation;

        RenderLoop(0.8f, midPoint, displacement.normalized, Vector3.Cross(displacement, Vector3.up).normalized);
    }

    public void RenderRope()
    {
        m_HandRopeLineRenderer.positionCount = 2;
        m_HandRopeLineRenderer.SetPosition(0, m_LassoGrabPoint.position);
        m_HandRopeLineRenderer.SetPosition(1, GetEndTransform.position);
    }

    public void RenderLoop(in float radius, in Vector3 centrePoint, in Vector3 normA, in Vector3 normB)
    {
        int numIterations = 30;
        m_LassoSpinningLoopLineRenderer.positionCount = numIterations+1;
        float angleIt = Mathf.Deg2Rad * 360 / numIterations;
        for (int i = 0; i <= numIterations; i++) 
        {
            float currAng = i * angleIt;
            Vector3 position = centrePoint + normA * Mathf.Cos(currAng) * radius + normB * Mathf.Sin(currAng) * radius;
            m_LassoSpinningLoopLineRenderer.SetPosition(i, position);
        }
    }

    public void RenderTrajectory() 
    {
        m_projectileParams = new ProjectileParams(GetThrowableObject, GetForceFromSwingTime(), m_ProjectionPoint.forward, m_LassoGrabPoint.position);
        int posCount = 40;
        m_TrajectoryLineRenderer.positionCount = posCount;
        for (int i = 0; i < posCount; i++) 
        {
            float time = (float)i /20;
            
            m_TrajectoryLineRenderer.SetPosition(i, m_projectileParams.EvaluatePosAtTime(time));
        }

    }

    public void SetPullingAnimal(bool set) 
    {
        OnSetPullingObject(set ? GetThrowableObject : null);
    }

    public void SetPullStrength(float totalForce, float tugTime)
    {
        OnSetPullingStrength?.Invoke(totalForce, tugTime);
        OnChangePowerBarValue(totalForce * 0.5f + 0.5f * tugTime);
    }

    public void SetSpinStrength(float strength) 
    {
        m_fThrowStrength = strength;
        OnChangePowerBarValue(strength);
    }

    public void SetRopeLineRenderer(bool enabled)
    {
        m_HandRopeLineRenderer.positionCount = 0;
        m_HandRopeLineRenderer.enabled = enabled;
    }

    public void SetLoopLineRenderer(bool enabled)
    {
        m_LassoSpinningLoopLineRenderer.positionCount = 0;
        m_LassoSpinningLoopLineRenderer.enabled = enabled;
    }

    public void SetTrajectoryRenderer(bool enabled)
    {
        m_TrajectoryLineRenderer.positionCount = 0;
        m_TrajectoryLineRenderer.enabled = enabled;
    }
}

public class LassoSpinningState : AStateBase
{
    float m_fCurrentTimeSpinning = 0.0f;

    float m_fCurrentAngle;
    float m_CurrentInitializeTime = 0.0f;

    private readonly LassoStartComponent m_Lasso;
    private readonly LassoParams m_LassoParams;
    public LassoSpinningState(LassoStartComponent lasso, LassoParams lassoParams)
    {
        m_LassoParams = lassoParams;
        m_Lasso = lasso;
    }

    public override void OnEnter()
    {
        m_Lasso.StartSwingingObject();
        m_CurrentInitializeTime = m_LassoParams.m_TimeBeforeUserCanThrow;
        m_fCurrentAngle = 0.0f;
        m_Lasso.SetLoopLineRenderer(true);
        m_Lasso.SetRopeLineRenderer(true);
        m_Lasso.SetTrajectoryRenderer(true);
        m_fCurrentTimeSpinning = 0.0f;
        m_Lasso.TriggerPowerBarAnimIn();
        m_LassoParams.SpinningIsInitializing = true;
        m_LassoParams.SpunUp = false;
    }

    public override void OnExit()
    {
        m_Lasso.TriggerPowerBarAnimOut();
        m_Lasso.StopSwingingObject();
        m_Lasso.SetLoopLineRenderer(false);
        m_Lasso.SetRopeLineRenderer(false);
        m_Lasso.SetTrajectoryRenderer(false);
        m_LassoParams.SpinningIsInitializing = false;
    }

    public override void Tick()
    {
        float time = 2 * m_fCurrentTimeSpinning / m_LassoParams.m_MaxTimeSpinning;
        m_fCurrentTimeSpinning = Mathf.Min(m_fCurrentTimeSpinning + Time.deltaTime, m_LassoParams.m_MaxTimeSpinning/2);

        float r = m_LassoParams.m_SpinSizeProfile.Evaluate(time);
        float height = m_LassoParams.m_SpinHeightProfile.Evaluate(time);
        m_Lasso.SetSpinStrength(time);
        m_Lasso.RenderTrajectory();

        m_Lasso.GetEndTransform.position = m_Lasso.GetSwingingTransform.position + new Vector3(r * Mathf.Cos(m_fCurrentAngle), height, r * Mathf.Sin(m_fCurrentAngle));

        m_Lasso.RenderRope();
        Vector3 centrePoint = m_Lasso.GetSwingingTransform.position + new Vector3(0, height, 0);
        Vector3 normA = (m_Lasso.GetEndTransform.position - centrePoint).normalized;
        Vector3 normB = Vector3.Cross(normA, Vector3.up);
        m_Lasso.RenderLoop(r, centrePoint, normA, normB);

        m_fCurrentAngle += m_LassoParams.m_SpinSpeedProfile.Evaluate(time) * Time.deltaTime;

        if (m_CurrentInitializeTime > 0) 
        {
            m_CurrentInitializeTime = Mathf.Max(m_CurrentInitializeTime - Time.deltaTime, 0);
            if (m_CurrentInitializeTime == 0) 
            {
                m_LassoParams.SpinningIsInitializing = false;
            }
        }

        if (time >= 1 - Mathf.Epsilon) 
        {
            m_LassoParams.SpunUp = true;
        }
    }
}

public class LassoAnimalSpinningState : AStateBase 
{

    float m_fCurrentTimeSpinning = 0.0f;
    float m_CurrentAngle;

    float m_CurrentInitializeTime = 0.0f;

    private readonly LassoStartComponent m_Lasso;
    private readonly LassoParams m_LassoParams;
    public LassoAnimalSpinningState(LassoStartComponent lasso, LassoParams lassoParams)
    {
        m_LassoParams = lassoParams;
        m_Lasso = lasso;
    }

    public override void OnEnter()
    {
        m_Lasso.StartSwingingObject();
        m_Lasso.SetTrajectoryRenderer(true);
        m_fCurrentTimeSpinning = 0.0f;
        m_CurrentInitializeTime = m_LassoParams.m_TimeBeforeUserCanThrow;
        m_CurrentAngle = 0.0f;
        m_Lasso.SetRopeLineRenderer(true);
        m_Lasso.GetThrowableObject.StartedSpinning();
        m_Lasso.TriggerPowerBarAnimIn();
        m_LassoParams.SpinningIsInitializing = true;
        m_LassoParams.SpunUp = false;
    }

    public override void OnExit()
    {
        m_Lasso.TriggerPowerBarAnimOut();
        m_Lasso.StopSwingingObject();
        m_Lasso.SetTrajectoryRenderer(false);
        m_Lasso.SetRopeLineRenderer(false);
        m_LassoParams.SpinningIsInitializing = false;
    }

    public override void Tick()
    {
        float time = m_fCurrentTimeSpinning / m_LassoParams.m_MaxTimeSpinning;
        float r = m_LassoParams.m_SpinSizeProfile.Evaluate(time);
        float height = m_LassoParams.m_SpinHeightProfile.Evaluate(time);

        m_Lasso.GetThrowableObject.GetMainTransform.position = m_Lasso.GetSwingingTransform.position + new Vector3(r * Mathf.Cos(m_CurrentAngle), height, r * Mathf.Sin(m_CurrentAngle));
        Vector3 forward = m_Lasso.GetLassoGrabPoint.position - m_Lasso.GetThrowableObject.GetAttachmentTransform.position ;
        m_Lasso.GetThrowableObject.GetMainTransform.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
        m_Lasso.SetSpinStrength(time);
        m_Lasso.RenderRope();

        m_Lasso.RenderTrajectory();
        m_fCurrentTimeSpinning = Mathf.Min(m_fCurrentTimeSpinning + Time.deltaTime, m_LassoParams.m_MaxTimeSpinning);

        m_CurrentAngle += m_LassoParams.m_SpinSpeedProfile.Evaluate(time) * Time.deltaTime;
        if (m_CurrentInitializeTime > 0)
        {
            m_CurrentInitializeTime = Mathf.Max(m_CurrentInitializeTime - Time.deltaTime, 0);
            if (m_CurrentInitializeTime == 0)
            {
                m_LassoParams.SpinningIsInitializing = false;
            }
        }
        if (time >= 1 - Mathf.Epsilon)
        {
            m_LassoParams.SpunUp = true;
        }
    }
}

public class LassoThrowingState : AStateBase 
{
    private readonly LassoStartComponent m_Lasso;
    public LassoThrowingState(LassoStartComponent lasso) 
    {
        m_Lasso = lasso;
    }
    public override void OnEnter()
    {
        m_Lasso.ActivateLassoCollider(true);
        m_Lasso.SetRopeLineRenderer(true);
        m_Lasso.SetLoopLineRenderer(true);
    }
    public override void OnExit()
    {
        m_Lasso.SetRopeLineRenderer(false);
        m_Lasso.SetLoopLineRenderer(false);
        m_Lasso.ActivateLassoCollider(false);
    }
    public override void Tick()
    {
        m_Lasso.RenderRope();
        m_Lasso.RenderThrownLoop();
    }
}

public class LassoAnimalAttachedState : AStateBase 
{
    float m_fCurrentJerkTime;
    float m_fTotalCurrentForce;
    float m_fTimeSinceClicked;

    private readonly LassoStartComponent m_Lasso;
    private readonly LassoParams m_LassoParams;
    public LassoAnimalAttachedState(LassoStartComponent lasso, LassoParams lassoParams)
    {
        m_LassoParams = lassoParams;
        m_Lasso = lasso;
    }
    public override void OnEnter()
    {
        m_Lasso.StartDraggingObject();
        m_fTotalCurrentForce = 0.0f;
        m_fTimeSinceClicked = 1.0f;
        m_fCurrentJerkTime = 0.0f;
        m_Lasso.SetRopeLineRenderer(true);
        m_Lasso.TriggerPowerBarAnimIn();
    }

    public override void OnExit()
    {
        m_Lasso.StopDraggingObject();
        m_Lasso.SetRopeLineRenderer(false);
        m_Lasso.TriggerPowerBarAnimOut();
        m_Lasso.SetCanGrabEntity(false);
    }

    public override void Tick()
    {
        m_Lasso.RenderRope();
        m_fTimeSinceClicked += Time.deltaTime;
        Vector3 cowToPlayer = (m_Lasso.GetLassoGrabPoint.position - m_Lasso.GetEndTransform.position).normalized;
        float fForceDecrease = m_LassoParams.m_ForceDecreasePerSecond.Evaluate(m_fTotalCurrentForce/ m_LassoParams.m_MaxForceForPull);
        m_fTotalCurrentForce = Mathf.Max(0.0f, m_fTotalCurrentForce - fForceDecrease * Time.deltaTime);
        m_fCurrentJerkTime = Mathf.Max(0.0f, m_fCurrentJerkTime - Time.deltaTime);

        if (Input.GetMouseButtonDown(0) && m_fTimeSinceClicked > 0.4f) 
        {
            m_Lasso.GetThrowableObject.TuggedByLasso();
            m_fCurrentJerkTime = m_LassoParams.m_JerkTimeForPull;
            float fForceIncrease = m_LassoParams.m_ForceIncreasePerPull.Evaluate(m_fTotalCurrentForce / m_LassoParams.m_MaxForceForPull);
            m_fTotalCurrentForce = Mathf.Min(m_fTotalCurrentForce + fForceIncrease, m_LassoParams.m_MaxForceForPull);
            m_fTimeSinceClicked = 0.0f;
        }

        float jerkScale = m_LassoParams.m_JerkProfile.Evaluate(m_fCurrentJerkTime / m_LassoParams.m_JerkTimeForPull);

        if ((m_Lasso.GetThrowableObject.GetMainTransform.position - m_Lasso.GetSwingingTransform.position).sqrMagnitude < m_LassoParams.m_GrabDistance * m_LassoParams.m_GrabDistance) 
        {
            m_Lasso.SetCanGrabEntity(true);
        }
		else 
        {
            m_Lasso.SetCanGrabEntity(false);
        }

        m_Lasso.GetThrowableObject.ApplyForceToObject(cowToPlayer * m_fTotalCurrentForce * jerkScale * Time.deltaTime);
        Debug.Log(cowToPlayer * m_fTotalCurrentForce * jerkScale * Time.deltaTime);
        m_Lasso.SetPullStrength(m_fTotalCurrentForce/ m_LassoParams.m_MaxForceForPull, m_fCurrentJerkTime / m_LassoParams.m_JerkTimeForPull);
    }
}

public class LassoReturnState : AStateBase 
{
    float m_LassoSpeed = 0.0f;

    private readonly LassoStartComponent m_Lasso;
    private readonly LassoParams m_LassoParams;
    public LassoReturnState(LassoStartComponent lasso, LassoParams lassoParams) 
    {
        m_LassoParams = lassoParams;
        m_Lasso = lasso;
    }

    public override void OnEnter()
    {
        m_LassoSpeed = 0.0f;
        m_Lasso.SetRopeLineRenderer(true);
        m_Lasso.SetLoopLineRenderer(true);
    }

    public override void OnExit()
    {
        m_Lasso.SetRopeLineRenderer(false);
        m_Lasso.SetLoopLineRenderer(false);
    }

    public override void Tick()
    {
       // m_Lasso.RenderLoop(0, Vector3.zero);
        m_Lasso.RenderRope();
        m_Lasso.RenderThrownLoop();
        m_LassoSpeed = (Mathf.Min(m_LassoSpeed + Time.deltaTime * m_LassoParams.m_LassoReturnAcceleration, m_LassoParams.m_LassoReturnAcceleration));
        Vector3 loopToPlayer = (m_Lasso.GetLassoGrabPoint.position - m_Lasso.GetEndTransform.position).normalized;
        m_Lasso.GetEndTransform.rotation = Quaternion.LookRotation(-loopToPlayer, Vector3.up);
        m_Lasso.GetEndTransform.position += m_LassoSpeed * loopToPlayer;
    }
}

public class LassoIdleState : AStateBase
{
    private readonly LassoStartComponent m_Lasso;
    public LassoIdleState(LassoStartComponent lasso)
    {
        m_Lasso = lasso;
    }
}
