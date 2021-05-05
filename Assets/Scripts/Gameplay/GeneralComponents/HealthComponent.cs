using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(EntityTypeComponent))]
public class HealthComponent : MonoBehaviour
{
    public event Action<GameObject, GameObject, DamageType> OnEntityDied;

    [SerializeField]
    private uint m_Health = 3;

    [SerializeField]
    private CowGameManager m_Manager;

    private Transform m_Transform;

    [SerializeField]
    private float m_InvulnerabilityTime = 1.0f;

    private bool m_IsInvulnerable = false;

    private bool m_bIsKilled = false;

    public event Action<GameObject, GameObject, DamageType> OnTakenDamageInstance;

    private void Awake()
    {
        m_Transform = GetComponent<Transform>();
        m_Manager.OnEntitySpawned(gameObject, GetComponent<EntityTypeComponent>().GetEntityInformation);
    }

    private IEnumerator SetInvulnerability() 
    {
        m_IsInvulnerable = true;
        yield return new WaitForSecondsRealtime(m_InvulnerabilityTime);
        m_IsInvulnerable = false;
    }

    protected virtual void OnKilled(GameObject killed, GameObject damagedBy, DamageType damageType) 
    {
        if (!m_bIsKilled) 
        {
            m_bIsKilled = true;
            m_Manager.OnEntityKilled(gameObject, GetComponent<EntityTypeComponent>().GetEntityInformation);
            OnEntityDied?.Invoke(gameObject, damagedBy, damageType);
        }
    }
   

    public void TakeDamageInstance(GameObject damagedBy, DamageType damageType, uint damageAmount = 1u) 
    {
        if (!m_IsInvulnerable)
        {
            m_Health -= damageAmount;
            if (m_Health == 0)
            {
                OnKilled(gameObject, damagedBy, damageType);
            }
            else
            {
                OnTakenDamageInstance?.Invoke(gameObject, damagedBy, damageType);
                StartCoroutine(SetInvulnerability());
            }
        }
    }

    public void OnTakeLethalDamage(DamageType damageType) 
    {
        OnKilled(gameObject, null, damageType);
    }
}

public enum DamageType 
{
    FallDamage,
    ImpactDamage,
    Undefined,
    UFODamage,
    PredatorDamage
}