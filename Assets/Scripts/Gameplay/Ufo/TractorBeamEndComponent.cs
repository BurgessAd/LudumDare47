using UnityEngine;
using System;

public class TractorBeamEndComponent : MonoBehaviour
{
    [SerializeField]
    private Transform m_Transform = default;
    [SerializeField]
    private GameObject m_OnAbductedEffectPrefab = default;

    public event Action<AbductableComponent> OnAbductableAbducted;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out AbductableColliderComponent abductable)) 
        {
            if (!abductable.GetAbductable.HasRegisteredAbduction) 
            {
                Instantiate(m_OnAbductedEffectPrefab, m_Transform.position, m_Transform.rotation, m_Transform);
                OnAbductableAbducted(abductable.GetAbductable);
            }
        }
    }
}
