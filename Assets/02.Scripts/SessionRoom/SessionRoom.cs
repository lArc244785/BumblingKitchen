using UnityEngine;
using Fusion;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using TMPro;

namespace BumblingKitchen.SessionRoom
{
	public struct NetworkUserInfo : INetworkStruct, IEquatable<NetworkUserInfo>
	{
		public PlayerRef playerRef;
		public NetworkString<_16> playerName;
		public int characterId;
		public NetworkBool isReady;

		public NetworkUserInfo(PlayerRef playerRef, NetworkString<_16> playerName, int characterId, NetworkBool isReady)
		{
			this.playerRef = playerRef;
			this.playerName = playerName;
			this.characterId = characterId;
			this.isReady = isReady;
		}

		public bool Equals(NetworkUserInfo other)
		{
			return (playerRef == other.playerRef);
		}
	}


	public class SessionRoom: NetworkBehaviour
	{
		[SerializeField] private Button _ready;
		[SerializeField] private Button _exit;
		[SerializeField] private DeshBoard _board;
		[SerializeField] private TMP_Text _startWaitText;
		[SerializeField] protected TMP_Text _sessionName;

		[Networked]
		[Capacity(4)]
		public NetworkLinkedList<NetworkUserInfo> _users { get;}
		= MakeInitializer(new NetworkUserInfo[] { });

		private ChangeDetector _changeDetector;

		private TickTimer _readyWaitTimer;
		private TickTimer _readyWaitDrawTimer;
		private float _readyDelay = 3.0f;

		private void Awake()
		{
			PlayerPrefs.SetInt("InGameStage", 4);
		}

		public override void Spawned()
		{
			Debug.Log("Spawned");
			_changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

			_sessionName.text = Runner.SessionInfo.Name;
			string playerName = PlayerPrefs.GetString("Name");
			int characterID = PlayerPrefs.GetInt("CharacterID");
			RPC_AddUserInfo(new NetworkUserInfo(Runner.LocalPlayer, playerName, characterID, false));

			_ready.onClick.AddListener(() =>
			{
				RPC_OnReayPlayer(Runner.LocalPlayer);
			});

			_exit.onClick.AddListener(() =>
			{
				FusionConnection.Instance.ExitSessionToTitle();
			});

			FusionConnection.Instance.OnPlayerLeftEvent += PlayerLeft;
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
			if (HasStateAuthority == false)
				return;

			if (TryFindUesrInfo(player, out var info))
			{
				_users.Remove(info);
			}
		}

		private bool TryFindUesrInfo(PlayerRef player, out NetworkUserInfo info)
		{
			info = default;

			if (TryFindUserInfoIndex(player, out var index))
			{
				info = _users[index];
				return true;
			}

			return false;
		}

		private bool TryFindUserInfoIndex(PlayerRef player, out int index)
		{
			index = -1;
			for (int i = 0; i < _users.Count; i++)
			{
				if (_users[i].playerRef.Equals(player) == true)
				{
					index = i;
					return true;
				}
			}
			return false;
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_OnReayPlayer(PlayerRef player)
		{
			if(TryFindUserInfoIndex(player, out var index))
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
			FusionConnection.Instance.OnPlayerLeftEvent -= PlayerLeft;
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
					Runner.SessionInfo.IsOpen = false;
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
