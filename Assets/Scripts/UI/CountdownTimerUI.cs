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
	[SerializeField] private AnimationCurve m_TextPulseOpacityByTimer;
	[SerializeField] private float m_TimerFadeTime;
	[SerializeField] private string m_FinalTimerTickString = "0";
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
			TimerTick(m_CurrentTime.ToString(), m_TimerTickAudioIdentifier);
			yield return new WaitForSecondsRealtime(1.0f);
		}
		TimerTick(m_FinalTimerTickString, m_TimerCompleteAudioIdentifier);
		yield return new WaitForSecondsRealtime(0.5f);
		OnTimerComplete?.Invoke();
	}

	private void TimerTick(in string timerText, in string audioIdentifier)
	{
		m_AudioManager.Play(audioIdentifier);
		m_TimerRect.localScale = Vector3.one * (1 + m_TextPulseSizeByTimer.Evaluate(m_CurrentTime));
		LeanTween.scale(m_TimerRect.gameObject, Vector3.one, 1.0f).setEaseInOutCubic();
		m_TextCanvasGroup.alpha = 1.0f;
		LeanTween.alphaCanvas(m_TextCanvasGroup, m_TextPulseOpacityByTimer.Evaluate(m_CurrentTime), 1.0f).setEaseInOutCubic();
		m_TimerText.text = timerText;
		m_CurrentTime--;
	}
}
