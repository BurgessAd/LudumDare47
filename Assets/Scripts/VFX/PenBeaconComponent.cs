using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenBeaconComponent : MonoBehaviour, IPauseListener
{
    [SerializeField] private CowGameManager m_GameManager;
    [SerializeField] private float m_FlashTime;
    [SerializeField] private Color EnterPenColor;
    [SerializeField] private Color LeavePenColor;
    [SerializeField] private List<MeshRendererColorChanger> m_BeaconColourChangers;
    [SerializeField] private List<PenBeaconElementComponent> m_BeaconElements;
    [SerializeField] private List<Rotator> m_BeaconRotators;
    [SerializeField] private float m_IntialOpacity;
    private StateMachine m_BeaconStateMachine;
    private Transform m_PlayerTransform;

    private void Start()
    {
        m_BeaconStateMachine = new StateMachine(new PenBeaconPlayState(this));
        m_BeaconStateMachine.AddState(new PenBeaconPrePlayState(this));
        m_BeaconStateMachine.AddState(new PenBeaconPostDeathState(this));
        m_BeaconStateMachine.InitializeStateMachine();

        m_PlayerTransform = m_GameManager.GetPlayer.transform;
        m_PlayerTransform.GetComponent<HealthComponent>().OnEntityDied += (GameObject, Vector3, DamageType) => OnLevelFinished();


		m_GameManager.GetCurrentLevel.OnLevelStarted += OnLevelStarted;
		m_GameManager.GetCurrentLevel.OnLevelFinished += OnLevelFinished;
        m_GameManager.AddToPauseUnpause(this);
	}
    private void OnLevelStarted() 
    {
        m_BeaconStateMachine.RequestTransition(typeof(PenBeaconPlayState));
    }



    private void OnLevelFinished() 
    {
        m_BeaconStateMachine.RequestTransition(typeof(PenBeaconPostDeathState));
    }

    public void Pause()
    {
        m_BeaconStateMachine.RequestTransition(typeof(PenBeaconPauseState));
    }

    public void Unpause()
    {
        m_BeaconStateMachine.RequestTransition(typeof(PenBeaconPlayState));
    }


    void Update()
    {
        m_BeaconStateMachine.Tick();
    }
    public void OnObjectEnterPen()
    {
        if (m_bIsFlashing)
        {
            StopCoroutine(m_FlashCoroutine);
        }
        m_FlashCoroutine = Flash(new Vector3(EnterPenColor.r, EnterPenColor.g, EnterPenColor.b), Vector3.one);
        StartCoroutine(m_FlashCoroutine);
    }

    public void OnObjectLeavePen()
    {
        if (m_bIsFlashing)
        {
            StopCoroutine(m_FlashCoroutine);
        }
        m_FlashCoroutine = Flash(new Vector3(LeavePenColor.r, LeavePenColor.g, LeavePenColor.b), Vector3.one);
        StartCoroutine(m_FlashCoroutine);
    }

    private IEnumerator m_FlashCoroutine;
    float m_CurrentFlashTime = 0.0f;
    bool m_bIsFlashing = false;
    private IEnumerator Flash(Vector3 initialColor, Vector3 finalColor)
    {
        m_bIsFlashing = true;
        m_CurrentFlashTime = m_FlashTime;
        while (m_CurrentFlashTime > 0)
        {
            float currentTime = m_CurrentFlashTime / m_FlashTime;
            SetChildColor(Vector3.Lerp(finalColor, initialColor, currentTime) * 2f);
            m_CurrentFlashTime -= Time.deltaTime;
            yield return null;
        }
        SetChildColor(finalColor * 2f);
        m_bIsFlashing = false;
    }

    private void SetChildColor(in Vector3 color) 
    {
        for (int i = 0; i < m_BeaconColourChangers.Count; i++)
        {
            m_BeaconColourChangers[i].SetDesiredColour(color);
        }
    }

    public void LetChildUpdateOpacity() 
    {
        for (int i = 0; i < m_BeaconColourChangers.Count; i++)
        {
            m_BeaconColourChangers[i].SetDesiredOpacity(m_BeaconElements[i].GetPlayerOpacity(m_PlayerTransform));
        }
    }

    public void SetInitialOpacity()
    {
        for (int i = 0; i < m_BeaconColourChangers.Count; i++) 
        {
            m_BeaconColourChangers[i].SetDesiredOpacity(m_IntialOpacity);
        }
    }

    public void FadeOutAtEnd()
    {
        for (int i = 0; i < m_BeaconColourChangers.Count; i++)
        {
            m_BeaconColourChangers[i].SetDesiredOpacity(0.0f);
        }
    }

    public void OnUnpause() 
    {
        for (int i = 0; i < m_BeaconColourChangers.Count; i++)
        {
            m_BeaconRotators[i].enabled = true;
            m_BeaconElements[i].enabled = true;
            m_BeaconColourChangers[i].enabled = true;
        }
    }

    public void OnPause() 
    {
        for (int i = 0; i < m_BeaconColourChangers.Count; i++)
        {
            m_BeaconRotators[i].enabled = true;
            m_BeaconElements[i].enabled = false;
            m_BeaconColourChangers[i].enabled = false;
        }
    }
}


class PenBeaconPlayState : AStateBase
{
    private readonly PenBeaconComponent m_BeaconElementComponent;
    public PenBeaconPlayState(PenBeaconComponent penBeaconComponent)
    {
        m_BeaconElementComponent = penBeaconComponent;
    }

    public override void Tick()
    {
        m_BeaconElementComponent.LetChildUpdateOpacity();
    }
}

class PenBeaconPauseState : AStateBase 
{
    private readonly PenBeaconComponent m_BeaconComponent;

    public PenBeaconPauseState(PenBeaconComponent beaconComponent) 
    {
        m_BeaconComponent = beaconComponent;
    }

	public override void OnEnter()
	{
        m_BeaconComponent.OnPause();
	}

	public override void OnExit()
	{
        m_BeaconComponent.OnUnpause();
	}
}

class PenBeaconPrePlayState : AStateBase
{
    private readonly PenBeaconComponent m_BeaconElementComponent;
    public PenBeaconPrePlayState(PenBeaconComponent penBeaconComponent)
    {
        m_BeaconElementComponent = penBeaconComponent;
    }

    public override void OnEnter()
    {
        m_BeaconElementComponent.SetInitialOpacity();
    }
}

class PenBeaconPostDeathState : AStateBase
{
    private readonly PenBeaconComponent m_BeaconElementComponent;
    public PenBeaconPostDeathState(PenBeaconComponent penBeaconComponent)
    {
        m_BeaconElementComponent = penBeaconComponent;
    }

    public override void OnEnter()
    {
        m_BeaconElementComponent.FadeOutAtEnd();

    }
}