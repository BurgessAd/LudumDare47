using UnityEngine;
using System;

[RequireComponent(typeof(FreeFallTrajectoryComponent))]
public class ThrowableObjectComponent : IThrowableObjectComponent
{
    [SerializeField] private Rigidbody m_ThrowingBody;

	[SerializeField] private Transform m_CameraFocusTransform;

	[SerializeField] private Transform m_AttachmentTransform;

	[SerializeField] private Transform m_MainTransform;

	[SerializeField] private float m_fMassMultiplier = 1.0f;

	[SerializeField] protected FreeFallTrajectoryComponent m_FreeFallComponent;

	public event Action OnDestroyed;

	public void ThrowableDestroyed()
	{
		OnDestroyed?.Invoke();
	}

	public bool IsImmediatelyThrowable { get; set; } = false;

	public override Transform GetCameraFocusTransform => m_CameraFocusTransform;

	public override Transform GetAttachmentTransform => m_AttachmentTransform;

	public override Transform GetMainTransform => m_MainTransform;

	public void Awake()
	{
		m_FreeFallComponent.OnObjectNotInFreeFall += OnObjectLanded;
		if (TryGetComponent(out HealthComponent healthComponent))
		{
			healthComponent.OnEntityDied += (GameObject, Vector3, DamageType) => ThrowableDestroyed();
		}
	}

	public override void ThrowObject(in ProjectileParams pParams)
	{
		m_FreeFallComponent.ThrowObject(pParams);
		base.ThrowObject(pParams);
	}

	public override float GetMass()
	{
		return m_ThrowingBody.mass * m_fMassMultiplier;
	}

	public override void ApplyForceToObject(Vector3 force)
	{
		m_ThrowingBody.AddForce(force, ForceMode.Impulse);
	}
}
