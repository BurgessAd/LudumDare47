using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimalAttacher
{
    AnimalComponent GetAttachedAnimal { get; }
}

public class LassoStartComponent : MonoBehaviour, IAnimalAttacher
{
    private StateMachine m_StateMachine;
    private AnimalComponent m_AttachedAnimal;

    private GameObject m_LassoCollider;

    [SerializeField]
    private Transform m_LassoStartTransform;
    [SerializeField]
    private Transform m_LassoEndTransform;
    [SerializeField]
    private Transform m_SwingPointTransform;
    [SerializeField]
    private Rigidbody m_LassoEndRigidBody;
    [SerializeField]
    private Transform m_ProjectionPoint;
    [SerializeField]
    private Transform m_LassoGrabPoint;

    [SerializeField]
    private PlayerMovement m_Player;
    [SerializeField]
    private PlayerCameraComponent m_PlayerCam;

    [SerializeField]
    private AudioManager m_AudioManager;

    [SerializeField]
    private LineRenderer m_LassoHandToLoopLineRenderer;

    [SerializeField]
    private LineRenderer m_LassoSpinningLoopLineRenderer;

    [SerializeField]
    private LineRenderer m_TrajectoryLineRenderer;

    float m_fThrowSpeed;

    float m_fMaxThrowSpeed;

    [SerializeField]
    private float m_fGravity;

    public AudioManager GetAudioManager => m_AudioManager;

    private Transform m_EndTransform;
    

    public AnimalComponent GetAttachedAnimal => m_AttachedAnimal;

    public Rigidbody GetLassoBody => m_LassoEndRigidBody;

    public Transform GetEndTransform => m_EndTransform;

    public Transform GetSwingingTransform => m_SwingPointTransform;

    public Transform GetLassoGrabPoint => m_LassoGrabPoint;

    [SerializeField]
    private Transform m_LassoNormalContainerTransform;
    [SerializeField]
    private Collider m_LassoHitCollider;
    [SerializeField]
    private LassoEndComponent m_LassoEnd;
    bool ShouldStartSpinning() 
    {
        return Input.GetMouseButtonDown(0) && !m_AttachedAnimal;
    }
    private void LateUpdate()
    {
        m_StateMachine.Tick();
    }

    private void Awake()
    {
        m_LassoEnd.OnHitGround += OnHitGround;
        m_LassoEnd.OnHitAnimal += OnHitCow;
        m_EndTransform = m_LassoEndTransform;
        m_StateMachine = new StateMachine();
        m_StateMachine.AddState(new LassoReturnState(this));
        m_StateMachine.AddState(new LassoThrowingState(this));
        m_StateMachine.AddState(new LassoSpinningState(this));
        m_StateMachine.AddState(new LassoAnimalAttachedState(this, this));
        m_StateMachine.AddState(new LassoAnimalSpinningState(this, this));
        m_StateMachine.AddState(new LassoIdleState(this));

        // for if we want to start spinning
        m_StateMachine.AddTransition(typeof(LassoIdleState), typeof(LassoSpinningState), () => ShouldStartSpinning());

        m_StateMachine.AddTransition(typeof(LassoReturnState), typeof(LassoIdleState), () => Vector3.SqrMagnitude(GetEndTransform.position - m_LassoGrabPoint.position) < 1.0f);
        // for if we're spinning and want to cancel 
        m_StateMachine.AddTransition(typeof(LassoSpinningState), typeof(LassoIdleState), () => Input.GetMouseButtonUp(1));
        // for if we're spinning and want to throw
        m_StateMachine.AddTransition(typeof(LassoSpinningState), typeof(LassoThrowingState), () => Input.GetMouseButtonUp(0));
        // for if we're spinning an animal and want to cancel
        m_StateMachine.AddTransition(typeof(LassoAnimalSpinningState), typeof(LassoIdleState), () => { if (Input.GetMouseButtonUp(1)) { UnattachLeash(); return true; } return false; });
        // for if we're throwing and want to cancel
        m_StateMachine.AddTransition(typeof(LassoThrowingState), typeof(LassoReturnState), () => Vector3.SqrMagnitude(GetEndTransform.position - m_LassoGrabPoint.position) > 20.0f * 20.0f);

        m_StateMachine.AddTransition(typeof(LassoThrowingState), typeof(LassoReturnState), () => (Input.GetMouseButtonUp(1)) );
        // for if we've decided we want to unattach to our target
        m_StateMachine.AddTransition(typeof(LassoAnimalAttachedState), typeof(LassoReturnState), () => {if (Input.GetMouseButtonUp(1)){ UnattachLeash(); return true; } return false; });
        // for if the cow has reached us
        m_StateMachine.AddTransition(typeof(LassoAnimalAttachedState), typeof(LassoAnimalSpinningState), () => Vector3.Distance(GetAttachedAnimal.GetLeashTransform.position, m_LassoStartTransform.position) < 2.0f);
        // for if we want to throw the animal
        m_StateMachine.AddTransition(typeof(LassoAnimalSpinningState), typeof(LassoIdleState), () => { if (Input.GetMouseButtonUp(0)) { GetAttachedAnimal.OnThrownByLasso(); ProjectCow(); UnattachLeash(); return true; } return false; });
        // instant transition back to idle state
        m_StateMachine.SetInitialState(typeof(LassoIdleState));

    }


