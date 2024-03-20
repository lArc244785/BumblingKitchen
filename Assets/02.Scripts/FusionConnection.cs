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
	public NetworkRunner runner { get; private set; }

	public List<SessionInfo> sessionList { get; private set; }

	public event Action<NetworkRunner, PlayerRef> OnPlayerLeftEvent;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			runner = gameObject.AddComponent<NetworkRunner>();
			runner.ProvideInput = true;
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


		await runner.StartGame(new StartGameArgs()
		{
			GameMode = GameMode.AutoHostOrClient,
			SessionName = sessionName,
			Scene = scene,
			PlayerCount = 4,
			SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
		});
	}

	public async void ConnectToLobby()
	{
		await runner.JoinSessionLobby(SessionLobby.Shared);
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
	}

	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
		OnPlayerLeftEvent?.Invoke(runner, player);
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
