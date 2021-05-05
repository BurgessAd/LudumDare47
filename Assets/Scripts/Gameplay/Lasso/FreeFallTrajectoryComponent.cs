using UnityEngine;
using System;
public class FreeFallTrajectoryComponent : MonoBehaviour
{
    ProjectileParams projectile;
    private float m_fCurrentTime = 0.0f;
 
    [SerializeField]
    private Rigidbody m_rMovingBody;

    public event Action<Collision> OnObjectHitGround;
    public event Action OnObjectNotInFreeFall;

	private void Awake()
	{
        enabled = false;
	}

	public void ThrowObject(in ProjectileParams projectileParams) 
    {
        m_fCurrentTime = 0.0f;
        projectile = projectileParams;
        m_rMovingBody.isKinematic = true;
        enabled = true;
        m_rMovingBody.position = projectile.EvaluatePosAtTime(0.0f);
        m_rMovingBody.rotation = projectile.EvaluateRotAtTime(0.0f);
    }

    public void StopThrowingObject() 
    {
        OnObjectNotInFreeFall?.Invoke();
        enabled = false;
    }

	private void OnCollisionEnter(Collision collision)
    {
        if (enabled)
        {
            OnObjectHitGround?.Invoke(collision);
            OnObjectNotInFreeFall?.Invoke();
            m_rMovingBody.velocity = projectile.EvaluateVelocityAtTime(m_fCurrentTime);
            m_rMovingBody.angularVelocity = projectile.m_vRotAxis * projectile.m_fAngVel;
        }
        enabled = false;
    }

    void Update()
    {
        m_fCurrentTime += Time.deltaTime;
        m_rMovingBody.MovePosition(projectile.EvaluatePosAtTime(m_fCurrentTime));
        m_rMovingBody.MoveRotation(projectile.EvaluateRotAtTime(m_fCurrentTime));
    }
}

public struct ProjectileParams 
{
    public float m_fThrowSpeed;
    public Vector3 m_vStartPos;
    public Vector3 m_vRotAxis;
    public Vector3 m_vForwardDir;
    public float m_fElevationAngle;
    public float m_fAngVel;
    public float m_fGravityMult;

    public ProjectileParams(IThrowableObjectComponent throwable, float force, Vector3 throwDirection, Vector3 origin, float angularVelocity = 0)
    {
        m_fThrowSpeed = force/throwable.GetMass();
        m_vStartPos = origin;
        m_fGravityMult = throwable.GetGravityMultiplier;
        m_vRotAxis = UnityEngine.Random.insideUnitSphere;
        m_vForwardDir = Vector3.ProjectOnPlane(throwDirection, Vector3.up).normalized;
        m_fElevationAngle = Mathf.Deg2Rad * (90 - Vector3.Angle(Vector3.up, throwDirection));
        m_fAngVel = angularVelocity;
    }

    public ProjectileParams(float speed, Vector3 throwDirection, Vector3 origin, Vector3 rotationAxis, float angularVelocity = 0) 
    {
        m_fGravityMult = 1;
        m_fThrowSpeed = speed;
        m_vStartPos = origin;
        m_vRotAxis = rotationAxis;
        m_vForwardDir = Vector3.ProjectOnPlane(throwDirection, Vector3.up).normalized;
        m_fElevationAngle = Mathf.Deg2Rad * (90 - Vector3.Angle(Vector3.up, throwDirection));
        m_fAngVel = angularVelocity;
    }

    public void SetAngularVelocity(in float angularVelocity) 
    {
        m_fAngVel = angularVelocity;
    }

    public Vector3 EvaluatePosAtTime(in float time) 
    {
        return m_vStartPos
             + Vector3.up * (-0.5f * UnityEngine.Physics.gravity.magnitude * m_fGravityMult * time * time + m_fThrowSpeed * Mathf.Sin(m_fElevationAngle) * time)
             + m_vForwardDir * (Mathf.Cos(m_fElevationAngle) * m_fThrowSpeed * time);
    }

    public Vector3 EvaluateVelocityAtTime(in float time) 
    {
        return Vector3.up * (-UnityEngine.Physics.gravity.magnitude * m_fGravityMult * time + m_fThrowSpeed * Mathf.Sin(m_fElevationAngle)) + m_vForwardDir * Mathf.Cos(m_fElevationAngle) * m_fThrowSpeed;
    }

    public Quaternion EvaluateRotAtTime(in float time) 
    {
        return Quaternion.AngleAxis(time * m_fAngVel, time * m_vRotAxis);
    }
}