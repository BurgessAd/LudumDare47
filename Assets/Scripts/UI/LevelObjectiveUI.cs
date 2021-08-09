using UnityEngine;
using UnityEngine.UI;
using System;

public class LevelObjectiveUI : MonoBehaviour, IObjectiveListener
{
	[Header("Internal References")]
	[SerializeField] private RectTransform m_GoalImage;
	[SerializeField] private RectTransform m_FailureImage;
	[SerializeField] private Slider m_Slider;
	[SerializeField] private RectTransform m_SliderBackgroundRect;
	[SerializeField] private Image m_SliderBackgroundImage;
	[SerializeField] private CountdownTimerUI m_CountdownTimer;

	[Header("Audio Settings")]
	[SerializeField] private string m_EnterGoalZoneAudioIdentifier;
	[SerializeField] private string m_ExitGoalZoneAudioIdentifier;
	[SerializeField] private AudioManager m_AudioManager;

	[Header("Animation Settings")]
	[SerializeField] private float m_fSliderAcceleration;
	[SerializeField] private Color m_EnterGoalPulseColour;
	[SerializeField] private Color m_ExitGoalPulseColour;
	[SerializeField] [Range(0.0f, 0.2f)] private float m_FailureEndBarSize;

	private float m_fCurrentSliderPosition;
	private float m_fCurrentSliderVelocity;
	private Color m_InitialBackgroundColor = default;
	private int m_PulseAnimationId = 0;

	#region UnityFunctions

	private void Awake()
	{
		m_InitialBackgroundColor = m_SliderBackgroundImage.color;
	}

	void Update()
	{
		m_fCurrentSliderPosition = Mathf.SmoothDamp(m_fCurrentSliderPosition, counterPos, ref m_fCurrentSliderVelocity, 1 / m_fSliderAcceleration);
		m_Slider.normalizedValue = m_fCurrentSliderPosition;
	}

	#endregion

	// Function implementations of IObjectiveListener
	#region IObjectiveListener

	public void OnCounterChanged(in int val)
	{
		counterPos = val;
	}

	private float counterPos = 0.0f;

	public void OnTimerTriggered(in Action callOnComplete, in int time)
	{
		m_CountdownTimer.ShowTimer();
		m_CountdownTimer.StartTimerFromTime(time);
		m_CountdownTimer.OnTimerComplete += callOnComplete;
	}

	public void OnTimerRemoved()
	{
		m_CountdownTimer.StopTimer();
		m_SliderBackgroundImage.color = m_EnterGoalPulseColour;
		LeanTween.color(m_SliderBackgroundRect, m_InitialBackgroundColor, 0.5f);
	}

	public void OnEnteredGoal()
	{
		m_AudioManager.Play(m_EnterGoalZoneAudioIdentifier);
		PulseBackground(m_EnterGoalPulseColour);
	}

	public void OnLeftGoal()
	{
		m_AudioManager.Play(m_ExitGoalZoneAudioIdentifier);
		PulseBackground(m_ExitGoalPulseColour);
	}

	public void InitializeData(LevelObjective objective)
	{
		return;
		float goalAnchorXMin = objective.GetStartGoalPos;
		float goalAnchorXMax = objective.GetEndGoalPos;

		m_GoalImage.anchorMin = new Vector2(goalAnchorXMin, m_GoalImage.anchorMin.y);
		m_GoalImage.anchorMax = new Vector2(goalAnchorXMax, m_GoalImage.anchorMax.y);

		m_FailureImage.anchorMin = new Vector2(1 - m_FailureEndBarSize, m_FailureImage.anchorMin.y);
		m_FailureImage.anchorMax = new Vector2(m_FailureEndBarSize, m_FailureImage.anchorMax.y);
	}

	public void OnObjectiveFailed()
	{

	}
	#endregion

	#region MiscellaneousHelperFunctions

	private void PulseBackground(in Color pulseColor)
	{
		LeanTween.cancel(m_PulseAnimationId);
		m_SliderBackgroundImage.color = pulseColor;
		m_PulseAnimationId = LeanTween.color(m_SliderBackgroundRect, m_InitialBackgroundColor, 0.5f).uniqueId;
	}

	#endregion
}
