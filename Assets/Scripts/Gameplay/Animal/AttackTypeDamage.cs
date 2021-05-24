using System;
using UnityEngine;

public class AttackTypeDamage : AttackBase
{
	[Range(0f, 5f)] [SerializeField] private float m_DamageAmount;

	public event Action<float, GameObject> OnDamagedTarget;
	public override void AttackTarget(in GameObject target)
	{
		target.GetComponent<HealthComponent>().TakeDamageInstance(gameObject, DamageType.PredatorDamage, m_DamageAmount);
		OnDamagedTarget?.Invoke(m_DamageAmount, target);
	}
}
