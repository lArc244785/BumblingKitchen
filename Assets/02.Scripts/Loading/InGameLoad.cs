using System;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace BumblingKitchen
{
	public class InGameLoad : NetworkBehaviour
	{
		public event Action OnLoadSceneStabilization;
		public event Action OnInGameLoaded;
		public event Action OnSpawnedPlayer;
		public event Action OnSpawnedAllPlayer;

		public event Action OnCompeleteLoad;

		private TickTimer _stabilizationTimer;
		private TickTimer _finshWaitTimer;

		private int _compeleteCheck;

		//Master Only
		private Dictionary<PlayerRef, bool> _playerReadyTable;

		public event Action<float> OnUpdateProgress;

		private void CompeletCheck()
		{
			_compeleteCheck++;
			OnUpdateProgress?.Invoke((float)_compeleteCheck / 4.0f);
		}

		private void Awake()
		{
			OnLoadSceneStabilization += CompeletCheck;
			OnInGameLoaded += CompeletCheck;
			OnSpawnedPlayer += CompeletCheck;
			OnSpawnedAllPlayer += CompeletCheck;

			OnLoadSceneStabilization += LoadInGameScene;
			OnSpawnedAllPlayer += FinshWait;

			_stabilizationTimer = TickTimer.None;
			_finshWaitTimer = TickTimer.None;
		}

		private void FinshWait()
		{
			Debug.Log("FinshWait");
			_finshWaitTimer = TickTimer.CreateFromSeconds(Runner, 1.0f);
		}

		public override void Spawned()
		{
			base.Spawned();
			Debug.Log("Loading Spawned");
			_stabilizationTimer = TickTimer.CreateFromSeconds(Runner, 1.0f);

			if (HasStateAuthority == true)
			{
				_playerReadyTable = new();
				foreach (var player in FusionConnection.Instance.connectPlayers)
				{
					_playerReadyTable.Add(player, false);
				}
			}
		}

		private void LoadInGameScene()
		{
			if(HasStateAuthority == true)
			{
				int ingameSceneIndex = PlayerPrefs.GetInt("InGameStage");
				Runner.LoadScene(SceneRef.FromIndex(ingameSceneIndex), LoadSceneMode.Additive);
			}
		}

		public override void Render()
		{
			if(_stabilizationTimer.Expired(Runner) == true)
			{
				Debug.Log("Loading Stabilization");
				_stabilizationTimer = TickTimer.None;
				GameObject.Find("Canvas Loading").GetComponent<LoadingUI>().Init(this);
				OnLoadSceneStabilization?.Invoke();
			}

			if (Runner.IsSceneAuthority == true)
			{
				if (_finshWaitTimer.Expired(Runner) == true)
				{
					OnCompeleteLoad?.Invoke();
					Runner.UnloadScene(SceneRef.FromIndex(3));

					_finshWaitTimer = TickTimer.None;
				}
			}
		}

		public void SpawedPlayerCharacter(NetworkRunner runner)
		{
			OnSpawnedPlayer?.Invoke();
			RPC_PlayerReady(runner.LocalPlayer);
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_PlayerReady(PlayerRef player)
		{
			Debug.Log("PlayerReady");
			if (_playerReadyTable.ContainsKey(player) == false)
				return;

			_playerReadyTable[player] = true;

			foreach (var isReay in _playerReadyTable.Values)
			{
				if (isReay == false)
					return;
			}

			RPC_CompleteSpawnAllPlayer();
		}

		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		private void RPC_CompleteSpawnAllPlayer()
		{
			OnSpawnedAllPlayer?.Invoke();
		}

		private void OnDestroy()
		{
			_stabilizationTimer = TickTimer.None;
		}

		public void CompleteLoadInGame()
		{
			OnInGameLoaded?.Invoke();
		}
	}
}
