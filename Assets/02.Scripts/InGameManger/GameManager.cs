using UnityEngine;
using Fusion;
using System;

namespace BumblingKitchen
{
	public class GameManager : NetworkBehaviour
	{
		[SerializeField] private NetworkPrefabRef _palyerPrefab;

		public override void Spawned()
		{
			base.Spawned();
			SpawnPlayer();
		}

		private void SpawnPlayer()
		{
			var player = Runner.Spawn(_palyerPrefab, Vector3.zero, Quaternion.identity, Runner.LocalPlayer);
			Runner.SetPlayerObject(Runner.LocalPlayer, player);
		}
	}
}
