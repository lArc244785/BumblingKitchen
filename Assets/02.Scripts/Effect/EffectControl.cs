using UnityEngine;

namespace BumblingKitchen
{
	public class EffectControl : MonoBehaviour
	{
		private PooledObject _pooledObject;
		private IEffectEnd[] _effects;

		private void Awake()
		{
			_pooledObject = GetComponent<PooledObject>();
			_effects = transform.GetComponentsInChildren<IEffectEnd>();
		}

		private void LateUpdate()
		{
			foreach(var effect in _effects)
			{
				if (effect.IsEffectEnd() == false)
					return;
			}
			_pooledObject.Relese();
		}
	}
}
