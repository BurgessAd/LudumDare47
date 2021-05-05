using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackComponentDamage : AttackComponentBase
{
	[Range(0u, 5u)]
	[SerializeField] private uint m_DamageAmount;
	public override void AttackTarget(in GameObject target)
	{
		target.GetComponent<HealthComponent>().TakeDamageInstance(gameObject, DamageType.PredatorDamage, m_DamageAmount);
	}
}
