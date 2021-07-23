using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName ="SettingsManager")]
public class SettingsManager : ScriptableObject
{
	public enum ScreenMode
	{
		Full,
		Windowed,
		Borderless
	}

	public void ForEachControlBinding(in Action<ControlBinding> act)
	{
		for (int i = 0; i < m_KeyBindings.Count; i++)
		{
			act.Invoke(m_KeyBindings[i]);
		}
	}

	[SerializeField] public List<ControlBinding> m_KeyBindings = new List<ControlBinding>();
	[SerializeField] public float m_SFXVol;
	[SerializeField] public float m_MusicVol;
	[SerializeField] public float m_UISFXVol;
	[SerializeField] public float m_bAmbientSFXVol;
	[SerializeField] public bool m_bIsMuted;

	[SerializeField] public float m_MouseSensitivityX;
	[SerializeField] public float m_MouseSensitivityY;
	[SerializeField] public bool m_InvertY;

	[SerializeField] public bool PauseOnMinimise;
	[SerializeField] public ScreenMode m_ScreenMode;

	//reset sensitivity bindings

	// reset keybindings

}
