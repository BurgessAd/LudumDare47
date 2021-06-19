﻿using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    public abstract void AttackTarget(in GameObject target, in Vector3 attackDirection);

	[SerializeField] private AnimationCurve m_AttackPitchAnimationCurve;
	[SerializeField] private AnimationCurve m_AttackForwardAnimationCurve;
	[SerializeField] private AnimationCurve m_AttackHopAnimationCurve;
	[Range(0f, 3f)][SerializeField] private float m_Duration = 1.0f;

	public AnimationCurve GetPitchCurve() => m_AttackPitchAnimationCurve;
	public AnimationCurve GetForwardCurve() => m_AttackForwardAnimationCurve;
	public AnimationCurve GetHopCurve() => m_AttackHopAnimationCurve;
	public float GetDuration() => m_Duration;
}