    public void ActivateLassoCollider(bool activate) 
    {
        m_LassoHitCollider.enabled = activate;
        m_LassoEndRigidBody.isKinematic = !activate;
    }
    public void SetLassoAsChildOfPlayer(bool set)   
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


    public void RenderRope()
    {
        m_LassoHandToLoopLineRenderer.positionCount = 2;
        m_LassoHandToLoopLineRenderer.SetPosition(0, m_LassoGrabPoint.position);
        m_LassoHandToLoopLineRenderer.SetPosition(1, GetEndTransform.position);
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

    public void OrientLoopCollider() 
    {
        GetEndTransform.rotation = Quaternion.LookRotation(GetEndTransform.position - m_LassoGrabPoint.position, Vector3.up);
        GetEndTransform.position = GetPositionFromThrown(cachedStartPos, cachedStartTime, cachedElevationAngle, cachedThrowSpeed, cachedForwardDir);
        cachedStartTime += Time.deltaTime;
    }

    public void RenderTrajectory() 
    {
        Vector3 startPos = m_LassoGrabPoint.position;
        Vector3 forwardDir = m_LassoGrabPoint.forward;
        Vector3 projectionDir = m_ProjectionPoint.forward;
        int posCount = 40;
        float elevationAngle = Mathf.Atan(Vector3.Dot(projectionDir, Vector3.up)/Vector3.Dot(projectionDir, forwardDir));
        m_TrajectoryLineRenderer.positionCount = posCount;
        for (int i = 0; i < posCount; i++) 
        {
            float time = (float)i /20;
            
            m_TrajectoryLineRenderer.SetPosition(i, GetPositionFromThrown(startPos, time, elevationAngle, m_fThrowSpeed, forwardDir));
        }

    }
    private float cachedThrowSpeed;
    private Vector3 cachedStartPos;
    private Vector3 cachedForwardDir;
    private float cachedElevationAngle;
    private float cachedStartTime;
    private Vector3 GetPositionFromThrown(in Vector3 startPos, in float time, in float angle, in float throwSpeed, in Vector3 forwardDir) 
    {
       return
            startPos
            + Vector3.up * (-0.5f * m_fGravity * time * time + throwSpeed * Mathf.Sin(angle) * time)
            + forwardDir * (Mathf.Cos(angle) * m_fThrowSpeed * time);

    }
   
    public void ProjectLasso() 
    {
        cachedThrowSpeed = m_fThrowSpeed;
        cachedStartPos = m_ProjectionPoint.position;
        cachedStartTime = 0.0f;
        cachedForwardDir = m_LassoGrabPoint.forward;
        cachedElevationAngle = Mathf.Atan(Vector3.Dot(m_ProjectionPoint.forward, Vector3.up) / Vector3.Dot(m_ProjectionPoint.forward, cachedForwardDir));
    }

    public void ProjectCow() 
    {
        GetAttachedAnimal.GetThrowable.ThrowObject
            (
            m_fThrowSpeed,
            UnityEngine.Random.Range(200.0f, 300.0f),
            m_ProjectionPoint.position,
            m_LassoGrabPoint.forward,
            Mathf.Atan(Vector3.Dot(m_ProjectionPoint.forward, Vector3.up) / Vector3.Dot(m_ProjectionPoint.forward, cachedForwardDir)),
            m_fGravity,
            Random.insideUnitSphere
            );
    }

    public void SetSpinState(bool set) 
    {
        m_TrajectoryLineRenderer.positionCount = 0;
        m_TrajectoryLineRenderer.enabled = set;
    }

    public void SetPullingAnimal(bool set) 
    {
        if (set)
        {
            m_Player.SetMovementSpeedMult(0.0f);
            m_PlayerCam.SetFocusedTransform(m_AttachedAnimal.GetBodyTransform);
            
        }
        else 
        {
            m_Player.SetMovementSpeedMult(1.0f);
            m_PlayerCam.ClearFocusedTransform();
        }
    }


    public void SetPullEffectsLevel(float level)
    {
        m_PlayerCam.SetFOVTargetStrength(level);
    }


    public void SetSpinStrength(float strength) 
    {
        m_fThrowSpeed = m_fMaxThrowSpeed * strength;
    }

    public void SetPlayerSpeed(float speed) 
    {
        m_Player.SetMovementSpeedMult(speed);
    }

    public void SetMaxThrowSpeed(float maxSpeed) 
    {
        m_fMaxThrowSpeed = maxSpeed;
    }

    public void RenderThrownLoop()
    {
        Vector3 displacement = GetEndTransform.position - GetLassoGrabPoint.position;
        Vector3 midPoint = GetEndTransform.position + displacement.normalized * 0.8f;
        RenderLoop(0.8f, midPoint, displacement.normalized, Vector3.Cross(displacement, Vector3.up).normalized);
    }

    public void SetRopeLineRenderer(bool enabled)
    {
        m_LassoHandToLoopLineRenderer.positionCount = 0;
        m_LassoHandToLoopLineRenderer.enabled = enabled;
    }

    public void SetLoopLineRenderer(bool enabled)
    {
        m_LassoSpinningLoopLineRenderer.positionCount = 0;
        m_LassoSpinningLoopLineRenderer.enabled = enabled;
    }

    private void OnHitCow(AnimalComponent animalStateHandler)
    {
       
        m_AttachedAnimal = animalStateHandler;
        m_AttachedAnimal.OnWrangledByLasso(m_LassoNormalContainerTransform);
        m_StateMachine.RequestTransition(typeof(LassoAnimalAttachedState));
        m_EndTransform.SetParent(GetAttachedAnimal.GetLeashTransform);
        m_EndTransform.localPosition = Vector3.zero;
        m_EndTransform = m_AttachedAnimal.GetLeashTransform;
    }

    private void OnHitGround()
    {
        m_StateMachine.RequestTransition(typeof(LassoReturnState));
    }

    public void UnattachLeash()
    {
        m_AttachedAnimal.OnReleasedByLasso();
        m_AttachedAnimal.GetMainTansform.SetParent(null);
        m_AttachedAnimal = null;
        m_LassoEndTransform.SetParent(m_LassoNormalContainerTransform);
        m_EndTransform = m_LassoEndTransform;
    }
}

public class LassoSpinningState : IState
{
    private readonly LassoStartComponent m_Lasso;

