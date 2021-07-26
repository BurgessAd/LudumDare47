using UnityEngine;

[RequireComponent(typeof(Transform))]
public class RopeSegmentComponent : MonoBehaviour
{
	#region Class member variables

    private Transform m_Transform;

	#endregion

	#region Properties

    public Vector3 Velocity { get { return m_Transform.position - GetLastPosition; } }
	#endregion

	#region Unity Messages

	private void Awake()
    {
        m_Transform = GetComponent<Transform>();
    }

	#endregion

	#region Public Methods

    public Vector3 GetCurrentPosition => m_Transform.position;

	public Vector3 GetLastPosition { get; private set; }

	public void UpdateVerlet(Vector3 gravityVector) 
    {
		Vector3 velocity = GetCurrentPosition - GetLastPosition;

		SetNewPosition(GetCurrentPosition + velocity + gravityVector * Time.fixedDeltaTime * Time.fixedDeltaTime);
	}

	public void SetNewPosition(in Vector3 position)
	{
		GetLastPosition = m_Transform.position;
		m_Transform.position = position;
	}

	public void AddToPosition(Vector3 additionalPosition) 
    {
        m_Transform.position += additionalPosition;
    }

	#endregion
}
