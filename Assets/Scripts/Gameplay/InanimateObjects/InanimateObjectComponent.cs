using UnityEngine;

[RequireComponent(typeof(ThrowableObjectComponent))]
[RequireComponent(typeof(FreeFallTrajectoryComponent))]
public class InanimateObjectComponent : MonoBehaviour
{
    private StateMachine m_StateMachine;

    [Header("Object References")]
    [SerializeField] private ThrowableObjectComponent m_throwableObjectComponent;
    [SerializeField] private FreeFallTrajectoryComponent m_freeFallTrajectoryComponent;
    [SerializeField] private Rigidbody m_objectRigidBody;
    [SerializeField] private GameObject m_ImpactEffectsPrefab;

	private void Awake()
	{
        m_throwableObjectComponent.OnWrangled += () => m_StateMachine.RequestTransition(typeof(IObjectPhysicalizedState));
        m_throwableObjectComponent.OnStartSpinning += () => m_StateMachine.RequestTransition(typeof(IObjectControlledState));
        m_throwableObjectComponent.OnReleased += () => m_StateMachine.RequestTransition(typeof(IObjectPhysicalizedState));
        m_throwableObjectComponent.OnThrown += (ProjectileParams) => m_StateMachine.RequestTransition(typeof(IObjectControlledState));

        m_freeFallTrajectoryComponent.OnObjectHitGround += OnHitObject;

        m_StateMachine = new StateMachine(new IObjectPhysicalizedState());

        m_StateMachine.AddState(new IObjectControlledState());

        m_StateMachine.SetCallback("disablePhysics", () => SetPhysicsState(false));
        m_StateMachine.SetCallback("enablePhysics", () => SetPhysicsState(true));
	}

    private void SetPhysicsState(bool state) 
    {
        m_objectRigidBody.isKinematic = !state;
        m_objectRigidBody.useGravity = state;
    }

	private void Start()
	{
        m_StateMachine.InitializeStateMachine();
	}

    private void OnHitObject(Collision collision) 
    {
        if (collision.gameObject.TryGetComponent(out AnimalColliderComponent colliderComponent)) 
        {
            colliderComponent.GetAnimalComponent.OnStruckByObject(m_objectRigidBody.velocity, m_objectRigidBody.mass);
        }
        else if (collision.gameObject.TryGetComponent(out AnimalComponent animal)) 
        {
            animal.OnStruckByObject(m_objectRigidBody.velocity, m_objectRigidBody.mass);
        }
		else 
        {
            if (m_ImpactEffectsPrefab != null)
                Instantiate(m_ImpactEffectsPrefab, collision.GetContact(0).point, Quaternion.LookRotation(Vector3.forward, collision.GetContact(0).normal));
       
        }
        m_StateMachine.RequestTransition(typeof(IObjectPhysicalizedState));
    }

	void Update()
    {
        m_StateMachine.Tick();
    }
}

public class IObjectControlledState : AStateBase 
{
	public override void OnEnter()
	{
        TriggerCallback("disablePhysics");
	}
}

public class IObjectPhysicalizedState : AStateBase 
{
    public override void OnEnter()
    {
        TriggerCallback("enablePhysics");
    }
}
