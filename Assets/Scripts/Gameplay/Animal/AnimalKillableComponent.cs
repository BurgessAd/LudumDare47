using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalKillableComponent : MonoBehaviour, IKillableComponent
{
    [SerializeField]
    private CowGameManager m_ManagerComponent;
    void IKillableComponent.OnKilled()
    {
        m_ManagerComponent.OnEntityDestroyed(gameObject, EntityType.Prey);
        Destroy(gameObject);
    }
}
