using UnityEngine;
using EZCameraShake;

public class AttackTypeRam : AttackBase
{
	[SerializeField] private float m_RamForce;
	[SerializeField] private float m_ElevationAngleAboveGround;
	[SerializeField] private Transform m_AnimationTransform;
	[SerializeField] private Transform m_RamPoint;
	[SerializeField] private GameObject m_RamFXPrefabs;
	public override void AttackTarget(in GameObject target)
	{
		IThrowableObjectComponent throwableComponent = target.GetComponent<IThrowableObjectComponent>();
		Instantiate(m_RamFXPrefabs, m_RamPoint.position, m_RamPoint.rotation, null);
		if (target.TryGetComponent(out PlayerComponent _)) 
		{
			CameraShaker.Instance.ShakeOnce(10.0f, 5.0f, 0.1f, 1.0f);
		}
		ProjectileParams throwParams = new ProjectileParams(throwableComponent, m_RamForce, m_AnimationTransform.forward, throwableComponent.GetMainTransform.position, 0);
		target.GetComponent<IThrowableObjectComponent>().ThrowObject(throwParams);
	}
}
