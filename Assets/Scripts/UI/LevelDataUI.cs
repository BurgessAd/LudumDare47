using UnityEngine;
using UnityEngine.Video;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class LevelDataUI : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler, IPointerDownHandler
{
	[SerializeField] private VideoPlayer m_VideoPlayer;
	[SerializeField] private CanvasGroup m_PadlockImageGroup;
	[SerializeField] private TextMeshProUGUI m_LevelName;
	[SerializeField] private TextMeshProUGUI m_LevelTime;
	[SerializeField] private Animator m_StarRatingAnimator;
	[SerializeField] private Animator m_BackgroundImageBlurAnimator;
	[SerializeField] private Animator m_PadlockJiggleAnimator;

	private bool m_bIsUnlocked = false;

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
	#endregion

	#region Initialization
	public void SetupData(LevelData m_Data, int levelNumber)
	{
		m_bIsUnlocked = m_Data.IsUnlocked;
		m_PadlockImageGroup.alpha = m_Data.IsUnlocked ? 0.0f : 1.0f;

		if (m_bIsUnlocked)
		{
			m_StarRatingAnimator.Play("Anim", -1, (float)m_Data.GetCurrentStarRating / 3);
			m_LevelName.name = "Level " + levelNumber.ToString() + " : " + m_Data.GetLevelName;
			m_LevelTime.name = m_Data.GetBestTimeAsString;
			m_BackgroundImageBlurAnimator.Play("Blur", -1, 1.0f);
		}
	}
	#endregion
}
