using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CreatureInformation")]
public class EntityInformation : ScriptableObject
{
    [SerializeField] private List<EntityInformation> m_Hunts;
    [SerializeField] private List<EntityInformation> m_ScaredOf;
    [SerializeField] private List<EntityInformation> m_Attacks;
    public ref List<EntityInformation> GetHunts => ref m_Hunts;
    public ref List<EntityInformation> GetScaredOf => ref m_ScaredOf;
    public ref List<EntityInformation> GetAttacks => ref m_Attacks;
}
