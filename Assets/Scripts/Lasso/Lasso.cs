using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimalAttacher
{
    public AnimalStateHandler GetAttachedAnimal { get; }
}

public interface ILasso 
{
    public void ActivateLassoCollider();
    public void DeactivateLassoCollider();

    public Rigidbody GetLassoBody { get; }

    public void RenderRope();

    public void RenderLoop();

    public void RenderThrownLoop();

    public void SetRopeLineRenderer(bool enabled);

    public void SetLoopLineRenderer(bool enabled);

    public void SetThrownLoopLineRenderer(bool enabled);

    public void SetPullEffectsLevel(float level);

    public Transform GetStartTransform { get; }

    public Transform GetEndTransform { get; }

    public Transform GetSwingingTransform { get; }

    public void ProjectObject(Rigidbody body, float projectionStrength);
}

public class Lasso : MonoBehaviour, IAnimalAttacher, ILasso
{
    private StateMachine m_StateMachine;
    private AnimalStateHandler m_AttachedAnimal;

    private GameObject m_LassoCollider;

    [SerializeField]
    private Transform m_LassoStartTransform;
    [SerializeField]
    private Transform m_LassoEndTransform;
    [SerializeField]
    private Transform m_SwingPointTransform;
    [SerializeField]
    private Rigidbody m_LassoEndRigidBody;



    private Transform m_EndTransform;
    

    public AnimalStateHandler GetAttachedAnimal => m_AttachedAnimal;

    public Rigidbody GetLassoBody => m_LassoEndRigidBody;

    public Transform GetStartTransform => m_LassoStartTransform;

    public Transform GetEndTransform => m_EndTransform;

    public Transform GetSwingingTransform => m_SwingPointTransform;

    private void Awake()
    {
        m_StateMachine = new StateMachine(new LassoIdleState());
        m_StateMachine.AddState(new LassoReturnState(this));
        m_StateMachine.AddState(new LassoThrowingState(this));
        m_StateMachine.AddState(new LassoSpinningState(this));
        m_StateMachine.AddState(new LassoAnimalAttachedState(this, this));
        m_StateMachine.AddState(new LassoAnimalSpinningState(this, this));
        m_StateMachine.AddState(new LassoAnimalThrowingState(this, this));

        // for if we want to start spinning
        m_StateMachine.AddTransition(typeof(LassoIdleState), typeof(LassoSpinningState), () => Input.GetMouseButtonDown(0) && !m_AttachedAnimal);
        
        // for if we're spinning and want to cancel 
        m_StateMachine.AddTransition(typeof(LassoSpinningState), typeof(LassoIdleState), () => Input.GetMouseButtonUp(1));
        // for if we're spinning and want to throw
        m_StateMachine.AddTransition(typeof(LassoSpinningState), typeof(LassoThrowingState), () => Input.GetMouseButtonUp(0));
        // for if we're spinning an animal and want to cancel
        m_StateMachine.AddTransition(typeof(LassoAnimalSpinningState), typeof(LassoIdleState), () => { if (Input.GetMouseButtonUp(1)) { UnattachLeash(); return true; } return false; });
        // for if we're throwing and want to cancel
        m_StateMachine.AddTransition(typeof(LassoThrowingState), typeof(LassoReturnState), () => (Input.GetMouseButtonUp(1)) );
        // for if we've decided we want to unattach to our target
        m_StateMachine.AddTransition(typeof(LassoAnimalAttachedState), typeof(LassoReturnState), () => {if (Input.GetMouseButtonUp(1)){UnattachLeash(); return true; } return false; });
        // for if the cow has reached us
        m_StateMachine.AddTransition(typeof(LassoAnimalAttachedState), typeof(LassoAnimalSpinningState), () => Vector3.Distance(GetAttachedAnimal.GetLeashTransform.position, m_LassoStartTransform.position) < 3.0f);
        // for if we want to throw the animal
        m_StateMachine.AddTransition(typeof(LassoAnimalSpinningState), typeof(LassoAnimalThrowingState), () => Input.GetMouseButtonUp(0));
        // instant transition back to idle state
        m_StateMachine.AddTransition(typeof(LassoAnimalThrowingState), typeof(LassoIdleState), () => true);
    }

    public void ActivateLassoCollider()
    {
        m_LassoEndRigidBody.coll
    }

