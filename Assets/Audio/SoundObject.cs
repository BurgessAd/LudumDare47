using UnityEngine;

[CreateAssetMenu(menuName = "New Sound Object", fileName = "New Sound Object")]
public class SoundObject : ScriptableObject
{

	public string m_Identifier;

	public AudioClip clip;

	[Range(0f, 2f)]
	public float defaultVolume = 1.0f;

	[Range(0.1f, 2f)]
	public float defaultPitch = 1.0f;

	public bool loop = false;
}
