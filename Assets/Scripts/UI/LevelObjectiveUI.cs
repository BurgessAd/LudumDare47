using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelObjectiveUI : MonoBehaviour, IObjectiveListener
{
    // Start is called before the first frame update
    void Start()
    {
        
    }


	[Header("Internal References")]
	[SerializeField] private RectTransform m_GoalImage;
	[SerializeField] private Slider m_Slider;
	[SerializeField] private Image m_SliderHandleImage;
	[SerializeField] private RectTransform m_SliderBackgroundRect;
	[SerializeField] private Image m_SliderBackgroundImage;
	[SerializeField] private CanvasGroup m_TextCanvasGroup;
	[SerializeField] private Text m_TimerText;
	[SerializeField] private RectTransform m_TimerRect;

	[Header("Audio Settings")]
	[SerializeField] private string m_TimerTickAudioIdentifier;
	[SerializeField] private string m_EnterGoalZoneAudioIdentifier;
	[SerializeField] private string m_ExitGoalZoneAudioIdentifier;
	[SerializeField] private AudioManager m_AudioManager;

	[Header("Animation Settings")]
	[SerializeField] private AnimationCurve m_TextPulseSizeByTimer;
	[SerializeField] private float m_fSliderAcceleration;
	[SerializeField] private Color m_EnterGoalPulseColour;
	[SerializeField] private Color m_ExitGoalPulseColour;
	[SerializeField] private float m_FailureEndBarSize;

	private LevelObjective m_LevelObjectiveData;

	private float m_fCurrentSliderPosition;
	private float m_fCurrentSliderVelocity;

	private Vector2 m_InitialTextSize = default;
	private Color m_InitialBackgroundColor = default;

	private int m_PulseAnimationId = 0;

	private void Awake()
	{
		m_InitialTextSize = m_TimerRect.sizeDelta;
		m_InitialBackgroundColor = m_SliderBackgroundImage.color;
	}

	public void Construct(in LevelObjective levelObjectiveData)
	{
		m_LevelObjectiveData = levelObjectiveData;
		UpdateUI();
	}

	private void UpdateUI()
	{
		// we want the goal counter to scale via this slider extent scalar.

	}

	void Update()
	{
		m_fCurrentSliderPosition = Mathf.SmoothDamp(m_fCurrentSliderPosition, counterPos, ref m_fCurrentSliderVelocity, 1 / m_fSliderAcceleration);
		m_Slider.normalizedValue = m_fCurrentSliderPosition;
	}

	private void PulseBackground(in Color pulseColor)
	{
		LeanTween.cancel(m_PulseAnimationId);
		m_SliderBackgroundImage.color = pulseColor;
		m_PulseAnimationId = LeanTween.color(m_SliderBackgroundRect, m_InitialBackgroundColor, 0.5f).uniqueId;
	}

	public void OnCounterChanged(in int val)
	{
		counterPos = val;
	}

	private float counterPos = 0.0f;

	public void OnTimerTriggered(IEnumerator input)
	{
		LeanTween.alphaCanvas(m_TextCanvasGroup, 1.0f, 1.0f).setEaseInCubic();
		StartCoroutine(input);
	}

	public void OnTimerValueShown(in int val, in float through)
	{
		float pulseSizeMult = m_TextPulseSizeByTimer.Evaluate(through);
		m_TimerRect.sizeDelta = m_InitialTextSize * pulseSizeMult;
		LeanTween.size(m_TimerRect, m_InitialTextSize, 1.0f).setEaseOutCubic();
	}

	public void OnTimerRemoved(IEnumerator input)
	{
		StopCoroutine(input);
		m_SliderBackgroundImage.color = m_EnterGoalPulseColour;
		LeanTween.color(m_SliderBackgroundRect, m_InitialBackgroundColor, 0.5f);
		LeanTween.alphaCanvas(m_TextCanvasGroup, 0.0f, 1.0f).setEaseInCubic();
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
		float goalAnchorXMin = objective.GetStartGoalPos;
		float goalAnchorXMax = objective.GetEndGoalPos;

		m_GoalImage.anchorMax = new Vector2(goalAnchorXMax, m_GoalImage.anchorMax.y);
		m_GoalImage.anchorMin = new Vector2(goalAnchorXMin, m_GoalImage.anchorMin.y);
	}
}
