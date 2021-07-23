using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class KeyboardBindingUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI m_BindingName;
	[SerializeField] private TextMeshProUGUI m_BindingKeyString;

	public event Action<KeyCode> OnAttemptSetBinding;
	private KeyCode[] values;

	public void UpdateUI(ControlBinding binding, Color32 normal, Color32 duplicated)
	{
		m_BindingName.name = binding.GetBindingDisplayName;
		m_BindingKeyString.name = binding.KeyCode.ToString();
		m_BindingKeyString.color = binding.IsDuplicated ? normal : duplicated;
	}

	public void OnClickToChangeKeycode()
	{
		values = (KeyCode[])Enum.GetValues(typeof(KeyCode));
		StartCoroutine(WaitingForInput());
	}

	private IEnumerator WaitingForInput()
	{
		while (true)
		{
			if (Input.GetKey(KeyCode.Escape))
				break;

			for (int i = 0; i < values.Length; i++)
			{
				if (Input.GetKey(values[i]))
				{
					OnAttemptSetBinding(values[i]);
					break;
				}
			}
			yield return null;
		}
	}
}
