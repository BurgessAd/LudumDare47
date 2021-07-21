using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "LevelData")]
public class LevelData : ScriptableObject
{
    [SerializeField] private string m_sLevelName = "";
    [SerializeField] private float m_nTargetTime = 0.0f;
	[SerializeField] private List<LevelObjective> m_LevelObjectives = new List<LevelObjective>();
    private bool m_bIsUnlocked = false;
    private float m_nAchievedTime = 0.0f;
    private bool m_bHasCompleted = false;

    public bool IsUnlocked() => m_bIsUnlocked;

    public bool IsCompleted() => m_bHasCompleted;

	public int GetObjectiveCount => m_LevelObjectives.Count;

	public void ForEachObjective(Action<LevelObjective> objectiveFunc)
	{
		foreach(LevelObjective objective in m_LevelObjectives)
		{
			objectiveFunc.Invoke(objective);
		}
	}

	public string GetLevelName() => m_sLevelName;

    public float GetTargetTime() => m_nTargetTime;

    public void HasCompletedLevel()
    {
        m_bHasCompleted = true;
    }
    public void UnlockLevel()
    {
        m_bIsUnlocked = true;
    }
    public void TrySetNewTime(in float time) 
    {
        if (m_nAchievedTime > time || m_bHasCompleted) 
        {
            m_nAchievedTime = time;
        }
    }

}
