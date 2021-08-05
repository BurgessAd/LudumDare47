using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Video;

public class LevelSelectUI : MonoBehaviour
{

	[SerializeField] private TextMeshProUGUI m_LevelNameLeft;
	[SerializeField] private TextMeshProUGUI m_LevelNameRight;
	[SerializeField] private TextMeshProUGUI m_LevelTime;
	[SerializeField] private CanvasGroup m_LevelInfoCanvasGroup;

	[SerializeField] private Transform m_LevelDataUITransform;

	[SerializeField] private float m_TextFadeInOutTime;

	[SerializeField] private StarUI m_StarUI;
	 
	[SerializeField] private GameObject m_LevelDataUIPrefab;

	[SerializeField] private CowGameManager m_GameManager;

	private int animId = -1;
	private int m_SelectedLevelId;
	public event Action<int> m_OnLevelSelected;

	private void Awake()
	{
		for(int i = 0; i < m_GameManager.GetLevelData.Count; i++)
		{
			LevelData levelDatum = m_GameManager.GetLevelData[i];
			levelDatum.SetLevelNumber(i);
			LevelDataUI levelDataUI = Instantiate(m_LevelDataUIPrefab, m_LevelDataUITransform).GetComponent<LevelDataUI>();
			levelDataUI.OnSelectLevel += () => UpdateSelectedLevelData(i);
			m_OnLevelSelected += levelDataUI.OnLevelNumSelected;
			levelDataUI.SetupData(levelDatum);
		}
		UpdateSelectedLevelData(0);
	}


	private void UpdateSelectedLevelData(int levelId)
	{
		m_SelectedLevelId = levelId;
		LevelData levelData = m_GameManager.GetLevelData[m_SelectedLevelId];
		m_OnLevelSelected.Invoke(m_SelectedLevelId);
		LeanTween.cancel(animId);
		LTDescr tween = LeanTween.alphaCanvas(m_LevelInfoCanvasGroup, 0.0f, m_TextFadeInOutTime).setEaseInOutCubic().setOnComplete(() => 
		{
			m_LevelNameLeft.name = "Level " + levelData.GetLevelNumber.ToString();
			m_LevelNameRight.name = levelData.GetLevelName;
			m_LevelTime.name = levelData.GetBestTimeAsString;
			m_StarUI.SetStarsVisible((int)levelData.GetCurrentStarRating);
			tween = LeanTween.alphaCanvas(m_LevelInfoCanvasGroup, 0.0f, m_TextFadeInOutTime).setEaseInOutCubic();
			animId = tween.uniqueId;
		});
		animId = tween.uniqueId;


	}
}
