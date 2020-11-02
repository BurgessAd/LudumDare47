using UnityEngine;
public class KillFloorComponent : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IKillableComponent killableObject)) 
        {
            killableObject.OnKilled();
        }
    }
}
