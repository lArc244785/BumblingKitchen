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
			for (int i = 0; i < _playerCards.Length; i++)
			{
				if (i < userInfos.Count)
				{
					_playerCards[i].Draw(userInfos[i].playerName.ToString(), userInfos[i].characterId, userInfos[i].isReady);
				}
				else
				{
					_playerCards[i].Blank();
				}
			}
		}

	}
}
