using BumblingKitchen.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BumblingKitchen
{
	public class IngredientUI : InGameUIBase
	{
		[SerializeField] private Transform _mixElementsParent;
		private Ingredient _ingredient;
		private MixElementUI[] _mixElementUIList;

		protected override void Awake()
		{
			base.Awake();

			_ingredient = GetComponentInParent<Ingredient>();
			_mixElementUIList = _mixElementsParent.GetComponentsInChildren<MixElementUI>();

			_ingredient.CookingStart += Close;
			_ingredient.DoneCooked += Open;
			_ingredient.UpdattingMixData += UpdateMixData;
		}

		private void UpdateMixData(List<IngredientData> mixDataList)
		{
			foreach(var item in _mixElementUIList)
			{
				item.Toggle(false);
			}

			for(int i = 0; i < mixDataList.Count; i++)
			{
				_mixElementUIList[i].Toggle(true);
				_mixElementUIList[i].SetMixElement(mixDataList[i].Icon);
			}
		}

		private void Open()
		{
			canvas.gameObject.SetActive(true);
		}

		private void Close()
		{
			canvas.gameObject.SetActive(false);
		}
	}
}
