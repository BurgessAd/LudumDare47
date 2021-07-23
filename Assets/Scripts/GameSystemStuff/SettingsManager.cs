using System.Collections.Generic;
using UnityEngine;
using System;
using UnityUtils;
using UnityWeld.Binding;
using System.ComponentModel;

[Binding]
[CreateAssetMenu(menuName ="SettingsManager")]
public class SettingsManager : ScriptableObject, INotifyPropertyChanged
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

	private float m_SFXVol;
	[Binding]
	public float SFXVol
	{
		get => m_SFXVol;
		set { m_SFXVol = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SFXVol")); } 
	}

	private float m_AmbientVol;
	[Binding]
	public float AmbientVol
	{
		get => m_AmbientVol;
		set { m_AmbientVol = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AmbientVol")); }
	}

	private float m_MusicVol;
	[Binding]
	public float MusicVol
	{
		get => m_MusicVol;
		set { m_MusicVol = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MusicVol")); }
	}

	private float m_UISFXVol;
	[Binding]
	public float UISFXVol
	{
		get => m_UISFXVol;
		set { m_UISFXVol = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UISFXVol")); }
	}

	private bool m_bIsMuted;
	[Binding]
	public bool IsMuted
	{
		get => m_bIsMuted;
		set { m_bIsMuted = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsMuted")); }
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public List<ControlBinding> m_KeyBindings = new List<ControlBinding>();

	//[Binding] [SerializeField] public ObservableVariable<float> m_MouseSensitivityX;
	//[Binding] [SerializeField] public ObservableVariable<float> m_MouseSensitivityY;
	//[Binding] [SerializeField] public ObservableVariable<bool> m_InvertY;

	//[Binding] [SerializeField] public ObservableVariable<bool> PauseOnMinimise;
	//[Binding] [SerializeField] public ObservableVariable<ScreenMode> m_ScreenMode;



	//reset sensitivity bindings

	// reset keybindings

}
