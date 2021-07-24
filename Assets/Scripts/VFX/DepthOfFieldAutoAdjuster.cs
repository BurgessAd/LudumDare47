using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[ExecuteInEditMode]
public class DepthOfFieldAutoAdjuster : MonoBehaviour
{
	[SerializeField] private Transform m_CamTransform;
	[SerializeField] private PostProcessVolume m_Volume;
	[SerializeField] private float m_fFocusDistanceSettleTime;
	[SerializeField] private float m_fMaxFocusDistanceSettleVelocity;
	[SerializeField] private float m_fMaxFocalLength;

	private float m_fFocusDistanceSettleVelocity;
	private DepthOfField m_DepthOfField;
	private float m_FocusDistance;
	private float m_fTargetFocusDistance;

	private Vector3 hitPos;

	private void Awake()
    {
		if (!m_DepthOfField)
		{
			PostProcessProfile volumeProfile = m_Volume?.profile;
			if (!volumeProfile) throw new System.NullReferenceException(nameof(PostProcessProfile));
			if (!volumeProfile.TryGetSettings(out m_DepthOfField)) throw new System.NullReferenceException(nameof(m_DepthOfField));
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawLine(m_CamTransform.position, hitPos);
	}

	void Update()
    {
		Awake();
		if (Physics.Raycast(m_CamTransform.position, m_CamTransform.forward, out RaycastHit hit, m_fMaxFocalLength))
		{
			hitPos = hit.point;
			m_fTargetFocusDistance = hit.distance;
		}
		else
		{
			hitPos = m_CamTransform.position + m_CamTransform.forward * m_fMaxFocalLength;
			m_fTargetFocusDistance = m_fMaxFocalLength;
		}

		m_FocusDistance = Mathf.SmoothDamp(m_FocusDistance, m_fTargetFocusDistance, ref m_fFocusDistanceSettleVelocity, m_fFocusDistanceSettleTime);
		m_fFocusDistanceSettleVelocity = Mathf.Clamp(m_fFocusDistanceSettleVelocity, -m_fMaxFocusDistanceSettleVelocity, m_fMaxFocusDistanceSettleVelocity);
		m_DepthOfField.focusDistance.Override(m_FocusDistance);
    }
}
