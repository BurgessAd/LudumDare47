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
    private Rigidbody m_Body;

    [SerializeField]
    private AnimationCurve m_AngularAccelerationDampening;

    public float GetMaxAngle => m_MaxRotationalAngle;
    public float GetMaxRotationSpeed => m_MaxAccelerationRotation;
    public float GetRotationalVelocity => m_RotationalVelocity;

    public float GetAccelerationDampingVal(in float angle) { return m_AngularAccelerationDampening.Evaluate(angle / 180); }
    public Rigidbody GetBody => m_Body;

    private StateMachine m_AnimationStateMachine;
    private void Awake()
    {
        m_AnimationStateMachine = new StateMachine();
        m_AnimationStateMachine.AddState(new UFOStaggeredAnimationState(this));
        m_AnimationStateMachine.AddState(new UFOFlyAnimationState(this));
        m_AnimationStateMachine.AddState(new UFOAbductAnimationState(this));
        m_AnimationStateMachine.AddState(new UFODeathAnimationState(this));
        m_AnimationStateMachine.SetInitialState(typeof(UFOFlyAnimationState));
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

public class UFOStaggeredAnimationState : IState 
{
    private readonly UfoAnimationComponent m_UfoAnimations;
    public UFOStaggeredAnimationState(in UfoAnimationComponent animationComponent)
    {
        m_UfoAnimations = animationComponent;
    }
    public override void Tick()
    {
        m_UfoAnimations.GetBody.rotation = Quaternion.RotateTowards(m_UfoAnimations.GetBody.rotation, Quaternion.identity, m_UfoAnimations.GetRotationalVelocity);
    }
}

public class UFOFlyAnimationState : IState 
{
    private readonly UfoAnimationComponent m_UfoAnimations;
    public UFOFlyAnimationState(in UfoAnimationComponent animationComponent) 
    {
        m_UfoAnimations = animationComponent;
    }
    private Vector3 velocityLastFrame;
    public override void Tick()
    {
        Vector3 velocity = m_UfoAnimations.GetBody.velocity;


        // we want rotations around x and z axes, not Y
        float xAng = Vector3.Angle(Vector3.forward, Vector3.ProjectOnPlane(velocity, Vector3.right));
        float zAng = Vector3.Angle(Vector3.right, Vector3.ProjectOnPlane(velocity, Vector3.forward));
        Quaternion targetQuat = Quaternion.Euler(xAng, 0, zAng);

        if (velocity.sqrMagnitude < 0.1f) 
        {
            targetQuat = Quaternion.identity;
        }

        float speed = velocity.magnitude;

        // tilt towards acceleration
        Vector3 acceleration = (velocity - velocityLastFrame) / Time.deltaTime;

        // only tilt if acceleration is parallel/antiparallel to velocity

        float angle = Vector3.Angle(acceleration, velocity);
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
        float angleTime = 1 - angle/90;



        //break down acceleration direction into planes defined by x and z axis
        // take acceleration direction
        float angularVelocity = m_UfoAnimations.GetRotationalVelocity * Time.deltaTime * m_UfoAnimations.GetAccelerationDampingVal(Quaternion.Angle(m_UfoAnimations.GetBody.rotation, targetQuat));

        m_UfoAnimations.GetBody.rotation = Quaternion.RotateTowards(m_UfoAnimations.GetBody.rotation, targetQuat, angularVelocity);


        velocityLastFrame = velocity;
    }
}

public class UFOAbductAnimationState : IState 
{
    private readonly UfoAnimationComponent m_UfoAnimations;
    public UFOAbductAnimationState(in UfoAnimationComponent animationComponent)
    {
        m_UfoAnimations = animationComponent;
    }
    public override void Tick()
    {
        m_UfoAnimations.GetBody.rotation = Quaternion.RotateTowards(m_UfoAnimations.GetBody.rotation, Quaternion.identity, m_UfoAnimations.GetRotationalVelocity);
    }
}

public class UFODeathAnimationState : IState 
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