using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CreatureInformation")]
public class EntityInformation : ScriptableObject
{
    [SerializeField] private List<EntityInformation> m_Hunts;
    [SerializeField] private List<EntityInformation> m_HuntedBy;
    public ref List<EntityInformation> GetHunts => ref m_Hunts;
    public ref List<EntityInformation> GetHuntedBy => ref m_HuntedBy;
}
