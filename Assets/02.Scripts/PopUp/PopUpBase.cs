using UnityEngine;

namespace BumblingKitchen.PopUp
{
	public abstract class PopUpBase : MonoBehaviour
	{
		public virtual void Close()
		{
			Destroy(gameObject);
		}

		public virtual void Open()
		{
			gameObject.SetActive(true);
		}
	}
}
