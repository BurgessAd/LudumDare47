using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Linq.Expressions;
using System;

public class VisualQualitySystem : MonoBehaviour
{
	[SerializeField] private PostProcessVolume m_Volume;
	[SerializeField] private SettingsManager m_Settings;

	private Bloom m_Bloom;
	private MotionBlur m_MotionBlur;
	private ColorGrading m_ColourGrading;
	private AmbientOcclusion m_AmbientOcclusion;

	private Dictionary<string, Action> m_PropertyChangeDict = new Dictionary<string, Action>(); 

	public void OnGameInitialized()
	{
		bool hasSettings = false;
		m_Settings.PropertyChanged += OnPropertyChanged;
		PostProcessProfile volumeProfile = m_Volume?.profile;
		if (!volumeProfile) throw new System.NullReferenceException(nameof(PostProcessProfile));
		hasSettings = volumeProfile.TryGetSettings(out m_Bloom);
		hasSettings = volumeProfile.TryGetSettings(out m_MotionBlur);
		hasSettings = volumeProfile.TryGetSettings(out m_ColourGrading);
		hasSettings = volumeProfile.TryGetSettings(out m_AmbientOcclusion);

		string bloomParam = GetPropertyName(() => m_Settings.Bloom);
		string brightnessParam = GetPropertyName(() => m_Settings.Brightness);
		string contrastParam = GetPropertyName(() => m_Settings.Contrast);
		string screenModeParam = GetPropertyName(() => m_Settings.DisplayMode);
		string motionBlurParam = GetPropertyName(() => m_Settings.MotionBlur);
		string ambientOcclusionParam = GetPropertyName(() => m_Settings.MotionBlur);

		List<Tuple<string, ParameterOverride>> paramAssociation = new List<Tuple<string, ParameterOverride>>
		{
			new Tuple<string, ParameterOverride>(bloomParam , m_Bloom.intensity),
			new Tuple<string, ParameterOverride>(brightnessParam , m_ColourGrading.brightness),
			new Tuple<string, ParameterOverride>(contrastParam , m_ColourGrading.contrast),
			new Tuple<string, ParameterOverride>(ambientOcclusionParam , m_AmbientOcclusion.enabled),
			new Tuple<string, ParameterOverride>(motionBlurParam , m_MotionBlur.enabled),
		};


		if (hasSettings)
		{
			foreach (var tuple in paramAssociation)
			{
				m_PropertyChangeDict.Add(tuple.Item1, () =>
				{
					dynamic paramOverride = Convert.ChangeType(tuple.Item2, tuple.Item2.GetType());
					OverrideParamWithPropertyInSettings(paramOverride, tuple.Item1);

				});
			}

			m_PropertyChangeDict.Add(screenModeParam, () => {
				FullScreenMode screenMode = ParseSettingsForPropertyVal<FullScreenMode>(screenModeParam);
				Screen.fullScreenMode = screenMode;
			});
			// update immediately upon hooking in to the properties
			foreach (var val in m_PropertyChangeDict.Values)
			{
				val.Invoke();
			}
		}
		else
		{
			Debug.LogError("Cannot find a required graphics setting in postprocessing!");
		}
	}

	public void OnGameUninitialized()
	{
		m_PropertyChangeDict.Clear();
		m_Settings.PropertyChanged -= OnPropertyChanged;
	}

	private T ParseSettingsForPropertyVal<T>(string name)
	{
		return (T)m_Settings.GetType().GetProperty(name).GetValue(m_Settings);
	}

	private void OverrideParamWithPropertyInSettings<T>(in ParameterOverride<T> floatParam, in string propertyName)
	{
		floatParam.Override(ParseSettingsForPropertyVal<T>(propertyName));
	}

	private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (m_PropertyChangeDict.TryGetValue(e.PropertyName, out Action val))
		{
			val.Invoke();
		}
	}

	private string GetPropertyName<T>(Expression<Func<T>> propertyLambda)
	{
		if (!(propertyLambda.Body is MemberExpression me))
		{
			throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
		}
		return me.Member.Name;
	}
}
