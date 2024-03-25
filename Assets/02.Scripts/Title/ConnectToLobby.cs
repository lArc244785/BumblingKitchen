using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;

namespace BumblingKitchen.Title
{
	public class ConnectToLobby : MonoBehaviour
	{
		[SerializeField] private Button _connetButton;
		[SerializeField] private FusionConnection _connectorPrefab;

		private void Awake()
		{
			if(FusionConnection.Instance == null)
			{
				Instantiate(_connectorPrefab);
			}

			_connetButton.onClick.AddListener(OnConnetingLobby);
		}

		private void OnConnetingLobby()
		{
			FusionConnection.Instance.ConnectToLobby();
		}
	}
}