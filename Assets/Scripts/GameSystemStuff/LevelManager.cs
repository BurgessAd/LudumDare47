﻿using System.Collections;
using UnityEngine;
using System;
using TMPro;

[RequireComponent(typeof(CustomAnimation))]
public class LevelManager : MonoBehaviour
{
	#region SerializedParams

	[Header("Animation parameters")]
	[SerializeField] private float m_fTransitionTime;
	[SerializeField] private float m_fMenuTransitionTime;

	[Header("Level parameters")]
	[SerializeField] private float m_MapSize;

	[Header("Object references")]
	[SerializeField] private CowGameManager m_Manager;
	[SerializeField] private CustomAnimation m_LevelEnterAnimation;
	[SerializeField] private TextMeshProUGUI m_LevelIntroTextLeft;
	[SerializeField] private TextMeshProUGUI m_LevelIntroTextRight;
	[SerializeField] private Transform m_Transform;
	[SerializeField] private Transform m_CameraTransform;
	[SerializeField] private Transform m_PlayerCameraContainerTransform;

	[Header("Canvas references")]
	[SerializeField] private CanvasGroup m_MainCanvas;
	[SerializeField] private Transform m_ObjectiveCanvasTransform;
	[SerializeField] private CountdownTimerUI m_FinalCountdownTimer;
	[SerializeField] private CountdownTimerUI m_StartCountdownTimer;
	[SerializeField] private CanvasGroup m_StartCountdownCanvas;
	[SerializeField] private CanvasGroup m_PauseCanvas;
	[SerializeField] private CanvasGroup m_EndSuccessCanvas;
	[SerializeField] private CanvasGroup m_EndFailureCanvas;
	[SerializeField] private CanvasGroup m_StartButtonCanvas;
	[SerializeField] private CanvasGroup m_TextCanvas;

	[Header("Animator References")]
	[SerializeField] private Animator m_LevelTransitionAnimator;
	[SerializeField] private Animator m_LevelEnterAnimator;
	[SerializeField] private Animator m_PauseScreenAnimator;
	[SerializeField] private Animator m_EndScreenFailureAnimator;
	[SerializeField] private Animator m_EndScreenSuccesseAnimator;
	#endregion

	#region PrivateParams
	private LevelData m_LevelData;
	private bool m_bIsPaused = false;
	private StateMachine m_LevelState;
	private CanvasGroup m_CurrentOpenCanvas;
	#endregion

	// publicly accessible properties and events
	#region Properties

	public float GetMapRadius => m_MapSize;

	public Vector3 GetMapCentre => m_Transform.position;

	public Transform GetCamTransform => m_CameraTransform;

	public Transform GetObjectiveCanvasTransform => m_ObjectiveCanvasTransform;

	public event Action OnLevelStarted;

	public event Action OnLevelPaused;

	public event Action OnLevelUnpaused;

	public event Action OnLevelFinished;

	#endregion

	// logic controlling the end countdown for a successful level completion
	#region EndCountdown
	public void StartSucceedCountdown()
	{
		int successTime = m_LevelData.GetSuccessTimerTime;
		m_FinalCountdownTimer.ShowTimer();
		m_FinalCountdownTimer.StartTimerFromTime(successTime);
		m_FinalCountdownTimer.OnTimerComplete += OnLevelSucceeded;
	}

	public void EndSucceedCountdown()
	{
		m_FinalCountdownTimer.StopTimer();
	}
	#endregion

