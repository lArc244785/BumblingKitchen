using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen
{
	public class PooledObject : MonoBehaviour
	{
		private ObjectPool _pool;

		public void InitPool(ObjectPool pool)
		{
			_pool = pool;
		}

		public void Relese()
		{
			_pool.Relese(this);
		}
	}
}
