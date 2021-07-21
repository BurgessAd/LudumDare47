using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "GameManager")]
public class CowGameManager : ScriptableObject
{
	[SerializeField] private EntityInformation m_PlayerEntityInformation;
	[SerializeField] private LayerMask m_TerrainLayerMask;
	[SerializeField] private RestartState m_RestartState;
	public event Action OnPaused;
	public event Action OnUnpaused;

	private readonly Dictionary<EntityInformation, List<EntityToken>> m_EntityCache = new Dictionary<EntityInformation, List<EntityToken>>();
	private readonly List<LevelObjective> m_ObjectiveDict = new List<LevelObjective>();
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
		SceneManager.LoadScene(0);
	}

	public int GetCurrentLevelIndex() 
	{
		return m_CurrentLevelIndex;
	}

	public Transform GetCameraTransform { get; private set; }
	public Transform GetPlayerCameraContainerTransform { get; private set; }
	private bool m_bIsPaused = false;

	public void OnUIElementSpawned(UIObjectElement element, UIObjectReference reference) 
	{
		m_UICache.Add(reference, element.gameObject);
	}

	public void AddToPauseUnpause(IPauseListener pausable) 
	{
		OnPaused += pausable.Pause;
		OnUnpaused += pausable.Unpause;
		if (m_bIsPaused) pausable.Pause();
		else pausable.Unpause();
	}

	public GameObject GetUIElementFromReference(in UIObjectReference reference) 
	{
		return m_UICache[reference];
	}

	private int m_NumObjectivesToComplete = 0;
	private int m_NumObjectivesCompleted = 0;



	public void OnObjectiveCompleted()
	{
		m_NumObjectivesCompleted++;
		if (m_NumObjectivesCompleted == m_NumObjectivesToComplete)
		{
			
		}
	}

	public void OnObjectiveUncompleted()
	{
		m_NumObjectivesCompleted--;
	}

	public void OnObjectiveFailure()
	{
		OnDefeated();
	}

	public RestartState GetRestartState() 
	{ 
		return m_RestartState; 
	}

	public float GetMapRadius => GetCurrentLevel.GetMapRadius;
	public EntityTypeComponent GetPlayer => m_EntityCache[m_PlayerEntityInformation][0].GetEntityType;

	public LevelManager GetCurrentLevel { get; private set; } = null;

	public void RegisterQuickRestart()
	{
		m_RestartState = RestartState.Quick;
	}

	public void ConstrainPointToPlayArea(ref Vector3 point)
	{
		float yPoint = point.y;
		Vector3 projectedDistance = Vector3.ProjectOnPlane(point - GetCurrentLevel.GetMapCentre, Vector3.up);

		float vectorLength = Mathf.Min(projectedDistance.magnitude, GetCurrentLevel.GetMapRadius);
		point = vectorLength * projectedDistance.normalized + GetCurrentLevel.GetMapCentre;
		point.y = yPoint;
	}

	public EntityToken GetTokenForEntity(in EntityTypeComponent gameObject, in EntityInformation entityType) 
	{
		if (m_EntityCache.TryGetValue(entityType, out List<EntityToken> value)) 
		{
			for(int i = 0; i < value.Count; i++) 
			{
				if (value[i].GetEntityType == gameObject) 
				{
					return value[i];
				}
			}
		}
		return null;
	}

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
								float sqDist = Vector3.SqrMagnitude(token.GetEntityType.GetTrackingTransform.position - currentPos);
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
						float sqDist = Vector3.SqrMagnitude(token.GetEntityType.GetTrackingTransform.position - currentPos);
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

	public void SetPausedState(bool pauseState)
	{
		m_bIsPaused = pauseState;
		if (pauseState)
		{
			Cursor.lockState = CursorLockMode.None;
			OnPaused();
		}
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			OnUnpaused();
		}
	}

	public void OnEntityEnterPen(GameObject go)
	{
		EntityInformation inf = go.GetComponent<EntityTypeComponent>().GetEntityInformation;
		for (int i = 0; i < m_ObjectiveDict.Count; i++)
		{
			if (m_ObjectiveDict[i].GetEntityInformation == inf && m_ObjectiveDict[i].GetObjectiveType == ObjectiveType.Capturing)
			{
				m_ObjectiveDict[i].IncrementCounter();
			}
		}
	}

	public void OnEntityLeavePen(GameObject go)
	{
		EntityInformation inf = go.GetComponent<EntityTypeComponent>().GetEntityInformation;
		for (int i = 0; i < m_ObjectiveDict.Count; i++)
		{
			if (m_ObjectiveDict[i].GetEntityInformation == inf && m_ObjectiveDict[i].GetObjectiveType == ObjectiveType.Capturing)
			{
				m_ObjectiveDict[i].IncrementCounter();
			}
		}
	}

	public void OnEntitySpawned(EntityTypeComponent entity, EntityInformation entityType)
	{
		if (!m_EntityCache.TryGetValue(entityType, out List<EntityToken> entities))
		{
			entities = new List<EntityToken>();
			m_EntityCache.Add(entityType, entities);
		}
		for (int i = 0; i < m_ObjectiveDict.Count; i++)
		{
			if (m_ObjectiveDict[i].GetEntityInformation == entity && m_ObjectiveDict[i].GetObjectiveType == ObjectiveType.Population)
			{
				m_ObjectiveDict[i].IncrementCounter();
			}
		}
		EntityToken newToken = new EntityToken(entity);
		entities.Add(newToken);		
	}

	public void OnEntityKilled(EntityTypeComponent entity, EntityInformation entityType)
	{

		for (int i = 0; i < m_EntityCache[entityType].Count; i++)
		{
			if (m_EntityCache[entityType][i].GetEntityType == entity)
			{
				m_EntityCache[entityType].RemoveAt(i);
				break;
			}
		}

		for (int i = 0; i < m_ObjectiveDict.Count; i++)
		{
			if (m_ObjectiveDict[i].GetEntityInformation == entity && m_ObjectiveDict[i].GetObjectiveType == ObjectiveType.Population)
			{
				m_ObjectiveDict[i].DecrementCounter();
			}
		}

		List<EntityToken> cache = m_EntityCache[entityType];
		if (cache.Count == 0) 
		{ 
			m_EntityCache.Remove(entityType); 
		}
	}

	public bool IsGroundLayer(in int layer)
	{
		return UnityUtils.IsLayerInMask(m_TerrainLayerMask, layer);
	}

	public void OnPlayerKilled()
	{
		GetCurrentLevel.RestartLevel();
		RegisterQuickRestart();
	}

	public void RegisterCamera(Transform camTransform) 
	{
		GetCameraTransform = camTransform;
	}

	public void RegisterInitialCameraContainerTransform(Transform containerTransform) 
	{
		GetPlayerCameraContainerTransform = containerTransform;
	}

	private void OnDefeated() 
	{
		GetCurrentLevel.OnLevelFailed();
		RegisterQuickRestart();	
	}


	public void NewLevelLoaded(LevelManager newLevel) 
	{
		m_CurrentLevelIndex = SceneManager.GetActiveScene().buildIndex;
		GetCurrentLevel = newLevel;
		m_NumObjectivesToComplete = m_LevelData[m_CurrentLevelIndex].GetObjectiveCount;
		m_LevelData[m_CurrentLevelIndex].ForEachObjective((LevelObjective objective) =>
		{
			m_ObjectiveDict.Add(objective);
		});
		newLevel.SetLevelData(m_LevelData[m_CurrentLevelIndex]);
	}

	public void ClearLevelData()
	{
		m_NumObjectivesToComplete = 0;
		m_NumObjectivesCompleted = 0;
		m_EntityCache.Clear();
		m_ObjectiveDict.Clear();
		m_UICache.Clear();
		OnPaused = null;
		OnUnpaused = null;
	}
}

public interface IPauseListener 
{
	void Pause();
	void Unpause();
}

public interface ILevelListener 
{
	void LevelStarted();
	void LevelFinished();
}

public enum EntityAbductionState 
{
	Free,
	Hunted,
	Abducted
}

public class EntityToken 
{
	public EntityToken(in EntityTypeComponent go) 
	{
		GetEntityTransform = go.transform;
		GetEntityType = go;
		GetEntityState = EntityAbductionState.Free;
	}
	public void SetAbductionState(in EntityAbductionState reservationType) 
	{
		GetEntityState = reservationType;
	}

	public EntityAbductionState GetEntityState { get; private set; }

	public Transform GetEntityTransform { get; private set; }

	public EntityTypeComponent GetEntityType { get; private set; }
}
