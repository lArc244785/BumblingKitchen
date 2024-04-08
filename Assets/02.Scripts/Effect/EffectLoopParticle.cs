using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen
{
    public class EffectLoopParticle : MonoBehaviour, IEffectEnd
    {
		private ParticleSystem _particle;
		private bool _isEnd;

		public bool IsEffectEnd()
		{
			return _isEnd == true;
		}

		private void Awake()
		{
			_particle = GetComponent<ParticleSystem>();
		}

		private void OnEnable()
		{
			_isEnd = false;
			_particle.time = 0.0f;
			_particle.Simulate(0.0f);
			_particle.Play();
		}

		public void StopParticle()
		{
			_isEnd = true;
		}
	}
}