    float m_StartAngle;
    float m_RotationSpeed = 360 * Mathf.Deg2Rad;

    float m_fCurrentTimeSpinning = 0.0f;
    float m_fMaxTimeSpinning = 2.0f;
    float m_fMaxTimeToSwitchStrengths = 0.5f;

    float m_fChosenStrength = 0.5f;
    float m_fCurrentStrength;

    float m_fMaxSpeedSpinning = 3.6f;
    float m_fInitialSpeedSpinning = 0.6f;

    float m_fMaxHeight = 3.0f;
    float m_fMinHeight = 1.0f;

    float m_fMaxRadius = 1.0f;
    float m_fInitialRadius = 2.0f;
    public LassoSpinningState(LassoStartComponent lasso)
    {
        m_Lasso = lasso;
    }

    public override void OnEnter()
    {
        m_StartAngle = 0.0f;
        m_fChosenStrength = 1.0f;
        m_fCurrentStrength = 1.0f;
        m_Lasso.SetLoopLineRenderer(true);
        m_Lasso.SetRopeLineRenderer(true);
        m_Lasso.SetSpinState(true);
        m_Lasso.SetMaxThrowSpeed(20.0f);
        m_fCurrentTimeSpinning = 0.0f;
    }

    public override void OnExit()
    {
        m_Lasso.SetLoopLineRenderer(false);
        m_Lasso.SetRopeLineRenderer(false);
        m_Lasso.SetSpinState(false);
        m_Lasso.SetPlayerSpeed(1.0f);
    }

