using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Video;

public class LevelSelectUI : MonoBehaviour
{
	[Space]
	[Header("Anim Params")]
	[SerializeField] private float m_TextFadeInOutTime;

	[Space]
	[Header("UI references")]
	[SerializeField] private TextMeshProUGUI m_LevelNameLeft;
	[SerializeField] private TextMeshProUGUI m_LevelNameRight;
	[SerializeField] private TextMeshProUGUI m_LevelTime;
	[SerializeField] private StarUI m_StarUI;
	[Space]
	[SerializeField] private CanvasGroup m_LevelInfoCanvasGroup;
	[SerializeField] private Transform m_LevelDataUITransform;

	[Space]
	[Header("Game System References")]
	[SerializeField] private GameObject m_LevelDataUIPrefab;
	[SerializeField] private CowGameManager m_GameManager;

	private int animId = -1;
	private int m_SelectedLevelId;
	public event Action<int> m_OnLevelSelected;

	public int GetChosenLevelId => m_SelectedLevelId;

	private void Awake()
	{
		bool lastLevelCompleted = true;

		for(int i = 0; i < m_GameManager.GetNumLevels; i++)
		{
			LevelData levelDatum = m_GameManager.GetLevelDataByLevelIndex(i);
			levelDatum.SetLevelNumber(i);

			if (lastLevelCompleted && !levelDatum.IsUnlocked)
				levelDatum.UnlockLevel();

			LevelDataUI levelDataUI = Instantiate(m_LevelDataUIPrefab, m_LevelDataUITransform).GetComponent<LevelDataUI>();
			levelDataUI.OnSelectLevel += UpdateSelectedLevelData;
			m_OnLevelSelected += levelDataUI.OnLevelNumSelected;
			levelDataUI.SetupData(levelDatum);
			lastLevelCompleted = levelDatum.IsCompleted;
		}
		UpdateSelectedLevelData(0);
	}

	private void UpdateSelectedLevelData(int levelId)
	{
		m_SelectedLevelId = levelId;
		LevelData levelData = m_GameManager.GetLevelDataByLevelIndex(m_SelectedLevelId);
		m_OnLevelSelected.Invoke(m_SelectedLevelId);
		LeanTween.cancel(animId);
		LTDescr tween = LeanTween.alphaCanvas(m_LevelInfoCanvasGroup, 0.0f, m_TextFadeInOutTime).setEaseInOutCubic().setOnComplete(() => 
		{
			m_LevelNameLeft.text = "Level " + UnityUtils.UnityUtils.NumberToWords(levelData.GetLevelNumber);
			m_LevelNameRight.text = levelData.GetLevelName;
			m_LevelTime.text = levelData.GetBestTimeAsString;
			m_StarUI.SetStarsVisible((int)levelData.GetCurrentStarRating);
			tween = LeanTween.alphaCanvas(m_LevelInfoCanvasGroup, 1.0f, m_TextFadeInOutTime).setEaseInOutCubic();
			animId = tween.uniqueId;
		});
		animId = tween.uniqueId;


	}
}
