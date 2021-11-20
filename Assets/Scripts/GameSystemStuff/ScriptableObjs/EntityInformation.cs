﻿using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CreatureInformation")]
public class EntityInformation : ScriptableObject
{
    [SerializeField] private EntityInformation[] m_Hunts;
    [SerializeField] private EntityInformation[] m_ScaredOf;
    [SerializeField] private EntityInformation[] m_Attacks;
    public ref EntityInformation[] GetHunts => ref m_Hunts;
    public ref EntityInformation[] GetScaredOf => ref m_ScaredOf;
    public ref EntityInformation[] GetAttacks => ref m_Attacks;
}