    public override void Tick()
    {
        m_fChosenStrength = Mathf.Clamp(m_fChosenStrength + Input.mouseScrollDelta.y * 0.1f, 0.0f, 1.0f);
        float time = m_fCurrentTimeSpinning / m_fMaxTimeSpinning;
        float chosenTime = Mathf.Min(time, m_fCurrentStrength);
        float r = m_fInitialRadius + (m_fMaxRadius - m_fInitialRadius) * chosenTime;
        float height = m_fMinHeight + (m_fMaxHeight - m_fMinHeight) * chosenTime;
        m_Lasso.GetEndTransform.position = m_Lasso.GetSwingingTransform.position + new Vector3(r * Mathf.Cos(m_StartAngle), height, r * Mathf.Sin(m_StartAngle));

        m_Lasso.SetSpinStrength(chosenTime * 0.5f + 0.5f);
        m_Lasso.RenderRope();
        m_Lasso.SetPlayerSpeed(1 - (0.3f * chosenTime + 0.3f));
        Vector3 centrePoint = m_Lasso.GetSwingingTransform.position + new Vector3(0, height, 0);
        Vector3 normA = (m_Lasso.GetEndTransform.position - centrePoint).normalized;
        Vector3 normB = Vector3.Cross(normA, Vector3.up);
        m_Lasso.RenderLoop(r, centrePoint, normA, normB);
        m_Lasso.RenderTrajectory();
        m_fCurrentTimeSpinning = Mathf.Min(m_fCurrentTimeSpinning + Time.deltaTime, m_fMaxTimeSpinning);
        if (m_fCurrentStrength != m_fChosenStrength) 
        {
            float sign = Mathf.Sign(m_fChosenStrength - m_fCurrentStrength);
            m_fCurrentStrength += sign * Mathf.Min(Time.deltaTime/ m_fMaxTimeToSwitchStrengths,Mathf.Abs(m_fChosenStrength - m_fCurrentStrength));
        }

        m_StartAngle += m_RotationSpeed * (m_fInitialSpeedSpinning + (m_fMaxSpeedSpinning-m_fInitialSpeedSpinning)* chosenTime) * Time.deltaTime;
    }
}

public class LassoAnimalSpinningState : IState 
{
    private readonly IAnimalAttacher m_AnimalAttacher;
    private readonly LassoStartComponent m_Lasso;
    float m_fMaxTimeSpinning = 2.0f;
    float m_fMaxTimeToSwitchStrengths = 0.5f;

    float m_fChosenStrength = 0.5f;
    float m_fCurrentStrength;

    float m_fMaxSpeedSpinning = 3.6f;
    float m_fInitialSpeedSpinning = 1.6f;

    float m_fMaxHeight = 1.0f;
    float m_fMinHeight = 0.5f;

    float m_fMaxRadius = 2.0f;
    float m_fInitialRadius = 4.0f;

