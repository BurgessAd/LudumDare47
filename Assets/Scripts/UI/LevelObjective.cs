using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

	public EntityInformation GetEntityInformation => m_CounterAnimalType;
	public ObjectiveType GetObjectiveType => m_ObjectiveType;

	private IObjectiveListener m_ObjectiveListener;




	private bool m_bIsCurrentlyFailing = false;
	private bool m_bIsCurrentlyWithinGoal = false;
	private IEnumerator m_FailureTimerCoroutine = default;
	private int m_CurrentTimer = 0;
	private int m_InternalCounterVal = 0;

	public float GetStartGoalPos => (float)(m_MinimumGoal - m_MinimumValue) / (m_MaximumValue - m_MinimumValue);
	public float GetEndGoalPos => (float)(m_MaximumGoal - m_MinimumValue) / (m_MaximumValue - m_MinimumValue);

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


	public void IncrementCounter()
	{
		m_InternalCounterVal = Mathf.Min(m_InternalCounterVal + 1, m_MaximumValue);
		m_ObjectiveListener.OnCounterChanged(m_InternalCounterVal);
		CheckChanged();
	}

	public void DecrementCounter()
	{
		m_InternalCounterVal = Mathf.Max(m_InternalCounterVal - 1, m_MinimumValue);
		m_ObjectiveListener.OnCounterChanged(m_InternalCounterVal);
		CheckChanged();
	}


	private IEnumerator FailureTimer()
	{

		while (m_CurrentTimer > 0)
		{
			m_CurrentTimer++;
			m_ObjectiveListener.OnTimerValueShown(m_CurrentTimer, (float)m_CurrentTimer / m_TimerMaximum);
			yield return new WaitForSecondsRealtime(1.0f);
		}
		m_Manager.OnObjectiveFailure();
	}

	private void StartFailureTimer()
	{
		m_FailureTimerCoroutine = FailureTimer();
		m_ObjectiveListener.OnTimerTriggered(m_FailureTimerCoroutine);
	}

	private void HaltFailureTimer()
	{
		m_ObjectiveListener.OnTimerRemoved(m_FailureTimerCoroutine);
	}

	private void EnteredGoal()
	{
		m_ObjectiveListener.OnEnteredGoal();
		m_Manager.OnObjectiveCompleted();
	}

	private void LeftGoal()
	{
		m_ObjectiveListener.OnLeftGoal();
		m_Manager.OnObjectiveUncompleted();
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
}

public interface IObjectiveListener
{
	void OnCounterChanged(in int val);

	void OnTimerTriggered(IEnumerator timerFunc);

	void OnTimerValueShown(in int val, in float through);

	void OnTimerRemoved(IEnumerator timerFunc);

	void OnEnteredGoal();

	void OnLeftGoal();

	void InitializeData(LevelObjective objective);
}
