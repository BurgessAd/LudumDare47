using UnityEngine;
using System.Collections.Generic;
using System;

public class AudioManager : MonoBehaviour
{
	[SerializeField]
	private SoundObject[] sounds;
	private readonly Dictionary<string, Sound> m_SoundDict = new Dictionary<string, Sound>();
	void Awake()
	{
		foreach(SoundObject sound in sounds) 
		{
			m_SoundDict.Add(sound.m_Identifier, new Sound(sound.defaultVolume, sound.defaultPitch, sound.loop, sound.clip, gameObject.AddComponent<AudioSource>()));
		}
	}

	public void Play(string soundIdentifier)
	{
		ApplyToSound(soundIdentifier, (Sound sound) => sound.Start());
	}

	public void SetPitch(string soundIdentifier, float newPitch) 
	{
		ApplyToSound(soundIdentifier, (Sound sound) => sound.SetPitch(newPitch));
	
	}

	public void SetVolume(string soundIdentifier, float volume) 
	{
		ApplyToSound(soundIdentifier, (Sound sound) => sound.SetVolume(volume));
	}

	public void StopPlaying(string soundIdentifier)
	{
		ApplyToSound(soundIdentifier, (Sound sound) => sound.Stop());
	}

	public void ApplyToSound(in string sound, in Action<Sound> soundAction) 
	{
		if (m_SoundDict.TryGetValue(sound, out Sound value))
		{
			soundAction.Invoke(value);
		}
		else
		{
			Debug.Log("Could not find sound with identifier " + sound + " in object " + gameObject.name);
		}
	}
}

public class Sound 
{
	private readonly AudioClip m_AudioClip;
	private readonly AudioSource m_AudioSource;
	private readonly float m_fDefaultVolume;
	private readonly float m_fDefaultPitch;

	public Sound(in float defaultVolume, in float defaultPitch, in bool doesLoop, in AudioClip clipToPlay, in AudioSource sourceToplayFrom) 
	{
		m_AudioClip = clipToPlay;
		m_AudioSource = sourceToplayFrom;
		m_fDefaultPitch = defaultPitch;
		m_fDefaultVolume = defaultVolume;
		m_AudioSource.loop = doesLoop;
		m_AudioSource.clip = m_AudioClip;
	}

	public void Start() 
	{
		m_AudioSource.Play();
	}

	public void Stop() 
	{
		m_AudioSource.Stop();
	}

	public void SetPitch(in float pitchPercent) 
	{
		m_AudioSource.pitch = pitchPercent * m_fDefaultPitch;
	}

	public void SetVolume(in float volumePercent) 
	{
		m_AudioSource.volume = volumePercent * m_fDefaultVolume;
	}
}
