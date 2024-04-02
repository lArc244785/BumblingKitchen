using UnityEngine;
using Fusion;
using System;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace BumblingKitchen
{
	public enum GameState
	{
		None,
		Wait,
		Play,
		End,
	}

	public class GameManager : NetworkBehaviour, IGameStateEvent
	{
		[SerializeField] private CharacterSpawner _characterSpanwer;
		[SerializeField] private NetworkPrefabRef _palyerPrefab;
		[Networked] public GameState State { get; private set; } = GameState.Wait;
		[Networked] public float PlayTime { get; private set; }
		private float _endTime = 120.0f;

		public event Action OnStarttingGame;
		public event Action OnEnddingGame;
		public event Action OnFinshedReady;

		public static GameManager Instance { get; private set; }

		public bool IsMove => State == GameState.Play;
		//Master Only
		private Dictionary<PlayerRef, bool> _playerReadyTable;

		private TickTimer _playWaitTimer;
		private TickTimer _endWaitTimer;

		private TickTimer _stabilizationTimer;

		private void Awake()
		{
			Instance = this;

		}

		public override void Spawned()
		{
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

		private void SpawnPlayer(PlayerRef inputPlayer)
		{
			int playerID = Runner.LocalPlayer.PlayerId - 1;
			int characterID = PlayerPrefs.GetInt("CharacterID");
			_characterSpanwer.SpawnPlayerCharacter(Runner, playerID, characterID, PlayerReady);
		}

		//플레이어블 캐릭터가 소환이 완료되고 CallBack한다.
		private void PlayerReady(NetworkRunner runner, NetworkObject obj)
		{
			RPC_PlayerReady(runner.LocalPlayer);
		}

		public override void FixedUpdateNetwork()
		{
			base.FixedUpdateNetwork();
			if(_stabilizationTimer.Expired(Runner) == true)
			{
				SpawnPlayer(Runner.LocalPlayer);
				_stabilizationTimer = TickTimer.None;
			}
		}

		public override void Render()
		{
			if (HasStateAuthority == false)
				return;

			//게임 시작하기전에 대기하고 시작한다.
			if(_playWaitTimer.Expired(Runner) == true)
			{
				NextGameState(GameState.Play);
				_playWaitTimer = TickTimer.None;
			}

			if(State == GameState.Play)
			{
				PlayTime += Runner.DeltaTime;
				if (PlayTime >= _endTime)
					NextGameState(GameState.End);
			}

			//게임이 종료 후 대기하였다가 결과 씬으로 넘겨준다.
			if(State == GameState.End && _endWaitTimer.Expired(Runner) == true)
			{
				GoToResult();
				_endWaitTimer = TickTimer.None;
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
					_endWaitTimer = TickTimer.CreateFromSeconds(Runner, 1.5f);
					break;
			}

			State = nextState;
		}

		private void GoToResult()
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

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_PlayerReady(PlayerRef player)
		{
			Debug.Log("PlayerReady");
			if (_playerReadyTable.ContainsKey(player) == false)
				return;

			_playerReadyTable[player] = true;

			foreach(var isReay in _playerReadyTable.Values)
			{
				if (isReay == false)
					return;
			}

			_playWaitTimer = TickTimer.CreateFromSeconds(Runner, 5.5f);
			RPC_CallFInshedReady();
		}

		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		private void RPC_CallFInshedReady()
		{
			OnFinshedReady?.Invoke();
		}

		private void OnDestroy()
		{
			Debug.Log("TA : Destroy");
		}
	}
}
