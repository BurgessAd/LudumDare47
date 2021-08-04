using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "GameManager")]
public class CowGameManager : ScriptableObject, IObjectiveListener
{
	[SerializeField] private EntityInformation m_PlayerEntityInformation;
	[SerializeField] private LayerMask m_TerrainLayerMask;
	[SerializeField] private RestartState m_RestartState;
	[SerializeField] private GameObject m_ObjectiveObjectPrefab;
	[SerializeField] private GameObject m_LevelSelectUIPrefab;
	[SerializeField] private readonly List<LevelData> m_LevelData = new List<LevelData>();

	private readonly Dictionary<EntityInformation, List<EntityToken>> m_EntityCache = new Dictionary<EntityInformation, List<EntityToken>>();
	private readonly List<LevelObjective> m_ObjectiveDict = new List<LevelObjective>();
	private readonly Dictionary<UIObjectReference, GameObject> m_UICache = new Dictionary<UIObjectReference, GameObject>();

	// Enums for defining entity state, restart state, etc.
	#region EnumDefinitions
	public enum EntityState
	{
		Free,
		Hunted,
		Abducted
	}

	public enum RestartState
	{
		Default,
		Quick,
		Debug
	}
	#endregion

	// Properties for variable access outside the game manager
	#region Properties
	public Transform GetCameraTransform { get; private set; }

	public Transform GetPlayerCameraContainerTransform { get; private set; }

	public int GetCurrentLevelIndex { get; private set; } = 0;

	public RestartState GetRestartState { get => m_RestartState; }

	public float GetMapRadius => GetCurrentLevel.GetMapRadius;

	public EntityTypeComponent GetPlayer => m_EntityCache[m_PlayerEntityInformation][0].GetEntityType;

	public LevelManager GetCurrentLevel { get; private set; } = null;

	public List<LevelData> GetLevelData => m_LevelData;
	#endregion

	// Called by LevelManager mostly, for scene transitions, etc
	#region LevelTransitionFunctions
	public void MoveToNextLevel() 
	{
		SceneManager.LoadScene(GetCurrentLevelIndex++);
	}

	public void MoveToLevelWithId(in int levelIndex)
	{
		GetCurrentLevelIndex = levelIndex;
		SceneManager.LoadScene(GetCurrentLevelIndex);
	}

	public void RestartCurrentLevel() 
	{
		m_RestartState = RestartState.Quick;
		SceneManager.LoadScene(GetCurrentLevelIndex);
	}

	public void MoveToMenu() 
	{
		GetCurrentLevelIndex = 0;
		SceneManager.LoadScene(GetCurrentLevelIndex);
	}

	// called when new scene is loaded
	public void NewLevelLoaded(LevelManager newLevel)
	{
		GetCurrentLevel = newLevel;
		m_NumObjectivesToComplete = m_LevelData[GetCurrentLevelIndex].GetObjectiveCount;
		m_LevelData[GetCurrentLevelIndex].ForEachObjective((LevelObjective objective) =>
		{
			m_ObjectiveDict.Add(objective);
			GameObject go = Instantiate(m_ObjectiveObjectPrefab, newLevel.GetObjectiveCanvasTransform);
			LevelObjectiveUI objectiveUI = go.GetComponent<LevelObjectiveUI>();
			objective.AddObjectiveListener(objectiveUI);
			objective.AddObjectiveListener(this);
		});
		newLevel.SetLevelData(m_LevelData[GetCurrentLevelIndex]);
	}

	public void MenuLoaded(MenuManager menuManager)
	{
		for (int i = 0; i < m_LevelData.Count; i++)
		{
			GameObject go = Instantiate(m_LevelSelectUIPrefab, menuManager.GetLevelSelectTabsTransform);
			LevelDataUI levelUI = go.GetComponent<LevelDataUI>();
			levelUI.SetupData(m_LevelData[i]);
			levelUI.OnSelectLevel += () => menuManager.OnRequestLevel(i);
		}
	}

	// called when new scene is beginning to load
	public void ClearLevelData()
	{
		m_LevelData[GetCurrentLevelIndex].ForEachObjective((LevelObjective objective) =>
		{
			objective.ClearListeners();
		});
		m_NumObjectivesToComplete = 0;
		m_NumObjectivesCompleted = 0;
		m_EntityCache.Clear();
		m_ObjectiveDict.Clear();
		m_UICache.Clear();
		OnPaused = null;
		OnUnpaused = null;
	}
	#endregion

	// Functions relating to pause/unpause functionality
	#region PauseUnpause
	public event Action OnPaused;
	public event Action OnUnpaused;

	private bool m_bIsPaused = false;

