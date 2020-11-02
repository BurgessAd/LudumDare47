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


	private readonly Dictionary<Type, List<Transform>> m_EntityCache = new Dictionary<Type, List<Transform>>();
	private readonly Dictionary<string, CounterBase> m_CounterBaseDict = new Dictionary<string, CounterBase>();

	private LevelManager m_CurrentLevel = null;
	private uint m_NumSuccesses = 0u;
	private uint m_SuccessesRequired = 0u;
	private bool m_bIsPaused = false;
	private static readonly List<Type> m_HostileTypes = new List<Type> { typeof(PlayerPauseComponent) };


	public bool GetClosestHostileTransform(in Vector3 currentPos, out Transform outTransform)
	{
		Transform cachedTransform = null;
		float cachedDist = Mathf.Infinity;
		foreach (Type type in m_HostileTypes)
		{
			foreach (Transform transform in m_EntityCache[type])
			{
				float dist = Vector3.Distance(transform.position, currentPos);
				if (dist < cachedDist)
				{
					cachedDist = dist;
					cachedTransform = transform;
				}
			}
		}
		outTransform = cachedTransform;
		return cachedTransform != null;
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
				objs[i].GetComponent<PauseComponent>().Pause();
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
				objs[i].GetComponent<PauseComponent>().Unpause();
			}
		}
	}

	public void OnEntitySpawned(GameObject entity, Type entityType)
	{
		if (!m_EntityCache.TryGetValue(entityType, out List<Transform> entities))
		{
			entities = new List<Transform>();
			m_EntityCache.Add(entityType, entities);
		}
		entities.Add(entity.transform);
	}

	public void OnEntityDestroyed(GameObject entity, Type entityType)
	{
		m_EntityCache[entityType].Remove(entity.transform);
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
	}


	private void OnSuccess() 
	{
		m_NumSuccesses++;
		if (m_NumSuccesses == m_SuccessesRequired) 
		{
			m_CurrentLevel.OnLevelSucceeded();
		}
	}

	private void OnRemoveSuccess() 
	{
		m_NumSuccesses--;
	}

	public void NewLevelLoaded(LevelManager newLevel, int levelIndex) 
	{
		m_bIsPaused = false;
		m_CurrentLevel = newLevel;
	}

	public void ClearLevelData() 
	{
		m_EntityCache.Clear();
		m_CounterBaseDict.Clear();
		m_NumSuccesses = 0u;
		m_SuccessesRequired = 0u;
	}

	public void OnPlayerKilled() 
	{
		m_CurrentLevel.PauseLevel(true);
		m_CurrentLevel.RestartLevel();
	}
}

