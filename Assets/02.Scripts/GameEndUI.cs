using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

namespace BumblingKitchen
{
	//ÃÑ È¿°ú ½Ã°£ : 5.5f
    public class GameEndUI : MonoBehaviour
    {
		[SerializeField] private Canvas _canvas;
		private RectTransform _rectTransform;
        private Sequence _sequence;

		public void Init(Canvas canvas)
		{
			_canvas = canvas;
			_rectTransform = GetComponent<RectTransform>();
			_sequence = DOTween.Sequence();

			_sequence.Append(_rectTransform.DOAnchorPos3DX(0.0f, 1.0f)).SetEase(Ease.OutExpo);

			gameObject.SetActive(false);
		}

		private void OnEnable()
		{
			Debug.Log("Restart");
			_rectTransform.anchoredPosition = new Vector2(1920.0f, 0.0f);
			_sequence.Restart();
		}
	}
}
