using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	[CreateAssetMenu(fileName = "new_recipe", menuName = "CustomAssets/NewRecipe")]
	public class Recipe : ScriptableObject
	{
		[field: SerializeField] public string Name { get; private set; }
		[field: SerializeField] public List<NetworkIngredientData> MixList { get; private set; }
		[field: SerializeField] public GameObject ModelPrefab { get; private set; }

		public bool SameRecipe(List<NetworkIngredientData> mixDatas)
		{
			if (MixList.Count == mixDatas.Count)
				return false;

			for(int i = 0; i < MixList.Count; i++)
			{
				if (MixList[i].CompareTo(mixDatas[i]) != 0)
					return false;
			}

			return true;
		}
	}
}
