using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    public abstract void AttackTarget(in GameObject target);

	[SerializeField] private AnimationCurve m_AttackPitchAnimationCurve;
	[SerializeField] private AnimationCurve m_AttackForwardAnimationCurve;
	[SerializeField] private AnimationCurve m_AttackHopAnimationCurve;

	public AnimationCurve GetPitchCurve() => m_AttackPitchAnimationCurve;
	public AnimationCurve GetForwardCurve() => m_AttackForwardAnimationCurve;
	public AnimationCurve GetHopCurve() => m_AttackHopAnimationCurve;
}