	public void AddToPauseUnpause(IPauseListener pausable) 
	{
		OnPaused += pausable.Pause;
		OnUnpaused += pausable.Unpause;
		if (m_bIsPaused) pausable.Pause();
		else pausable.Unpause();
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
	#endregion

	// Called when level loads up for initialization purposes
	#region LevelInitializationFunctions
	public void RegisterCamera(Transform camTransform)
	{
		GetCameraTransform = camTransform;
	}

	public void RegisterInitialCameraContainerTransform(Transform containerTransform)
	{
		GetPlayerCameraContainerTransform = containerTransform;
	}
	#endregion

	// Called due to objectives completing/uncompleting, animals/players dying, etc.
	#region LevelEventFunctions

	public void OnPlayerKilled()
	{
		GetCurrentLevel.RestartLevel();
		m_RestartState = RestartState.Quick;
	}

	private int m_NumObjectivesToComplete = 0;
	private int m_NumObjectivesCompleted = 0;

	public void OnCounterChanged(in int val){}
	public void OnTimerTriggered(in Action totalTime, in int time){}
	public void OnTimerRemoved(){}
	public void InitializeData(LevelObjective objective) { }

	public void OnObjectiveFailed()
	{
		GetCurrentLevel.OnLevelFailed();
		m_RestartState = RestartState.Quick;
	}

	public void OnEnteredGoal()
	{
		m_NumObjectivesCompleted++;
		if (m_NumObjectivesCompleted == m_NumObjectivesToComplete)
		{
			GetCurrentLevel.StartSucceedCountdown();
		}
	}

	public void OnLeftGoal()
	{
		if(m_NumObjectivesCompleted == m_NumObjectivesToComplete)
		{
			GetCurrentLevel.EndSucceedCountdown();
		}
		m_NumObjectivesCompleted--;
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
	#endregion

	// Miscellaneous used helper functions
	#region MiscFunctions

	public bool IsGroundLayer(in int layer)
	{
		return UnityUtils.UnityUtils.IsLayerInMask(m_TerrainLayerMask, layer);
	}

	public EntityToken GetTokenForEntity(in EntityTypeComponent gameObject, in EntityInformation entityType)
	{
		if (m_EntityCache.TryGetValue(entityType, out List<EntityToken> value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				if (value[i].GetEntityType == gameObject)
				{
					return value[i];
				}
			}
		}
		return null;
	}

	public bool GetClosestTransformsMatchingList(in Vector3 currentPos, in List<EntityInformation> entities, out List<EntityToken> outEntityToken, in List<EntityState> validEntities = null)
	{
		outEntityToken = new List<EntityToken>();
		return false;
	}

	public bool GetClosestTransformMatchingList(in Vector3 currentPos, in List<EntityInformation> entities, out EntityToken outEntityToken, List<EntityState> validEntities)
	{
		if (validEntities == null)
		{
			validEntities = new List<EntityState> { EntityState.Abducted, EntityState.Free, EntityState.Hunted };
		}
		outEntityToken = null;
		float cachedSqDist = Mathf.Infinity;
		foreach (EntityInformation entityInformation in entities)
		{
			if (m_EntityCache.ContainsKey(entityInformation))
			{
				// for all entities in the cache
				foreach (EntityToken token in m_EntityCache[entityInformation])
				{
					// if the entity is in a valid state for the purposes of this request
					if (validEntities != null)
					{
						foreach (EntityState data in validEntities)
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

	public void OnUIElementSpawned(UIObjectElement elem, UIObjectReference refe)
	{
		m_UICache.Add(refe, elem.gameObject);
	}

	public GameObject GetUIElementFromReference(UIObjectReference refe)
	{
		return m_UICache[refe];
	}

	public void ConstrainPointToPlayArea(ref Vector3 point)
	{
		float yPoint = point.y;
		Vector3 projectedDistance = Vector3.ProjectOnPlane(point - GetCurrentLevel.GetMapCentre, Vector3.up);

		float vectorLength = Mathf.Min(projectedDistance.magnitude, GetCurrentLevel.GetMapRadius);
		point = vectorLength * projectedDistance.normalized + GetCurrentLevel.GetMapCentre;
		point.y = yPoint;
	}

	#endregion

}

// Interfaces for use for listeners of pausing/starting/finishing levels
#region ListenersInterfaces
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
#endregion

public class EntityToken 
{
	public EntityToken(in EntityTypeComponent go) 
	{
		GetEntityTransform = go.transform;
		GetEntityType = go;
		GetEntityState = CowGameManager.EntityState.Free;
	}
	public void SetAbductionState(in CowGameManager.EntityState reservationType) 
	{
		GetEntityState = reservationType;
	}

	public CowGameManager.EntityState GetEntityState { get; private set; }

	public Transform GetEntityTransform { get; private set; }

	public EntityTypeComponent GetEntityType { get; private set; }
}
