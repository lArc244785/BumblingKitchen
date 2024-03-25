using Fusion;
using System;

namespace BumblingKitchen.Interaction
{
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

	[Serializable]
	public struct NetworkIngredientData : IComparable<NetworkIngredientData>
	{
		public IngredientType ingredientType;
		public CookingType cookingType;

		public NetworkIngredientData(IngredientType ingredientType, CookingType cookingType)
		{
			this.ingredientType = ingredientType;
			this.cookingType = cookingType;
		}

		//우선 순위
		//1. 조리타입이 높을 수록 뒤로
		//2. 재료타입이 높을 수록 뒤고
		public int CompareTo(NetworkIngredientData other)
		{
			if(cookingType > other.cookingType)
			{
				return 1;
			}
			else if(cookingType < other.cookingType)
			{
				return -1;
			}
			else
			{
				if(ingredientType > other.ingredientType)
				{
					return 1;
				}
				else if(ingredientType < other.ingredientType)
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

}
