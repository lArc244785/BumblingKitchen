using UnityEngine;
using Fusion;
using System;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

namespace BumblingKitchen
{
	public enum GameState
	{
		None,
		Wait,
		Play,
		End,
	}

	public class GameManager : NetworkBehaviour
	{
		[SerializeField] private NetworkPrefabRef _palyerPrefab;
		[Networked] public GameState State { get; private set; } = GameState.Wait;
		[Networked] public float PlayTime { get; private set; }
		private float _endTime = 120.0f;

		private int _readyPlayer = 0;

		public event Action OnStarttingGame;
		public event Action OnEnddingGame;

		public static GameManager Instance { get; private set; }

		private void Awake()
		{
			Instance = this;
		}

		public override void Spawned()
		{
			SpawnPlayer(Runner.LocalPlayer);
		}

		private void SpawnPlayer(PlayerRef inputPlayer)
		{
			if (inputPlayer != Runner.LocalPlayer)
				return;

			float x = Random.RandomRange(-1.0f, 1.0f);
			float z = Random.RandomRange(-1.0f, 1.0f);
			Vector3 spawnPoint = new Vector3(x, 0.0f, z);

			var player = Runner.Spawn(
				_palyerPrefab,
				spawnPoint,
				Quaternion.identity,
				inputPlayer,
				PlayerReady); ;
			Debug.Log($"Spawn {inputPlayer.PlayerId}");
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_PlayerReady()
		{
			_readyPlayer++;
			if (FusionConnection.Instance.connectPlayers.Count == _readyPlayer)
			{
				NextGameState(GameState.Play);
			}
		}

		private void PlayerReady(NetworkRunner runner, NetworkObject obj)
		{
			RPC_PlayerReady();
		}

		public override void Render()
		{
			if (HasStateAuthority == false)
				return;

			if(State == GameState.Play)
			{
				PlayTime += Runner.DeltaTime;
				if (PlayTime >= _endTime)
					NextGameState(GameState.End);
			}
		}

		private void NextGameState(GameState nextState)
		{
			switch (nextState)
			{
				case GameState.Play:
					Debug.Log("GameStart!");
					RPC_CallGameStartEvent();
					break;
				case GameState.End:
					RPC_CallGameEndEvent();
					Next();
					break;
			}

			State = nextState;
		}

		private void Next()
		{
			if (Runner.IsSceneAuthority == false)
				return;

			Debug.Log("게임 종료 결과 씬으로 옮겨주세요");
			Runner.LoadScene(SceneRef.FromIndex(4), LoadSceneMode.Single);
		}

		public int GetEndTime()
		{
			int playTime = (int)PlayTime;
			return (int)_endTime - playTime;
		}

		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		private void RPC_CallGameStartEvent()
		{
			OnStarttingGame?.Invoke();
		}
		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		private void RPC_CallGameEndEvent()
		{
			OnEnddingGame?.Invoke();
		}
	}
}
