using UnityEngine;
using System;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "GameManager")]
public class CowGameManager : ScriptableObject
{
	public struct EntityData
	{
		public readonly Transform EntityTransform;
		public readonly GameObject EntityObject;
		public EntityData(Transform transform, GameObject gameObject)
		{
			EntityTransform = transform;
			EntityObject = gameObject;
		}
	}

	public void RegisterQuickRestart() 
	{
		m_bQuickRestart = true;
	}
	[SerializeField]
	private bool m_bQuickRestart;

	
	public bool ShouldQuickRestart => m_bQuickRestart;
	private Transform m_PlayerTransform;
	
	private Transform m_CameraTransform;
	private readonly Dictionary<EntityType, List<EntityToken>> m_EntityCache = new Dictionary<EntityType, List<EntityToken>>();
	private readonly Dictionary<string, CounterBase> m_CounterBaseDict = new Dictionary<string, CounterBase>();

	private LevelManager m_CurrentLevel = null;
	private uint m_NumSuccesses = 0u;
	private uint m_SuccessesRequired = 0u;
	private bool m_bIsPaused = false;
	private static readonly Dictionary<EntityType, List<EntityType>> m_HostileTypes = new Dictionary<EntityType, List<EntityType>>
	{
		{ EntityType.Predator, new List<EntityType> {EntityType.Player, EntityType.Alien }},
		{ EntityType.Prey, new List<EntityType>{ EntityType.Player, EntityType.Alien, EntityType.Predator }} 
	};

	public void ConstrainPointToPlayArea(ref Vector3 point) 
	{
		float yPoint = point.y;
		Vector3 projectedDistance = Vector3.ProjectOnPlane(point - m_CurrentLevel.GetMapCentre, Vector3.up);

		float vectorLength = Mathf.Min(projectedDistance.magnitude, m_CurrentLevel.GetMapRadius);
		point = vectorLength * projectedDistance.normalized + m_CurrentLevel.GetMapCentre;
		point.y = yPoint;
	}

	public float GetMapRadius() 
	{
		return m_CurrentLevel.GetMapRadius;
	}
	
	public bool GetClosestHostileTransform(in EntityType entityType, in Vector3 currentPos, out Transform outTransform)
	{
		outTransform = null;
		float cachedDist = Mathf.Infinity;
		foreach (EntityType hostileType in m_HostileTypes[entityType])
		{
			if (m_EntityCache.ContainsKey(hostileType))
			foreach (EntityToken token in m_EntityCache[hostileType])
			{
				float dist = Vector3.Distance(token.Entity.transform.position, currentPos);
				if (dist < cachedDist)
				{
					cachedDist = dist;
					outTransform = token.Entity.transform;
				}
			}
		}
		return outTransform != null;
	}

	private static readonly List<EntityType> AbductableTypes = new List<EntityType> { EntityType.Prey, EntityType.Predator };

	public Transform GetCowToAbduct() 
	{
		int numAbductables = 0;
		for (int i = 0; i < AbductableTypes.Count; i++) 
		{
			numAbductables += m_EntityCache[AbductableTypes[i]].Count;
		}
		int entityChoice = UnityEngine.Random.Range(0, numAbductables);
		for (int i = 0; i < AbductableTypes.Count; i++) 
		{
			if (entityChoice - m_EntityCache[AbductableTypes[i]].Count < 0) 
			{
				if (m_EntityCache[AbductableTypes[i]][entityChoice].EntityState != EntityState.Occupied) 
				{
					m_EntityCache[AbductableTypes[i]][entityChoice].SetEntityState(EntityState.Occupied);
					return m_EntityCache[AbductableTypes[i]][entityChoice].Entity.transform;
				}				
			}
			entityChoice -= m_EntityCache[AbductableTypes[i]].Count;
		}
		return null;
	}

	public void ToggleLevelPausing()
	{
		if (m_bIsPaused)
		{
			Cursor.lockState = CursorLockMode.Locked;
			UnpauseGame();
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
			PauseGame();
		}
		m_bIsPaused = !m_bIsPaused;

	}

	private void PauseGame()
	{
		foreach (var objs in m_EntityCache.Values)
		{
			for (int i = 0; i < objs.Count; i++)
			{
				objs[i].Entity.GetComponent<PauseComponent>().Pause();
			}
		}
	}

	public void SetPauseState(bool pauseState)
	{
		if (m_bIsPaused != pauseState)
		{
			ToggleLevelPausing();
		}
	}

