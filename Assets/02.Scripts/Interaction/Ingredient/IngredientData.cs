using Fusion;
using System;
using UnityEngine;

namespace BumblingKitchen
{
	[CreateAssetMenu(fileName = "new_ingredient_data", menuName = "CustomAssets/NewIngredient")]
	public class IngredientData : ScriptableObject, IComparable<IngredientData>
	{
		[field: SerializeField] public IngredientType IngredientType { get; private set; }
		[field: SerializeField] public CookingType CookingType { get; private set; }
		[field: SerializeField] public Sprite Icon { get; private set; }

		//우선 순위
		//1. 조리타입이 높을 수록 뒤로
		//2. 재료타입이 높을 수록 뒤고
		public int CompareTo(IngredientData other)
		{
			if(CookingType > other.CookingType)
			{
				return 1;
			}
			else if(CookingType < other.CookingType)
			{
				return -1;
			}
			else
			{
				if(IngredientType > other.IngredientType)
				{
					return 1;
				}
				else if(IngredientType < other.IngredientType)
				{
					return -1;
				}
				else
				{
					return 0;
				}
			}
		}
	}

	public enum IngredientType
	{
		None,
		Onion,
		Meat,
		Tomato,
		Bread,
		Leaf,
		Cheese,
	}

	public enum CookingType
	{
		None,
		Raw,
		Slice,
		Grill,
		Boil,
		Fail,
	}

}
