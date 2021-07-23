using UnityEngine;
using System.Collections.Generic;
using System;

namespace UnityUtils
{
	public static class UnityUtils
	{
		public static bool IsLayerInMask(in LayerMask mask, in int layer) 
		{
			return (mask == (mask | (1 << layer)));
		}

		public static string TurnTimeToString(in float time)
		{
			int seconds = Mathf.FloorToInt(time % 60);
			int minutes = Mathf.FloorToInt(time / 60);
			return minutes.ToString() + ":" + seconds.ToString();
		}
	}

	public class ListenerSet<T> : HashSet<T>
	{
		public void ForEachListener(Action<T> act)
		{
			foreach (T t in this)
			{
				act.Invoke(t);
			}
		}
	}

	[Serializable]
	public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
	{
		[SerializeField]
		private List<TKey> keys = new List<TKey>();

		[SerializeField]
		private List<TValue> values = new List<TValue>();

		// save the dictionary to lists
		public void OnBeforeSerialize()
		{
			keys.Clear();
			values.Clear();
			foreach (KeyValuePair<TKey, TValue> pair in this)
			{
				keys.Add(pair.Key);
				values.Add(pair.Value);
			}
		}

		// load dictionary from lists
		public void OnAfterDeserialize()
		{
			this.Clear();

			if (keys.Count != values.Count)
				throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

			for (int i = 0; i < keys.Count; i++)
				this.Add(keys[i], values[i]);
		}
	}
}


