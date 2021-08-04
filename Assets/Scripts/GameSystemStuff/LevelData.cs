using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "LevelData")]
public class LevelData : ScriptableObject
{
    [SerializeField] private string m_sLevelName = "";
    [SerializeField] private float m_nTargetTime = 0.0f;
	[SerializeField] private int m_LevelCompleteTime = 0;
	[SerializeField] private StarRating m_StarRating = StarRating.Zero;
	[SerializeField] private int m_Score = 0;
	[SerializeField] private List<LevelObjective> m_LevelObjectives = new List<LevelObjective>();
	[SerializeField] private float[] m_Checkpoints = new float[] { 0f, 0f };
	[SerializeField] private int m_LevelNumber = 0;

	private float m_nAchievedTime = 0.0f;

	public enum StarRating
	{
		Zero = 0,
		Half = 1,
		Two = 2,
		Three = 3
	}

	#region Properties

	public bool IsUnlocked { get; private set; } = false;

	public bool IsCompleted { get; private set; } = false;

	public int GetObjectiveCount => m_LevelObjectives.Count;

	public int GetSuccessTimerTime => m_LevelCompleteTime;

	public StarRating GetCurrentStarRating => m_StarRating;

	public ref float[] GetCheckpoints => ref m_Checkpoints;

	public int GetScore => m_Score;

	public string GetLevelName => m_sLevelName;

	public float GetTargetTime => m_nTargetTime;

	public string GetBestTimeAsString => UnityUtils.UnityUtils.TurnTimeToString(m_nTargetTime);

	public int GetLevelNumber => m_LevelNumber;

	#endregion

	#region PublicFunctions

	public void ForEachObjective(Action<LevelObjective> objectiveFunc)
	{
		foreach(LevelObjective objective in m_LevelObjectives)
		{
			objectiveFunc.Invoke(objective);
		}
	}

    public void HasCompletedLevel()
    {
        IsCompleted = true;
    }

    public void UnlockLevel()
    {
        IsUnlocked = true;
    }

    public void TrySetNewTime(in float time) 
    {
        if (m_nAchievedTime > time || IsCompleted) 
        {
            m_nAchievedTime = time;
        }
    }

	public void TrySetNewStarRating(StarRating newStarRating)
	{
		if (m_StarRating < newStarRating)
		{
			m_StarRating = newStarRating;
		}
	}

	public void TrySetNewScore(in int score)
	{
		if (m_Score < score)
		{
			m_Score = score;
		}
	}

	public void SetLevelNumber(in int num)
	{
		m_LevelNumber = num;
	}

	#endregion
}
