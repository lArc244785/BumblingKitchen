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
		[SerializeField] private Button _exitButton;
		[SerializeField] private FusionConnection _connectorPrefab;

		private void Awake()
		{
			if(FusionConnection.Instance == null)
			{
				Instantiate(_connectorPrefab);
			}

			_connetButton.onClick.AddListener(OnConnetingLobby);
			_exitButton.onClick.AddListener(ExitGame);
		}

		private void OnConnetingLobby()
		{
			FusionConnection.Instance.ConnectToLobby();
		}

		private void ExitGame()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
	}
}
