using UnityEngine;

namespace BumblingKitchen
{
	public abstract class InGameUIBase : MonoBehaviour
	{
		[SerializeField] protected Canvas canvas;
		private Vector3 _localPosition;

		protected virtual void Awake()
		{
			_localPosition = transform.localPosition;
		}

		private void Update()
		{
			canvas.transform.localPosition = _localPosition;
			canvas.transform.rotation = Quaternion.Euler(60.0f, 0.0f, 0.0f);
		}

		protected void Toogle(bool toogle)
		{
			canvas.enabled = toogle;
		}
	}
}
