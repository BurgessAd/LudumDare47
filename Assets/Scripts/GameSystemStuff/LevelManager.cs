using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
	[Header("Animation parameters")]
	[SerializeField]
	private float m_fTransitionTime;
	[SerializeField]
	private float m_fMenuTransitionTime;

	[Header("Object references")]
	[SerializeField]
	private CowGameManager m_Manager;
	[SerializeField]
	private CanvasGroup m_MainCanvas;
	[SerializeField]
	private CanvasGroup m_StartCanvas;
	[SerializeField]
	private CanvasGroup m_PauseCanvas;
	[SerializeField]
	private CanvasGroup m_EndSuccessCanvas;
	[SerializeField]
	private CanvasGroup m_EndFailureCanvas;
	[SerializeField]
	private Animator m_LevelTransitionAnimator;

	private StateMachine m_LevelState;

	private int m_LevelIndex;

	private CanvasGroup m_CurrentOpenCanvas;

	private int m_LevelToLoad;

	private void Awake()
	{
		m_LevelState = new StateMachine();
		m_LevelState.AddState(new PausedState(this, m_PauseCanvas));
		m_LevelState.AddState(new EndFailureState(this, m_EndFailureCanvas));
		m_LevelState.AddState(new EndSuccessState(this, m_EndSuccessCanvas));
		m_LevelState.AddState(new PlayingState(this, m_MainCanvas));
		m_LevelState.SetInitialState(typeof(PlayingState));
		m_Manager.NewLevelLoaded(this, m_LevelIndex);
		m_LevelIndex = SceneManager.GetActiveScene().buildIndex;
	}

	private void Update() 
	{
		m_LevelState.Tick();
	}

	public void SetCurrentCanvas(CanvasGroup canvas, Action callOnComplete) 
	{
		ClearCanvas();
		LeanTween.alphaCanvas(canvas, 1.0f, m_fMenuTransitionTime).setEaseInOutCubic().setOnComplete(callOnComplete);
		m_CurrentOpenCanvas = canvas;
	}

	public void ClearCanvas() 
	{
		if (m_CurrentOpenCanvas)
		{
			m_CurrentOpenCanvas.interactable = false;
			m_CurrentOpenCanvas.blocksRaycasts = false;
			LeanTween.alphaCanvas(m_CurrentOpenCanvas, 0.0f, m_fMenuTransitionTime).setEaseInOutCubic();
		}
	}

	private IEnumerator BeginSceneTransition()
	{
		m_Manager.ClearLevelData();
		m_LevelTransitionAnimator.SetTrigger("Start");
		yield return new WaitForSeconds(m_fTransitionTime);
		SceneManager.LoadScene(m_LevelToLoad);
	}

	public void PauseLevel(bool pause) 
	{
		m_Manager.SetPauseState(pause);
	}

	// called by UI
	public void LoadNextLevel()
	{
		m_LevelToLoad = m_LevelIndex + 1;
		StartCoroutine(BeginSceneTransition());
	}

	public void RestartLevel()
	{
		m_LevelToLoad = m_LevelIndex;
		StartCoroutine(BeginSceneTransition());
	}

	public void LoadMenu()
	{
		m_LevelToLoad = 0;
		StartCoroutine(BeginSceneTransition());
	}

	public void OnLevelSucceeded() 
	{
		m_LevelState.RequestTransition(typeof(EndSuccessState)); 
	}

	public void OnLevelFailed() 
	{
		m_LevelState.RequestTransition(typeof(EndFailureState));
	}

	public void ResumeLevel()
	{
		m_LevelState.RequestTransition(typeof(PlayingState));
	}
}

public class PlayingState : IState 
{
	private readonly LevelManager m_LevelLoader;
	private readonly CanvasGroup m_CanvasGroup;
	public PlayingState(LevelManager loader, CanvasGroup pauseGroup) 
	{
		m_LevelLoader = loader;
		m_CanvasGroup = pauseGroup;
	}
	public override void OnEnter()
	{
		m_LevelLoader.PauseLevel(false);
		m_LevelLoader.SetCurrentCanvas(m_CanvasGroup, () => { m_CanvasGroup.blocksRaycasts = true; m_CanvasGroup.interactable = true; });
	}

	public override void Tick()
	{
		if (Input.GetKeyDown(KeyCode.Escape)) 
		{
			RequestTransition<PausedState>();
		}
	}

	public override void OnExit()
	{
		m_LevelLoader.PauseLevel(true);
	}
}

public class PausedState : IState 
{
	private readonly LevelManager m_LevelLoader;
	private readonly CanvasGroup m_CanvasGroup;
	public PausedState(LevelManager loader, CanvasGroup pauseGroup)
	{
		m_LevelLoader = loader;
		m_CanvasGroup = pauseGroup;
	}
	public override void OnEnter()
	{
		m_LevelLoader.SetCurrentCanvas(m_CanvasGroup, () => { m_CanvasGroup.blocksRaycasts = true; m_CanvasGroup.interactable = true; });
	}

	public override void Tick()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			m_LevelLoader.ResumeLevel();
		}
	}
}

public class EndFailureState : IState 
{
	private readonly LevelManager m_LevelLoader;
	private readonly CanvasGroup m_CanvasGroup;
	public EndFailureState(LevelManager loader, CanvasGroup pauseGroup)
	{
		m_LevelLoader = loader;
		m_CanvasGroup = pauseGroup;
	}
	public override void OnEnter()
	{
		m_LevelLoader.SetCurrentCanvas(m_CanvasGroup, () => { m_CanvasGroup.blocksRaycasts = true; m_CanvasGroup.interactable = true; });
	}
}

public class EndSuccessState : IState
{
	private readonly LevelManager m_LevelLoader;
	private readonly CanvasGroup m_CanvasGroup;
	public EndSuccessState(LevelManager loader, CanvasGroup pauseGroup)
	{
		m_LevelLoader = loader;
		m_CanvasGroup = pauseGroup;
	}
	public override void OnEnter()
	{
		m_LevelLoader.SetCurrentCanvas(m_CanvasGroup, () => { m_CanvasGroup.blocksRaycasts = true; m_CanvasGroup.interactable = true; });
	}
}

public class StartState : IState 
{
	public override void OnEnter()
	{
		
	}
}
