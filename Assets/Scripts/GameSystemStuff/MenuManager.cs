using System.Collections;
using UnityEngine;
using System;
using MenuManagerStates;
using UnityWeld.Binding;
using System.Collections.Generic;
using TMPro;

[Binding]
public class MenuManager : MonoBehaviour
{
	[Header("Animation parameters")]
	[SerializeField] private float m_fTransitionTime;
	[SerializeField] private float m_fMenuTransitionTime;

	[Header("Object references")]
	[SerializeField] private CowGameManager m_Manager;
	[SerializeField] private Transform m_LevelSelectTabsTransform;

	[Header("Canvas references")]
	[SerializeField] private CanvasGroup m_MainCanvas;
	[SerializeField] private CanvasGroup m_SettingsCanvas;
	[SerializeField] private CanvasGroup m_LevelSelectCanvas;
	[SerializeField] private CanvasGroup m_QuitCanvas;

	[SerializeField] private List<CanvasGroup> m_MenuButtons;
	[SerializeField] private List<Animator> m_SettingsAnimators;

	[SerializeField] private GameObject m_LevelSelectUIPrefab;

	[Header("Animator References")]
	[SerializeField] private Animator m_LevelTransitionAnimator;
	[SerializeField] private Animator m_MainScreenAnimator;

	private StateMachine m_MenuStateMachine;
	private CanvasGroup m_CurrentOpenCanvas;

	public Transform GetLevelSelectTabsTransform => m_LevelSelectTabsTransform;

	#region UnityFunctions

	void Awake()
    {
		m_MenuStateMachine = new StateMachine(new MenuManagerStates.MainState(this, m_MainCanvas, m_MainScreenAnimator));
		m_MenuStateMachine.AddState(new MenuManagerStates.LevelSelectState(this, m_LevelSelectCanvas, m_MainScreenAnimator));
		m_MenuStateMachine.AddState(new MenuManagerStates.SettingsState(this, m_SettingsCanvas, m_MainScreenAnimator));
		m_MenuStateMachine.AddState(new MenuManagerStates.PreQuitState(this, m_QuitCanvas, m_MainScreenAnimator));
		m_MenuStateMachine.InitializeStateMachine();

		for (int i = 0; i < m_Manager.GetLevelData.Count; i++)
		{
			LevelData levelData = m_Manager.GetLevelData[i];
			GameObject go = Instantiate(m_LevelSelectUIPrefab, m_LevelSelectTabsTransform);
			LevelDataUI levelUI = go.GetComponent<LevelDataUI>();
			levelUI.SetupData(levelData);
			levelUI.OnSelectLevel += () => OnRequestLevel(i);
		}
	}

    void Update()
    {
		m_MenuStateMachine.Tick();
    }

	public void ShowMenuStuff(bool shouldShow)
	{
		StartCoroutine(ChangeMenuCoroutine(shouldShow));
	}

	private IEnumerator ChangeMenuCoroutine(bool menuState)
	{
		for (int i = 0; i < m_MenuButtons.Count; i++)
		{
			m_MenuButtons[i].interactable = menuState;
			yield return new WaitForSecondsRealtime(0.15f);
		}
	}

	public void ShowSettingsStuff(bool shouldShow)
	{
		StartCoroutine(ChangeSettingsCoroutine(shouldShow));
	}

	private IEnumerator ChangeSettingsCoroutine(bool settingsState)
	{
		string toPlay = settingsState ? "AnimIn" : "AnimOut";
		for (int i = 0; i < m_SettingsAnimators.Count; i++)
		{
			m_SettingsAnimators[i].Play(toPlay, -1);
			yield return new WaitForSecondsRealtime(0.15f);
		}
	}

	#endregion

	#region CanvasSceneFunctions

	private IEnumerator BeginSceneTransition(Action queuedOnFinish)
	{
		m_LevelTransitionAnimator.Play("TransitionOut", -1);
		yield return new WaitForSeconds(m_fTransitionTime);
		m_Manager.ClearLevelData();
		queuedOnFinish();
	}
	#endregion