	// logic to control the intro animation for levels (zooming in and around the stage before starting)
	#region IntroAnimation
	private void InitializeIntroAnimation()
	{
		switch (m_Manager.GetRestartState)
		{
			case (CowGameManager.RestartState.Debug):
				m_CameraTransform.SetParent(m_Manager.GetPlayerCameraContainerTransform);
				m_CameraTransform.localPosition = Vector3.zero;
				m_CameraTransform.localRotation = Quaternion.identity;
				OnLevelStarted?.Invoke();
				m_LevelState.RequestTransition(typeof(PlayingState));
				break;
			case (CowGameManager.RestartState.Quick):
				m_StartButtonCanvas.alpha = 0.0f;
				m_StartButtonCanvas.blocksRaycasts = false;
				m_StartButtonCanvas.interactable = false;
				m_CameraTransform.GetComponent<CameraStartEndAnimator>().AnimateIn(3.0f);
				StartCoroutine(StartLevelWithoutCountdown());
				break;
			default:
				m_LevelEnterAnimation.AddClipStartedCallbackToClip(0, OnFirstIntroAnimationPortionShown);
				m_LevelEnterAnimation.AddClipStartedCallbackToClip(1, OnSecondIntroAnimationPortionShown);
				m_LevelEnterAnimation.AddClipStartedCallbackToClip(2, OnThirdIntroAnimationPortionShown);
				m_LevelEnterAnimation.AddOnTransitionInCompleteCallbackToClip(3, OnCanStartLevel);
				m_LevelEnterAnimation.StartAnimation();
				break;
		}
	}

	private void ShowIntroText(CustomAnimation.AnimationClip clip) 
	{
		float animInOutTime = 1.0f;
		float animInOutBuffer = 0.1f;
		float lengthCanBeShownFor = Mathf.Max(0, clip.animationTime - clip.entranceAnimationDelay - clip.exitAnimationDelay - clip.entranceAnimationTime/2 - clip.exitAnimationTime - 2 * animInOutTime - 2 * animInOutBuffer);
		LeanTween.cancel(m_TextCanvas.gameObject);
		LeanTween.alphaCanvas(m_TextCanvas, 1.0f, animInOutTime).setDelay(animInOutBuffer + clip.entranceAnimationDelay + clip.entranceAnimationTime/2).setOnComplete(() => LeanTween.alphaCanvas(m_TextCanvas, 0.0f, animInOutTime).setDelay(lengthCanBeShownFor));
	}

	private void OnFirstIntroAnimationPortionShown(CustomAnimation.AnimationClip clip) 
	{
		m_LevelIntroTextLeft.text = "Level " + (m_Manager.GetCurrentLevelIndex + 1).ToString();
		m_LevelIntroTextRight.text = m_LevelData.GetLevelName;
		ShowIntroText(clip);
	}

	private void OnSecondIntroAnimationPortionShown(CustomAnimation.AnimationClip clip)
	{
		m_LevelIntroTextLeft.text = "Time to Beat";
		m_LevelIntroTextRight.text = UnityUtils.TurnTimeToString(m_LevelData.GetTargetTime);
		ShowIntroText(clip);
	}

	private void OnThirdIntroAnimationPortionShown(CustomAnimation.AnimationClip clip)
	{
		m_LevelIntroTextLeft.text = "I dont know what to put here.";
		m_LevelIntroTextRight.text = "Difficulty?";
		ShowIntroText(clip);
	}

	private void OnCanStartLevel(CustomAnimation.AnimationClip _)
	{
		m_CameraTransform.SetParent(m_Manager.GetPlayerCameraContainerTransform);
		m_CameraTransform.localPosition = Vector3.zero;
		m_CameraTransform.localRotation = Quaternion.identity;
		OnLevelStarted?.Invoke();
		m_LevelState.RequestTransition(typeof(PlayingState));
	}

	private IEnumerator StartLevelWithoutCountdown()
	{
		yield return new WaitForSeconds(3.0f);
		OnLevelStarted?.Invoke();
		m_LevelState.RequestTransition(typeof(PlayingState));
	}
	#endregion

	// Start, Update, Awake
	#region UnityFunctions

