using Fusion;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BumblingKitchen
{
	public enum GameState
	{
		None,
		Wait,
		Ready,
		Play,
		End,
	}

	public class GameManager : NetworkBehaviour, IGameStateEvent
	{
		private bool _isSpawned = false;
		private int _gameTime = 180;

		[SerializeField] private CharacterSpawner _characterSpanwer;
		[SerializeField] private NetworkPrefabRef _inGameData;
		[Networked, OnChangedRender(nameof(OnChangeGameState))] public GameState State { get; private set; } = GameState.Wait;
		[Networked] public TickTimer PlayTickTimer { private set; get; }

		public event Action OnReadying;
		public event Action OnPlaying;
		public event Action OnEnddingGame;
		public event Action OnFinshedReady;

		public static GameManager Instance { get; private set; }

		public bool IsMove => State == GameState.Play;


		private TickTimer _readyWaitTimer;
		private TickTimer _readyToPlayWaitTimer;
		private TickTimer _endWaitTimer;
		private TickTimer _stabilizationTimer;

		private InGameLoad _load;
		private NetworkObjectPool _networkObjectPool;

		private void Awake()
		{
			Instance = this;
		}

		private void OnChangeGameState()
		{
			switch (State)
			{
				case GameState.Ready:
					OnReadying?.Invoke();
					break;
				case GameState.Play:
					OnPlaying?.Invoke();
					break;
				case GameState.End:
					OnEnddingGame?.Invoke();
					break;
			}
		}

		public override void Spawned()
		{
			_stabilizationTimer = TickTimer.CreateFromSeconds(Runner, 1.0f);
		}

		public override void Render()
		{
			switch (State)
			{
				case GameState.Wait:
					WaitRender();
					break;
				case GameState.Ready:
					ReadyRender();
					break;
				case GameState.Play:
					PlayRender();
					break;
				case GameState.End:
					EndRender();
					break;
			}
		}

		private void SpawnPlayer()
		{
			int playerID = Runner.LocalPlayer.PlayerId - 1;
			int characterID = PlayerPrefs.GetInt("CharacterID");
			_characterSpanwer.SpawnPlayerCharacter(Runner, playerID, characterID, PlayerReady);
		}

		//플레이어블 캐릭터가 소환이 완료되고 CallBack한다.
		private void PlayerReady(NetworkRunner runner, NetworkObject obj)
		{
			Debug.Log("Spawned Player");
			_load.SpawedPlayerCharacter(runner);
			Runner.MoveGameObjectToScene(obj.gameObject, SceneRef.FromIndex(4));
		}

		private void SetUpLoad(InGameLoad load)
		{
			_load = load;
			if (HasStateAuthority == true)
			{
				_load.SpawningNetworkPooledObjects += SpawnNetworkObject;
				_load.CompeleteProcess += GameReadyWait;
			}
		}

		private void SpawnNetworkObject()
		{
			var netObjSetup = GameObject.Find("NetworkObjectSetUp").GetComponent<InGameNetworkObjectSetup>();
			netObjSetup.OnCompletSpawn += _load.RPC_SpawnedNetworkObject;
			netObjSetup.SetUpNetworkObject();
			_load.CompeleteProcess += netObjSetup.AllDespawn;
		}

		private void GameReadyWait()
		{
			_readyWaitTimer = TickTimer.CreateFromSeconds(Runner, 0.5f);
		}

		private void GameReady()
		{
			Debug.Log("GameReady");
			State = GameState.Ready;
			Runner.Spawn(_inGameData);
			_readyToPlayWaitTimer = TickTimer.CreateFromSeconds(Runner, 5.6f);
		}

		private void GameStart()
		{
			State = GameState.Play;
			PlayTickTimer = TickTimer.CreateFromSeconds(Runner, _gameTime);
		}

		private void EndRender()
		{
			if (HasStateAuthority == false)
				return;

			if (_endWaitTimer.Expired(Runner) == true)
			{
				GoToResult();
			}
		}

		private void ReadyRender()
		{
			if (HasStateAuthority == false)
				return;

			if (_readyToPlayWaitTimer.Expired(Runner) == true)
			{
				GameStart();
				_readyToPlayWaitTimer = TickTimer.None;
			}
		}

		private void WaitRender()
		{
			//안정화 완료
			if (_stabilizationTimer.Expired(Runner) == true)
			{
				SetUpLoad(GameObject.Find("Loading").GetComponent<InGameLoad>());
				_stabilizationTimer = TickTimer.None;
				FindObjectOfType<GameStartEnd>().Init();
				_isSpawned = true;
				_load.InGameStabilizationed(SpawnPlayer);
			}

			if(HasStateAuthority == true)
			{
				if (_readyWaitTimer.Expired(Runner) == true)
				{
					GameReady();
					_readyWaitTimer = TickTimer.None;
				}
			}
		}

		private void PlayRender()
		{
			if (HasStateAuthority == false)
				return;

			if (PlayTickTimer.Expired(Runner) == true)
			{
				GameEnd();
				PlayTickTimer = TickTimer.None;
			}
		}

		private void GameEnd()
		{
			Instance = null;
			if (HasStateAuthority == false)
				return;
			State = GameState.End;
			_endWaitTimer = TickTimer.CreateFromSeconds(Runner, 1.0f);
		}

		private void GoToResult()
		{
			if (Runner.IsSceneAuthority == false)
				return;

			Debug.Log("게임 종료 결과 씬으로 옮겨주세요");
			Runner.LoadScene(SceneRef.FromIndex(5), LoadSceneMode.Single);
		}

		public int GetEndTime()
		{
			if (_isSpawned == false)
			{
				return _gameTime;
			}

			float? time = PlayTickTimer.RemainingTime(Runner);

			if (time == null)
			{
				return _gameTime;
			}
			else
			{
				return (int)time;
			}
		}

		public int GetPlayTime()
		{
			if (_isSpawned == false)
			{
				return -1;
			}

			float? time = PlayTickTimer.RemainingTime(Runner);
			if (time == null)
			{
				return -1;
			}
			else
			{
				int result = _gameTime - (int)time;
				return result;
			}
		}
	}
}
