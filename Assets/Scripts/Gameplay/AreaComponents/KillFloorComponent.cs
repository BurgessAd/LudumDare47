using UnityEngine;
public class KillFloorComponent : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out AnimalColliderComponent colliderObject)) 
        {
            colliderObject.GetAnimalComponent.GetComponent<HealthComponent>().OnTakeLethalDamage(DamageType.FallDamage);
        }
        else if (other.TryGetComponent(out HealthComponent healthComponent)) 
        {
            healthComponent.OnTakeLethalDamage(DamageType.FallDamage);
        }

        if (other.TryGetComponent(out KillFloorDelayedDestroy killFoorComponent)) 
        {
            killFoorComponent.OnHitKillFloor();
        }
    }
}
