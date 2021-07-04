using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComponent : MonoBehaviour, IPauseListener
{
	[SerializeField] private CowGameManager m_GameManager;

	[SerializeField] private HealthComponent m_HealthComponent;

	[SerializeField] private LassoStartComponent m_LassoComponent;

	[SerializeField] private Transform m_CamContainer;

	[SerializeField] private BoxCollider m_GrapplingBufferCollider;

	[SerializeField] private int m_OnThrowLayer;
	private void Awake()
	{
		m_LassoComponent.OnSetPullingObject += (ThrowableObjectComponent throwable) => OnStartGrappling();
		m_LassoComponent.OnStoppedPullingObject += OnStopGrappling;
		m_HealthComponent.OnEntityDied += (GameObject _, GameObject __, DamageType ___) => OnDied();
		m_GrapplingBufferCollider.enabled = false;
		m_GameManager.AddToPauseUnpause(this);
		m_GameManager.RegisterInitialCameraContainerTransform(m_CamContainer);
	}
	private void OnDied()
	{
		m_GameManager.OnPlayerKilled();
	}

	public void Pause() 
	{
		enabled = false;
	}

	public void Unpause() 
	{
		enabled = true;
	}

	private void OnStartGrappling() 
	{
		m_GrapplingBufferCollider.enabled = true;
	}

	private void OnStopGrappling() 
	{
		m_GrapplingBufferCollider.enabled = false;
	}
}
