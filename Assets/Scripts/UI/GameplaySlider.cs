using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplaySlider : MonoBehaviour
{
	[SerializeField] private bool m_HasMinimumFailure;
	[SerializeField] private int m_MinimumFailure;
	[SerializeField] private bool m_HasMaximumFailure;
	[SerializeField] private int m_MaximumFailure;
	[SerializeField] private int m_MinimumGoal;
	[SerializeField] private int m_MaximumGoal;

	[SerializeField] private RectTransform m_GoalImage;
	[SerializeField] private RectTransform m_FailImage;

	[SerializeField] private Slider m_Slider;

	private int m_SliderAreaMinimum;
	private int m_SliderAreaMaximum;

	private float m_fCurrentSliderPosition;
	private float m_fCurrentSliderVelocity;
	[SerializeField] private float m_fSliderAcceleration;
	private int m_InternalCounterVal;

	private void OnValidate()
	{
		// its only valid to have the maximum greater than the minimum in both cases
		m_MaximumFailure = Mathf.Max(m_MaximumFailure, m_MinimumFailure);
		m_MaximumGoal = Mathf.Max(m_MaximumGoal, m_MinimumGoal);

		// it's only valid to have the goal within the minimum
		if (m_HasMinimumFailure)
			m_MinimumGoal = Mathf.Max(m_MinimumFailure, m_MinimumGoal);
		if (m_HasMaximumFailure)
			m_MaximumGoal = Mathf.Min(m_MaximumGoal, m_MaximumFailure);
		if (!m_HasMinimumFailure || !m_HasMaximumFailure)
			Debug.LogWarningFormat("Gameplay UI %s has no failure states", gameObject.name);
		m_SliderAreaMinimum = m_HasMinimumFailure ? m_MinimumFailure : m_MinimumGoal;
		m_SliderAreaMaximum = m_HasMaximumFailure ? m_MaximumFailure : m_MaximumGoal;

		UpdateUI();
	}

	private void UpdateUI()
	{
		int min = m_SliderAreaMinimum;
		int max = m_SliderAreaMaximum;
		int sliderExtent = max - min;

		// we want the goal counter to scale via this slider extent scalar.
		float goalAnchorXMin = (float)(m_MinimumGoal - min) / sliderExtent;
		float goalAnchorXMax = (float)(m_MaximumGoal - min) / sliderExtent;

		m_GoalImage.anchorMax = new Vector2(goalAnchorXMax, m_GoalImage.anchorMax.y);
		m_GoalImage.anchorMin = new Vector2(goalAnchorXMin, m_GoalImage.anchorMin.y);
	}

	private void OnIncrementCounter()
	{
		m_InternalCounterVal = Mathf.Min(m_InternalCounterVal + 1, m_SliderAreaMaximum);
	}

	private void OnDecrementCounter()
	{
		m_InternalCounterVal = Mathf.Max(m_InternalCounterVal - 1, m_SliderAreaMinimum);
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		float targetSliderPosition = (m_InternalCounterVal - m_SliderAreaMinimum) / (m_SliderAreaMaximum - m_SliderAreaMinimum);
		m_fCurrentSliderPosition = Mathf.SmoothDamp(m_fCurrentSliderPosition, targetSliderPosition, ref m_fCurrentSliderVelocity, 1 / m_fSliderAcceleration);
		m_Slider.normalizedValue = m_fCurrentSliderPosition;

		//TODO: add gameplay logic such that failure occurs if slider is at edge AND in failure
		if (HasFailureChanged())
		{
			DoFailureTick();
		}
	}

	bool m_bIsCurrentlyFailing = false;

	private void DoFailureTick() { }

	private bool HasFailureChanged()
	{
		if (m_HasMaximumFailure)
		{
			if (m_InternalCounterVal == m_MaximumFailure) return true;
		}
		else if (m_HasMinimumFailure)
		{
			if  (m_InternalCounterVal == m_MinimumFailure) return true;
		}
		return false;
	}
}
