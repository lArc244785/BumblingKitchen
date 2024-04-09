using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen
{
	public class NetworkObjectPool : NetworkObjectProviderDefault
	{
		[SerializeField] private List<NetworkObject> _poolableObjects;
		private Dictionary<NetworkObjectTypeId, Stack<NetworkObject>> _poolTable = new();

		protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
		{
			NetworkObject instance = null;

			if (ShouldPool(prefab) == true)
			{
				instance = GetFromObjetPool(prefab);
				instance.transform.position = Vector3.zero;
			}

			if (instance == null)
			{
				instance = Instantiate(prefab);
			}

			return instance;
		}

		private NetworkObject GetFromObjetPool(NetworkObject prefab)
		{
			NetworkObject instance = null;

			if (_poolTable.TryGetValue(prefab.NetworkTypeId, out var pool) == true)
			{
				if (pool.TryPop(out var pooledObject) == true)
				{
					instance = pooledObject;
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
			Debug.Log($"[NetworkObject Pool] New Object {prefab.NetworkTypeId}");

			if (_poolTable.TryGetValue(prefab.NetworkTypeId, out var stack) == false)
			{
				stack = new Stack<NetworkObject>();
				_poolTable.Add(prefab.NetworkTypeId, stack);
			}

			return instance;
		}

		private bool ShouldPool(NetworkObject prefab)
		{
			foreach (var poolableObjct in _poolableObjects)
			{
				if (prefab == poolableObjct)
					return true;
			}
			return false;
		}

		protected override void DestroyPrefabInstance(NetworkRunner runner, NetworkPrefabId prefabId, NetworkObject instance)
		{
			Debug.Log($"[NetworkObject Pool] Despawn {prefabId}");
			if (_poolTable.TryGetValue(prefabId, out var stack) == true)
			{
				instance.transform.SetParent(null);
				instance.gameObject.SetActive(false);
				stack.Push(instance);
			}
			else
			{
				Destroy(instance.gameObject);
			}
		}
	}
}
