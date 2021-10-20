using UnityEngine;

[RequireComponent(typeof(ThrowableObjectComponent))]
[RequireComponent(typeof(FreeFallTrajectoryComponent))]
public class BombComponent : MonoBehaviour
{
    [SerializeField] private GameObject m_HazardRef;
    [SerializeField] private GameObject m_DamageRef;
    [SerializeField] private Transform m_Transform;
    [SerializeField] private FreeFallTrajectoryComponent m_FreeFallComponent;
    // Start is called before the first frame update
    void Awake()
    {
        m_Transform = transform;
        GetComponent<FreeFallTrajectoryComponent>().OnObjectHitGround += OnHitGround;
    }

    void OnHitGround(Collision _)
    {
        Instantiate(m_HazardRef, m_Transform.position, m_Transform.rotation);
        Instantiate(m_DamageRef, m_Transform.position, m_Transform.rotation);
    }
}
