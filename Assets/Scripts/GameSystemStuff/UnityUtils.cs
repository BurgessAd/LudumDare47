using UnityEngine;
using System.Collections.Generic;
using System;

public static class UnityUtils
{
    public static bool IsLayerInMask(in LayerMask mask, in int layer) 
    {
        return (mask == (mask | (1 << layer)));
    }

	public class ListenerSet<T> : HashSet<T>
	{
		public void ForEachListener(Action<T> act)
		{
			foreach(T t in this)
			{
				act.Invoke(t);
			}
		}
	}

	public static string TurnTimeToString(in float time)
	{
		int seconds = Mathf.FloorToInt(time % 60);
		int minutes = Mathf.FloorToInt(time / 60);
		return minutes.ToString() + ":" + seconds.ToString();
	}
}
