using System;
using System.Collections;
using UnityEngine;

public class UfoMain : MonoBehaviour
{
	private StateMachine m_UfoStateMachine;

	[SerializeField]
	private CowGameManager m_Manager;
	[SerializeField]
	private TractorBeamComponent m_TractorBeamComponent;

	[SerializeField]
	private UfoAnimationComponent m_AnimationComponent;

	[SerializeField]
	private Transform m_UfoTransform;

	[SerializeField]
	private FlightComponent m_FlightComponent;

	[SerializeField]
	private float m_UfoStaggerTime;

	[SerializeField]
	private float m_UfoRoamTimeMin;

	[SerializeField]
	private float m_UfoRoamTimeMax;

	[SerializeField]
	private float m_PatrolDistanceMax;

	[SerializeField]
	private float m_PatrolDistanceMin;

	[SerializeField]
	private float m_UfoInvulnerableTime;

	private float m_PatrolHeight;

	public float GetUfoRoamTimeMin => m_UfoRoamTimeMin;
	public float GetUfoRoamTimeMax => m_UfoRoamTimeMax;


	private Transform m_TargetCow;

	public Transform GetTargetCowTransform => m_TargetCow;


	public void SetOnReachedDestination(Action OnReachedDestination) 
	{
		if (OnReachedDestination == null) 
		{
			m_FlightComponent.ResetFlightCallback();
		}
		else
		{
			m_FlightComponent.OnAutopilotEventCompleted += OnReachedDestination;
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
		m_UfoStateMachine = new StateMachine();
		m_UfoStateMachine.AddState(new UFOAbductState(this, m_AnimationComponent));
		m_UfoStateMachine.AddState(new UFOPatrolState(this));
		m_UfoStateMachine.AddState(new UFOSearchState(this));
		m_UfoStateMachine.AddState(new UFOSwoopDownState(this));
		m_UfoStateMachine.AddState(new UFOEnterMapState(this));
		m_UfoStateMachine.AddState(new UFOStaggeredState(this, m_AnimationComponent));
		m_UfoStateMachine.AddState(new UFODeathState(m_AnimationComponent));
		m_UfoStateMachine.SetInitialState(typeof(UFOEnterMapState));

		m_FlightComponent.SetLinearDestination(GetStartingDestination());
		m_PatrolHeight = m_UfoTransform.position.y;

		m_TractorBeamComponent.OnTractorBeamFinished += OnCowTargetLost;
	}

	private Vector3 GetStartingDestination() 
	{
		float radiusOut = Mathf.Sqrt(UnityEngine.Random.Range(0, 1)) * m_Manager.GetMapRadius();
		float angle = UnityEngine.Random.Range(0, 1) * Mathf.Deg2Rad * 360;
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
		Vector3 currentPosition = m_UfoTransform.position;
		return new Vector3(Mathf.Sin(randomDirection) * dst + currentPosition.x, m_PatrolHeight, -Mathf.Cos(randomDirection) * dst + currentPosition.z);
	}

	public void SetSwoopingUp() 
	{
		float randomDirection = UnityEngine.Random.Range(0, Mathf.Deg2Rad * 360);
		SetDestination(new Vector3(Mathf.Sin(randomDirection) * m_PatrolDistanceMax + m_UfoTransform.position.x, m_PatrolHeight, -Mathf.Cos(randomDirection) * m_PatrolDistanceMax + m_UfoTransform.position.z));
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

	public void ReachedSwoopTarget() 
	{
		m_TargetCow.GetComponent<HealthComponent>().OnEntityDied -= OnCowTargetLost;
	}



	private void OnCowTargetLost()
	{
		m_UfoStateMachine.RequestTransition(typeof(UFOSwoopUpState));
	}

	public bool FindCowToAbduct() 
	{
		Transform cowTransform = m_Manager.GetCowToAbduct();
		if (cowTransform != null) 
		{
			// in case it dies before we get to it
			cowTransform.GetComponent<HealthComponent>().OnEntityDied += OnCowTargetLost;
			m_TargetCow = cowTransform;
			return true;
		}
		return false;
	}
}

public class UFOAbductState : IState 
{
	private readonly UfoMain m_UfoMain;
	private readonly UfoAnimationComponent m_UfoAnimation;
	public UFOAbductState(UfoMain ufoMain, UfoAnimationComponent ufoAnimation)
	{
		m_UfoMain = ufoMain;
		m_UfoAnimation = ufoAnimation;
	}

	public override void OnEnter()
	{
		m_UfoAnimation.OnAbducting();
		m_UfoMain.SetTractorBeam(true);
	}

	public override void OnExit()
	{
		m_UfoAnimation.OnFlying();
		m_UfoMain.SetTractorBeam(false);
	}
}

public class UFOSearchState : IState 
{
	private readonly UfoMain m_UfoMain;
	public UFOSearchState(UfoMain ufoMain)
	{
		m_UfoMain = ufoMain;
	}

	public override void OnEnter()
	{
		if (m_UfoMain.FindCowToAbduct()) 
		{
			RequestTransition<UFOSwoopDownState>();
		}
		else 
		{
			RequestTransition<UFOPatrolState>();
		}
	}
}

public class UFOSwoopUpState : IState 
{
	private readonly UfoMain m_UfoMain;
	public UFOSwoopUpState(UfoMain ufoMain)
	{
		m_UfoMain = ufoMain;
	}

	public override void OnEnter()
	{
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

public class UFOSwoopDownState : IState 
{
	private readonly UfoMain m_UfoMain;
	public UFOSwoopDownState(UfoMain ufoMain)
	{
		m_UfoMain = ufoMain;
	}

	public override void OnEnter()
	{
		m_UfoMain.SetTargetSpeedAtPathEnd(0.5f);
		m_UfoMain.SetOnReachedDestination(() =>
		{ 
			RequestTransition<UFOAbductState>();
			m_UfoMain.ReachedSwoopTarget();
		});
		m_UfoMain.SetDestination(m_UfoMain.GetTargetCowTransform.position);
	}

	public override void Tick()
	{
		m_UfoMain.UpdateDestination(m_UfoMain.GetTargetCowTransform.position);
	}

	public override void OnExit()
	{
		m_UfoMain.SetOnReachedDestination(null);
	}
}

public class UFOEnterMapState : IState 
{
	private readonly UfoMain m_UfoMain;
	public UFOEnterMapState(UfoMain ufoMain)
	{
		m_UfoMain = ufoMain;
	}

	public override void OnEnter()
	{
		m_UfoMain.SetTargetSpeedAtPathEnd(0.0f);
		m_UfoMain.SetOnReachedDestination(() => RequestTransition<UFOPatrolState>());
	}

	public override void OnExit()
	{
		m_UfoMain.SetOnReachedDestination(null);
	}
}

public class UFOStaggeredState : IState 
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
		m_UfoAnimation.OnStaggered();
		m_UfoMain.ResetMotion();
	}

	public override void OnExit()
	{
		m_UfoAnimation.OnFlying();
	}
}

public class UFODeathState : IState 
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

public class UFOPatrolState : IState 
{
	private readonly UfoMain m_UfoMain;
	private float m_fTimeToPatrolFor;
	private float m_fCurrentPatrolTime;
	public UFOPatrolState(UfoMain ufoMain) 
	{
		m_UfoMain = ufoMain;
	}
	public override void OnEnter()
	{
		m_fTimeToPatrolFor = UnityEngine.Random.Range(m_UfoMain.GetUfoRoamTimeMin, m_UfoMain.GetUfoRoamTimeMax);
		m_UfoMain.SetTargetSpeedAtPathEnd(10.0f);
		m_UfoMain.SetOnReachedDestination(OnReachedPatrolWaypoint);
		m_UfoMain.SetDestination(m_UfoMain.GetNewPatrolDestination());
	}

	public override void Tick()
	{
		m_fCurrentPatrolTime += Time.deltaTime;
		if (m_fCurrentPatrolTime > m_fTimeToPatrolFor) 
		{
			RequestTransition<UFOSearchState>();
		}
	}

	private void OnReachedPatrolWaypoint() 
	{
		m_UfoMain.SetDestination(m_UfoMain.GetNewPatrolDestination());
	}

	public override void OnExit()
	{
		m_UfoMain.SetOnReachedDestination(null);
	}
}
