using BumblingKitchen.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

namespace BumblingKitchen.Interaction
{
	public class FryingPan : PickableInteractable , IInteractable, IGetCookingRecipe, ISpillIngredient
	{
		public override InteractionType Type => InteractionType.FireKitchenTool;


		[SerializeField] private List<CookingRecipe> _recipeList;
		[SerializeField] private float _addProgress;
		[SerializeField] private Transform _putPoint;

		[SerializeField]
		private Ingredient _putIngredient;

		public event Action OnCookingStart;
		public event Action OnDoenCooked;



		public event Action OnCookingSucess;
		public event Action<float> OnUpdattingProgress;
		public event Action<CookingRecipe> OnSettingRecipe;
		public event Action OnCookingFail;

		public bool IsOnGasRange { set; get; } = false;

		void Awake()
		{
			OnDoenCooked += ResetCookingEvent;
		}

		public override bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if(base.TryInteraction(interactor, interactable) == true)
				return true;

			if(CanPutIngredient() == true)
			{
				if (interactor.HasPickUpObject == false)
					return false;
				if (interactor.GetPickObject().Type != InteractionType.Ingredient)
					return false;
				Ingredient ingredient = interactor.GetPickObject() as Ingredient;
				if (ingredient.MixDataList.Count > 1)
					return false;
				if (CanCook(ingredient.MixDataList[0], out var recipeIndex))
				{
					interactor.Drop();
					RPC_PutIngredient(ingredient.Object, recipeIndex);
					ingredient.RPC_StartCook(Object, recipeIndex);
				}
			}

			return false;
		}

		internal Ingredient GetIngredient()
		{
			return _putIngredient;
		}

		internal bool CanDropIngredient()
		{
			if (_putIngredient == null)
				return false;
			return _putIngredient.CurrentState == CookState.Sucess;
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_PutIngredient(NetworkId id, int recipeIndex)
		{
			Ingredient ingredient = Runner.FindObject(id).GetComponent<Ingredient>();
			ingredient.transform.SetParent(_putPoint);
			ingredient.transform.localPosition = Vector3.zero;
			ingredient.transform.localRotation = Quaternion.identity;
			_putIngredient = ingredient;

			_putIngredient.OnCookingStart += OnCookingStart;
			_putIngredient.OnDoneCooked += OnDoenCooked;
			_putIngredient.OnUpdattingProgress += OnUpdattingProgress;
			_putIngredient.OnCookingSucess += OnCookingSucess;
			_putIngredient.OnCookingFail += OnCookingFail;

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
			_putIngredient.OnCookingStart -= OnCookingStart;
			_putIngredient.OnDoneCooked -= OnDoenCooked;
			_putIngredient.OnUpdattingProgress -= OnUpdattingProgress;
			_putIngredient.OnCookingSucess -= OnCookingSucess;
			_putIngredient.OnCookingFail -= OnCookingFail;
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_RelesePutIngredient(PlayerRef player)
		{
			_putIngredient.RPC_DoneCook(player);
			_putIngredient = null;
		}

		public override void FixedUpdateNetwork()
		{
			if (HasStateAuthority == false)
				return;

			if(IsOnGasRange == true && _putIngredient != null)
			{
				_putIngredient.RPC_Cooking(_addProgress * Runner.DeltaTime);
			}
		}

		public bool CanPutGasRange()
		{
			return _putIngredient?.CurrentState == CookState.None ||
				 _putIngredient?.CurrentState == CookState.Cooking;
		}

		public Ingredient SpillIngredient()
		{
			var spill = _putIngredient;
			RPC_RelesePutIngredient(Runner.LocalPlayer);
			return spill;
		}
	}
}
