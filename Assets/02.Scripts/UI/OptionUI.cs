using BumblingKitchen.PopUp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BumblingKitchen
{
	public class OptionUI : MonoBehaviour
	{
		[SerializeField] private PopupManger _manager;
		[SerializeField] PopUpBase _soundPopUp;
		[SerializeField] Button _sound;

		private void Awake()
		{
			_sound.onClick.AddListener(PopUpSoundOption);
		}

		private void PopUpSoundOption()
		{
			var soundPopUp = Instantiate(_soundPopUp);
			_manager.RegistrationPopUp(soundPopUp);
		}
	}
}
