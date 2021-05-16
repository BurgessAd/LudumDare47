﻿using UnityEngine;
[CreateAssetMenu(menuName = "Lasso Params")]
public class LassoParams : ScriptableObject
{
	[Header("Lasso Return Params")]
	[Range(0f, 4f)]
	public float m_LassoReturnAcceleration;
	[Range(0f, 20f)]
	public float m_MaxLassoReturnSpeed;

	[Header("Lasso Attached Params")]
	[Range(0f, 40f)]
	public float m_LassoLength;
	[Range(0f, 3f)]
	public float m_GrabDistance;

	[Header("Lasso Throw Params")]
	public AnimationCurve m_ThrowForceCurve;

	[Header("Lasso Pull Force Params")]
	[Range(0f, 2000f)]
	public float m_MaxForceForPull;
	public AnimationCurve m_ForceIncreasePerPull;
	public AnimationCurve m_ForceDecreasePerSecond;
	public AnimationCurve m_JerkProfile;
	[Range(0f, 1f)]
	public float m_JerkTimeForPull;

	[Header("Lasso Spinning Params")]
	[Range(0f, 1f)]
	public float m_TimeBeforeUserCanThrow;
	[Range(0f, 4f)]
	public float m_MaxTimeSpinning;
	[Range(0f,1f)]
	public float m_MaxTimeToSwitchStrength;
	public AnimationCurve m_SpinHeightProfile;
	public AnimationCurve m_SpinSizeProfile;
	public AnimationCurve m_SpinSpeedProfile;

	public bool SpinningIsInitializing = false;
	public bool SpunUp;
}