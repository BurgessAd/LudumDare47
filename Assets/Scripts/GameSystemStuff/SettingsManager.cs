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

	public List<ControlBinding> m_KeyBindings = new List<ControlBinding>();

	public void ForEachControlBinding(in Action<ControlBinding> act)
	{
		for (int i = 0; i < m_KeyBindings.Count; i++)
		{
			act.Invoke(m_KeyBindings[i]);
		}
	}

	#region BindingProperties

	private float m_SFXVol;
	[Binding]
	public float SFXVol
	{
		get => m_SFXVol;
		set {
			m_SFXVol = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SFXVol"));
		} 
	}

	private float m_AmbientVol = 1.0f;
	[Binding]
	public float AmbientVol
	{
		get { return m_AmbientVol; }
		set {
			m_AmbientVol = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AmbientVol"));
		}
	}

	private float m_MusicVol;
	[Binding]
	public float MusicVol
	{
		get => m_MusicVol;
		set {
			m_MusicVol = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MusicVol"));
		}
	}

	private float m_UISFXVol;
	[Binding]
	public float UISFXVol
	{
		get => m_UISFXVol;
		set {
			m_UISFXVol = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UISFXVol"));
		}
	}

	private bool m_bIsMuted;
	[Binding]
	public bool IsMuted
	{
		get => m_bIsMuted;
		set { m_bIsMuted = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsMuted")); }
	}

	private float m_MouseSensitivityX;
	[Binding]
	public float MouseSensitivityX
	{
		get => m_MouseSensitivityX;
		set { m_MouseSensitivityX = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MouseSensitivityX")); }
	}

	private float m_MouseSensitivityY;
	[Binding]
	public float MouseSensitivityY
	{
		get => m_MouseSensitivityY;
		set { m_MouseSensitivityY = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MouseSensitivityY")); }
	}

	private float m_InvertY;
	[Binding]
	public float InvertY
	{
		get => m_InvertY;
		set { m_InvertY = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InvertY")); }
	}

	private bool m_bPauseOnMinimise;
	[Binding]
	public bool PauseOnMinimise
	{
		get => m_bPauseOnMinimise;
		set { m_bPauseOnMinimise = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PauseOnMinimise")); }
	}

	private ScreenMode m_DisplayMode;
	[Binding]
	public ScreenMode DisplayMode
	{
		get => m_DisplayMode;
		set { m_DisplayMode = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DisplayMode")); }
	}

	private float m_FoV;
	[Binding]
	public float FoV
	{
		get => m_FoV;
		set { m_FoV = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FoV")); }
	}

	private float m_Bloom;
	[Binding]
	public float Bloom
	{
		get => m_Bloom;
		set { m_Bloom = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bloom")); }
	}

	private float m_MotionBlur;
	[Binding]
	public float MotionBlur
	{
		get => m_MotionBlur;
		set { m_MotionBlur = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MotionBlur")); }
	}

	private float m_Brightness;
	[Binding]
	public float Brightness
	{
		get => m_Brightness;
		set { m_Brightness = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Brightness")); }
	}

	private float m_Contrast;
	[Binding]
	public float Contrast
	{
		get => m_Contrast;
		set { m_Contrast = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Contrast")); }
	}
	#endregion


	public event PropertyChangedEventHandler PropertyChanged;

}
