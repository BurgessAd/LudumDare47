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

	private bool m_InvertY;
	[Binding]
	public bool InvertY
	{
		get => m_InvertY;
		set { m_InvertY = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InvertY")); }
	}

	private FullScreenMode m_DisplayMode;
	[Binding]
	public FullScreenMode DisplayMode
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

	private bool m_Bloom;
	[Binding]
	public bool Bloom
	{
		get => m_Bloom;
		set { m_Bloom = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bloom")); }
	}

	private bool m_MotionBlur;
	[Binding]
	public bool MotionBlur
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

	private bool m_DepthOfField;
	[Binding]
	public bool DepthOfField
	{
		get => m_DepthOfField;
		set { m_DepthOfField = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DepthOfField")); }
	}
	#endregion


	public event PropertyChangedEventHandler PropertyChanged;

}
