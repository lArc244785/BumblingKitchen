using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

namespace BumblingKitchen
{
	public struct IngamePlayerData : INetworkStruct
	{
		public PlayerRef player;
		public int gold;
		public int spawnObject;
		public int succesCooking;
		public int failCooking;
		public int sendToOrder;
		public int cleanPlate;
	}

	public class InGameData : NetworkBehaviour
	{
		[Networked] public int Gold { get; set; }
		[Networked] public int GoelGold { get; set; }
		[Networked, Capacity(4)]
		public NetworkLinkedList<IngamePlayerData> PlayerDataList { get; }
		= MakeInitializer(new IngamePlayerData[] { });

		public static InGameData Instance { private set; get; }

		public event Action<int> OnUpdateGold;

		public override void Spawned()
		{
			if(Instance != null)
			{
				Runner.Despawn(Instance.Object);
			}

			Debug.Log("InitData!");
			Instance = this;
			DontDestroyOnLoad(gameObject);

			if (HasStateAuthority == false)
				return;

			foreach (var player in FusionConnection.Instance.connectPlayers)
			{
				var newPlayerData = new IngamePlayerData();
				newPlayerData.player = player;
				PlayerDataList.Add(newPlayerData);
			}
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void RPC_AddGold(int gold, PlayerRef player)
		{
			Debug.Log("AddGold");
			int playerIndex = FindIndex(player);
			Gold += gold;

			var newPlayerData = PlayerDataList[playerIndex];
			newPlayerData.gold += gold;

			PlayerDataList.Set(playerIndex, newPlayerData);
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void RPC_AddSpawnObject(PlayerRef player)
		{
			int playerIndex = FindIndex(player);

			var newPlayerData = PlayerDataList[playerIndex];
			newPlayerData.spawnObject++;

			PlayerDataList.Set(playerIndex, newPlayerData);

			Debug.Log($"Add SpawnObject {PlayerDataList[playerIndex].spawnObject}");
		}


		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void RPC_AddSuccesCooking(PlayerRef player)
		{
			Debug.Log("Add SuccesCooking");
			int playerIndex = FindIndex(player);
			
			var newPlayerData = PlayerDataList[playerIndex];
			newPlayerData.succesCooking++;
			PlayerDataList.Set(playerIndex, newPlayerData);
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void RPC_AddFailCooking(PlayerRef player)
		{
			Debug.Log("Add FailCooking");
			int playerIndex = FindIndex(player);

			var newPlayerData = PlayerDataList[playerIndex];
			newPlayerData.failCooking++;
			PlayerDataList.Set(playerIndex, newPlayerData);
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void RPC_AddSendToOrder(PlayerRef player)
		{
			Debug.Log("Add SendToOrder");
			int playerIndex = FindIndex(player);

			var newPlayerData = PlayerDataList[playerIndex];
			newPlayerData.sendToOrder++;
			PlayerDataList.Set(playerIndex, newPlayerData);
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void RPC_AddCleanPlate(PlayerRef player)
		{
			Debug.Log("Add CleanPlate");
			int playerIndex = FindIndex(player);

			var newPlayerData = PlayerDataList[playerIndex];
			newPlayerData.cleanPlate++;
			PlayerDataList.Set(playerIndex, newPlayerData);
		}

		private int FindIndex(PlayerRef player)
		{
			for (int i = 0; i < PlayerDataList.Count; i++)
			{
				if (PlayerDataList[i].player == player)
					return i;
			}
			return -1;
		}

		private void UpdateGold()
		{
			OnUpdateGold?.Invoke(Gold);
		}
	}


}
