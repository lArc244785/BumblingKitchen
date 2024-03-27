using BumblingKitchen;
using BumblingKitchen.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CuttinBoardUI : MonoBehaviour
{
	[SerializeField] private CuttingBoard _board;
	[SerializeField] private Slider _progress;
	[SerializeField] private Canvas _canvas;

	private void Awake()
	{
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
		_canvas.enabled = false;
	}

	private void Open()
	{
		_canvas.enabled = true;
	}
}
