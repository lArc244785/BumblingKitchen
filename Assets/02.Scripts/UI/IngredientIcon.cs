using UnityEngine;
using UnityEngine.UI;

namespace BumblingKitchen
{
	public class IngredientIcon : MonoBehaviour
	{
		[SerializeField] private RectTransform _rectTransform;
		[SerializeField] private Image _image;

		public void Init(Transform parent, Sprite icon)
		{
			_rectTransform.SetParent(parent);
			_rectTransform.localPosition = Vector3.zero;
			_rectTransform.localScale = Vector3.one;
			_rectTransform.localRotation = Quaternion.identity;

			_image.sprite = icon;
		}
	}
}
