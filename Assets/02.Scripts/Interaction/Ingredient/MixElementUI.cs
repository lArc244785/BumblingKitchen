using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MixElementUI : MonoBehaviour
{
	[SerializeField]private Image _image;

	public void Toggle(bool isActive)
	{
		gameObject.SetActive(isActive);
	}

	public void SetMixElement(Sprite sprite)
	{
		_image.sprite = sprite;
	}
}
