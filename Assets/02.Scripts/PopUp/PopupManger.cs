using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen.PopUp
{
	public class PopupManger : SerializedMonoBehaviour
	{
		public static PopupManger Instance { get; private set; }

		[SerializeField] private Stack<PopUpBase> _popups = new();

		private void Awake()
		{
			if(Instance != null)
			{
				Destroy(this);
				return;
			}

			Instance = this;
		}

		public void RegistrationPopUp(PopUpBase popUp)
		{
			popUp.transform.SetParent(transform, false);
			if(_popups.TryPop(out var popup))
			{
				popup.Close();
			}
			_popups.Push(popUp);
			_popups.Peek().Open();
		}

		public void ClosePopUp()
		{
			_popups.Pop().Close();
			if (_popups.TryPeek(out var popUp))
			{
				popUp.Open();
			}
		}

	}
}