    float m_fCurrentTimeSpinning = 0.0f;
    float m_StartAngle;
    private float m_RotationSpeed = 60 * Mathf.Deg2Rad;

    public LassoAnimalSpinningState(LassoStartComponent lasso, IAnimalAttacher animalAttacher)
    {
        m_Lasso = lasso;
        m_AnimalAttacher = animalAttacher;
    }

    public override void OnEnter()
    {
        m_Lasso.SetSpinState(true);
        m_Lasso.SetMaxThrowSpeed(100.0f);
        m_fChosenStrength = 1.0f;
        m_fCurrentStrength = 1.0f;
        m_fCurrentTimeSpinning = 0.0f;
        m_StartAngle = 0.0f;
        m_Lasso.SetRopeLineRenderer(true);
        m_AnimalAttacher.GetAttachedAnimal.OnStartedLassoSpinning();
        m_Lasso.SetPlayerSpeed(0.05f);
        m_AnimalAttacher.GetAttachedAnimal.GetBodyTransform.rotation = Quaternion.identity;
    }

    public override void OnExit()
    {
        m_Lasso.SetSpinState(false);
        m_Lasso.SetRopeLineRenderer(false);
        m_Lasso.SetPlayerSpeed(1.0f);
        m_Lasso.SetPullEffectsLevel(0.0f);
    }

    public override void Tick()
    {
        m_fChosenStrength = Mathf.Clamp(m_fChosenStrength + Input.mouseScrollDelta.y * 0.1f, 0.0f, 1.0f);
        float time = m_fCurrentTimeSpinning / m_fMaxTimeSpinning;
        float chosenTime = Mathf.Min(time, m_fCurrentStrength);
        float r = m_fInitialRadius + (m_fMaxRadius - m_fInitialRadius) * chosenTime;
        float height = m_fMinHeight + (m_fMaxHeight - m_fMinHeight) * chosenTime;
        m_AnimalAttacher.GetAttachedAnimal.GetCowRigidBody.position = m_Lasso.GetSwingingTransform.position + new Vector3(r * Mathf.Cos(m_StartAngle), height, r * Mathf.Sin(m_StartAngle));
        Vector3 forward = m_Lasso.GetLassoGrabPoint.position - m_AnimalAttacher.GetAttachedAnimal.GetLeashTransform.position ;
        m_AnimalAttacher.GetAttachedAnimal.GetCowRigidBody.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
        m_Lasso.SetSpinStrength(chosenTime * 0.5f + 0.5f);
        m_Lasso.RenderRope();

        m_Lasso.RenderTrajectory();
        m_fCurrentTimeSpinning = Mathf.Min(m_fCurrentTimeSpinning + Time.deltaTime, m_fMaxTimeSpinning);
        if (m_fCurrentStrength != m_fChosenStrength)
        {
            float sign = Mathf.Sign(m_fChosenStrength - m_fCurrentStrength);
            m_fCurrentStrength += sign * Mathf.Min(Time.deltaTime / m_fMaxTimeToSwitchStrengths, Mathf.Abs(m_fChosenStrength - m_fCurrentStrength));
        }

        m_StartAngle += m_RotationSpeed * (m_fInitialSpeedSpinning + (m_fMaxSpeedSpinning - m_fInitialSpeedSpinning) * chosenTime) * Time.deltaTime;


        m_Lasso.SetPullEffectsLevel(0.5f + 0.5f * m_fCurrentStrength);
    }
}

public class LassoThrowingState : IState 
{
    private readonly LassoStartComponent m_Lasso;
    public LassoThrowingState(LassoStartComponent lasso) 
    {
        m_Lasso = lasso;
    }

    public override void OnEnter()
    {
        m_Lasso.ActivateLassoCollider(true);
        m_Lasso.SetLassoAsChildOfPlayer(false);
        m_Lasso.SetRopeLineRenderer(true);
        m_Lasso.SetLoopLineRenderer(true);
        m_Lasso.ProjectLasso();
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
        m_Lasso.OrientLoopCollider();
    }
}

public class LassoAnimalAttachedState : IState 
{
    private readonly IAnimalAttacher m_AnimalAttacher;
    private readonly LassoStartComponent m_Lasso;

