using UnityEngine;

namespace BumblingKitchen.PopUp
{
	public abstract class PopUpBase : MonoBehaviour
	{
		private PopupManger _manger;

		public void Init(PopupManger manager)
		{
			_manger = manager;
		}

		public virtual void Close()
		{
			_manger.ClosePopUp();
		}

		public virtual void Open()
		{
			gameObject.SetActive(true);
		}

		public void DestoryPopUp()
		{
			Destroy(gameObject);
		}
	}
}
