using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;
using System.Text;

namespace MyFusion
{
	public class FusionConnection : MonoBehaviour, INetworkRunnerCallbacks
	{
		public static FusionConnection instance;
		public NetworkRunner runner { get; private set; }

		private void Awake()
		{
			if(instance == null)
			{
				instance = this;
				runner = gameObject.AddComponent<NetworkRunner>();
			}
			else
			{
				Debug.LogWarning("Overlap FusionConnection!");
				Destroy(gameObject);
				return;
			}

			DontDestroyOnLoad(gameObject);
		}

		public async void ConnectToRunner(string playerName)
		{

			var scene = SceneRef.FromIndex(1);
			var sceneInfo = new NetworkSceneInfo();
			if(scene.IsValid)
			{
				sceneInfo.AddSceneRef(scene, LoadSceneMode.Single);
			}


			await runner.StartGame(new StartGameArgs()
			{
				GameMode = GameMode.AutoHostOrClient,
				SessionName = "Lobby",
				Scene = scene,
				PlayerCount = 2,
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
			Debug.Log("OnSessionListUpdated");

			StringBuilder sb = new StringBuilder();

			foreach (var item in sessionList)
			{
				sb.AppendLine(item.Name);
			}

			Debug.Log($"Sessions\n{sb}");
		}

		public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
		{
		}

		public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
		{
		}
	}
}
