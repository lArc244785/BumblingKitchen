using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BumblingKitchen
{
	public class Result : MonoBehaviour
	{
		[SerializeField] private Button _button;
		[SerializeField] private PlayerDataUI _playerDataUI;
		[SerializeField] private Transform _playerDataParent;

		private void Awake()
		{
			var ingameData = FindObjectOfType<InGameData>();
			_button.onClick.AddListener(() => FusionConnection.Instance.ExitSessionToTitle());
			foreach (var item in ingameData.PlayerDataList)
			{
				Instantiate(_playerDataUI, _playerDataParent).InitDataSetting(
					item.player.PlayerId.ToString(),
					item.gold.ToString(),
					item.spawnObject.ToString(),
					item.succesCooking.ToString(),
					item.failCooking.ToString(),
					item.sendToOrder.ToString(),
					item.cleanPlate.ToString());
			}
		}

	}
}
