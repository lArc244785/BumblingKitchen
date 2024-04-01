using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
namespace BumblingKitchen
{

    public class ObjectPool : MonoBehaviour
    {
		[field:SerializeField] public PoolObjectType Type { get; private set; }
		[SerializeField] private PooledObject _prefab;
        [SerializeField] private int _initAmount;

		private Stack<PooledObject> _pool = new Stack<PooledObject>();

		public void Awake()
		{
			SetUpObjectPool();
		}

		public void SetUpObjectPool()
		{
			for (int i = 0; i < _initAmount; i++)
			{
				PooledObject obj = CreatePooledObject();
				obj.transform.parent = transform;
				obj.gameObject.SetActive(false);
				_pool.Push(obj);
			}
		}
		public PooledObject GetPooledObject()
		{
			PooledObject instance = null;

			if( _pool.Count > 0 )
			{
				instance = _pool.Pop();
			}

			if(instance == null)
			{
				instance = CreatePooledObject();
			}

			instance.gameObject.SetActive(true);
			return instance;
		}
		public void Relese(PooledObject pooledObject)
		{
			pooledObject.transform.parent = transform;
			pooledObject.transform.position = Vector3.zero;
			pooledObject.transform.rotation = Quaternion.identity;
			pooledObject.gameObject.SetActive(false);
			_pool.Push(pooledObject);
		}
		private PooledObject CreatePooledObject()
		{
			var pooledObject = Instantiate(_prefab, Vector3.zero, Quaternion.identity);
			pooledObject.InitPool(this);
			return pooledObject;
		}

		[Button("SetUpName")]
		private void SetPoolName()
		{
			gameObject.name = $"Pool {Type.ToString()}";
		}
	}
}
