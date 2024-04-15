using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen
{
	public class PooledObject : MonoBehaviour
	{
		private ObjectPool _pool;
		public event Action OnRelese;

		public void InitPool(ObjectPool pool)
		{
			_pool = pool;
		}

		public void Relese()
		{
			OnRelese?.Invoke();
			OnRelese = null;
			_pool.Relese(this);
		}
	}
}