	private void UnpauseGame()
	{
		foreach (var objs in m_EntityCache.Values)
		{
			for (int i = 0; i < objs.Count; i++)
			{
				objs[i].Entity.GetComponent<PauseComponent>().Unpause();
			}
		}
	}

	public void OnEntitySpawned(GameObject entity, EntityType entityType)
	{
		if (!m_EntityCache.TryGetValue(entityType, out List<EntityToken> entities))
		{
			entities = new List<EntityToken>();
			m_EntityCache.Add(entityType, entities);
		}
		entities.Add(new EntityToken(entity));
	}

	public void RegisterPlayer(Transform thisTransform) 
	{
		m_PlayerTransform = thisTransform;
	}

	public void RegisterCamera(Transform camTransform) 
	{
		m_CameraTransform = camTransform;
	}

	public void OnEntityDestroyed(GameObject entity, EntityType entityType)
	{
	
		for (int i = 0; i < m_EntityCache[entityType].Count; i++) 
		{
			if (m_EntityCache[entityType][i].Entity == entity) 
			{
				m_EntityCache[entityType].RemoveAt(i);
				break;
			}
		}

		if (entity.TryGetComponent(out GameTagComponent tagComponent))
		{
			if (m_CounterBaseDict.TryGetValue(tagComponent.GetObjectTag + " destroyed", out CounterBase baseValue))
			{
				baseValue.IncrementCounter();
			}
		}
		if (m_EntityCache[entityType].Count == 0) { m_EntityCache.Remove(entityType); }
	}


	public void OnCowEnterGoal(GameObject entity)
	{
		if (entity.TryGetComponent(out GameTagComponent tagComponent))
		{
			if (m_CounterBaseDict.TryGetValue(tagComponent.GetObjectTag + " goal", out CounterBase baseValue))
			{
				baseValue.IncrementCounter();
			}
		}
	}

	public void OnCowLeaveGoal(GameObject entity)
	{
		if (entity.TryGetComponent(out GameTagComponent tagComponent))
		{
			if (m_CounterBaseDict.TryGetValue(tagComponent.GetObjectTag + " goal", out CounterBase baseValue))
			{
				baseValue.DecrementCounter();
			}
		}
	}

	public void RegisterSuccessCounter(CounterBase counter)
	{
		m_SuccessesRequired++;
		counter.OnCounterCapped += OnSuccess;
		counter.OnCounterUncapped += OnRemoveSuccess;
	}

	public void RegisterDefeatCounter(CounterBase counter)
	{
		counter.OnCounterCapped += OnDefeated;
	}

	public void RegisterCounter(CounterBase counter) 
	{
		m_CounterBaseDict.Add(counter.GetBindingString, counter);
	}

	private void OnDefeated() 
	{
		m_CurrentLevel.OnLevelFailed();
		RegisterQuickRestart();
		m_CameraTransform.GetComponent<CameraStartEndAnimator>().AnimateOut();
	}

	public void OnPlayerStartedLevel() 
	{
		m_CameraTransform.GetComponent<CameraStartEndAnimator>().AnimateIn();
	}


	private void OnSuccess() 
	{
		m_NumSuccesses++;
		if (m_NumSuccesses == m_SuccessesRequired) 
		{
			m_CameraTransform.GetComponent<CameraStartEndAnimator>().AnimateOut();
			m_CurrentLevel.OnLevelSucceeded();
		}
	}

	private void OnRemoveSuccess() 
	{
		m_NumSuccesses--;
	}

	public void NewLevelLoaded(LevelManager newLevel) 
	{
		m_bIsPaused = false;
		SetPauseState(true);
		m_CurrentLevel = newLevel;
		m_bQuickRestart = false;
	}

	public void ClearLevelData()
	{
		m_EntityCache.Clear();
		m_CounterBaseDict.Clear();
		m_PlayerTransform = null;
		m_NumSuccesses = 0u;
		m_SuccessesRequired = 0u;
	}

	public LevelManager GetCurrentLevel => m_CurrentLevel;

	public void OnPlayerKilled() 
	{
		m_CurrentLevel.RestartLevel();
		RegisterQuickRestart();
	}
}

public enum EntityState 
{
	Free,
	Occupied
}

public enum EntityType 
{
	Player,
	Prey,
	Predator,
	Alien
}
public struct EntityToken 
{
	public GameObject Entity;
	public EntityState EntityState;

	public EntityToken(in GameObject go) 
	{
		Entity = go;
		EntityState = EntityState.Free;
	}
	public void SetEntityState(in EntityState state) 
	{
		EntityState = state;
	}

	public EntityState GetEntityState => EntityState; 
}
