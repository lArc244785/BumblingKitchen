using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Sirenix.OdinInspector;

namespace BumblingKitchen
{
	public class CookingInfoUI : SerializedMonoBehaviour
	{
		[SerializeField] private GridLayoutGroup _gridlayoutGroup;
		[SerializeField] private Image _elementPrefab;
		[SerializeField] private RectTransform _rectTransform;
		[SerializeField] private Transform _elementParent;
		[SerializeField] private Image _infoIcon;
		[SerializeField] private Dictionary<CookingType, Sprite> _cookingTypeIconTable;


		/// <summary>
		/// IngredientData의 수 만큼 내부에 Icon을 및 높이를 설정한다.
		/// </summary>
		/// <param name="datas"></param>
		/// <returns></returns>
		public void InitSetting(CookingType type, List<IngredientData> datas)
		{
			_infoIcon.sprite = _cookingTypeIconTable[type];

			foreach (var data in datas)
			{
				var newIcon = Instantiate(_elementPrefab, _elementParent);
				newIcon.sprite = data.Icon;
			}

			float hight = (_gridlayoutGroup.cellSize.y + (_gridlayoutGroup.spacing.y * 2));
			int col =  Mathf.CeilToInt((float)datas.Count / 2.0f);
			hight *= col;

			_rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, _rectTransform.sizeDelta.y + hight);
		}
	}
}
