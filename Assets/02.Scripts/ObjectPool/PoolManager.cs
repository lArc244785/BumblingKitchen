using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen
{
	public enum PoolObjectType
	{
		None,
		CuttingBoardEffect,
		IngredientModel_BreadCheese,
		IngredientModel_BreadLeaf,
		IngredientModel_BreadLeafCheese,
		IngredientModel_BreadMeat,
		IngredientModel_BreadMeatCheese,
		IngredientModel_BreadMeatLeaf,
		IngredientModel_LeafCheese,
		IngredientModel_MeatCheese,
		IngredientModel_MeatLeaf,
		IngredientModel_MeatLeafCheese,
		IngredientModel_Hamburger,
		IngredientModel_Bread,
		IngredientModel_BreadSlice,
		IngredientModel_Cheese,
		IngredientModel_CheeseSlice,
		IngredientModel_Leaf,
		IngredientModel_LeafSlice,
		IngredientModel_Meat,
		IngredientModel_MeatSlice,
		IngredientModel_MeatGrill,
		IngredientModel_Tomato,
		IngredientModel_TomatoSlice,
		IngredientModel_Trash,
	}

	public class PoolManager : MonoBehaviour
	{
		private Dictionary<PoolObjectType, ObjectPool> poolTable = new Dictionary<PoolObjectType, ObjectPool>();

		public static PoolManager Instance { private set; get; }

		private void Awake()
		{
			if(Instance != null)
			{
				throw new System.Exception("over PoolManger");
			}

			Instance = this;

			ObjectPool[] pools = GetComponentsInChildren<ObjectPool>();
			foreach(var pool in pools)
			{
				pool.SetUpObjectPool();
				poolTable.Add(pool.Type, pool);
			}
		}

		public PooledObject GetPooledObject(PoolObjectType type)
		{
			if(poolTable.ContainsKey(type) == false)
			{
				throw new System.Exception($"PoolTable isn't containsKey {type}");
				return null;
			}

			return poolTable[type].GetPooledObject();
		}
	}
}
