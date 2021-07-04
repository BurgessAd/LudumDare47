using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SolidRenderReplacementEffect : MonoBehaviour
{
	[SerializeField] private Shader ReplacementShader;
	[SerializeField] private Camera cam;
	private void OnEnable()
	{

		if (ReplacementShader != null && cam != null)
			cam.SetReplacementShader(ReplacementShader, "");
	}

	private void OnDisable()
	{
		cam.ResetReplacementShader();
	}
}
