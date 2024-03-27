using BumblingKitchen;
using BumblingKitchen.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BumblingKitchen
{
	public class CuttinBoardUI : InGameUIBase
	{
		[SerializeField] private CuttingBoard _board;
		[SerializeField] private Slider _progress;

		protected override void Awake()
		{
			base.Awake();
			_board.OnCookingStart += Open;
			_board.OnCookingSucess += Close;
			_board.OnUpdattingProgress += UpdateProgress;
			_board.OnSettingRecipe += SetupProgress;

			Close();
		}

		private void SetupProgress(CookingRecipe recipe)
		{
			_progress.minValue = 0.0f;
			_progress.maxValue = recipe.SucessProgress;
			_progress.value = 0.0f;
		}

		private void UpdateProgress(float value)
		{
			_progress.value = value;
		}

		private void Close()
		{
			Toogle(false);
		}

		private void Open()
		{
			Toogle(true);
		}
	}
}
