using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "LevelData")]
public class LevelData : ScriptableObject
{
    [SerializeField] private string m_sLevelName = "";
    [SerializeField] private float m_nTargetTime = 0.0f;
	[SerializeField] private int m_LevelCompleteTime = 0;
	[SerializeField] private List<LevelObjective> m_LevelObjectives = new List<LevelObjective>();

	private float m_nAchievedTime = 0.0f;

	#region Properties

	public bool IsUnlocked { get; private set; } = false;

	public bool IsCompleted { get; private set; } = false;

	public int GetObjectiveCount => m_LevelObjectives.Count;

	public int GetSuccessTimerTime => m_LevelCompleteTime;

	public string GetLevelName => m_sLevelName;

	public float GetTargetTime => m_nTargetTime;

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

	#endregion
}
