using UnityEngine;
using TMPro;
using BumblingKitchen.Interaction;
using System;
using UnityEngine.UI;

namespace BumblingKitchen
{
	public class SinkUI : InGameUIBase
	{
		[SerializeField] private Sink _sink;
		[SerializeField] private TMP_Text _text;
		[SerializeField] private Slider _slider;

		protected override void Awake()
		{
			base.Awake();
			_slider.minValue = 0.0f;
			_slider.maxValue = _sink.CleanPlateProgress;
			_slider.value = 0.0f;

			_sink.OnUpdateDirtyPlates += DrawDrityPlateInfo;
			_sink.OnUpdateProgress += UpdateSlider;
		}

		private void DrawDrityPlateInfo(int count, int capacity)
		{
			_text.text = $"{count}/{capacity}";
		}

		private void UpdateSlider(float progress)
		{
			_slider.value = progress;
		}
	}
}
