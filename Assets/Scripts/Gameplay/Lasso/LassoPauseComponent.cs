using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LassoPauseComponent : PauseComponent
{
    [SerializeField]
    private CowGameManager m_Manager;
    [SerializeField]
    private LassoStartComponent m_LassoStartComponent;
    private void Start()
    {
        m_Manager.OnEntitySpawned(gameObject, EntityType.Player);
    }

    public override void Pause()
    {
        m_LassoStartComponent.enabled = false;
    }

    public override void Unpause()
    {
        m_LassoStartComponent.enabled = true;
    }
}
