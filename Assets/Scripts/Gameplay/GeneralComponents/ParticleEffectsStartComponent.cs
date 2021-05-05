using UnityEngine;
public class ParticleEffectsStartComponent : MonoBehaviour
{
    void Awake()
    {
        GetComponent<ParticleSystem>().Stop();
    }
}