    public void DeactivateLassoCollider()
    {

    }

    public void RenderRope()
    {

    }

    public void RenderLoop()
    {

    }

    public void RenderThrownLoop()
    {

    }

    public void SetRopeLineRenderer(bool enabled)
    {

    }

    public void SetLoopLineRenderer(bool enabled)
    {

    }

    public void SetThrownLoopLineRenderer(bool enabled)
    {

    }

    public void SetPullEffectsLevel(float level)
    {

    }

    public void ProjectObject(Rigidbody body, float forceStrength)
    {
        body.MovePosition(m_LassoStartTransform.position);
        body.AddForce(m_LassoStartTransform.forward * forceStrength, ForceMode.VelocityChange);
    }

    private void OnHitCow(AnimalStateHandler animalStateHandler)
    {
        m_StateMachine.RequestTransition(typeof(LassoAnimalAttachedState));
        m_AttachedAnimal = animalStateHandler;
        AttachLeash();
    }

    private void OnHitGround()
    {
        m_StateMachine.RequestTransition(typeof(LassoReturnState));
    }

    private void AttachLeash()
    {
        m_EndTransform.SetParent(GetAttachedAnimal.transform);
        m_EndTransform = GetAttachedAnimal.GetCowRigidBody.transform;
        m_EndTransform.localPosition = Vector3.zero;
    }

    private void UnattachLeash()
    {
        GetAttachedAnimal.GetCowRigidBody.transform.SetParent(null);
        m_EndTransform = m_LassoEndTransform;
    }
}

public class LassoSpinningState : IState
{
    private ILasso m_Lasso;

    float m_StartAngle;
    float m_RotationSpeed = 360 * Mathf.Deg2Rad;
    public LassoSpinningState(ILasso lasso)
    {
        m_Lasso = lasso;
    }

    public override void OnEnter()
    {
        m_StartAngle = 0.0f;
        m_Lasso.SetLoopLineRenderer(true);
        m_Lasso.SetRopeLineRenderer(true);
    }

    public override void OnExit()
    {
        m_Lasso.SetLoopLineRenderer(false);
        m_Lasso.SetRopeLineRenderer(false);
    }

    public override void Tick()
    {
        float r = 3.0f;
        m_Lasso.GetEndTransform.position = m_Lasso.GetSwingingTransform.position + new Vector3(r * Mathf.Cos(m_StartAngle), 0, r * Mathf.Sin(m_StartAngle));
        m_Lasso.RenderRope();
        m_Lasso.RenderLoop();
        m_StartAngle += m_RotationSpeed * Time.deltaTime;
    }
}

public class LassoAnimalSpinningState : IState 
{
    private readonly IAnimalAttacher m_AnimalAttacher;
    private readonly ILasso m_Lasso;

    float m_StartAngle;
    private float m_RotationSpeed = 360 * Mathf.Deg2Rad;

    public LassoAnimalSpinningState(ILasso lasso, IAnimalAttacher animalAttacher)
    {
        m_Lasso = lasso;
        m_AnimalAttacher = animalAttacher;
    }

    public override void OnEnter()
    {
        m_StartAngle = 0.0f;
        m_Lasso.SetRopeLineRenderer(true);
        m_AnimalAttacher.GetAttachedAnimal.OnStartedLassoSpinning();
    }

    public override void OnExit()
    {
        m_Lasso.SetRopeLineRenderer(false);
        m_AnimalAttacher.GetAttachedAnimal.OnReleasedByLasso();
    }

    public override void Tick()
    {
        float r = 3.0f;
        m_Lasso.GetEndTransform.position = m_Lasso.GetSwingingTransform.position + new Vector3(r * Mathf.Cos(m_StartAngle), 0, r * Mathf.Sin(m_StartAngle));
        m_Lasso.RenderRope();
        m_Lasso.RenderLoop();
        m_StartAngle += m_RotationSpeed * Time.deltaTime;
    }
}

public class LassoThrowingState : IState 
{
    private readonly ILasso m_Lasso;
    public LassoThrowingState(ILasso lasso) 
    {
        m_Lasso = lasso;
    }

    public override void OnEnter()
    {
        m_Lasso.ActivateLassoCollider();
        m_Lasso.SetRopeLineRenderer(true);
        m_Lasso.SetThrownLoopLineRenderer(true);
        m_Lasso.ProjectObject(m_Lasso.GetLassoBody, 10.0f);
    }

