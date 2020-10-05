using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
	//        FindObjectOfType<AudioManager>().Play(moo);

	public static AudioManager instance;

	public Sound[] sounds;
	private List<Sound> soundsToIncrease = new List<Sound>();
	private float pitchSpeed = 1f;
	void Awake()
	{
		if (instance != null)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}

		foreach (Sound s in sounds)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;
			s.source.loop = s.loop;

		}
	}

	public void Play(string sound)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

		s.source.volume = s.volume;
		s.source.pitch = 1f;

		s.source.Play();
	}
	public void PlayAt(string sound, Vector3 position)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

		s.source.volume = s.volume;
		s.source.pitch = 0f;

		AudioSource.PlayClipAtPoint(s.clip, position);
	}

	public Sound PlayAtReturn(string sound, Vector3 position)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);


		s.source.volume = s.volume;
		s.source.pitch = 0f;

		AudioSource.PlayClipAtPoint(s.clip, position);

		return s;
	}
	public void stop(string sound)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		s.source.Stop();
	}

	private void Update()
    {

/*		//While the pitch is below 1, decrease it as time passes.
		if (audioSource.pitch > 1)
		{
			audioSource.pitch += Time.deltaTime * pitchSpeed;
		}*/
		
	}
}
