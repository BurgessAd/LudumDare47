using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UfoAnimationComponent : MonoBehaviour
{
    [SerializeField]
    private float m_RotationalVelocity;
    [SerializeField]
    private float m_MaxRotationalAngle;
    [SerializeField]
    private float m_MaxAccelerationRotation;
    [SerializeField]
    private float m_AccelerationRequiredToTilt;
    [SerializeField]
    private Rigidbody m_Body;
    [SerializeField]
    private ParticleSystem m_SpeedParticleSystem;
    [SerializeField]
    private float m_StaggerAnimationTime;

    [SerializeField] private AnimationCurve m_AngularAccelerationDampening;
    [SerializeField] private AnimationCurve m_PitchAnimationCurve;
    [SerializeField] private AnimationCurve m_TiltAnimationCurve;

    public float GetMaxAngle => m_MaxRotationalAngle;
    public float GetMaxRotationSpeed => m_MaxAccelerationRotation;
    public float GetRotationalVelocity => m_RotationalVelocity;
    public float GetAccelerationRequiredToTilt => m_AccelerationRequiredToTilt;
    public float EvaluateTiltCurve(in float time) => m_TiltAnimationCurve.Evaluate(time);
    public float EvaluatePitchCurve(in float time) => m_PitchAnimationCurve.Evaluate(time);

    public float GetStaggerAnimationTime => m_StaggerAnimationTime;
    public float GetAccelerationDampingVal(in float angle) { return m_AngularAccelerationDampening.Evaluate(angle / m_MaxRotationalAngle); }
    public Rigidbody GetBody => m_Body;

    private StateMachine m_AnimationStateMachine;
    private void Awake()
    {
        m_AnimationStateMachine = new StateMachine(new UFOFlyAnimationState(this));
        m_AnimationStateMachine.AddState(new UFOStaggeredAnimationState(this));
        m_AnimationStateMachine.AddState(new UFOAbductAnimationState(this));
        m_AnimationStateMachine.AddState(new UFODeathAnimationState(this));
    }

    private void Update()
    {
        m_AnimationStateMachine.Tick();
    }
    public void OnStaggered() 
    {
        m_AnimationStateMachine.RequestTransition(typeof(UFOStaggeredAnimationState));
    }

    public void OnFlying() 
    {
        m_AnimationStateMachine.RequestTransition(typeof(UFOFlyAnimationState));
    }

    public void OnAbducting() 
    {
        m_AnimationStateMachine.RequestTransition(typeof(UFOAbductAnimationState));
    }

    public void OnDeath() 
    {
        m_AnimationStateMachine.RequestTransition(typeof(UFODeathAnimationState));
    }
}

public class UFOStaggeredAnimationState : AStateBase 
{
    private readonly UfoAnimationComponent m_UfoAnimations;
    private float m_CurrentAnimationTime = 0.0f;
    public UFOStaggeredAnimationState(in UfoAnimationComponent animationComponent)
    {
        m_UfoAnimations = animationComponent;
    }
    public override void OnEnter()
    {
        m_CurrentAnimationTime = 0.0f;
    }
    public override void Tick()
    {
        m_CurrentAnimationTime += Time.deltaTime;
        float scaledTime = m_CurrentAnimationTime / m_UfoAnimations.GetStaggerAnimationTime;
        float pitch = m_UfoAnimations.EvaluatePitchCurve(scaledTime);
        float roll = m_UfoAnimations.EvaluateTiltCurve(scaledTime);

        m_UfoAnimations.GetBody.rotation = Quaternion.RotateTowards(m_UfoAnimations.GetBody.rotation, Quaternion.identity, m_UfoAnimations.GetRotationalVelocity);
    }
}

public class UFOFlyAnimationState : AStateBase 
{
    private readonly UfoAnimationComponent m_UfoAnimations;
    public UFOFlyAnimationState(in UfoAnimationComponent animationComponent) 
    {
        m_UfoAnimations = animationComponent;
    }
    private Vector3 velocityLastFrame;
    private Vector3 accelerationLastFrame;
    public override void Tick()
    {
        Vector3 velocity = m_UfoAnimations.GetBody.velocity;
        Vector3 acceleration = (velocity - velocityLastFrame) / Time.deltaTime;

        if (velocity.magnitude > 2.0f) 
        {

        }    


        float velocityContinuation = Vector3.Dot(accelerationLastFrame.normalized, acceleration.normalized);

        Vector3 accelerationInPlane = Vector3.ProjectOnPlane(acceleration, Vector3.up).normalized;
        float tiltAngle = 30;
        Quaternion targetQuat = Quaternion.identity;
        if (acceleration.magnitude > m_UfoAnimations.GetAccelerationRequiredToTilt) 
        {
            targetQuat = Quaternion.AngleAxis(tiltAngle, Vector3.Cross(Vector3.up, accelerationInPlane).normalized);
        }

        // tilt towards acceleration

        // only tilt if acceleration is parallel/antiparallel to velocity

       // float angle = Vector3.Angle(acceleration, velocity);
        // now angleTime goes from 1 at parallel, 0 at perpendicular, and -1 at fully antiparallel
        //1 -  
        //      -
        //          -
        //              -
        //0 ============    -    ==============
        //                      -
        //                          -
        //                              -
        //-1                                -
        // paralell  perpendicular  antiparallel
        //float angleTime = 1 - angle/90;



        //break down acceleration direction into planes defined by x and z axis
        // take acceleration direction
        float angularVelocity = m_UfoAnimations.GetRotationalVelocity * Time.deltaTime * m_UfoAnimations.GetAccelerationDampingVal(Quaternion.Angle(m_UfoAnimations.GetBody.rotation, targetQuat));

        m_UfoAnimations.GetBody.rotation = Quaternion.RotateTowards(m_UfoAnimations.GetBody.rotation, targetQuat, angularVelocity);

        accelerationLastFrame = acceleration;

        velocityLastFrame = velocity;
    }
}

public class UFOAbductAnimationState : AStateBase 
{
    private readonly UfoAnimationComponent m_UfoAnimations;
    public UFOAbductAnimationState(in UfoAnimationComponent animationComponent)
    {
        m_UfoAnimations = animationComponent;
    }
    public override void Tick()
    {
        m_UfoAnimations.GetBody.rotation = Quaternion.RotateTowards(m_UfoAnimations.GetBody.rotation, Quaternion.identity, m_UfoAnimations.GetRotationalVelocity * Time.deltaTime);
    }
}

public class UFODeathAnimationState : AStateBase 
{
    private readonly UfoAnimationComponent m_UfoAnimations;
    public UFODeathAnimationState(in UfoAnimationComponent animationComponent)
    {
        m_UfoAnimations = animationComponent;
    }
    public override void Tick()
    {
        
    }
}