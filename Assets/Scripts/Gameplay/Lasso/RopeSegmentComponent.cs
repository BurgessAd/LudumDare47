using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Transform))]
[RequireComponent(typeof(SphereCollider))]
public class RopeSegmentComponent : MonoBehaviour
{
	#region Class member variables

	private SphereCollider m_SphereCollider;
    private Transform m_Transform;
    private Vector3 m_CachedAcceleration = Vector3.zero;

    private Transform m_BoundTransform;
    private Rigidbody m_BoundBody;
    private Vector3 m_OldPosition;

    private bool m_bIsEndPointBound;
    #endregion
    #region Properties
    private bool IsFree => m_BoundBody == null;

    private bool IsBound => m_BoundBody != null;

    public bool IsEndPoint => m_bIsEndPointBound;

    public Vector3 Velocity { get { return m_Transform.position - m_OldPosition; } }
	#endregion
	#region Unity Messages
	private void Awake()
    {
        m_Transform = GetComponent<Transform>();
        m_SphereCollider = GetComponent<SphereCollider>();
    }
	#endregion
	#region Public Methods
	public void SetNewPosition(in Vector3 position) 
    {
        m_OldPosition = m_Transform.position;
        m_Transform.position = position;
    }

    public void AddAcceleration(in Vector3 force) 
    {
        m_CachedAcceleration += force;
    }

    public Vector3 GetCurrentPosition => m_Transform.position;

    public Vector3 GetLastPosition => m_OldPosition;
    
    public ref Vector3 GetAdditionalAcceleration() 
    {
        return ref m_CachedAcceleration;
    }

    public void UpdateVerlet(Vector3 gravityVector) 
    {
		Vector3 velocity = GetCurrentPosition - GetLastPosition;
		//Vector3 totalAcceleration = Vector3.up * -m_GravityStrength + currentRopeSegment.GetAdditionalAcceleration();
		SetNewPosition(GetCurrentPosition + velocity + gravityVector * Time.fixedDeltaTime * Time.fixedDeltaTime);
		// currentRopeSegment.FinishUpdateLoop();
		//switch (m_BoundBody.interpolation)
		//{
		//	case RigidbodyInterpolation.Interpolate:
		//		this.UpdatePosition(m_BoundBody.position + (m_BoundBody.velocity * Time.fixedDeltaTime) / 2);
		//		break;
		//	case RigidbodyInterpolation.None:
		//	default:
		//		this.UpdatePosition(m_BoundBody.position + m_BoundBody.velocity * Time.fixedDeltaTime);
		//		break;
		//}
	}

    public void UpdatePosition(in Vector3 newPosition) 
    {

    }

    public void SetRopeSize(float ropeRadius) 
    {
        m_SphereCollider.radius = ropeRadius;
    }

    public void AddToPosition(Vector3 additionalPosition) 
    {
        m_Transform.position += additionalPosition;
    }

    public void BindToObject(Transform to) 
    {

    }

    public void UnbindFromObject() 
    {

    }
	#endregion
}
