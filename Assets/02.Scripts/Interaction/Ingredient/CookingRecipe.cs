using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BumblingKitchen.Interaction;

namespace BumblingKitchen
{
	public enum CookState
	{
		None,
		Cooking,
		Sucess,
		Fail,
	}

	[CreateAssetMenu(fileName = "new_cooking_recipe", menuName = "CustomAssets/NewCookingRecipe")]
    public class CookingRecipe : ScriptableObject
    {
		[field: SerializeField] public IngredientData Source { get; private set; }
		[field: SerializeField] public Recipe Sucess { get; private set; }
		[field: SerializeField] public Recipe Fail { get; private set; }
		[field: SerializeField] public float SucessProgress { get; private set; }
		[field: SerializeField] public float FailProgress { get; private set; }

		public bool IsCookable(IngredientData data)
		{
			if (Source == null)
				return false;
			if (data == null)
				return false;

			return Source.CompareTo(data) == 0;
		}
    }
}