    float m_ForceDecreasePerSec = 150.0f;
    float m_ForceIncreasePerClick = 120.0f;

    float m_fMiniDecayTimeMax = 0.15f;
    float m_fCurrentMiniDecayTime;

    float m_fTimeSinceClicked;

    float m_TotalForce = 0.0f;
    float m_MaxForce = 350.0f;
    public LassoAnimalAttachedState(LassoStartComponent lasso, IAnimalAttacher animalAttacher)
    {
        m_Lasso = lasso;
        m_AnimalAttacher = animalAttacher;
    }
    public override void OnEnter()
    {
        m_TotalForce = 0.0f;
        m_fTimeSinceClicked = 1.0f;
        m_fCurrentMiniDecayTime = 0.0f;
        m_Lasso.SetRopeLineRenderer(true);
        m_Lasso.SetPullingAnimal(true);
    }

    public override void OnExit()
    {
        m_Lasso.SetRopeLineRenderer(false);
        m_Lasso.SetPullingAnimal(false);
    }

    public override void Tick()
    {
        m_Lasso.RenderRope();
        m_fTimeSinceClicked += Time.deltaTime;
        Vector3 cowToPlayer = (m_Lasso.GetLassoGrabPoint.position - m_Lasso.GetEndTransform.position).normalized;
        m_TotalForce = Mathf.Max(0.0f, m_TotalForce - m_ForceDecreasePerSec * Time.deltaTime);
        m_fCurrentMiniDecayTime = Mathf.Max(0.0f, m_fCurrentMiniDecayTime - Time.deltaTime);

        float miniDecayScale = m_fCurrentMiniDecayTime / m_fMiniDecayTimeMax;
        if (Input.GetMouseButtonDown(0) && m_fTimeSinceClicked > 0.4f) 
        {
            m_AnimalAttacher.GetAttachedAnimal.OnPulledByLasso();
            m_fCurrentMiniDecayTime = m_fMiniDecayTimeMax;
            m_TotalForce = Mathf.Min(m_TotalForce + m_ForceIncreasePerClick, m_MaxForce);
            m_fTimeSinceClicked = 0.0f;

        }
        m_AnimalAttacher.GetAttachedAnimal.GetCowRigidBody.velocity += cowToPlayer * m_TotalForce * miniDecayScale * Time.deltaTime;
        m_Lasso.SetPullEffectsLevel(0.5f * m_TotalForce / m_MaxForce + 0.5f * miniDecayScale);

    }
}

public class LassoReturnState : IState 
{
    private readonly LassoStartComponent m_Lasso;

    float m_LassoSpeed = 0.0f;

    float m_MaxLassoSpeed = 10.0f;

    float m_Acceleration = 0.7f;
    public LassoReturnState(LassoStartComponent lasso) 
    {
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
        m_Lasso.SetLassoAsChildOfPlayer(true);

    }

    public override void Tick()
    {
       // m_Lasso.RenderLoop(0, Vector3.zero);
        m_Lasso.RenderRope();
        m_Lasso.RenderThrownLoop();
        m_LassoSpeed = (Mathf.Min(m_LassoSpeed + Time.deltaTime * m_Acceleration, m_MaxLassoSpeed));
        Vector3 loopToPlayer = (m_Lasso.GetLassoGrabPoint.position - m_Lasso.GetEndTransform.position).normalized;
        m_Lasso.GetEndTransform.rotation = Quaternion.LookRotation(-loopToPlayer, Vector3.up);
        m_Lasso.GetEndTransform.position += m_LassoSpeed * loopToPlayer;
    }
}

public class LassoIdleState : IState
{
    private readonly LassoStartComponent m_Lasso;
    public LassoIdleState(LassoStartComponent lasso)
    {
        m_Lasso = lasso;
    }
    public override void OnEnter()
    {
        m_Lasso.SetLassoAsChildOfPlayer(true);

    }

}
