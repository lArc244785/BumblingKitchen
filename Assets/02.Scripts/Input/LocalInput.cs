using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;

namespace BumblingKitchen
{
    public class LocalInput : MonoBehaviour, INetworkRunnerCallbacks
    {
		public Vector3 Dircation { set; get; }
		public bool Interaction { set; get; }


		private void Update()
		{
			if (SystemInfo.deviceType == DeviceType.Desktop)
			{
				Interaction = Interaction | Input.GetMouseButtonDown(0);
			}
		}


		public void OnInput(NetworkRunner runner, NetworkInput input)
		{
			var data = new NetworkInputData();

			float h = Input.GetAxisRaw("Horizontal");
			float v = Input.GetAxisRaw("Vertical");

			data.direction = Dircation;
			data.buttons.Set(NetworkInputData.INTERACTION_BUTTON, Interaction);
			Interaction = false;

			input.Set(data);
		}

		public void OnConnectedToServer(NetworkRunner runner)
		{
			
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
			
		}

		public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
		{
			
		}

		public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
		{
			
		}

    }
}
