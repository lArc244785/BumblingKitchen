using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen
{
	public class NetworkObjectPool : NetworkObjectProviderDefault
	{
        [SerializeField] private List<NetworkObject> _poolableObjects;

        private Dictionary<NetworkObjectTypeId, Stack<NetworkObject>> _free = new();

        protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
        {
            if (ShouldPool(runner, prefab))
            {
                Debug.Log("Object Pool");
                var instance = GetObjectFromPool(prefab);

                instance.transform.position = Vector3.zero;

                return instance;
            }

            return Instantiate(prefab);
        }

        protected override void DestroyPrefabInstance(NetworkRunner runner, NetworkPrefabId prefabId, NetworkObject instance)
        {
            if (_free.TryGetValue(instance.NetworkTypeId, out var stack))
            {
                Debug.Log("Input Object Pool");
                instance.gameObject.SetActive(false);
                stack.Push(instance);
            }
            else
            {
                Debug.Log("Desotry");
                Destroy(instance.gameObject);
            }
        }


        private NetworkObject GetObjectFromPool(NetworkObject prefab)
        {
            NetworkObject instance = null;

            if (_free.TryGetValue(prefab.NetworkTypeId, out var stack))
            {
                while (stack.Count > 0 && instance == null)
                {
                    instance = stack.Pop();
                }
            }

            if (instance == null)
                instance = GetNewInstance(prefab);

            instance.gameObject.SetActive(true);
            return instance;
        }

        private NetworkObject GetNewInstance(NetworkObject prefab)
        {
            NetworkObject instance = Instantiate(prefab);

            if (_free.TryGetValue(prefab.NetworkTypeId, out var stack) == false)
            {
                stack = new Stack<NetworkObject>();
                _free.Add(prefab.NetworkTypeId, stack);
                Debug.Log($"free add {prefab.NetworkTypeId}");
            }

            return instance;
        }

        private bool ShouldPool(NetworkRunner runner, NetworkObject prefab)
        {
            if (_poolableObjects.Count == 0)
            {
                return true;
            }

            return IsPoolableObject(prefab);
        }

        private bool IsPoolableObject(NetworkObject networkObject)
        {
            foreach (var poolableObject in _poolableObjects)
            {
                if (networkObject == poolableObject)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
