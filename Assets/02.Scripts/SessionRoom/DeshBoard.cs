using System;
using UnityEngine;
using Fusion;
using System.Collections.Generic;

namespace BumblingKitchen.SessionRoom
{
	public class DeshBoard : MonoBehaviour
	{
		private PlayerCard[] _playerCards;

		private void Awake()
		{
			_playerCards = GetComponentsInChildren<PlayerCard>();
		}

		public void Draw(NetworkLinkedList<NetworkUserInfo> userInfos)
		{
			Debug.Log($"Draw Call {userInfos.Count}");
			for (int i = 0; i < _playerCards.Length; i++)
			{
				if (i < userInfos.Count)
				{
					Debug.Log("Draw " + i);
					_playerCards[i].Draw(userInfos[i].playerName.ToString(), userInfos[i].characterId, userInfos[i].isReady);
				}
				else
				{
					Debug.Log("No Draw " + i);
					_playerCards[i].Blank();
				}
			}
		}

	}
}
