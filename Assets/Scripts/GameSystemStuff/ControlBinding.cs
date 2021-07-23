using UnityEngine;
using System;

[CreateAssetMenu(menuName = "KeyboardBinding")]
public class ControlBinding : ScriptableObject
{
	[SerializeField] private string m_ControlBindingDisplayName;
	[SerializeField] private KeyCode m_DefaultKeycode;
	[SerializeField] private KeyCode m_KeyCode;

	private bool m_bIsDuplicated;
	public Action OnControlBindingChanged;

	public KeyCode KeyCode
	{
		get { return m_KeyCode; }
		set {if (m_KeyCode != value){ m_KeyCode = value; OnControlBindingChanged?.Invoke(); }}
	}

	public void Reset()
	{
		m_KeyCode = m_DefaultKeycode;
	}

	public bool IsDuplicated
	{
		get { return m_bIsDuplicated; }
		set { if (m_bIsDuplicated != value){ m_bIsDuplicated = value; OnControlBindingChanged?.Invoke(); } }
	}

	public void ClearOnChangeCallback() { OnControlBindingChanged = null; }

	public string GetBindingDisplayName => m_ControlBindingDisplayName;
}
