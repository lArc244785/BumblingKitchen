using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BumblingKitchen.PopUp
{
	public class MessagePopUp : PopUpBase
	{
		[SerializeField] private TMP_Text _title;
		[SerializeField] private TMP_Text _message;
		[SerializeField] private Button _closeButton;

		public void Init(string title, string message)
		{
			_title.text = title;
			_message.text = message;
			_closeButton.onClick.AddListener(Close);
		}
	}
}
