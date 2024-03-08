using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyFusion;
using TMPro;
using UnityEngine.UI;

public class ConnectToLobby : MonoBehaviour
{
	[SerializeField] private Button onConnectToLobby;


	private void Start()
	{
		onConnectToLobby.onClick.AddListener(OnConnectToLobby);
	}

	public void OnConnectToLobby()
	{
		FusionConnection.instance.ConnectToLobby();
	}



}
