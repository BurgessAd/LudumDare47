using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "GameManager")]
public class CowGameManager : ScriptableObject
{
	public int deadCows = 0;
	public int level = 1;
	public List<GameObject> cows = new List<GameObject>();

	private Dictionary<Type, List<GameObject>> m_EntityCache;

	public void OnEntitySpawned(GameObject entity, Type entityType) 
	{

	}

	public void OnEntityDestroyed(GameObject entity, Type entityType) 
	{

	}

}
