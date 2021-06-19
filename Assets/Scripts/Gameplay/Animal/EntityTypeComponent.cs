using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EntityTypeComponent : MonoBehaviour
{
    [SerializeField] private EntityInformation m_EntityInformation;

    [SerializeField] private CowGameManager m_Manager;

    public EntityInformation GetEntityInformation => m_EntityInformation;

    private event Action OnCancelTracking;

    private bool m_bIsKnownToGameSystem = false;

	private void Awake()
	{
        AddToTrackable();	
	}

	public void BeginTrackingObject(in Action OnUnableToTrackFurther) 
    {
        OnCancelTracking += OnUnableToTrackFurther;
    }

    public void EndTrackingObject(in Action RemoveOnUnableToTrackFurther)
    {
        OnCancelTracking -= RemoveOnUnableToTrackFurther;
    }

    public void RemoveFromTrackable() 
    {
        if (m_bIsKnownToGameSystem) 
        {
            m_Manager.OnEntityKilled(gameObject, GetEntityInformation);
            OnCancelTracking?.Invoke();
            OnCancelTracking = null;
            m_bIsKnownToGameSystem = false;
        }
    }

    public void AddToTrackable() 
    {
        if (!m_bIsKnownToGameSystem) 
        {
            m_Manager.OnEntitySpawned(gameObject, GetEntityInformation);
            m_bIsKnownToGameSystem = true;
        }
    }
}