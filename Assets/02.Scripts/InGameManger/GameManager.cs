using UnityEngine;
using Fusion;
using System;
using Random = UnityEngine.Random;

namespace BumblingKitchen
{
	public class GameManager : NetworkBehaviour
	{
		[SerializeField] private NetworkPrefabRef _palyerPrefab;

		public override void Spawned()
		{
			base.Spawned();
			if(HasStateAuthority == true)
			{
				foreach(var player in FusionConnection.Instance.connectPlayers)
				{
					SpawnPlayer(player);
				}
			}
		}

		private void SpawnPlayer(PlayerRef inputPlayer)
		{
			float x = Random.RandomRange(-1.0f, 1.0f);
			float z = Random.RandomRange(-1.0f, 1.0f);
			Vector3 spawnPoint = new Vector3(x, 0.0f, z);

			var player = Runner.Spawn(_palyerPrefab, spawnPoint, Quaternion.identity, inputPlayer);
		}
	}
}
