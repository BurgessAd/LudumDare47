using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UfoMain : MonoBehaviour
{
	[Header("Object references")]
	[SerializeField] private CowGameManager m_Manager;
	[SerializeField] private TractorBeamComponent m_TractorBeamComponent;
	[SerializeField] private UfoAnimationComponent m_AnimationComponent;
	[SerializeField] private Transform m_UfoTransform;
	[SerializeField] private FlightComponent m_FlightComponent;
	[SerializeField] private EntityInformation m_EntityInformation;
	[Header("Gameplay params")]
	[SerializeField] private float m_UfoStaggerTime;
	[SerializeField] private float m_UfoRoamTimeMin;
	[SerializeField] private float m_UfoRoamTimeMax;
	[SerializeField] private float m_PatrolDistanceMax;
	[SerializeField] private float m_PatrolDistanceMin;
	[SerializeField] private float m_UfoInvulnerableTime;
	[SerializeField] private float m_PatrolHeight;
	[Header("Debug params")]
	[SerializeField] private bool m_bDebugSkipIntro;

	public float GetUfoRoamTimeMin => m_UfoRoamTimeMin;
	public float GetUfoRoamTimeMax => m_UfoRoamTimeMax;
	public Transform GetTargetCowTransform => m_TargetCow.transform;

	private StateMachine m_UfoStateMachine;
	private GameObject m_TargetCow;



	public void SetOnStoppedCallback(Action OnStopped) 
	{

		if (OnStopped == null) 
		{
			m_FlightComponent.ResetStoppedCallback();
		}
		else 
		{
			m_FlightComponent.OnAutopilotArrested += OnStopped;		
		}
	}

	public void SetShouldHoldFlight(in bool shouldHold) 
	{
		m_FlightComponent.SetHold(shouldHold);
	}

	
	public void SetOnReachedDestination(Action OnReachedDestination) 
	{
		if (OnReachedDestination == null) 
		{
			m_FlightComponent.ResetFlightCallback();
		}
		else
		{
			m_FlightComponent.OnAutopilotPositionCompleted += OnReachedDestination;
		}
	}

	public void SetTractorBeam(in bool state) 
	{
		if (state) 
		{
			m_TractorBeamComponent.OnBeginTractorBeam();
		}
		else 
		{
			m_TractorBeamComponent.OnStopTractorBeam();
		}
	}


	private void Awake()
	{
		m_UfoStateMachine = new StateMachine(new UFOEnterMapState(this));
		m_UfoStateMachine.AddState(new UFOAbductState(this, m_AnimationComponent));
		m_UfoStateMachine.AddState(new UFOPatrolState(this));
		m_UfoStateMachine.AddState(new UFOSearchState(this));
		m_UfoStateMachine.AddState(new UFOSwoopDownState(this));
		m_UfoStateMachine.AddState(new UFOStaggeredState(this, m_AnimationComponent));
		m_UfoStateMachine.AddState(new UFODeathState(m_AnimationComponent));
		m_UfoStateMachine.AddState(new UFOSwoopUpState(this));
		m_TractorBeamComponent.SetParent(this);
		m_TractorBeamComponent.OnTractorBeamFinished += () => OnCowDied(null, null, DamageType.Undefined);
		m_Manager.AddToPauseUnpause(() => enabled = false, () => enabled = true);
	}

	private void Start()
	{
		m_UfoStateMachine.InitializeStateMachine();
		m_FlightComponent.SetLinearDestination(GetStartingDestination());
	}

	private void Update()
	{
		m_UfoStateMachine.Tick();
	}

	private Vector3 GetStartingDestination() 
	{
		float mapRadius = m_Manager.GetMapRadius;
		float radiusOut = Mathf.Sqrt(UnityEngine.Random.Range(0f, 1f)) * mapRadius;
		float angle = UnityEngine.Random.Range(0f, 1f) * Mathf.Deg2Rad * 360;
		return new Vector3( Mathf.Cos(angle) * radiusOut, m_PatrolHeight, Mathf.Sin(angle));
	}

	public void SetDestination(in Vector3 destination) 
	{
		m_FlightComponent.SetLinearDestination(destination);
	}

	public void UpdateDestination(in Vector3 destination) 
	{
		m_FlightComponent.UpdateLinearDestination(destination);
	}

	public void SetTargetSpeedAtPathEnd(in float speed) 
	{
		m_FlightComponent.SetTargetSpeed(speed);
	}

	public void ResetMotion() 
	{
		m_FlightComponent.StopFlight();
	}

	public Vector3 GetNewPatrolDestination() 
	{
		float dst = UnityEngine.Random.Range(m_PatrolDistanceMin, m_PatrolDistanceMax);
		float randomDirection = UnityEngine.Random.Range(0, Mathf.Deg2Rad * 360);
		Vector2 currentPlanarPosition = new Vector2(m_UfoTransform.position.x, m_UfoTransform.position.z);
		Vector2 newPlanarPosition = new Vector3(Mathf.Sin(randomDirection) * dst + currentPlanarPosition.x, -Mathf.Cos(randomDirection) * dst + currentPlanarPosition.y);
		Vector2 limitedPlanarPosition = Mathf.Min(m_Manager.GetMapRadius, newPlanarPosition.magnitude) * newPlanarPosition.normalized;
		Vector3 newWorldPosition = new Vector3(limitedPlanarPosition.x, m_PatrolHeight, limitedPlanarPosition.y);
		return newWorldPosition;
	}

	public void SetSwoopingUp() 
	{
		SetDestination(GetNewPatrolDestination());
	}
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.GetComponent<DamagingProjectileComponent>()) 
		{
			if (m_bCanBeHit)
			{
				StartCoroutine(InvulnerabilityCoroutine());
				StartCoroutine(StaggerCoroutine());
				m_UfoStateMachine.RequestTransition(typeof(UFOStaggeredState));
			}
		}
	}
	private bool m_bCanBeHit = true;

	private IEnumerator InvulnerabilityCoroutine() 
	{
		m_bCanBeHit = false;
		yield return new WaitForSecondsRealtime(m_UfoInvulnerableTime);
		m_bCanBeHit = true;
	}

	private IEnumerator StaggerCoroutine() 
	{
		yield return new WaitForSecondsRealtime(m_UfoStaggerTime);
		m_UfoStateMachine.RequestTransition(typeof(UFOSwoopUpState));
	}

	public void OnCowDied(GameObject cow, GameObject target, DamageType killingType) 
	{
		if (m_TargetCow) 
		{
			EntityToken cowToken = m_Manager.GetTokenForEntity(m_TargetCow.GetComponent<EntityTypeComponent>(), m_TargetCow.GetComponent<EntityTypeComponent>().GetEntityInformation);
			cowToken.SetAbductionState(EntityAbductionState.Free);
		}


		m_UfoStateMachine.RequestTransition(typeof(UFOSwoopUpState));
	}

	public void OnTargetedCowStartedAbducted(UfoMain ufo, AbductableComponent cow) 
	{
		if (ufo != this) 
		{
			m_UfoStateMachine.RequestTransition(typeof(UFOSwoopUpState));
			cow.GetComponent<HealthComponent>().OnEntityDied -= OnCowDied;
			cow.OnStartedAbducting -= OnTargetedCowStartedAbducted;
		}
	}

	private static readonly List<EntityAbductionState> validEntitiesToFind = new List<EntityAbductionState>() { EntityAbductionState.Free };

	public bool FindCowToAbduct() 
	{
		if (m_Manager.GetClosestTransformMatchingList(m_UfoTransform.position, m_EntityInformation.GetHunts, out EntityToken outEntityToken, validEntitiesToFind)) 
		{
			// in case it dies before we get to it
			outEntityToken.GetEntityType.GetComponent<HealthComponent>().OnEntityDied += OnCowDied;
			outEntityToken.GetEntityType.GetComponent<AbductableComponent>().OnStartedAbducting += OnTargetedCowStartedAbducted;
			m_TargetCow = outEntityToken.GetEntityType.gameObject;
			outEntityToken.SetAbductionState(EntityAbductionState.Hunted);
			return true;
		}
		return false;
	}
}

