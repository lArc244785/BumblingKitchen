using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public class Ingredient : PickableInteractable
	{
		[SerializeField] Recipe _initRecipe;
		[SerializeField] Transform _modelParent;

		/// <summary>
		/// 들어간 재료에 대한 정보를 담는다 추가할 때 마다 정렬된다.
		/// </summary>
		public List<NetworkIngredientData> MixDataList { get; } = new();
		private GameObject _modelObject;


		private bool _isInitModel = false;

		[field: SerializeField]
		[Networked, OnChangedRender(nameof(UpdateIngredientData))] NetworkString<_32> NetName { set; get; }

		public override void Spawned()
		{
			base.Spawned();
			if (HasStateAuthority)
			{
				RPC_ChangeIngredient(_initRecipe.Name);
			}

			if (_isInitModel == false)
			{
				UpdateIngredientData();
			}
		}


		/// <summary>
		/// 두개의 재료를 합칠고자할 때 사용.
		/// 두개를 합쳤을 때 용량을 넘어가게 되면 합치지 않는다.
		/// </summary>
		/// <param name="inputMixList"></param>
		/// <returns></returns>
		public bool TryMix(Ingredient ingredinet)
		{
			//용량을 벗어나는지
			if (MixDataList.Count + ingredinet.MixDataList.Count > MixDataList.Capacity)
			{
				return false;
			}

			var newMixIngredientList = CreateMixIngredient(ingredinet.MixDataList);
			//레시피에 있는 조합인지 확인
			var foundRecipe = StageRecipeManager.Instance.FindRecipe(newMixIngredientList);
			if (foundRecipe == null)
				return false;

			RPC_ChangeIngredient(foundRecipe.Name);
			return true;
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_ChangeIngredient(NetworkString<_32> recipeName)
		{
			Recipe recipe = StageRecipeManager.Instance.RecipeTable[recipeName.ToString()];
			if (recipe == null)
			{
				throw new Exception($"There is No RecipeTable[{recipeName.ToString()}].");
			}

			NetName = recipe.Name;
		}

		private void UpdateIngredientData()
		{
			Debug.Log($"재료 모델 갱신! {NetName.ToString()}");

			if (_isInitModel == false)
			{
				_isInitModel = true;
			}

			Recipe newRecipe = StageRecipeManager.Instance.RecipeTable[NetName.ToString()];
			MixDataList.Clear();
			foreach (var item in newRecipe.MixList)
			{
				MixDataList.Add(item);
			}

			if (_modelObject != null)
			{
				Destroy(_modelObject);
			}

			_modelObject = Instantiate(newRecipe.ModelPrefab, _modelParent, false);
			gameObject.name = newRecipe.name;
		}


		private List<NetworkIngredientData> CreateMixIngredient(List<NetworkIngredientData> mixDataList)
		{
			List<NetworkIngredientData> newMixIngredient = new();
			//원본
			foreach (var ingredient in MixDataList)
			{
				newMixIngredient.Add(ingredient);
			}

			//추가
			foreach (var ingredient in mixDataList)
			{
				newMixIngredient.Add(ingredient);
			}

			newMixIngredient.Sort();

			return newMixIngredient;
		}

		public bool CanInteraction(InteractionType type)
		{
			return type == InteractionType.Ingredient;
		}

		public bool CanInteraction(Interactor interactor)
		{
			if (interactor.HasPickUpObject == false)
				return true;
			return CanInteraction(interactor.GetPickUpObjectType());
		}

		public void Interaction(Interactor interactor, IInteractable interactionObject)
		{
			
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_DespawnIngredient(NetworkObject despawnObject)
		{
			Runner.Despawn(despawnObject);
		}

		public override bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if(base.TryInteraction(interactor, interactable) == true)
			{
				return true;
			}

			//재료만 상호작용이 가능하다.
			if (interactable.Type != InteractionType.Interactor)
			{
				return false;
			}

			Ingredient ingredient = interactable as Ingredient;

			// 재료 섞기 시도
			if (TryMix(ingredient) == true)
			{
				PickableInteractable pickableObject = interactable as PickableInteractable;
				
				if(pickableObject == null)
				{
					throw new Exception("This ingredient isn't PickableInteractable!");
				}

				if(interactor.IsPickUpInteractor(pickableObject) == true)
				{
					Runner.Despawn(interactor.Drop().Object);
				}
			}

			return false;
		}
	}
}
