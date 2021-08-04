using UnityEngine;
using UnityEngine.Video;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class LevelDataUI : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler, IPointerDownHandler
{
	[SerializeField] private VideoPlayer m_VideoPlayer;
	[SerializeField] private CanvasGroup m_PadlockImageGroup;
	[SerializeField] private CanvasGroup m_OutGlowCanvasGroup;
	[SerializeField] private Animator m_BackgroundImageBlurAnimator;
	[SerializeField] private Animator m_PadlockJiggleAnimator;

	[SerializeField] private float m_OutGlowFadeTime;
	private int m_OutGlowFadeId;

	private bool m_bIsUnlocked = false;
	private bool m_bIsSelected = false;
	private int m_LevelId;

	public event Action OnSelectLevel;

	#region UnityInterfaces
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (m_bIsUnlocked)
		{
			m_BackgroundImageBlurAnimator.Play("Unblur", -1);
			m_VideoPlayer.Play();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (m_bIsUnlocked)
		{
			m_BackgroundImageBlurAnimator.Play("Blur", -1);
			m_VideoPlayer.Pause();
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (m_bIsUnlocked)
			{
				OnSelectLevel.Invoke();
			}
			else
			{
				m_PadlockJiggleAnimator.Play("Anim", -1);
			}
		}
	}

	public void OnLevelNumSelected(int levelNum)
	{
		if (levelNum == m_LevelId)
		{
			if (!m_bIsSelected)
			{
				LeanTween.cancel(m_OutGlowFadeId);
				m_OutGlowFadeId = LeanTween.alphaCanvas(m_OutGlowCanvasGroup, 1.0f, m_OutGlowFadeTime).setEaseInOutCubic().uniqueId;
				m_bIsSelected = true;
			}
		}
		else
		{
			if (m_bIsSelected)
			{
				LeanTween.cancel(m_OutGlowFadeId);
				m_OutGlowFadeId = LeanTween.alphaCanvas(m_OutGlowCanvasGroup, 0.0f, m_OutGlowFadeTime).setEaseInOutCubic().uniqueId;
				m_bIsSelected = false;
			}
		}
	}
	#endregion

	#region Initialization
	public void SetupData(LevelData m_Data)
	{
		m_LevelId = m_Data.GetLevelNumber;
		m_bIsUnlocked = m_Data.IsUnlocked;
		m_PadlockImageGroup.alpha = m_Data.IsUnlocked ? 0.0f : 1.0f;

		if (m_bIsUnlocked)
		{
			m_BackgroundImageBlurAnimator.Play("Blur", -1, 1.0f);
		}
	}
	#endregion
}
