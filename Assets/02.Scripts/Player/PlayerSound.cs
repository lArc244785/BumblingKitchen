using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
	[SerializeField] private AudioClip[] _steps;
	[SerializeField] private AudioClip[] _chops;
	[SerializeField] private AudioSource _source;

	public void PlayStep()
	{
		_source.PlayOneShot(GetRandomSound(_steps));
	}

	public void PlayChrop()
	{
		_source.PlayOneShot(GetRandomSound(_chops));
	}

	private AudioClip GetRandomSound(AudioClip[] sounds)
	{
		return sounds[Random.RandomRange(0, sounds.Length)];
	}
}
