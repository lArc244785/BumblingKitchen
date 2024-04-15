using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Sirenix.OdinInspector;

namespace BumblingKitchen
{
	public class CookingInfoUI : SerializedMonoBehaviour
	{
		[SerializeField] private GridLayoutGroup _gridlayoutGroup;
		[SerializeField] private RectTransform _rectTransform;
		[SerializeField] private Transform _elementParent;
		[SerializeField] private Image _infoIcon;
		[SerializeField] private Dictionary<CookingType, Sprite> _cookingTypeIconTable;

		private List<PooledObject> _elements = new();

		private float _defaultSizeDeltaY;

		private void Awake()
		{
			_defaultSizeDeltaY = _rectTransform.sizeDelta.y;
		}


		/// <summary>
		/// IngredientData의 수 만큼 내부에 Icon을 및 높이를 설정한다.
		/// </summary>
		/// <param name="datas"></param>
		/// <returns></returns>
		public void InitSetting(CookingType type, List<IngredientData> datas)
		{
			PooledObject pooled = GetComponent<PooledObject>();
			_elements.Clear();
			_infoIcon.sprite = _cookingTypeIconTable[type];

			foreach (var data in datas)
			{
				IngredientIcon newIcon = PoolManager.Instance.GetPooledObject(PoolObjectType.UI_CookingInfo_element).GetComponent<IngredientIcon>();

				newIcon.Init(_elementParent, data.Icon);
				_elements.Add(newIcon.GetComponent<PooledObject>());

				pooled.OnRelese += newIcon.GetComponent<PooledObject>().Relese;
			}

			float hight = (_gridlayoutGroup.cellSize.y + (_gridlayoutGroup.spacing.y * 2));
			int col =  Mathf.CeilToInt((float)datas.Count / 2.0f);
			hight *= col;

			_rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, _defaultSizeDeltaY + hight);
		}
	}
}
