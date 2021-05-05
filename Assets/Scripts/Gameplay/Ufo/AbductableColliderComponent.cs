using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbductableColliderComponent : MonoBehaviour
{
    [SerializeField]
    private AbductableComponent m_Abductable;
    public AbductableComponent GetAbductable => m_Abductable;
}
