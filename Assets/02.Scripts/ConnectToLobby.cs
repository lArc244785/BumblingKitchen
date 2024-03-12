using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BumblingKitchen.Fusion
{
	public class ConnectToLobby : MonoBehaviour
	{
		[SerializeField] private Button onConnectToLobby;


		private void Start()
		{
			onConnectToLobby.onClick.AddListener(OnConnectToLobby);
		}

		public void OnConnectToLobby()
		{
			FusionConnection.Instance.ConnectToLobby();
		}
	}
}
