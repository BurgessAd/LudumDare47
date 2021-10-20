using System.Collections;
using UnityEngine;

public class HazardComponent : MonoBehaviour
{
    [SerializeField] private EntityTypeComponent m_EntityTypeComponent;
    [SerializeField] private float m_HazardLifetime = 0.0f;
    [SerializeField] private float m_HazardRadius = 0.0f;

    public float GetHazardRadius => m_HazardRadius;

    void Start()
    {
        StartCoroutine(StartDestroyTimer());
    }

    private IEnumerator StartDestroyTimer() 
    {
        yield return new WaitForSecondsRealtime(m_HazardLifetime);
        m_EntityTypeComponent.OnKilled();
    }
}
