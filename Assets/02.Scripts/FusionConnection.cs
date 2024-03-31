using BumblingKitchen;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


public class FusionConnection : MonoBehaviour, INetworkRunnerCallbacks
{
	public static FusionConnection Instance;
	public NetworkRunner _runner;

	public List<SessionInfo> sessionList { get; private set; }

	public event Action<NetworkRunner, PlayerRef> OnPlayerLeftEvent;

	public List<PlayerRef> connectPlayers = new();

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			_runner = gameObject.AddComponent<NetworkRunner>();
			_runner.ProvideInput = true;
		}
		else
		{
			Debug.LogWarning("Overlap FusionConnection!");
			Destroy(gameObject);
			return;
		}

		DontDestroyOnLoad(gameObject);
	}

	public bool CreateSession(string playerName, string sessionName)
	{
		foreach (SessionInfo session in sessionList)
		{
			if (session.Name == sessionName)
				return false;
		}

		ConnectToSession(playerName, sessionName);
		return true;
	}

	public async void ConnectToSession(string playerName, string sessionName)
	{
		var scene = SceneRef.FromIndex(2);
		var sceneInfo = new NetworkSceneInfo();
		if (scene.IsValid)
		{
			sceneInfo.AddSceneRef(scene, LoadSceneMode.Single);
		}


		await _runner.StartGame(new StartGameArgs()
		{
			GameMode = GameMode.Shared,
			SessionName = sessionName,
			Scene = scene,
			PlayerCount = 4,
			SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
			ObjectProvider = _runner.GetComponent<NetworkObjectPool>()
		});
	}

	public async void ExitSessionToTitle()
	{
		await _runner.Shutdown();
		SceneManager.LoadScene(0);
	}

	public async void ConnectToLobby()
	{
		connectPlayers.Clear();
		await _runner.JoinSessionLobby(SessionLobby.Shared);
		SceneManager.LoadScene(1);
	}


	public void OnConnectedToServer(NetworkRunner runner)
	{
		Debug.Log("OnConnectedToServer");
	}

	public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
	{
	}

	public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
	{
	}

	public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
	{
	}

	public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
	{
	}

	public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
	{
	}

	public void OnInput(NetworkRunner runner, NetworkInput input)
	{
	}

	public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
	{
	}

	public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
	}

	public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
	}

	public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
	{
		Debug.Log("Player Joined");
		connectPlayers.Add(player);
	}

	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
		OnPlayerLeftEvent?.Invoke(runner, player);
		connectPlayers.Remove(player);
	}

	public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
	{
	}

	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
	{
	}

	public void OnSceneLoadDone(NetworkRunner runner)
	{
	}

	public void OnSceneLoadStart(NetworkRunner runner)
	{
	}

	public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
	{
		if (this.sessionList != null)
			this.sessionList.Clear();
		this.sessionList = sessionList;
	}

	public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
	{
	}

	public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
	{
	}
}
