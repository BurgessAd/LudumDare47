using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPauseComponent : PauseComponent
{
    [SerializeField]
    private PlayerCameraComponent m_CamMovement = null;
    [SerializeField]
    private PlayerMovement m_PlayerMovement = null;
    [SerializeField]
    private PlayerCameraComponent m_MouseLook;
    [SerializeField]
    private CowGameManager m_Manager;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        m_Manager.OnEntitySpawned(gameObject, typeof(PlayerPauseComponent));
    }

    public override void Pause()
    {
        m_CamMovement.enabled = false;
        m_MouseLook.enabled = false;
        m_PlayerMovement.enabled = false;
    }

    public override void Unpause()
    {
        m_CamMovement.enabled = true;
        m_PlayerMovement.enabled = true;
        m_MouseLook.enabled = true;
    }
}
