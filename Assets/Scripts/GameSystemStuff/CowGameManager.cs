﻿using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "GameManager")]
public class CowGameManager : ScriptableObject
{
	[SerializeField] private EntityInformation m_PlayerEntityInformation;
	[SerializeField] private LayerMask m_TerrainLayerMask;
	[SerializeField] private RestartState m_RestartState;

	private readonly Dictionary<EntityInformation, List<EntityToken>> m_EntityCache = new Dictionary<EntityInformation, List<EntityToken>>();
	private readonly List<Tuple<EntityInformation, CounterType, CounterBase>> m_CounterDict = new List<Tuple<EntityInformation, CounterType, CounterBase>>();
	private readonly Dictionary<UIObjectReference, GameObject> m_UICache = new Dictionary<UIObjectReference, GameObject>();

	[SerializeField] private List<LevelData> m_LevelData = new List<LevelData>();

	private int m_CurrentLevelIndex = 0;

	public enum RestartState 
	{
		Intro,
		Quick,
		Debug
	}

	public void MoveToNextLevel() 
	{
		SceneManager.LoadScene(m_CurrentLevelIndex++);
	}

	public void RestartCurrentLevel() 
	{
		SceneManager.LoadScene(m_CurrentLevelIndex);
	}

	public void MoveToMenu() 
	{

	}

	public int GetCurrentLevelIndex() 
	{
		return m_CurrentLevelIndex;
	}

	public Transform GetCameraTransform => m_CameraTransform;
	public Transform GetPlayerCameraContainerTransform => m_PlayerCameraContainerTransform;
	private Transform m_CameraTransform;
	private Transform m_PlayerCameraContainerTransform;
	private LevelManager m_CurrentLevel = null;
	private uint m_NumSuccesses = 0u;
	private uint m_SuccessesRequired = 0u;
	private bool m_bIsPaused = false;
	public void OnUIElementSpawned(UIObjectElement element, UIObjectReference reference) 
	{
		m_UICache.Add(reference, element.gameObject);
	}

	public GameObject GetUIElementFromReference(in UIObjectReference reference) 
	{
		return m_UICache[reference];
	}

	public bool TryGetCounter(in CounterType type, in  EntityInformation entityType, out CounterBase counter)
	{
		counter = null;
		foreach (var element in m_CounterDict) 
		{
			if (element.Item1 == entityType && element.Item2 == type) 
			{
				counter = element.Item3;
				return true;
			}
		}
		return false;
	}

	private void AddCounterToDict(in CounterType type, in EntityInformation entityType, in CounterBase counter) 
	{
		m_CounterDict.Add(new Tuple<EntityInformation, CounterType, CounterBase>(entityType, type, counter));
	}

	public RestartState GetRestartState() 
	{ 
		return m_RestartState; 
	}

	public float GetMapRadius => m_CurrentLevel.GetMapRadius;
	public GameObject GetPlayer => m_EntityCache[m_PlayerEntityInformation][0].GetEntity;

	public LevelManager GetCurrentLevel => m_CurrentLevel;

	public void RegisterQuickRestart()
	{
		m_RestartState = RestartState.Quick;
	}

	public void ConstrainPointToPlayArea(ref Vector3 point)
	{
		float yPoint = point.y;
		Vector3 projectedDistance = Vector3.ProjectOnPlane(point - m_CurrentLevel.GetMapCentre, Vector3.up);

		float vectorLength = Mathf.Min(projectedDistance.magnitude, m_CurrentLevel.GetMapRadius);
		point = vectorLength * projectedDistance.normalized + m_CurrentLevel.GetMapCentre;
		point.y = yPoint;
	}

	public EntityToken GetTokenForEntity(in GameObject gameObject, in EntityInformation entityType) 
	{
		if (m_EntityCache.TryGetValue(entityType, out List<EntityToken> value)) 
		{
			for(int i = 0; i < value.Count; i++) 
			{
				if (value[i].GetEntity == gameObject) 
				{
					return value[i];
				}
			}
		}
		return null;
	}

	// UFO goes for closest unoccupied
	// Predator goes for closest unabducted

	public bool GetClosestTransformsMatchingList(in Vector3 currentPos, in List<EntityInformation> entities, out List<EntityToken> outEntityToken, in List<EntityAbductionState> validEntities = null) 
	{
		outEntityToken = new List<EntityToken>();
		return false;
	}

	public bool GetClosestTransformMatchingList(in Vector3 currentPos, in List<EntityInformation> entities, out EntityToken outEntityToken, List<EntityAbductionState> validEntities) 
	{
		if (validEntities == null) 
		{
			validEntities = new List<EntityAbductionState> { EntityAbductionState.Abducted, EntityAbductionState.Free, EntityAbductionState.Hunted };
		}
		outEntityToken = null;
		float cachedSqDist = Mathf.Infinity;
		foreach(EntityInformation entityInformation in entities) 
		{
			if (m_EntityCache.ContainsKey(entityInformation))
			{
				// for all entities in the cache
				foreach (EntityToken token in m_EntityCache[entityInformation])
				{
					// if the entity is in a valid state for the purposes of this request
					if (validEntities != null)
					{
						foreach (EntityAbductionState data in validEntities)
						{
							if (token.GetEntityState == data)
							{
								float sqDist = Vector3.SqrMagnitude(token.GetEntity.transform.position - currentPos);
								if (sqDist < cachedSqDist)
								{
									cachedSqDist = sqDist;
									outEntityToken = token;
								}
							}
						}
					}
					else 
					{
						float sqDist = Vector3.SqrMagnitude(token.GetEntity.transform.position - currentPos);
						if (sqDist < cachedSqDist)
						{
							cachedSqDist = sqDist;
							outEntityToken = token;
						}
					}
				}
			}
		}
		return outEntityToken != null;
	}

