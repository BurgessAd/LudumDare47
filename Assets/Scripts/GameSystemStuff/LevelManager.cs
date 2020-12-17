using System.Collections;
using UnityEngine.UI;
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
	private CanvasGroup m_StartCountdownCanvas;
	[SerializeField]
	private CanvasGroup m_PauseCanvas;
	[SerializeField]
	private CanvasGroup m_EndSuccessCanvas;
	[SerializeField]
	private CanvasGroup m_EndFailureCanvas;
	[SerializeField]
	private CanvasGroup m_StartButtonCanvas;
	[SerializeField]
	private Animator m_LevelTransitionAnimator;
	[SerializeField]
	private Text m_CountdownText;
	[SerializeField]
	private Animator m_LevelCountdownAnimator;

	[SerializeField]
	private float m_MapSize;
	[SerializeField]
	private Transform m_Transform;

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
		m_LevelState.AddState(new StartState(this, m_StartButtonCanvas));
		m_LevelState.SetInitialState(typeof(StartState));
		m_LevelTransitionAnimator.Play("Base Layer.TransitionIn");

		m_LevelIndex = SceneManager.GetActiveScene().buildIndex;
	}

	private void Start()
	{
		if (m_Manager.ShouldQuickRestart)
		{
			m_StartButtonCanvas.alpha = 0.0f;
			m_StartButtonCanvas.blocksRaycasts = false;
			m_StartButtonCanvas.interactable = false;
			PlayerReloadedLevel();
		}
		else 
		{
			SetCurrentCanvas(m_StartButtonCanvas, () => { m_StartButtonCanvas.blocksRaycasts = true; m_StartButtonCanvas.interactable = true; });
		}
		m_Manager.NewLevelLoaded(this);
	}

	private void Update() 
	{
		m_LevelState.Tick();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireSphere(GetMapCentre, m_MapSize);
	}

	public void SetCurrentCanvas(CanvasGroup canvas, Action callOnComplete, in float delay = 0.0f) 
	{
		ClearCanvas();
		LeanTween.alphaCanvas(canvas, 1.0f, m_fMenuTransitionTime).setEaseInOutCubic().setOnComplete(callOnComplete).setDelay(delay);
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
		m_LevelTransitionAnimator.Play("Base Layer.TransitionOut");
		yield return new WaitForSeconds(m_fTransitionTime);
		m_Manager.ClearLevelData();
		SceneManager.LoadScene(m_LevelToLoad);
	}

	private IEnumerator BeginCountdown() 
	{
		m_CountdownText.text = "3";
		m_LevelCountdownAnimator.Play("Base Layer.CountdownAnimation");
		yield return new WaitForSecondsRealtime(1.0f);
		m_CountdownText.text = "2";
		m_LevelCountdownAnimator.Play("Base Layer.CountdownAnimation");
		yield return new WaitForSecondsRealtime(1.0f);
		m_CountdownText.text = "1";
		m_LevelCountdownAnimator.Play("Base Layer.CountdownAnimation");
		yield return new WaitForSecondsRealtime(1.0f);
		m_CountdownText.text = "Go!";
		m_LevelCountdownAnimator.Play("Base Layer.CountdownAnimation");
		yield return new WaitForSecondsRealtime(0.2f);
		m_LevelState.RequestTransition(typeof(PlayingState));

	}

	private IEnumerator StartLevelWithoutCountdown() 
	{
		yield return new WaitForSecondsRealtime(3.0f);
		m_LevelState.RequestTransition(typeof(PlayingState));
	}

	public void PauseLevel(bool pause) 
	{
		m_Manager.SetPauseState(pause);
	}

	public float GetMapRadius => m_MapSize;

	public Vector3 GetMapCentre => m_Transform.position;

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


	public void PlayerStartedLevel() 
	{
		m_Manager.OnPlayerStartedLevel();
		SetCurrentCanvas(m_StartCountdownCanvas, () => { });
		StartCoroutine(BeginCountdown());
	}

	public void PlayerReloadedLevel()
	{
		m_Manager.OnPlayerStartedLevel();
		StartCoroutine(StartLevelWithoutCountdown());
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
		m_LevelLoader.SetCurrentCanvas(m_CanvasGroup, () => { m_CanvasGroup.blocksRaycasts = true; m_CanvasGroup.interactable = true; }, delay: 1.0f);
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
		m_LevelLoader.SetCurrentCanvas(m_CanvasGroup, () => { m_CanvasGroup.blocksRaycasts = true; m_CanvasGroup.interactable = true; }, delay: 1.0f);
		
	}
}

public class StartState : IState 
{
	private readonly LevelManager m_LevelLoader;
	private readonly CanvasGroup m_CanvasGroup;
	public StartState(LevelManager loader, CanvasGroup startGroup) 
	{
		m_LevelLoader = loader;
		m_CanvasGroup = startGroup;
	}
}
