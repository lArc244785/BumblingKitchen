using UnityEngine;

namespace BumblingKitchen.PopUp
{
	public enum PopUpType
	{
		None,
		Message,
		CreateSession,
		SoundOption,
		Loading,
	}

	public abstract class PopUpBase : MonoBehaviour
	{
		protected PopupManger manger;
		public abstract PopUpType Type { get; }

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
