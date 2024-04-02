using UnityEngine;
using Fusion;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using TMPro;

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
		[SerializeField] private TMP_Text _startWaitText;

		[Networked]
		[Capacity(4)]
		public NetworkLinkedList<NetworkUserInfo> _users { get;}
		= MakeInitializer(new NetworkUserInfo[] { });

		private ChangeDetector _changeDetector;

		private TickTimer _readyWaitTimer;
		private TickTimer _readyWaitDrawTimer;
		private float _readyDelay = 3.0f;

		public override void Spawned()
		{
			Debug.Log("Spawned");
			_changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

			string playerName = PlayerPrefs.GetString("Name");
			int characterID = PlayerPrefs.GetInt("CharacterID");
			RPC_AddUserInfo(new NetworkUserInfo(Runner.UserId, playerName, characterID, false));

			_ready.onClick.AddListener(() =>
			{
				RPC_OnReayPlayer(Runner.UserId);
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
		private void RPC_OnReayPlayer(NetworkString<_16> userID)
		{
			if(TryFindUserInfoIndex(userID.ToString(), out var index))
			{
				NetworkUserInfo info = _users[index];
				info.isReady = !info.isReady;
				_users.Set(index, info);
				if(IsAllReady())
				{
					_readyWaitTimer = TickTimer.CreateFromSeconds(Runner, _readyDelay);
					_readyWaitDrawTimer = TickTimer.CreateFromSeconds(Runner, 0.0f);
				}
				else
				{
					_readyWaitTimer = TickTimer.None;
					RPC_DrawReadyWaitText(-1.0f);
				}
			}
		}

		private bool IsAllReady()
		{
			foreach(NetworkUserInfo info in _users)
			{
				if(!info.isReady)
					return false;
			}
			return true;
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

		public override void FixedUpdateNetwork()
		{
			if(_readyWaitTimer.IsRunning)
			{
				if (_readyWaitDrawTimer.Expired(Runner) == true)
				{
					float t = _readyWaitTimer.RemainingTime(Runner) ?? 0.0f;
					RPC_DrawReadyWaitText(t);
					_readyWaitDrawTimer = TickTimer.CreateFromSeconds(Runner, 1.0f);
				}
			}

			if(_readyWaitTimer.Expired(Runner))
			{
				if(Runner.IsSceneAuthority)
				{
					Runner.LoadScene(SceneRef.FromIndex(3), LoadSceneMode.Single);
				}
			}

		}


		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		private void RPC_DrawReadyWaitText(float time)
		{
			if(time <= 0.0f)
			{
				_startWaitText.text = String.Empty;
				return;
			}

			_startWaitText.text = $"Ready Wait {time.ToString("F0")}";
		}


	}
}
