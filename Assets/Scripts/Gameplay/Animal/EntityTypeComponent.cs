using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityTypeComponent : MonoBehaviour
{
    [SerializeField]
    private EntityInformation m_EntityInformation;

    public EntityInformation GetEntityInformation => m_EntityInformation;
}