public class UFOAbductState : AStateBase 
{
	private readonly UfoMain m_UfoMain;
	private readonly UfoAnimationComponent m_UfoAnimation;
	Vector3 hoverPos;
	public UFOAbductState(UfoMain ufoMain, UfoAnimationComponent ufoAnimation)
	{
		m_UfoMain = ufoMain;
		m_UfoAnimation = ufoAnimation;
	}

	public override void OnEnter()
	{
		Debug.Log("On enter abduct state");
		hoverPos = m_UfoMain.transform.position;
		m_UfoMain.SetDestination(hoverPos);
		m_UfoMain.SetTargetSpeedAtPathEnd(0.0f);
		m_UfoMain.SetShouldHoldFlight(true);
		m_UfoAnimation.OnAbducting();
		m_UfoMain.SetTractorBeam(true);
	}

	public override void Tick()
	{
		m_UfoMain.UpdateDestination(hoverPos);
	}

	public override void OnExit()
	{
		m_UfoAnimation.OnFlying();
		m_UfoMain.SetTractorBeam(false);
		m_UfoMain.SetShouldHoldFlight(false);
	}
}

public class UFOSearchState : AStateBase 
{
	private readonly UfoMain m_UfoMain;
	public UFOSearchState(UfoMain ufoMain)
	{
		m_UfoMain = ufoMain;
	}

	public override void OnEnter()
	{
		Debug.Log("Entered search statea");
		if (m_UfoMain.FindCowToAbduct()) 
		{
			RequestTransition<UFOSwoopDownState>();
		}
		else 
		{
			RequestTransition<UFOPatrolState>();
		}
	}

	public override void OnExit()
	{
		m_UfoMain.SetOnStoppedCallback(null);
	}
}

public class UFOSwoopUpState : AStateBase 
{
	private readonly UfoMain m_UfoMain;
	public UFOSwoopUpState(UfoMain ufoMain)
	{
		m_UfoMain = ufoMain;
	}

