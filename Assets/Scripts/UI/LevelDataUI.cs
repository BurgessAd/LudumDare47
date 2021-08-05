using UnityEngine;
using UnityEngine.Video;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class LevelDataUI : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler, IPointerDownHandler
{
	[Header("Object References")]
	[SerializeField] private VideoPlayer m_VideoPlayer;
	[SerializeField] private GameObject m_BlurPlaneGo;
	[SerializeField] private RectTransform m_RectTransformForVideo;
	[SerializeField] private CanvasGroup m_OutGlowCanvasGroup;
	[SerializeField] private CanvasGroup m_LevelSplashCanvasGroup;

	[Header("Animation References and Params")]
	[SerializeField] [Range(0.05f, 0.3f)] private float m_AnimInOutFadeTime;
	[SerializeField] private StarUI m_StarUI;

	private int m_OutGlowFadeId;
	private int m_LevelSplashId;

	private bool m_bIsUnlocked = false;
	private bool m_bIsSelected = false;
	private int m_LevelId;

	public event Action OnSelectLevel;

	#region UnityInterfaces
	private void OnVideoPlayerPrepared(VideoPlayer source)
	{
		m_VideoPlayer.frame = 0;
		m_VideoPlayer.Pause();
	}

	private void OnVideoPlayerLoop(VideoPlayer source)
	{
		m_VideoPlayer.frame = 0;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (m_bIsUnlocked)
		{
			LeanTween.cancel(m_LevelSplashId);
			m_LevelSplashId = LeanTween.alphaCanvas(m_LevelSplashCanvasGroup, 1.0f, m_AnimInOutFadeTime).setEaseInOutCubic().uniqueId;
			m_VideoPlayer.Play();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (m_bIsUnlocked)
		{
			LeanTween.cancel(m_LevelSplashId);
			m_LevelSplashId = LeanTween.alphaCanvas(m_LevelSplashCanvasGroup, 1.0f, m_AnimInOutFadeTime).setEaseInOutCubic().uniqueId;
			m_VideoPlayer.Stop();
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
		}
	}

	public void OnLevelNumSelected(int levelNum)
	{
		if (levelNum == m_LevelId)
		{
			if (!m_bIsSelected)
			{
				LeanTween.cancel(m_OutGlowFadeId);
				m_OutGlowFadeId = LeanTween.alphaCanvas(m_OutGlowCanvasGroup, 1.0f, m_AnimInOutFadeTime).setEaseInOutCubic().uniqueId;
				m_bIsSelected = true;
			}
		}
		else
		{
			if (m_bIsSelected)
			{
				LeanTween.cancel(m_OutGlowFadeId);
				m_OutGlowFadeId = LeanTween.alphaCanvas(m_OutGlowCanvasGroup, 0.0f, m_AnimInOutFadeTime).setEaseInOutCubic().uniqueId;
				m_bIsSelected = false;
			}
		}
	}
	#endregion

	#region Initialization
	public void SetupData(LevelData m_Data)
	{
		m_LevelSplashCanvasGroup.alpha = m_bIsUnlocked ? 1.0f : 0.0f;
		m_StarUI.SetStarsVisible((int)m_Data.GetCurrentStarRating);
		m_LevelId = m_Data.GetLevelNumber;
		m_bIsUnlocked = m_Data.IsUnlocked;
		m_BlurPlaneGo.SetActive(!m_bIsUnlocked);

		m_VideoPlayer.clip = m_Data.GetLevelVideoClip;
		m_VideoPlayer.renderMode = VideoRenderMode.RenderTexture;
		m_VideoPlayer.targetTexture = new RenderTexture((int)m_RectTransformForVideo.rect.height, (int)m_RectTransformForVideo.rect.width, 1);
		m_VideoPlayer.targetTexture.Create();
		m_VideoPlayer.Prepare();
		m_VideoPlayer.prepareCompleted += OnVideoPlayerPrepared;
	}
	#endregion

	private void OnDestroy()
	{
		m_VideoPlayer.targetTexture.Release();
	}
}
