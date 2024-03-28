using BumblingKitchen.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BumblingKitchen
{
    public class Order : MonoBehaviour
    {
		[SerializeField] private Slider _slider;
		[SerializeField] private Image _icon;
		[SerializeField] private Transform _cookingInfoParent;
		[SerializeField] private CookingInfoUI _cookingInfoUIPrefab;

		public float EndTiem { get; private set; }
		public String RecipeName { get; private set; }

		public void InitSetting(Recipe recipe, float startTime, float endTime)
		{
			RecipeName = recipe.Name;
			EndTiem = endTime;
			//슬라이더 설정
			_slider.minValue = startTime;
			_slider.maxValue = endTime;
			_slider.value = startTime;
			//아이콘
			_icon.sprite = recipe.Icon;
			//조리 UI들 생성
			Dictionary<CookingType, List<IngredientData>> cookingInfoTable = new();
			foreach(var item in recipe.MixList)
			{
				if(cookingInfoTable.ContainsKey(item.CookingType) == false)
				{
					cookingInfoTable.Add(item.CookingType, new List<IngredientData>());
				}
				cookingInfoTable[item.CookingType].Add(item);
			}
			
			foreach(var type in cookingInfoTable.Keys)
			{
				CreateCookingInfoUI(type, cookingInfoTable[type]);
			}
		}

		private void CreateCookingInfoUI(CookingType type, List<IngredientData> info)
		{
			var newCookingInfoUI =  Instantiate(_cookingInfoUIPrefab, _cookingInfoParent);
			newCookingInfoUI.InitSetting(type,info);
		}


		public void Draw(float networkTime)
		{
			_slider.value = networkTime;
		}
	}
}
