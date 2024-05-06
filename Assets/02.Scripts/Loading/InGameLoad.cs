using System;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace BumblingKitchen
{
	/// <summary>
	/// �ε� ���μ���
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
		/// ������ �ε� ���μ��� �ܰ�
		/// </summary>
		[SerializeField] private LoadProcess _process;

		/// <summary>
		/// NetworkRunner�� �����ϰ� ����� �� ���� ����ϴ� Ÿ�̸�
		/// </summary>
		private TickTimer _stabilizationTimer;
		
		/// <summary>
		/// stabilizationTimer�� Ÿ�̸� �ð�
		/// </summary>
		private float _stabilizationTime = 1.0f;
		
		/// <summary>
		/// ��� ���μ����� �Ϸ��ϰ� ����ϴ� Ÿ�̸�
		/// </summary>
		private TickTimer _complentWaitTimer;

		/// <summary>
		/// complentWaitTimer�� Ÿ�̸� �ð�
		/// </summary>
		private float _complentWaitTime = 1.0f;
		
		/// <summary>
		/// �÷��̾��� �غ� ���¸� ��� ���̺�
		/// </summary>
		private Dictionary<PlayerRef, bool> _playerReadyTable;
		#endregion


		#region Event
		/// <summary>
		/// ��Ʈ��ũ Ǯ���ؾߵ� ������Ʈ�� �����ϴ� �̺�Ʈ
		/// </summary>
		public event Action SpawningNetworkPooledObjects;

		/// <summary>
		/// ��� ���μ��� �Ϸ�� ȣ��Ǵ� �̺�Ʈ
		/// </summary>
		public event Action ProcessCompeleted;

		/// <summary>
		/// ���μ��� ������Ʈ �� ȣ��Ǵ� �̺�Ʈ
		/// </summary>
		public event Action<float> UpdattingProgress;

		/// <summary>
		/// �÷��̾� ĳ���� ������ �Ϸ�Ǿ��� �� ȣ��Ǵ� �̺�Ʈ
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
		/// �ε� ���μ����� �����մϴ�.
		/// </summary>
		private void StartProcess()
		{
			_process = LoadProcess.None;
			UpdateProcess(LoadProcess.LoadStabilization);
		}

		/// <summary>
		/// �ε��� ���� ������Ʈ�ϰ� ���¿� ���߾� ���μ����� �����մϴ�.
		/// </summary>
		/// <param name="process"></param>
		private void UpdateProcess(LoadProcess process)
		{
			_process = process;
			switch (_process)
			{
				//�ε� ��ũ��Ʈ�� Runner�� ���������� ����� �Ϸ�� ������ ����մϴ�.
				case LoadProcess.LoadStabilization:
					_stabilizationTimer = TickTimer.CreateFromSeconds(Runner, _stabilizationTime);
					break;
				//�� ���Ӿ��� �ε� �մϴ�.
				case LoadProcess.LoadInGameScene:
					LoadInGameScene();
					break;
				//�÷��̾� ĳ���͸� ��ȯ�մϴ�.
				case LoadProcess.SpawnPlayer:
					PlayerSpawned?.Invoke();
					break;
				//��� �÷��̾ �غ� �Ϸ�� �� ���� ����մϴ�.
				case LoadProcess.WaitAllClientStabilization:
					RPC_PlayerReady(Runner.LocalPlayer);
					break;
				//��Ʈ��ũ Ǯ ������Ʈ�� �غ��մϴ�.
				case LoadProcess.SetupPoolNetworkobject:
					SpawningNetworkPooledObjects?.Invoke();
					break;
				//��� �غ� �Ϸ�Ǿ��� ���� �ð� ��� �� ���μ����� �����մϴ�.
				case LoadProcess.Complent:
					WaitComplent();
					break;
			}
			OnUpdateProgress();
		}

		/// <summary>
		/// ���μ����� ������Ʈ �� ��ü ���μ��� �ܰ迡�� ��ŭ ����Ǿ������� ����ϰ� �ش� �̺�Ʈ�� ȣ���մϴ�.
		/// </summary>
		private void OnUpdateProgress()
		{
			int currentProcess = (int)_process;
			int totalProcess = (int)LoadProcess.Total;

			float progress = (float)currentProcess / (float)totalProcess;
			UpdattingProgress?.Invoke(progress);
		}

		/// <summary>
		/// ��� ���μ����� �Ϸ��ϰ� ��� ����ϴ� Ÿ�̸��� �ð��� �����մϴ�.
		/// </summary>
		private void WaitComplent()
		{
			_complentWaitTimer = TickTimer.CreateFromSeconds(Runner, 1.0f);
		}

		/// <summary>
		/// ���ӿ� ������ �÷��̾ �غ� ���� ���̺��� �����մϴ�.
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
		/// ���� ���� ȣ���մϴ�.
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
		/// Ÿ�̸Ӱ� �����Ǿ��� �� �̸� Ȯ���Ͽ� Ÿ�̸Ӱ� ������ �ش� ���μ����� ȣ���մϴ�.
		/// </summary>
		public override void Render()
		{
			//�ش� Ÿ�̸Ӹ� �ʰ��� ������ �����ϸ� ����ȭ�� �Ǿ��ٰ� �Ǵ�.
			if(_process == LoadProcess.LoadStabilization &&
				_stabilizationTimer.Expired(Runner) == true)
			{
				_stabilizationTimer = TickTimer.None;
				GameObject.Find("Canvas Loading").GetComponent<LoadingUI>().Init(this);
				SetupPlayerReadyTable();
				UpdateProcess(LoadProcess.LoadInGameScene);
			}

			//���μ����� �Ϸ� �� ��� ����Ͽ��ٰ� ������.
			if(_process == LoadProcess.Complent &&
				_complentWaitTimer.Expired(Runner) == true)
			{
				ProcessCompeleted?.Invoke();
				Runner.UnloadScene(SceneRef.FromIndex(3));
				_complentWaitTimer = TickTimer.None;
			}
		}

		/// <summary>
		/// �� ���� ���� ����ȭ�� �Ϸ�Ǿ� ���μ��� ���¸� �÷��̾� ���� ���·� ������Ʈ �մϴ�.
		/// </summary>
		/// <param name="spawnPlayer"></param>
		public void InGameStabilizationed(Action spawnPlayer)
		{
			PlayerSpawned = spawnPlayer;
			UpdateProcess(LoadProcess.SpawnPlayer);
		}

		/// <summary>
		/// �÷��̾� ĳ���Ͱ� ��ȯ�� �Ϸ�Ǿ� ���μ��� ���¸� �ٸ� �÷��̾� ��� ���·� ������Ʈ �մϴ�.
		/// </summary>
		/// <param name="runner"></param>
		public void SpawedPlayerCharacter(NetworkRunner runner)
		{
			UpdateProcess(LoadProcess.WaitAllClientStabilization);
		}

		/// <summary>
		/// [RPC] �÷��̾��� �غ� �Ǿ����� ���������� �˷��ݴϴ�.
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
		/// [RPC] ������ ��� �÷��̾ �Ϸ�Ǿ����� ��� Ŭ���̾�Ʈ���� �˷��ݴϴ�.
		/// </summary>

		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		private void RPC_CompleteSpawnAllPlayer()
		{
			UpdateProcess(LoadProcess.SetupPoolNetworkobject);
		}

		/// <summary>
		/// [RPC] �����Ͱ� ��� Ŭ���̾�Ʈ���� ��� ���μ����� �Ϸ�Ǿ��ٰ� �˷��ݴϴ�.
		/// </summary>
		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		public void RPC_SpawnedNetworkObject()
		{
			UpdateProcess(LoadProcess.Complent);
		}
		#endregion
	}
}