    public override void OnExit()
    {
        m_Lasso.SetRopeLineRenderer(false);
        m_Lasso.SetThrownLoopLineRenderer(false);
        m_Lasso.DeactivateLassoCollider();
    }

    public override void Tick()
    {
        m_Lasso.RenderThrownLoop();
    }
}

public class LassoAnimalThrowingState : IState 
{
    private readonly IAnimalAttacher m_AnimalAttacher;
    private readonly ILasso m_Lasso;

    public LassoAnimalThrowingState(IAnimalAttacher animalAttacher, ILasso lasso) 
    {
        m_AnimalAttacher = animalAttacher;
        m_Lasso = lasso;
    }

    public override void OnEnter()
    {
        m_AnimalAttacher.GetAttachedAnimal.OnThrownByLasso();
        m_Lasso.ProjectObject(m_AnimalAttacher.GetAttachedAnimal.GetCowRigidBody, 5.0f);
        RequestTransition<LassoIdleState>();
    }
}

public class LassoAnimalAttachedState : IState 
{
    private readonly IAnimalAttacher m_AnimalAttacher;
    private readonly ILasso m_Lasso;

    float m_ForceDecreasePerSec = 2.0f;
    float m_ForceIncreasePerClick = 1.0f;

    float m_TotalForce = 0.0f;
    float m_MaxForce = 10.0f;
    public LassoAnimalAttachedState(ILasso lasso, IAnimalAttacher animalAttacher)
    {
        m_Lasso = lasso;
        m_AnimalAttacher = animalAttacher;
    }
    public override void OnEnter()
    {
        m_TotalForce = 0.0f;
        m_AnimalAttacher.GetAttachedAnimal.OnWrangledByLasso();
        m_Lasso.SetRopeLineRenderer(true);
    }

    public override void OnExit()
    {
        m_AnimalAttacher.GetAttachedAnimal.OnReleasedByLasso();
        m_Lasso.SetRopeLineRenderer(false);
    }

    public override void Tick()
    {
        m_Lasso.RenderRope();
        Vector3 cowToPlayer = (m_Lasso.GetStartTransform.position - m_Lasso.GetEndTransform.position).normalized;
        m_TotalForce = Mathf.Max(0.0f, m_ForceDecreasePerSec * Time.deltaTime);
        if (Input.mouseScrollDelta.y < 0.0f) 
        {
            m_TotalForce = Mathf.Min(m_TotalForce + m_ForceIncreasePerClick, m_MaxForce);
            m_AnimalAttacher.GetAttachedAnimal.GetCowRigidBody.AddForce(cowToPlayer * m_TotalForce, ForceMode.Impulse);
            m_Lasso.SetPullEffectsLevel(m_TotalForce / m_MaxForce);
        }
        
    }
}

public class LassoReturnState : IState 
{
    private readonly ILasso m_Lasso;

    float m_LassoSpeed = 0.0f;

    float m_MaxLassoSpeed = 10.0f;

    float m_Acceleration = 10.0f;
    public LassoReturnState(ILasso lasso) 
    {
        m_Lasso = lasso;
    }

    public override void OnEnter()
    {
        m_LassoSpeed = 0.0f;
        m_Lasso.SetRopeLineRenderer(true);
        m_Lasso.SetThrownLoopLineRenderer(true);
    }

    public override void OnExit()
    {
        m_Lasso.SetRopeLineRenderer(false);
        m_Lasso.SetThrownLoopLineRenderer(false);

    }

    public override void Tick()
    {
        m_Lasso.RenderLoop();
        m_Lasso.RenderRope();
        m_LassoSpeed = (Mathf.Min(m_LassoSpeed + Time.deltaTime * m_Acceleration, m_MaxLassoSpeed));
        Vector3 loopToPlayer = (m_Lasso.GetStartTransform.position - m_Lasso.GetEndTransform.position).normalized;
        m_Lasso.GetEndTransform.rotation = Quaternion.LookRotation(-loopToPlayer, Vector3.up);
        m_Lasso.GetLassoBody.velocity = m_LassoSpeed * loopToPlayer;
    }
}

public class LassoIdleState : IState 
{

}
