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
}
