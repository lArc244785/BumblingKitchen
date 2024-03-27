using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public class Ingredient : PickableInteractable
	{
		public override InteractionType Type => InteractionType.Ingredient;

		[SerializeField] Recipe _initRecipe;
		[SerializeField] Transform _modelParent;

		/// <summary>
		/// 들어간 재료에 대한 정보를 담는다 추가할 때 마다 정렬된다.
		/// </summary>
		public List<IngredientData> MixDataList { get; } = new();
		private GameObject _modelObject;


		private bool _isInitModel = false;

		[field: SerializeField]
		[Networked, OnChangedRender(nameof(UpdateIngredientData))] NetworkString<_32> NetName { set; get; }

		//조리 레시피로 재료를 가공할 때 사용된다.
		private CookingRecipe _cookingRecipe;

		private float _currentProgress;
		public CookState CurrentState { get; private set; }


		public Action OnCookingStart;
		public Action<float> OnUpdattingProgress;
		public Action OnCookingSucess;
		public Action OnDoneCooked;
		public Action OnCookingFail;
		public Action<List<IngredientData>> OnUpdateMixData;

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

		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_DoneCook()
		{
			CurrentState = CookState.None;
			_cookingRecipe = null;
			OnDoneCooked?.Invoke();
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_Cooking(float addProgress)
		{
			if (CurrentState == CookState.None || CurrentState == CookState.Fail)
				return;

			_currentProgress += addProgress;
			OnUpdattingProgress?.Invoke(_currentProgress);

			switch (CurrentState)
			{
				case CookState.Cooking:
					{
						if(_currentProgress >= _cookingRecipe.SucessProgress)
						{
							CurrentState = CookState.Sucess;
							RPC_ChangeIngredient(_cookingRecipe.Sucess.Name);
							OnCookingSucess?.Invoke();
						}
						break;
					}
				case CookState.Sucess:
					{
						if(_currentProgress >= _cookingRecipe.FailProgress)
						{
							CurrentState = CookState.Fail;
							RPC_ChangeIngredient(_cookingRecipe.Fail.Name);
							OnCookingFail?.Invoke();
						}
						break;
					}
			}

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

			OnUpdateMixData?.Invoke(MixDataList);
			_modelObject = Instantiate(newRecipe.ModelPrefab, _modelParent, false);
			gameObject.name = newRecipe.name;
		}


		private List<IngredientData> CreateMixIngredient(List<IngredientData> mixDataList)
		{
			List<IngredientData> newMixIngredient = new();
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
			if (interactable.Type != InteractionType.Ingredient)
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

		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_StartCook(NetworkObject cookingToolID, int recipeIndex)
		{
			var board = Runner.FindObject(cookingToolID).GetComponent<CuttingBoard>();
			_cookingRecipe = board.GetRecipe(recipeIndex);
			_currentProgress = 0.0f;
			CurrentState = CookState.Cooking;
			OnCookingStart?.Invoke();
		}

	}
}