	#region UIFunctions

	public void OpenSettingsMenu()
	{
		m_MenuStateMachine.RequestTransition(typeof(SettingsState));
	}

	public void OpenLevelSelect()
	{
		m_MenuStateMachine.RequestTransition(typeof(LevelSelectState));
	}

	public void ReturnToMainMenu()
	{
		m_MenuStateMachine.RequestTransition(typeof(MainState));
	}

	public void OpenQuitScreen()
	{
		m_MenuStateMachine.RequestTransition(typeof(PreQuitState));
	}

	public void Quit()
	{
		Application.Quit(0);
	}

	private int levelIdChosen = 1;

	private void OnRequestLevel(int levelId)
	{
		levelIdChosen = levelId;
	}

	public void OnClickPlay()
	{
		m_CurrentOpenCanvas.blocksRaycasts = false;
		m_CurrentOpenCanvas.interactable = false;
		StartCoroutine(BeginSceneTransition(() => m_Manager.MoveToLevelWithId(levelIdChosen)));
	}

	#endregion
}

namespace MenuManagerStates
{
	public class SettingsState : AStateBase
	{
		private readonly Animator m_Animator;
		private readonly MenuManager m_MenuManager;
		private readonly CanvasGroup m_CanvasGroup;
		public SettingsState(MenuManager menuManager, CanvasGroup settingsGroup, Animator animator)
		{
			m_MenuManager = menuManager;
			m_CanvasGroup = settingsGroup;
			m_Animator = animator;
		}
		public override void OnEnter()
		{
			m_Animator.Play("AnimSettingsIn", -1);
		}

		public override void OnExit()
		{
			m_Animator.Play("AnimSettingsOut", -1);
			m_MenuManager.ShowSettingsStuff(false);
		}

		public override void Tick()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				RequestTransition<MenuManagerStates.MainState>();
			}
		}
	}

	public class MainState : AStateBase
	{
		private readonly Animator m_Animator;
		private readonly MenuManager m_MenuManager;
		private readonly CanvasGroup m_CanvasGroup;

		public MainState(MenuManager menuManager, CanvasGroup levelSelectGroup, Animator animator)
		{
			m_MenuManager = menuManager;
			m_CanvasGroup = levelSelectGroup;
			m_Animator = animator;
		}

		public override void OnEnter()
		{
			m_MenuManager.ShowMenuStuff(true);
		}

		public override void OnExit()
		{
			m_MenuManager.ShowMenuStuff(false);	
		}
	}

	public class PreQuitState : AStateBase
	{
		private readonly MenuManager m_MenuManager;
		private readonly CanvasGroup m_CanvasGroup;
		private readonly Animator m_Animator;

		public PreQuitState(MenuManager menuManager, CanvasGroup levelSelectGroup, Animator animator)
		{
			m_MenuManager = menuManager;
			m_CanvasGroup = levelSelectGroup;
			m_Animator = animator;
		}

		public override void OnEnter()
		{
			m_Animator.Play("AnimQuitIn", -1);
		}

		public override void OnExit()
		{
			m_Animator.Play("AnimQuitOut", -1);
		}

		public override void Tick()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				RequestTransition<MenuManagerStates.MainState>();
			}
		}
	}

	public class LevelSelectState : AStateBase
	{
		private readonly Animator m_Animator;
		private readonly MenuManager m_MenuManager;
		private readonly CanvasGroup m_CanvasGroup;

		public LevelSelectState(MenuManager menuManager, CanvasGroup levelSelectGroup, Animator animator)
		{
			m_MenuManager = menuManager;
			m_CanvasGroup = levelSelectGroup;
			m_Animator = animator;
		}
		public override void OnEnter()
		{
			m_Animator.Play("AnimLevelsIn", -1);
		}

		public override void OnExit()
		{
			m_Animator.Play("AnimLevelsOut", -1);
		}

		public override void Tick()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				RequestTransition<MenuManagerStates.MainState>();
			}
		}
	}
}