	public override void OnEnter()
	{
		Debug.Log("On enter swoop up state");
		m_UfoMain.SetTargetSpeedAtPathEnd(0.0f);
		m_UfoMain.SetSwoopingUp();
		m_UfoMain.SetOnReachedDestination(() =>
		{
			RequestTransition<UFOPatrolState>();
		});
	}

	public override void OnExit()
	{
		m_UfoMain.SetOnReachedDestination(null);
	}
}

public class UFOSwoopDownState : AStateBase 
{
	private readonly UfoMain m_UfoMain;
	public UFOSwoopDownState(UfoMain ufoMain)
	{
		m_UfoMain = ufoMain;
	}

	public override void OnEnter()
	{
		Debug.Log("On enter swoop down state");
		m_UfoMain.SetTargetSpeedAtPathEnd(1.0f);
		m_UfoMain.SetOnReachedDestination(() =>
		{ 
			RequestTransition<UFOAbductState>();
			m_UfoMain.GetTargetCowTransform.GetComponent<HealthComponent>().OnEntityDied -= m_UfoMain.OnCowDied;
		});
		m_UfoMain.SetDestination(m_UfoMain.GetTargetCowTransform.position);
	}

	public override void Tick()
	{
		m_UfoMain.UpdateDestination(m_UfoMain.GetTargetCowTransform.position + Vector3.up * 10.0f);
	}

	public override void OnExit()
	{
		m_UfoMain.SetOnReachedDestination(null);
	}
}

public class UFOEnterMapState : AStateBase 
{
	private readonly UfoMain m_UfoMain;
	public UFOEnterMapState(UfoMain ufoMain)
	{
		m_UfoMain = ufoMain;
	}

	public override void OnEnter()
	{
		Debug.Log("On enter map state");
		m_UfoMain.SetTargetSpeedAtPathEnd(0.0f);
		m_UfoMain.SetOnReachedDestination(() => RequestTransition<UFOPatrolState>());
	}

	public override void OnExit()
	{
		m_UfoMain.SetOnReachedDestination(null);
	}
}

public class UFOStaggeredState : AStateBase 
{
	private readonly UfoMain m_UfoMain;
	private readonly UfoAnimationComponent m_UfoAnimation;
	public UFOStaggeredState(UfoMain ufoMain, UfoAnimationComponent ufoAnimation)
	{
		m_UfoMain = ufoMain;
		m_UfoAnimation = ufoAnimation;
	}

	public override void OnEnter()
	{
		Debug.Log("On enter staggered state");
		m_UfoAnimation.OnStaggered();
		m_UfoMain.ResetMotion();
	}

	public override void OnExit()
	{
		m_UfoAnimation.OnFlying();
	}
}

public class UFODeathState : AStateBase 
{
	private readonly UfoAnimationComponent m_UfoAnimation;

	public UFODeathState(UfoAnimationComponent ufoAnimation) 
	{
		m_UfoAnimation = ufoAnimation;
	}

	public override void OnEnter()
	{
		m_UfoAnimation.OnDeath();
	}
}

public class UFOPatrolState : AStateBase 
{
	private readonly UfoMain m_UfoMain;
	private float m_fTimeToPatrolFor;
	private float m_fCurrentPatrolTime;
	private bool m_bIsPatrolling;
	private bool m_bFinishedPatrolling;
	public UFOPatrolState(UfoMain ufoMain) 
	{
		m_UfoMain = ufoMain;
	}
	public override void OnEnter()
	{
		m_waypointTimer = 2.0f;
		m_fCurrentPatrolTime = 0.0f;
		m_bIsPatrolling = false;
		m_bFinishedPatrolling = false;
		Debug.Log("On entered patrol state");
		m_fTimeToPatrolFor = UnityEngine.Random.Range(m_UfoMain.GetUfoRoamTimeMin, m_UfoMain.GetUfoRoamTimeMax);
		m_UfoMain.SetTargetSpeedAtPathEnd(0.0f);
		m_UfoMain.SetOnReachedDestination(OnReachedPatrolWaypoint);
	}

	public override void Tick()
	{
		m_fCurrentPatrolTime += Time.deltaTime;

		if (m_fCurrentPatrolTime > m_fTimeToPatrolFor) 
		{
			m_bFinishedPatrolling = true;
		}

		// need this part of the code to be called once, when we're about to start patrolling again
		if (m_waypointTimer >= 0.0f) 
		{
			m_waypointTimer -= Time.deltaTime;
		}
		// if the waypoint timer has expired
		// and we're not currently patrolling, I.E we're at a waypoint
		else if (!m_bIsPatrolling)
		{
			// then if we're not finished patrolling, choose another destination
			if (!m_bFinishedPatrolling)
			{
				
				m_bIsPatrolling = true;
				m_UfoMain.SetDestination(m_UfoMain.GetNewPatrolDestination());

			}
			// else we change states
			else
			{
				RequestTransition<UFOSearchState>();
			}
		}
	}
	private float m_waypointTimer;
	private void OnReachedPatrolWaypoint() 
	{
		m_bIsPatrolling = false;
		m_waypointTimer = 1.0f;
	}

	public override void OnExit()
	{
		m_UfoMain.SetOnReachedDestination(null);
	}
}
