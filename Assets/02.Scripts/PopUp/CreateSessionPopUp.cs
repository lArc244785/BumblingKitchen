using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BumblingKitchen.PopUp
{
	public class CreateSessionPopUp : PopUpBase
	{

		[SerializeField] private Button _create;
		[SerializeField] private Button _cancle;
		[SerializeField] private TMP_InputField _sessionName;
		[SerializeField] private PopUpLoading _popupLoading;

		public override PopUpType Type => PopUpType.CreateSession;

		public event Action<string> OnCreate;

		private void Awake()
		{
			_create.onClick.AddListener(Create);
			_cancle.onClick.AddListener(Close);
		}

		private void Create()
		{
			OnCreate?.Invoke(_sessionName.text);
			Close();
			var loading = Instantiate(_popupLoading);
			manger.RegistrationPopUp(loading);
		}
	}
}
