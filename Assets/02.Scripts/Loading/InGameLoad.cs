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
		#region Field
		/// <summary>
		/// 현재의 로딩 프로세스 단계
		/// </summary>
		[SerializeField] private LoadProcess _process;

		/// <summary>
		/// NetworkRunner에 안정하게 등록할 때 까지 대기하는 타이머
		/// </summary>
		private TickTimer _stabilizationTimer;
		
		/// <summary>
		/// stabilizationTimer의 타이머 시간
		/// </summary>
		private float _stabilizationTime = 1.0f;
		
		/// <summary>
		/// 모든 프로세스를 완료하고 대기하는 타이머
		/// </summary>
		private TickTimer _complentWaitTimer;

		/// <summary>
		/// complentWaitTimer의 타이머 시간
		/// </summary>
		private float _complentWaitTime = 1.0f;
		
		/// <summary>
		/// 플레이어의 준비 상태를 담는 테이블
		/// </summary>
		private Dictionary<PlayerRef, bool> _playerReadyTable;
		#endregion


		#region Event
		/// <summary>
		/// 네트워크 풀링해야될 오브젝트를 스폰하는 이벤트
		/// </summary>
		public event Action SpawningNetworkPooledObjects;

		/// <summary>
		/// 모든 프로세스 완료시 호출되는 이벤트
		/// </summary>
		public event Action ProcessCompeleted;

		/// <summary>
		/// 프로세스 업데이트 시 호출되는 이벤트
		/// </summary>
		public event Action<float> UpdattingProgress;

		/// <summary>
		/// 플레이어 캐릭터 생성이 완료되었을 때 호출되는 이벤트
		/// </summary>
		private event Action PlayerSpawned;
		#endregion


		#region Method
		private void Awake()
		{
			_stabilizationTimer = TickTimer.None;
			_complentWaitTimer = TickTimer.None;
		}

		public override void Spawned()
		{
			base.Spawned();
			StartProcess();
		}

		/// <summary>
		/// 로딩 프로세스를 시작합니다.
		/// </summary>
		private void StartProcess()
		{
			_process = LoadProcess.None;
			UpdateProcess(LoadProcess.LoadStabilization);
		}

		/// <summary>
		/// 로딩의 상태 업데이트하고 상태에 맞추어 프로세스를 진행합니다.
		/// </summary>
		/// <param name="process"></param>
		private void UpdateProcess(LoadProcess process)
		{
			_process = process;
			switch (_process)
			{
				//로딩 스크립트가 Runner에 안정적으로 등록이 완료될 때까지 대기합니다.
				case LoadProcess.LoadStabilization:
					_stabilizationTimer = TickTimer.CreateFromSeconds(Runner, _stabilizationTime);
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
			OnUpdateProgress();
		}

		/// <summary>
		/// 프로세스가 업데이트 시 전체 프로세스 단계에서 얼만큼 진행되었는지를 계산하고 해당 이벤트를 호출합니다.
		/// </summary>
		private void OnUpdateProgress()
		{
			int currentProcess = (int)_process;
			int totalProcess = (int)LoadProcess.Total;

			float progress = (float)currentProcess / (float)totalProcess;
			UpdattingProgress?.Invoke(progress);
		}

		/// <summary>
		/// 모든 프로세스를 완료하고 잠시 대기하는 타이머의 시간을 설정합니다.
		/// </summary>
		private void WaitComplent()
		{
			_complentWaitTimer = TickTimer.CreateFromSeconds(Runner, 1.0f);
		}

		/// <summary>
		/// 게임에 참여한 플레이어에 준비 상태 테이블을 생성합니다.
		/// </summary>
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

		/// <summary>
		/// 게임 씬을 호출합니다.
		/// </summary>
		private void LoadInGameScene()
		{
			if(HasStateAuthority == true)
			{
				int ingameSceneIndex = PlayerPrefs.GetInt("InGameStage");
				Runner.LoadScene(SceneRef.FromIndex(ingameSceneIndex), LoadSceneMode.Additive);
			}
		}

		/// <summary>
		/// 타이머가 설정되었을 때 이를 확인하여 타이머가 경과대면 해당 프로세스를 호출합니다.
		/// </summary>
		public override void Render()
		{
			//해당 타이머를 초과할 때까지 생존하면 안정화가 되었다고 판다.
			if(_process == LoadProcess.LoadStabilization &&
				_stabilizationTimer.Expired(Runner) == true)
			{
				_stabilizationTimer = TickTimer.None;
				GameObject.Find("Canvas Loading").GetComponent<LoadingUI>().Init(this);
				SetupPlayerReadyTable();
				UpdateProcess(LoadProcess.LoadInGameScene);
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

		/// <summary>
		/// 인 게임 씬이 안정화가 완료되어 프로세스 상태를 플레이어 스폰 상태로 업데이트 합니다.
		/// </summary>
		/// <param name="spawnPlayer"></param>
		public void InGameStabilizationed(Action spawnPlayer)
		{
			PlayerSpawned = spawnPlayer;
			UpdateProcess(LoadProcess.SpawnPlayer);
		}

		/// <summary>
		/// 플레이어 캐릭터가 소환이 완료되어 프로세스 상태를 다른 플레이어 대기 상태로 업데이트 합니다.
		/// </summary>
		/// <param name="runner"></param>
		public void SpawedPlayerCharacter(NetworkRunner runner)
		{
			UpdateProcess(LoadProcess.WaitAllClientStabilization);
		}

		/// <summary>
		/// [RPC] 플레이어의 준비가 되었음을 마스터한테 알려줍니다.
		/// </summary>
		/// <param name="player"></param>
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

		/// <summary>
		/// [RPC] 마스터 모든 플레이어가 완료되었음을 모든 클라이언트에게 알려줍니다.
		/// </summary>

		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		private void RPC_CompleteSpawnAllPlayer()
		{
			UpdateProcess(LoadProcess.SetupPoolNetworkobject);
		}

		/// <summary>
		/// [RPC] 마스터가 모든 클라이언트에게 모든 프로세스가 완료되었다고 알려줍니다.
		/// </summary>
		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		public void RPC_SpawnedNetworkObject()
		{
			UpdateProcess(LoadProcess.Complent);
		}
		#endregion
	}
}
