using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(EntityTypeComponent))]
public class HealthComponent : MonoBehaviour
{
    public event Action<GameObject, GameObject, DamageType> OnEntityDied;

    public event Action<GameObject, GameObject, DamageType, float> OnTakenDamageInstance;

    [SerializeField]
    private float m_MaxHealth = 3;

    [SerializeField]
    private CowGameManager m_Manager;

    private Transform m_Transform;

    [SerializeField]
    private float m_InvulnerabilityTime = 1.0f;

    [SerializeField]
    private bool m_bCanDie = true;

    private bool m_IsInvulnerable = false;

    [SerializeField]
    private float m_CurrentHealth = 0;

    private bool m_bIsKilled = false;



    private void Awake()
    {
        m_Transform = GetComponent<Transform>();
        m_Manager.OnEntitySpawned(gameObject, GetComponent<EntityTypeComponent>().GetEntityInformation);
        m_CurrentHealth = m_MaxHealth;
    }

    public void Revive(in float health) 
    {
        m_CurrentHealth = health;
        m_Manager.OnEntitySpawned(gameObject, GetComponent<EntityTypeComponent>().GetEntityInformation);
        m_bIsKilled = false;
    }

    public void ReplenishHealth(in float healthAmount) 
    {
        if (GetCurrentHealthPercentage == 0) 
        {
            Revive(healthAmount);
        }
		else
        {
            m_CurrentHealth = Mathf.Min(m_CurrentHealth + healthAmount, m_MaxHealth);
        }
    }

    public void Revive() 
    {
        m_CurrentHealth = m_MaxHealth;
        m_Manager.OnEntitySpawned(gameObject, GetComponent<EntityTypeComponent>().GetEntityInformation);
        m_bIsKilled = true;
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
            m_CurrentHealth = 0;
            m_bIsKilled = true;
            m_Manager.OnEntityKilled(gameObject, GetComponent<EntityTypeComponent>().GetEntityInformation);
            OnEntityDied?.Invoke(gameObject, damagedBy, damageType);
        }
    }

    public float GetCurrentHealthPercentage => m_CurrentHealth / m_MaxHealth;

    public bool TakeDamageInstance(GameObject damagedBy, DamageType damageType, uint damageAmount = 1u) 
    {
        if (!m_IsInvulnerable)
        {
            m_CurrentHealth -= damageAmount;
            if (m_CurrentHealth == 0)
            {
                OnKilled(gameObject, damagedBy, damageType);
            }
            else
            {
                OnTakenDamageInstance?.Invoke(gameObject, damagedBy, damageType, GetCurrentHealthPercentage);
                StartCoroutine(SetInvulnerability());
            }
            return true;
        }
        return false;
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