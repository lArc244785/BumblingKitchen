using BumblingKitchen.Player;
using System;
using UnityEngine;

namespace BumblingKitchen
{
	internal class EffectMove : MonoBehaviour
	{
		private ParticleSystem _particleSystem;
		private PlayerControl _playerControl;

		private void Awake()
		{
			_playerControl = transform.GetComponentInParent<PlayerControl>();
			_particleSystem = GetComponent<ParticleSystem>();
			_playerControl.OnBegineMove += StartMove;
			_playerControl.OnEndMove += EndMove;
		}

		private void EndMove()
		{
			_particleSystem.Stop();
		}

		private void StartMove()
		{
			_particleSystem.Simulate(0.0f);
			_particleSystem.loop = true;
			_particleSystem.Play();
		}

	}
}
