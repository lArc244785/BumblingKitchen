using UnityEngine;
using Fusion;
using UnityEngine.UI;
using System.Collections.Generic;

namespace BumblingKitchen.SessionRoom
{
	public struct NetworkUserInfo : INetworkStruct
	{
		public NetworkString<_16> playerId;
		public NetworkString<_16> playerName;
		public int characterId;
		public NetworkBool isReady;

		public NetworkUserInfo(NetworkString<_16> playerId, NetworkString<_16> playerName, int characterId, NetworkBool isReady)
		{
			this.playerId = playerId;
			this.playerName = playerName;
			this.characterId = characterId;
			this.isReady = isReady;
		}
	}


	public class SessionRoom: NetworkBehaviour
	{
		[SerializeField] private Button _ready;
		[SerializeField] private DeshBoard _board;

		[Networked]
		[Capacity(4)]
		public NetworkLinkedList<NetworkUserInfo> _users { get;}
		= MakeInitializer(new NetworkUserInfo[] { });

		private ChangeDetector _changeDetector;

		public override void Spawned()
		{
			Debug.Log("Spawned");
			_changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);


			string playerName = PlayerPrefs.GetString("Name");
			int characterID = PlayerPrefs.GetInt("CharacterID");
			RPC_AddUserInfo(new NetworkUserInfo(Runner.UserId, playerName, characterID, false));

			_ready.onClick.AddListener(() =>
			{
				RPC_Ready(Runner.UserId);
			});
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_AddUserInfo(NetworkUserInfo info)
		{
			_users.Add(info);
		}

		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		private void RPC_Draw()
		{
			_board.Draw(_users);
		}


		private void PlayerLeft(NetworkRunner runner, PlayerRef player)
		{
			Debug.Log("Player Left!!!!");
			if (TryFindUesrInfo(runner.UserId, out var info))
			{
				_users.Remove(info);
			}
		}

		private bool TryFindUesrInfo(string playerId, out NetworkUserInfo info)
		{
			info = default;

			if (TryFindUserInfoIndex(playerId, out var index))
			{
				info = _users[index];
				return true;
			}

			return false;
		}

		private bool TryFindUserInfoIndex(string playerId, out int index)
		{
			index = -1;
			for (int i = 0; i < _users.Count; i++)
			{
				if (_users[i].playerId == playerId)
				{
					index = i;
					return true;
				}
			}
			return false;
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_Ready(NetworkString<_16> userID)
		{
			if(TryFindUserInfoIndex(userID.ToString(), out var index))
			{
				NetworkUserInfo info = _users[index];
				info.isReady = !info.isReady;
				_users.Set(index, info);
				//RPC_Draw();
			}
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			base.Despawned(runner, hasState);
			if(runner.IsServer)
			{
				FusionConnection.Instance.OnPlayerLeftEvent -= PlayerLeft;
			}
		}

		public override void Render()
		{
			foreach (var change in _changeDetector.DetectChanges(this))
			{
				switch (change)
				{
					case nameof(_users):
						_board.Draw(_users);
						break;
				}
			}
		}

	}
}
