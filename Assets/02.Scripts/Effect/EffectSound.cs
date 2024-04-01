using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen
{
    public class EffectSound : MonoBehaviour, IEffectEnd
    {
		private AudioSource _audioSource;

		public bool IsEffectEnd()
		{
			return _audioSource.isPlaying == false;
		}

		private void Awake()
		{
			_audioSource = GetComponent<AudioSource>();
		}

		private void OnEnable()
		{
			_audioSource.Stop();
			_audioSource.Play();
		}
	}
}
