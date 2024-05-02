using BumblingKitchen.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

namespace BumblingKitchen.Interaction
{
	public class CuttingBoard : NetworkBehaviour , IInteractable, IGetCookingRecipe
	{
		public InteractionType Type => InteractionType.KitchenTool;

		public NetworkId NetworkId => Object.Id;

		[SerializeField] private List<CookingRecipe> _recipeList;
		[SerializeField] private float _addProgress;
		[SerializeField] private Transform _putPoint;

		private Ingredient _putIngredient;

		public event Action OnCookingStart;
		public event Action OnDoenCooked;
		public event Action OnCookingSucess;
		public event Action<float> OnUpdattingProgress;
		public event Action<CookingRecipe> OnSettingRecipe;

		void Awake()
		{
			OnDoenCooked += ResetCookingEvent;
		}

		public override void Spawned()
		{
			base.Spawned();
		}

		public bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if(CanPutIngredient() == true)
			{
				if (interactor.HasPickUpObject == false)
					return false;
				if (interactor.PickUpObject.Type != InteractionType.Ingredient)
					return false;
				Ingredient ingredient = interactor.PickUpObject as Ingredient;
				if (ingredient.MixDataList.Count > 1)
					return false;
				if (CanCook(ingredient.MixDataList[0], out var recipeIndex))
				{
					interactor.DropPickUpObject();
					RPC_PutIngredient(ingredient.Object, recipeIndex);
					ingredient.RPC_StartCook(Object, recipeIndex);
				}
			}
			else
			{
				if (_putIngredient == null)
					return false;
				if(_putIngredient.CurrentState == CookState.Cooking)
				{
					RPC_Effect();
					_putIngredient.RPC_Cooking(_addProgress);
					interactor.RPC_OnCutEvent();
				}
				else if(_putIngredient.CurrentState == CookState.Sucess)
				{
					if (interactor.HasPickUpObject == true)
						return false;

					_putIngredient.RPC_OnDoneCooked(Runner.LocalPlayer);
					interactor.RPC_OnPickuping(_putIngredient.Object);
					RPC_RelesePutIngredient();
				}
			}

			return false;
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_Effect()
		{
			var effect = PoolManager.Instance.GetPooledObject(PoolObjectType.Effect_CuttingBoard);
			effect.transform.position = _putPoint.position;
			effect.transform.rotation = _putPoint.rotation;
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_PutIngredient(NetworkId id, int recipeIndex)
		{
			Ingredient ingredient = Runner.FindObject(id).GetComponent<Ingredient>();
			ingredient.transform.SetParent(_putPoint);
			ingredient.transform.localPosition = Vector3.zero;
			ingredient.transform.localRotation = Quaternion.identity;
			_putIngredient = ingredient;

			_putIngredient.CookingStart += OnCookingStart;
			_putIngredient.DoneCooked += OnDoenCooked;
			_putIngredient.UpdattingProgress += OnUpdattingProgress;
			_putIngredient.CookingSucess += OnCookingSucess;

			OnSettingRecipe?.Invoke(_recipeList[recipeIndex]);
		}

		private bool CanCook(IngredientData ingredientData, out int recipeIndex)
		{
			recipeIndex = -1;

			for(int i = 0; i < _recipeList.Count; i++)
			{
				if (_recipeList[i].Source.CompareTo(ingredientData) == 0)
				{
					recipeIndex = i;
					return true;
				}
			}

			return false;
		}

		private bool CanPutIngredient()
		{
			return _putIngredient == null;
		}

		public CookingRecipe GetCookingRecipe(int index)
		{
			return _recipeList[index];
		}

		private void ResetCookingEvent()
		{
			_putIngredient.CookingStart -= OnCookingStart;
			_putIngredient.DoneCooked -= OnDoenCooked;
			_putIngredient.UpdattingProgress -= OnUpdattingProgress;
			_putIngredient.CookingSucess -= OnCookingSucess;
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_RelesePutIngredient()
		{
			_putIngredient = null;
		}
	}
}
