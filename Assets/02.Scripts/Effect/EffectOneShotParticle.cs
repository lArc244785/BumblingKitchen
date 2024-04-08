using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen
{
    public class EffectOneShotParticle : MonoBehaviour, IEffectEnd
    {
		private ParticleSystem _particle;

		public bool IsEffectEnd()
		{
			return _particle.isPlaying == false;
		}

		private void Awake()
		{
			_particle = GetComponent<ParticleSystem>();
		}

		private void OnEnable()
		{
			_particle.time = 0.0f;
			_particle.Simulate(0.0f);
			_particle.Play();
		}
	}
}
