using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FoodSourceComponent : MonoBehaviour
{
    [SerializeField] private AnimationCurve m_RegenerationRateByCurrentHealth = default;

    [SerializeField] private Animator m_FoodHealthAnimator = default;

    [SerializeField] private float m_fHealthThresholdForEaten = default;

    [SerializeField] private float m_fHealthThresholdForReadyForEating = default;

    [SerializeField] private float m_fFoodSizeChangeTime = default;

    [SerializeField] private string m_FoodAnimatorParamName = default;

    [SerializeField] private GameObject m_EatingParticlesPrefab = default;

    [SerializeField] private CowGameManager m_Manager = default;

    [SerializeField] private EntityTypeComponent m_EntityInformation = default;

    private float m_fCurrentFoodSize = 1.0f;
    private HealthComponent m_HealthComponent = default;
    private float m_fFoodSizeChangeVelocity = 0.0f;
    private enum FoodStatus 
    {
        ReadyToEat,
        Growing
    }

    private FoodStatus m_CurrentFoodStatus = FoodStatus.Growing;

	// Update is called once per frame
	private void Awake()
	{
        m_HealthComponent = GetComponent<HealthComponent>();
        m_Manager.AddToPauseUnpause(() => enabled = false, () => enabled = true);
    }

    private void OnTakeDamage(GameObject source, GameObject target, DamageType damageType, float currentHealthPercentage)
    {
        m_HealthComponent.OnTakenDamageInstance += OnTakeDamage;
    }

    void Update()
    {
        m_HealthComponent.ReplenishHealth(m_RegenerationRateByCurrentHealth.Evaluate(m_HealthComponent.GetCurrentHealthPercentage) * Time.deltaTime);
        m_fCurrentFoodSize = Mathf.SmoothDamp(m_fCurrentFoodSize, m_HealthComponent.GetCurrentHealthPercentage, ref m_fFoodSizeChangeVelocity, m_fFoodSizeChangeTime);
        m_FoodHealthAnimator.Play(m_FoodAnimatorParamName, 0, m_fCurrentFoodSize);

        if (m_HealthComponent.GetCurrentHealthPercentage < m_fHealthThresholdForEaten && m_CurrentFoodStatus == FoodStatus.ReadyToEat)
        {
            m_CurrentFoodStatus = FoodStatus.Growing;
            m_EntityInformation.RemoveFromTrackable();
        }
        else if (m_HealthComponent.GetCurrentHealthPercentage > m_fHealthThresholdForReadyForEating && m_CurrentFoodStatus == FoodStatus.Growing)
        {
            m_CurrentFoodStatus = FoodStatus.ReadyToEat;
            m_EntityInformation.AddToTrackable();
        }
    }
}
