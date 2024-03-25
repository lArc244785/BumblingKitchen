using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BumblingKitchen.Interaction;

namespace BumblingKitchen
{
	[CreateAssetMenu(fileName = "new_cooking_recipe", menuName = "CustomAssets/NewCookingRecipe")]
    public class CookingRecipe : ScriptableObject
    {
		[field: SerializeField] public NetworkIngredientData Source { get; private set; }
		[field: SerializeField] public Recipe Sucess { get; private set; }
		[field: SerializeField] public Recipe Fail { get; private set; }
		[field: SerializeField] public float SucessProgress { get; private set; }
		[field: SerializeField] public float FailProgress { get; private set; }
    }
}
