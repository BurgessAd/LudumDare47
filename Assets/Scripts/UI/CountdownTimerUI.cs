using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class CountdownTimerUI : MonoBehaviour
{
	[Header("Internal References")]
	[SerializeField] private TextMeshProUGUI m_TimerText;
	[SerializeField] private RectTransform m_TimerRect;
	[SerializeField] private CanvasGroup m_TextCanvasGroup;
	[SerializeField] private AudioManager m_AudioManager;

	[Header("Animation and Audio references")]
	[SerializeField] private string m_TimerCompleteAudioIdentifier;
	[SerializeField] private string m_TimerTickAudioIdentifier;
	[SerializeField] private AnimationCurve m_TextPulseSizeByTimer;
	[SerializeField] private float m_TimerFadeTime;
	// Start is called before the first frame update
	private int m_CurrentTime;
	private IEnumerator m_TimerCoroutine;
	private Vector2 m_InitialTextSize = default;
	public event Action OnTimerComplete;

	private void Awake()
	{
		m_InitialTextSize = m_TimerRect.sizeDelta;
	}

	public void ShowTimer()
	{
		LeanTween.alphaCanvas(m_TextCanvasGroup, 1.0f, m_TimerFadeTime).setEaseInCubic();
	}

	public void StartTimerFromTime(in float time)
    {
		m_TimerCoroutine = StartTimer(time);
		StartCoroutine(m_TimerCoroutine);
    }

	public void StopTimer()
	{
		LeanTween.alphaCanvas(m_TextCanvasGroup, 0.0f, m_TimerFadeTime).setEaseInCubic();
		StopCoroutine(m_TimerCoroutine);
		OnTimerComplete = null;
	}

	private IEnumerator StartTimer(float time)
	{
		float remainder = time % 1;
		m_CurrentTime = Mathf.FloorToInt(time);
		if (remainder > 0.01f)
		{
			yield return new WaitForSecondsRealtime(remainder);
		}

		
		while (m_CurrentTime > 0)
		{
			m_AudioManager.Play(m_TimerTickAudioIdentifier);
			float pulseSizeMult = m_TextPulseSizeByTimer.Evaluate(m_CurrentTime);
			m_TimerRect.sizeDelta = m_InitialTextSize * pulseSizeMult;
			LeanTween.size(m_TimerRect, m_InitialTextSize, 1.0f).setEaseOutCubic();
			m_TimerText.text = m_CurrentTime.ToString();
			m_CurrentTime--;
			yield return new WaitForSecondsRealtime(1.0f);
		}
		m_AudioManager.Play(m_TimerCompleteAudioIdentifier);
		OnTimerComplete?.Invoke();
	}
}
