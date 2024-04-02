using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System;

namespace BumblingKitchen
{
	//ÃÑ È¿°ú ½Ã°£ : 5.5f
    public class GameStartUI : MonoBehaviour
    {
        private TMP_Text _text;
		private Canvas _canvas;

		private Sequence _sequence;

		internal void Init(Canvas canvas)
		{
			_canvas = canvas;
			_text = GetComponent<TMP_Text>();
			_rectTransform = GetComponent<RectTransform>();
			_anchoredPosition = _rectTransform.anchoredPosition;

			Tween move1 = _rectTransform.DOAnchorPos3DY(300.0f, 0.5f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutSine).OnComplete(DownCount);
			Tween move2 = _rectTransform.DOAnchorPos3DY(200.0f, 0.5f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutSine).OnComplete(DownCount);
			Tween move3 = _rectTransform.DOAnchorPos3DY(100.0f, 0.5f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutSine).OnComplete(DownCount);

			_sequence = DOTween.Sequence();
			_sequence.Append(move1).AppendInterval(0.5f).Append(move2).AppendInterval(0.5f).Append(move3).OnComplete(GameStartTween);
			gameObject.SetActive(false);
		}

		private Vector2 _anchoredPosition;
		private RectTransform _rectTransform;

		private int _count = 3;

		private void OnEnable()
		{
			_count = 3;
			_text.text = _count.ToString();
			_rectTransform.anchoredPosition = _anchoredPosition;

			_sequence.Restart();
		}

		private void DownCount()
		{
			_count--;
			_text.text = _count.ToString();
		}

		private void GameStartTween()
		{
			_text.text = "GAME START";

			Sequence sequence = DOTween.Sequence();
			Tween tween = _rectTransform.DOPunchScale(Vector3.one * 0.5f, 1.0f);
			sequence.Append(tween).AppendInterval(0.5f).OnComplete(() => {
				_canvas.enabled = false;
				gameObject.SetActive(false);
			});

			sequence.Play();
		}
	}
}
