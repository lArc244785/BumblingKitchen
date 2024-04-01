using BumblingKitchen.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
	[SerializeField] private AudioClip[] _steps;
	[SerializeField] private AudioClip[] _chops;
	[SerializeField] private AudioSource _source;
	[SerializeField] private AudioClip _pickUp;
	[SerializeField] private AudioClip _drop;

	private IHandEvents _handEvents;

	private void Awake()
	{
		_handEvents = GetComponent<IHandEvents>();
		_handEvents.OnPickUp += PickUp;
		_handEvents.OnDrop += Drop;
	}


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

	private void PickUp()
	{
		_source.PlayOneShot(_pickUp);
	}

	private void Drop()
	{
		_source.PlayOneShot(_drop);
	}
}
