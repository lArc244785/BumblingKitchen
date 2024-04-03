using UnityEngine;

namespace BumblingKitchen.PopUp
{
	public abstract class PopUpBase : MonoBehaviour
	{
		protected PopupManger manger;

		public void Init(PopupManger manager)
		{
			manger = manager;
		}

		public virtual void Close()
		{
			manger.ClosePopUp();
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
