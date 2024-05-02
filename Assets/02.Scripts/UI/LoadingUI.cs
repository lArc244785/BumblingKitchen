using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace BumblingKitchen
{
	public class LoadingUI : MonoBehaviour
	{
		[SerializeField] private Slider _slider;

		private Tween tween;

		private void Awake()
		{
			_slider.minValue = 0.0f;
			_slider.maxValue = 1.0f;
			_slider.value = 0.0f;
		}

		public void Init(InGameLoad load)
		{
			load.UpdattingProgress += DrawProgress;
		}

		private void DrawProgress(float value)
		{
			if(tween != null && tween.IsPlaying() == true)
			{
				tween.Kill();
			}

			tween = _slider.DOValue(value, 0.1f).Play().SetAutoKill(true);
			tween.Play();
		}
	}
}
