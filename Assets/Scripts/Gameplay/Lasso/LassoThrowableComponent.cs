using UnityEngine;
using System;
public class LassoThrowableComponent : MonoBehaviour
{
    private float m_fThrowSpeed;
    private Vector3 m_vStartPos;
    private Vector3 m_vRotAxis;
    private Vector3 m_vForwardDir;
    private float m_fElevationAngle;
    private float m_fCurrentTime = 0.0f;
    private float m_fAngVel;
    private float m_fGravity;
    [SerializeField]
    private Rigidbody m_rMovingBody;

    public event Action<Collision> OnObjectHitGround;

    public void ThrowObject(in float speed,in float angVel, in Vector3 startPos, in Vector3 forwardDir, in float angle, in float gravity, in Vector3 rotAxis) 
    {
        m_fThrowSpeed = speed;
        m_vStartPos = startPos;
        m_vForwardDir = forwardDir;
        m_fElevationAngle = angle;
        m_fGravity = gravity;
        m_fCurrentTime = 0.0f;
        m_fAngVel = angVel;
        m_vRotAxis = rotAxis;
        enabled = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enabled)
        OnObjectHitGround?.Invoke(collision);
        enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        m_rMovingBody.position =
             m_vStartPos
             + Vector3.up * (-0.5f * m_fGravity * m_fCurrentTime * m_fCurrentTime + m_fThrowSpeed * Mathf.Sin(m_fElevationAngle) * m_fCurrentTime)
             + m_vForwardDir * (Mathf.Cos(m_fElevationAngle) * m_fThrowSpeed * m_fCurrentTime);
        m_fCurrentTime += Time.fixedDeltaTime;
        m_rMovingBody.rotation = Quaternion.AngleAxis(Time.fixedDeltaTime * m_fAngVel, Time.fixedDeltaTime * m_vRotAxis)* m_rMovingBody.rotation;
    }
}