	private void Awake()
	{
		m_LevelState = new StateMachine(new StartState(this, m_StartButtonCanvas));
		m_LevelState.AddState(new PausedState(this, m_PauseCanvas, m_PauseScreenAnimator));
		m_LevelState.AddState(new EndFailureState(this, m_EndFailureCanvas, m_EndScreenFailureAnimator));
		m_LevelState.AddState(new EndSuccessState(this, m_EndSuccessCanvas, m_EndScreenSuccesseAnimator));
		m_LevelState.AddState(new PlayingState(this, m_MainCanvas));
		m_LevelTransitionAnimator.Play("Base Layer.TransitionIn");

		PauseLevel(true);
		m_Manager.NewLevelLoaded(this);
	}

	private void Start()
	{
		m_LevelState.InitializeStateMachine();
		m_CameraTransform = m_Manager.GetCameraTransform;
		InitializeIntroAnimation();
	}

	private void Update() 
	{
		m_LevelState.Tick();
	}

	#endregion

	// Logic to set/unset certain gameplay canvases and start scene transitions out (they automatically start in)
	#region CanvasSceneFunctions

	private IEnumerator BeginSceneTransition(Action queuedOnFinish)
	{
		m_LevelTransitionAnimator.Play("TransitionOut", -1);
		yield return new WaitForSeconds(m_fTransitionTime);
		m_Manager.ClearLevelData();
		queuedOnFinish();
	}

	public void SetCurrentCanvas(CanvasGroup canvas, Action callOnComplete, in float delay = 0.0f) 
	{
		ClearCanvas();
		LeanTween.alphaCanvas(canvas, 1.0f, m_fMenuTransitionTime).setEaseInOutCubic().setOnComplete(callOnComplete).setDelay(delay);
		m_CurrentOpenCanvas = canvas;
	}

	public void SetCurrentCanvas(CanvasGroup canvas, in float delay = 0.0f)
	{
		ClearCanvas();
		LeanTween.alphaCanvas(canvas, 1.0f, m_fMenuTransitionTime).setEaseInOutCubic().setDelay(delay);
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

	#endregion

	// Functions called primarily by player UI requests, sent up to manager
	#region UILevelEvents
	public void LoadNextLevel()
	{
		StartCoroutine(BeginSceneTransition(() => m_Manager.MoveToNextLevel()));
	}

	public void RestartLevel()
	{
		StartCoroutine(BeginSceneTransition(() => m_Manager.RestartCurrentLevel()));
	}

	public void LoadMenu()
	{
		StartCoroutine(BeginSceneTransition(() => m_Manager.MoveToMenu()));
	}
	#endregion

	// Functions called primarily by the manager, for defining animations before exiting levels, etc
	#region ManagerLevelEvents

	public void PauseLevel(bool shouldPause)
	{
		if (m_bIsPaused != shouldPause)
		{
			if (shouldPause)
			{
				OnLevelPaused?.Invoke();
			}
			else
			{
				OnLevelUnpaused?.Invoke();
			}
			m_Manager.SetPausedState(shouldPause);
			m_bIsPaused = shouldPause;
		}
	}

	public void SetLevelData(LevelData levelData)
	{
		m_LevelData = levelData;
	}


	public void OnLevelSucceeded() 
	{
		OnLevelFinished();
		m_CameraTransform.GetComponent<CameraStartEndAnimator>().AnimateOut();
		m_LevelState.RequestTransition(typeof(EndSuccessState)); 
	}

	public void OnLevelFailed() 
	{
		OnLevelFinished();
		m_CameraTransform.GetComponent<CameraStartEndAnimator>().AnimateOut();
		m_LevelState.RequestTransition(typeof(EndFailureState));
	}

	public void ResumeLevel()
	{
		m_LevelState.RequestTransition(typeof(PlayingState));
	}

	public void PlayerStartedLevel() 
	{
		SetCurrentCanvas(m_StartCountdownCanvas, () => { });
		m_CameraTransform.GetComponent<CameraStartEndAnimator>().AnimateIn(3.2f);
		m_FinalCountdownTimer.StartTimerFromTime(3);
		m_FinalCountdownTimer.ShowTimer();
		m_FinalCountdownTimer.OnTimerComplete += () => OnLevelStarted?.Invoke(); m_LevelState.RequestTransition(typeof(PlayingState));
	}

	#endregion

	// Functions called by the state machine
	#region StateMachineEvents

	public void PopulateFailureScreen()
	{
		// Time 
		// objective statistics

		// need to populate objectives using UIObjectiveElements
	}

	public void PopulateSuccessScreen()
	{
		// Time completed
		// objective statistics 
		// Points (?) - speed, efficiency (how close to optimum the scores were)
		// star rating (?)
	}

	#endregion

}

#region StateMachineStates
public class PlayingState : AStateBase 
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
		m_LevelLoader.SetCurrentCanvas(m_CanvasGroup, () => 
		{
			m_CanvasGroup.blocksRaycasts = true;
			m_CanvasGroup.interactable = true;
		});
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

public class PausedState : AStateBase 
{
	private readonly LevelManager m_LevelLoader;
	private readonly CanvasGroup m_CanvasGroup;
	private readonly Animator m_AnimationController;
	public PausedState(LevelManager loader, CanvasGroup pauseGroup, Animator animator)
	{
		m_AnimationController = animator;
		m_LevelLoader = loader;
		m_CanvasGroup = pauseGroup;
	}

