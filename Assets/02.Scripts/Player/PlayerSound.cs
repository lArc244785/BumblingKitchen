using BumblingKitchen;
using BumblingKitchen.Interaction;
using BumblingKitchen.Player;
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
	private IMoveEvents _moveEvents;

	private IEnumerator _stepCoroutine;

	private void Awake()
	{
		_moveEvents = GetComponent<IMoveEvents>();
		_handEvents = GetComponent<IHandEvents>();
		_handEvents.OnPickUp += PickUp;
		_handEvents.OnDrop += Drop;

		_moveEvents.OnBegineMove += StartStep;
		_moveEvents.OnEndMove += StopStep;
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
		return sounds[Random.Range(0, sounds.Length)];
	}

	private void PickUp()
	{
		_source.PlayOneShot(_pickUp);
	}

	private void Drop()
	{
		_source.PlayOneShot(_drop);
	}

	private void StartStep()
	{
		if(_stepCoroutine != null)
		{
			StopCoroutine(_stepCoroutine);
		}
		_stepCoroutine = StepSoundCortutine();
		StartCoroutine(_stepCoroutine);
	}

	private void StopStep()
	{
		if (_stepCoroutine != null)
		{
			StopCoroutine(_stepCoroutine);
		}
		_stepCoroutine = null;
	}

	private IEnumerator StepSoundCortutine()
	{
		while(GameManager.Instance.State == GameState.Play)
		{
			PlayStep();
			yield return new WaitForSeconds(0.25f);
		}
	}
}
