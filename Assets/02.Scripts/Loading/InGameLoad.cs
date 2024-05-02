using System;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace BumblingKitchen
{
	/// <summary>
	/// 로딩 프로세스
	/// </summary>
	public enum LoadProcess
	{
		None,
		LoadStabilization,
		LoadInGameScene,
		SpawnPlayer,
		WaitAllClientStabilization,
		SetupPoolNetworkobject,
		Complent,
		Total
	}

	public class InGameLoad : NetworkBehaviour
	{
		// PUBLIC	=======================================


		// PRIVATE	======================================
		[SerializeField] private LoadProcess _process;

		private TickTimer _stabilizationTimer;
		private TickTimer _complentWaitTimer;
		//Master Only
		private Dictionary<PlayerRef, bool> _playerReadyTable;

		// EVENT	=======================================
		public event Action SpawningNetworkPooledObjects;
		public event Action ProcessCompeleted;
		public event Action<float> UpdattingProgress;
		private event Action PlayerSpawned;
		//=================================================

		private void Awake()
		{
			_stabilizationTimer = TickTimer.None;
			_complentWaitTimer = TickTimer.None;
		}

		private void StartProcess()
		{
			_process = LoadProcess.None;
			NextProcess(LoadProcess.LoadStabilization);
		}

		private void NextProcess(LoadProcess process)
		{
			_process = process;
			switch (_process)
			{
				//로딩 스크립트가 Runner에 안정적으로 등록이 완료될 때까지 대기합니다.
				case LoadProcess.LoadStabilization:
					_stabilizationTimer = TickTimer.CreateFromSeconds(Runner, 1.0f);
					break;
				//인 게임씬을 로딩 합니다.
				case LoadProcess.LoadInGameScene:
					LoadInGameScene();
					break;
				//플레이어 캐릭터를 소환합니다.
				case LoadProcess.SpawnPlayer:
					PlayerSpawned?.Invoke();
					break;
				//모든 플레이어가 준비가 완료될 때 까지 대기합니다.
				case LoadProcess.WaitAllClientStabilization:
					RPC_PlayerReady(Runner.LocalPlayer);
					break;
				//네트워크 풀 오브젝트를 준비합니다.
				case LoadProcess.SetupPoolNetworkobject:
					SpawningNetworkPooledObjects?.Invoke();
					break;
				//모든 준비가 완료되었고 일정 시간 대기 후 프로세스를 종료합니다.
				case LoadProcess.Complent:
					WaitComplent();
					break;
			}
			UpdateProgress();
		}

		private void UpdateProgress()
		{
			int currentProcess = (int)_process;
			int totalProcess = (int)LoadProcess.Total;

			float progress = (float)currentProcess / (float)totalProcess;
			UpdattingProgress?.Invoke(progress);
		}

		public override void Spawned()
		{
			base.Spawned();
			StartProcess();
		}

		private void WaitComplent()
		{
			_complentWaitTimer = TickTimer.CreateFromSeconds(Runner, 1.0f);
		}

		private void SetupPlayerReadyTable()
		{
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
			//해당 타이머를 초과할 때까지 생존하면 안정화가 되었다고 판다.
			if(_process == LoadProcess.LoadStabilization &&
				_stabilizationTimer.Expired(Runner) == true)
			{
				_stabilizationTimer = TickTimer.None;
				GameObject.Find("Canvas Loading").GetComponent<LoadingUI>().Init(this);
				SetupPlayerReadyTable();
				NextProcess(LoadProcess.LoadInGameScene);
			}

			//프로세스를 완료 후 잠시 대기하였다가 끝낸다.
			if(_process == LoadProcess.Complent &&
				_complentWaitTimer.Expired(Runner) == true)
			{
				ProcessCompeleted?.Invoke();
				Runner.UnloadScene(SceneRef.FromIndex(3));
				_complentWaitTimer = TickTimer.None;
			}
		}

		public void InGameStabilizationed(Action spawnPlayer)
		{
			PlayerSpawned = spawnPlayer;
			NextProcess(LoadProcess.SpawnPlayer);
		}

		public void SpawedPlayerCharacter(NetworkRunner runner)
		{
			NextProcess(LoadProcess.WaitAllClientStabilization);
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_PlayerReady(PlayerRef player)
		{
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
			NextProcess(LoadProcess.SetupPoolNetworkobject);
		}

		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		public void RPC_SpawnedNetworkObject()
		{
			NextProcess(LoadProcess.Complent);
		}

		private void OnDestroy()
		{
			_stabilizationTimer = TickTimer.None;
		}
	}
}