	public override void OnEnter()
	{
		m_LevelLoader.SetCurrentCanvas(m_CanvasGroup, () => 
		{
			m_CanvasGroup.blocksRaycasts = true;
			m_CanvasGroup.interactable = true;
		});
		m_AnimationController.Play("AnimIn", -1);
	}

	public override void Tick()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			m_LevelLoader.ResumeLevel();
		}
	}

	public override void OnExit()
	{
		m_AnimationController.Play("AnimOut", -1);
	}
}

public class EndFailureState : AStateBase 
{
	private readonly LevelManager m_LevelLoader;
	private readonly CanvasGroup m_CanvasGroup;
	private readonly Animator m_AnimationController;
	public EndFailureState(LevelManager loader, CanvasGroup pauseGroup, Animator animator)
	{
		m_AnimationController = animator;
		m_LevelLoader = loader;
		m_CanvasGroup = pauseGroup;
	}
	public override void OnEnter()
	{
		m_LevelLoader.SetCurrentCanvas(m_CanvasGroup, () => 
		{
			m_CanvasGroup.blocksRaycasts = true;
			m_CanvasGroup.interactable = true;
		}, delay: 1.0f);
		m_LevelLoader.PopulateFailureScreen();
		m_AnimationController.Play("AnimIn", -1);
	}

	public override void OnExit()
	{
		m_AnimationController.Play("AnimOut", -1);
	}
}

public class EndSuccessState : AStateBase
{
	private readonly LevelManager m_LevelLoader;
	private readonly CanvasGroup m_CanvasGroup;
	private readonly Animator m_AnimationController;

	public EndSuccessState(LevelManager loader, CanvasGroup pauseGroup, Animator animator)
	{
		m_AnimationController = animator;
		m_LevelLoader = loader;
		m_CanvasGroup = pauseGroup;
	}

	public override void OnEnter()
	{
		m_LevelLoader.SetCurrentCanvas(m_CanvasGroup, () => 
		{
			m_CanvasGroup.blocksRaycasts = true;
			m_CanvasGroup.interactable = true;
		}, delay: 1.0f);
		m_LevelLoader.PopulateSuccessScreen();
		m_AnimationController.Play("AnimIn", -1);
	}

	public override void OnExit()
	{
		m_AnimationController.Play("AnimOut", -1);
	}
}

public class StartState : AStateBase 
{
	private readonly LevelManager m_LevelLoader;
	private readonly CanvasGroup m_CanvasGroup;
	public StartState(LevelManager loader, CanvasGroup startGroup) 
	{
		m_LevelLoader = loader;
		m_CanvasGroup = startGroup;
	}
}
#endregion