	private void PauseGame()
	{
		foreach (var objs in m_EntityCache.Values)
		{
			for (int i = 0; i < objs.Count; i++)
			{
				PauseGameForObject(objs[i]);
			}
		}
	}

	private void PauseGameForObject(in EntityToken @object) 
	{
		@object.GetEntity.GetComponent<PauseComponent>().Pause();
	}

	public void SetPausedState(bool pauseState)
	{

		if (pauseState)
		{
			Cursor.lockState = CursorLockMode.None;
			PauseGame();
		}
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			UnpauseGame();
		}
	}

	private void UnpauseGame()
	{
		foreach (var objs in m_EntityCache.Values)
		{
			for (int i = 0; i < objs.Count; i++)
			{
				objs[i].GetEntity.GetComponent<PauseComponent>().Unpause();
			}
		}
	}

	public void OnEntitySpawned(GameObject entity, EntityInformation entityType)
	{
		if (!m_EntityCache.TryGetValue(entityType, out List<EntityToken> entities))
		{
			entities = new List<EntityToken>();
			m_EntityCache.Add(entityType, entities);
		}
		EntityToken newToken = new EntityToken(entity);
		entities.Add(newToken);
		if (m_bIsPaused)
			newToken.GetEntity.GetComponent<PauseComponent>().Pause();
			
	}

	public bool IsGroundLayer(in int layer) 
	{
		return UnityUtils.IsLayerInMask(m_TerrainLayerMask, layer);
	}

	public void OnEntityKilled(GameObject entity, EntityInformation entityType)
	{

		for (int i = 0; i < m_EntityCache[entityType].Count; i++)
		{
			if (m_EntityCache[entityType][i].GetEntity == entity)
			{
				m_EntityCache[entityType].RemoveAt(i);
				break;
			}
		}

		if (entity.TryGetComponent(out EntityTypeComponent tagComponent))
		{
			if (TryGetCounter(CounterType.Destroyed, tagComponent.GetEntityInformation, out CounterBase counter))
			{
				counter.IncrementCounter();
			}
		}
		if (m_EntityCache[entityType].Count == 0) { m_EntityCache.Remove(entityType); }
	}

	public void OnCowEnterGoal(GameObject entity)
	{
		if (entity.TryGetComponent(out EntityTypeComponent tagComponent))
		{
			if (TryGetCounter(CounterType.Goal, tagComponent.GetEntityInformation, out CounterBase counter))
			{
				counter.IncrementCounter();
			}
		}
	}

	public void OnCowLeaveGoal(GameObject entity)
	{
		if (entity.TryGetComponent(out EntityTypeComponent tagComponent))
		{
			if (TryGetCounter(CounterType.Goal, tagComponent.GetEntityInformation, out CounterBase counter))
			{
				counter.DecrementCounter();
			}
		}
	}

	public void OnPlayerKilled()
	{
		m_CurrentLevel.RestartLevel();
		RegisterQuickRestart();
	}

	public void RegisterCamera(Transform camTransform) 
	{
		m_CameraTransform = camTransform;
	}

	public void RegisterInitialCameraContainerTransform(Transform containerTransform) 
	{
		m_PlayerCameraContainerTransform = containerTransform;
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

	public void RegisterCounter(CounterBase counter, CounterType counterType, EntityInformation entityInformation) 
	{
		m_CounterDict.Add(new Tuple<EntityInformation, CounterType, CounterBase>(entityInformation, counterType, counter));
	}

	private void OnDefeated() 
	{
		m_CurrentLevel.OnLevelFailed();
		RegisterQuickRestart();	
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

	public void NewLevelLoaded(LevelManager newLevel) 
	{
		m_CurrentLevelIndex = SceneManager.GetActiveScene().buildIndex;
		m_CurrentLevel = newLevel;
		newLevel.SetLevelData(m_LevelData[m_CurrentLevelIndex]);
	}

	public void ClearLevelData()
	{
		m_EntityCache.Clear();
		m_CounterDict.Clear();
		m_UICache.Clear();
		m_NumSuccesses = 0u;
		m_SuccessesRequired = 0u;
	}
}

public enum EntityAbductionState 
{
	Free,
	Hunted,
	Abducted
}

public class EntityToken 
{
	public EntityToken(in GameObject go) 
	{
		GetEntity = go;
		GetEntityTransform = go.transform;
		GetEntityState = EntityAbductionState.Free;
	}
	public void SetAbductionState(in EntityAbductionState reservationType) 
	{
		GetEntityState = reservationType;
	}

	public EntityAbductionState GetEntityState { get; private set; }

	public Transform GetEntityTransform { get; }

	public GameObject GetEntity { get; }
}
