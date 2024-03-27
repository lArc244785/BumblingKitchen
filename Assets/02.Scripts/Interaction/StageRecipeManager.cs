using System;
using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public class StageRecipeManager : MonoBehaviour
	{
		[SerializeField] private List<Recipe> _recipeList;
		public Dictionary<string, Recipe> RecipeTable { get; private set; } = new Dictionary<string, Recipe>();

		public static StageRecipeManager Instance { get; private set; }

		private void Awake()
		{
			if(Instance != null)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;

			foreach(var recipe in _recipeList)
			{
				recipe.SortMixList();
				RecipeTable.Add(recipe.Name, recipe);
			}
		}

		public Recipe FindRecipe(List<IngredientData> mixList)
		{
			foreach (var recipe in _recipeList)
			{
				if(recipe.SameRecipe(mixList) == true)
				{
					return recipe;
				}
			}

			return null;
		}
	}
}
