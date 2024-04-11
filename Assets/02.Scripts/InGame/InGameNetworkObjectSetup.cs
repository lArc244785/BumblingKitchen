using BumblingKitchen.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

namespace BumblingKitchen
{
	[Serializable]
    public struct SpawnInfo
	{
        public NetworkObject prefab;
        public int count;
	}

    public class InGameNetworkObjectSetup : NetworkBehaviour
    {
        [SerializeField] List<SpawnInfo> _spawnObjects;
        private List<NetworkObject> _spawnedObjects = new();

        private int _spawnCount = 0;
        public event Action OnCompletSpawn;

		public override void Spawned()
		{
            if (HasStateAuthority == false)
                return;
        }

		public void SetUpNetworkObject()
		{
            if (HasStateAuthority == false)
                return;

            foreach(var spawninfo in _spawnObjects)
			{
                _spawnCount += spawninfo.count;
			}

            foreach (var spawninfo in _spawnObjects)
            {
                int count = spawninfo.count;
                while(count > 0)
				{
                    Runner.Spawn(spawninfo.prefab,
                        onBeforeSpawned: SpawedObject);
                    count--;
				}
            }
        }

        private void SpawedObject(NetworkRunner runner, NetworkObject obj)
		{
            runner.MoveGameObjectToScene(obj.gameObject, SceneRef.FromIndex(4));
            _spawnedObjects.Add(obj);

            _spawnCount--;
            if (_spawnCount == 0)
            {
                OnCompletSpawn?.Invoke();
            }
        }

        public void AllDespawn()
		{
            foreach(var obj in _spawnedObjects)
			{
                Runner.Despawn(obj);
			}
		}
    }
}
