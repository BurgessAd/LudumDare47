using UnityEngine;
using System;

public enum ObjectiveType
{
	Population,
	Capturing
}

public class LevelObjective : ScriptableObject
{
	[Header("Gameplay Settings")]
	[SerializeField] private bool m_HasMinimumFailure;
	[SerializeField] private int m_MinimumValue;
	[SerializeField] private bool m_HasMaximumFailure;
	[SerializeField] private int m_MaximumValue;
	[SerializeField] private int m_MinimumGoal;
	[SerializeField] private int m_MaximumGoal;
	[SerializeField] private int m_TimerMaximum;
	[SerializeField] private ObjectiveType m_ObjectiveType;

	[Header("Static References")]
	[SerializeField] private CowGameManager m_Manager;
	[SerializeField] private EntityInformation m_CounterAnimalType;

	private UnityUtils.ListenerSet<IObjectiveListener> m_ObjectiveListeners;

	// Properties for external access of data by the game manager
	#region AccessibleProperties

	public EntityInformation GetEntityInformation => m_CounterAnimalType;
	public ObjectiveType GetObjectiveType => m_ObjectiveType;

	public float GetStartGoalPos => (float)(m_MinimumGoal - m_MinimumValue) / (m_MaximumValue - m_MinimumValue);
	public float GetEndGoalPos => (float)(m_MaximumGoal - m_MinimumValue) / (m_MaximumValue - m_MinimumValue);

	#endregion

	#region UnityFunctions
	private void Awake()
	{
		OnValidate();
	}

	private void OnValidate()
	{
		// its only valid to have the maximum greater than the minimum in both cases
		m_MaximumValue = Mathf.Max(m_MaximumValue, m_MinimumValue);
		m_MaximumGoal = Mathf.Max(m_MaximumGoal, m_MinimumGoal);

		// it's only valid to have the goal within the minimum
		if (m_HasMinimumFailure)
			m_MinimumGoal = Mathf.Max(m_MinimumValue, m_MinimumGoal);
		if (m_HasMaximumFailure)
			m_MaximumGoal = Mathf.Min(m_MaximumGoal, m_MaximumValue);
		if (!m_HasMinimumFailure || !m_HasMaximumFailure)
			Debug.LogErrorFormat("Gameplay UI %s has no failure states", name);
		if ((!m_HasMinimumFailure &&  m_MinimumValue == m_MinimumGoal)|| (!m_HasMaximumFailure && m_MaximumValue == m_MaximumGoal))
			Debug.LogErrorFormat("Gameplay UI %s failure state is the same as the goal minimum - they should be at least slightly different", name);
	}
	#endregion

	// pretty much entirely called by the game manager
	#region PublicFunctions

	public void AddObjectiveListener(IObjectiveListener listener)
	{
		m_ObjectiveListeners.Add(listener);
		listener.InitializeData(this);
	}

	public void RemoveObjectiveListener(IObjectiveListener listener)
	{
		m_ObjectiveListeners.Remove(listener);
	}

	public void ClearListeners()
	{
		m_ObjectiveListeners.Clear();
	}

	public void IncrementCounter()
	{
		m_InternalCounterVal = Mathf.Min(m_InternalCounterVal + 1, m_MaximumValue);
		m_ObjectiveListeners.ForEachListener((IObjectiveListener listener) => listener.OnCounterChanged(m_InternalCounterVal));
		CheckChanged();
	}

	public void DecrementCounter()
	{
		m_InternalCounterVal = Mathf.Max(m_InternalCounterVal - 1, m_MinimumValue);
		m_ObjectiveListeners.ForEachListener((IObjectiveListener listener) => listener.OnCounterChanged(m_InternalCounterVal));
		CheckChanged();
	}

	#endregion

	// internal functions for setting UI and telling the game manager what state this objective is in
	#region PrivateFunctions

	private bool m_bIsCurrentlyFailing = false;
	private bool m_bIsCurrentlyWithinGoal = false;
	private int m_InternalCounterVal = 0;

	// for each listener, trigger the timer (only one will actually start a timer)
	// and tell it to trigger ObjectiveFailed at the end of it.
	private void StartFailureTimer()
	{
		m_ObjectiveListeners.ForEachListener( (IObjectiveListener listener) => {
			listener.OnTimerTriggered(OnObjectiveFailed, m_TimerMaximum);
		});
	}

	private void OnObjectiveFailed()
	{
		m_ObjectiveListeners.ForEachListener((IObjectiveListener listener) => {
			listener.OnObjectiveFailed();
		});
	}

	private void HaltFailureTimer()
	{
		m_ObjectiveListeners.ForEachListener((IObjectiveListener listener) => listener.OnTimerRemoved());
	}

	private void EnteredGoal()
	{
		m_ObjectiveListeners.ForEachListener((IObjectiveListener listener) => listener.OnEnteredGoal());
	}

	private void LeftGoal()
	{
		m_ObjectiveListeners.ForEachListener((IObjectiveListener listener) => listener.OnLeftGoal());
	}

	private void CheckChanged()
	{
		bool withinGoal = false;
		bool withininFailure = false;

		if (m_InternalCounterVal > m_MinimumGoal || m_InternalCounterVal < m_MaximumGoal)
			withinGoal = true;
		if ((m_HasMaximumFailure && m_InternalCounterVal == m_MaximumValue) || (m_HasMinimumFailure && m_InternalCounterVal == m_MinimumValue))
			withininFailure = true;

		if (withinGoal != m_bIsCurrentlyWithinGoal)
		{
			if (withinGoal)
			{
				EnteredGoal();
			}
			else
			{
				LeftGoal();
			}
			m_bIsCurrentlyWithinGoal = withinGoal;
		}

		if (withininFailure != m_bIsCurrentlyFailing)
		{
			if (withininFailure)
			{
				StartFailureTimer();
			}
			else
			{
				HaltFailureTimer();
			}
			m_bIsCurrentlyFailing = withininFailure;
		}
	}

	#endregion
}

public interface IObjectiveListener
{
	void OnCounterChanged(in int val);

	void OnTimerTriggered( in Action totalTime, in int time);

	void OnTimerRemoved();

	void OnEnteredGoal();

	void OnLeftGoal();

	void OnObjectiveFailed();

	void InitializeData(LevelObjective objective);
}
