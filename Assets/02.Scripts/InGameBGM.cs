using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen
{
    public class InGameBGM : MonoBehaviour
    {
        private AudioSource _audioSource;

		private void Awake()
		{
			_audioSource = GetComponent<AudioSource>();
		}

		private void Start()
		{
			GameManager.Instance.OnPlaying += _audioSource.Play;
			GameManager.Instance.OnEnddingGame += _audioSource.Stop;
		}
	}
}
