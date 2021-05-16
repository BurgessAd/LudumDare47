using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSourceComponent : MonoBehaviour
{
    [SerializeField] private AnimationCurve m_RegenerationRateByCurrentHealth;

    [SerializeField] private Animator m_FoodHealthAnimator;

    [SerializeField] private float m_fHealthThresholdForEaten;

    [SerializeField] private float m_fHealthThresholdForReadyForEating;

    [SerializeField] private float m_fFoodSizeChangeTime;

    [SerializeField] private string m_FoodAnimatorParamName;

    private float m_fCurrentFoodSize = 1.0f;
    private HealthComponent m_HealthComponent;
    private float m_fFoodSizeChangeVelocity = 0.0f;

	// Update is called once per frame
	private void Awake()
	{
        m_HealthComponent = GetComponent<HealthComponent>();
	}
	void Update()
    {
        m_HealthComponent.ReplenishHealth(m_RegenerationRateByCurrentHealth.Evaluate(m_HealthComponent.GetCurrentHealthPercentage) * Time.deltaTime);
        m_fCurrentFoodSize = Mathf.SmoothDamp(m_fCurrentFoodSize, m_HealthComponent.GetCurrentHealthPercentage, ref m_fFoodSizeChangeVelocity, m_fFoodSizeChangeTime);
        m_FoodHealthAnimator.Play(m_FoodAnimatorParamName, 0, m_fCurrentFoodSize);
    }
}
