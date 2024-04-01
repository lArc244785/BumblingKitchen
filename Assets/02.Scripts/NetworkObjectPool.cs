using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen
{
	public class NetworkObjectPool : NetworkObjectProviderDefault
	{
		[SerializeField] private List<NetworkObject> _poolableObjects = new List<NetworkObject>();
		private Dictionary<NetworkObjectTypeId, Stack<NetworkObject>> _free = new Dictionary<NetworkObjectTypeId, Stack<NetworkObject>>();

		protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
		{
			if (IsPoolableObject(prefab) == true)
			{
				NetworkObject instance = GetFromObjectPool(prefab);
				instance.transform.position = Vector3.zero;
				return instance;
			}

			return Instantiate(prefab);
		}

		protected override void DestroyPrefabInstance(NetworkRunner runner, NetworkPrefabId prefabId, NetworkObject instance)
		{
			if (_free.TryGetValue(prefabId, out var stack) == true)
			{
				instance.gameObject.SetActive(false);
				stack.Push(instance);
			}
			else
			{
				Destroy(instance.gameObject);
			}
		}


		private NetworkObject GetFromObjectPool(NetworkObject prefab)
		{
			NetworkObject instance = null;
			if (_free.TryGetValue(prefab.NetworkTypeId, out var stack) == true)
			{
				if (stack.Count > 0)
				{
					instance = stack.Pop();
				}	
			}

			if (instance == null)
			{
				instance = GetNewInstance(prefab);
			}

			instance.gameObject.SetActive(true);
			return instance;
		}

		private NetworkObject GetNewInstance(NetworkObject prefab)
		{
			NetworkObject instance = Instantiate(prefab);

			if (_free.TryGetValue(prefab.NetworkTypeId, out var stack) == false)
			{
				_free.Add(prefab.NetworkTypeId, new Stack<NetworkObject>());
			}
			return instance;
		}

		private bool IsPoolableObject(NetworkObject prefab)
		{
			foreach (var poolableObject in _poolableObjects)
			{
				if (poolableObject == prefab)
					return true;
			}
			return false;
		}
	}
}
