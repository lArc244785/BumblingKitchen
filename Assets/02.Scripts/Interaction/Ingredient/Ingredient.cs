using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public class Ingredient : PickableInteractable
	{
		#region Property
		public override InteractionType Type => InteractionType.Ingredient;
		/// <summary>
		/// 조합된 재료 이름
		/// </summary>
		public string Name { private set; get; }
		/// <summary>
		/// 들어간 재료 리스트 추가할 때 마다 정렬된다.
		/// </summary>
		public List<IngredientData> MixDataList { get; } = new();
		/// <summary>
		/// 재료의 조리 상태
		/// </summary>
		public CookState CurrentState { get; private set; }
		/// <summary>
		/// [Networked] 현재 조합된 재료들의 레시피 이름
		/// </summary>
		[Networked] private NetworkString<_32> RecipeName { get; set; }
		#endregion


		#region Field
		/// <summary>
		/// 모델 오브젝트가 위치할 부모 Transfrom
		/// </summary>
		[SerializeField] private Transform _modelParent;

		/// <summary>
		/// 모델 오브젝트 해당 오브젝트는 모두 PooledObject를 가지고 있어야됩니다.
		/// </summary>
		private PooledObject _modelObject;
		
		/// <summary>
		/// 조리 시 사용되는 레시피
		/// </summary>
		private CookingRecipe _cookingRecipe;
		
		/// <summary>
		/// 조리 도구를 이용한 조리 진행도
		/// </summary>
		private float _currentProgress;
		#endregion


		#region Event
		/// <summary>
		/// 조리 시작 시 호출되는 이벤트
		/// </summary>
		public Action CookingStart;

		/// <summary>
		/// 조리의 진행도가 업데이트 시 호출되는 이벤트
		/// </summary>
		public Action<float> UpdattingProgress;
		
		/// <summary>
		/// 조리 성공 시 호출되는 이벤트
		/// </summary>
		public Action CookingSucess;
		
		/// <summary>
		/// 조리가 끝났을 때 호출되는 이벤트
		/// </summary>
		public Action DoneCooked;
		
		/// <summary>
		/// 조리 실패 시 호출되는 이벤트
		/// </summary>
		public Action CookingFail;
		
		/// <summary>
		/// 재료의 섞인 재료가 업데이트 시 호출되는 이벤트
		/// </summary>
		public Action<List<IngredientData>> UpdattingMixData;
		#endregion


		#region Method
		/// <summary>
		/// 초기 생성 시 해당 재료의 레시피 이름을 설정합니다.
		/// </summary>
		/// <param name="recipe"></param>
		public void Init(Recipe recipe)
		{
			RecipeName = recipe.Name;
		}

		/// <summary>
		/// 네트워크 오브젝트가 생성 시 호출됩니다.
		/// 동기화 된 레시피의 이름 통해서 현재 재료의 상태를 업데이트 합니다.
		/// </summary>
		public override void Spawned()
		{
			if (RecipeName.ToString().Equals(string.Empty))
				return;

			Recipe recipe = StageRecipeManager.Instance.RecipeTable[RecipeName.ToString()];
			MixDataList.Clear();
			foreach (var item in recipe.MixList)
			{
				MixDataList.Add(item);
			}
			Name = recipe.Name;
			UpdateModel(recipe);
			UpdattingMixData?.Invoke(MixDataList);
			gameObject.name = recipe.Name;
		}

		/// <summary>
		/// 1. 상호작용 주체가 픽업이 가능한 경우 해당 오브젝트를 픽업합니다.
		/// 2. 상호작용 주체가 재료를 들고 있는 경우 해당 재료를 섞기를 시도해보고 가능한 경우 RPC를 통해서 섞기를 진행합니다.
		/// </summary>
		/// <returns>상호 작용 성공 여부</returns>
		/// <exception cref="주체가 픽업한 오브젝트가 PickableInteractable에서 파생된 객체가 아닌 경우"></exception>
		public override bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if (base.TryInteraction(interactor, interactable) == true)
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

				if (pickableObject == null)
				{
					throw new Exception("This ingredient isn't PickableInteractable!");
				}

				if (interactor.IsPickUpInteractor(pickableObject) == true)
				{
					interactor.DropPickUpObject();
				}

				RPC_DespawnIngredient(ingredient.Object);
			}

			return false;
		}

		/// <summary>
		/// 두개의 재료를 합치기를 시도합니다.
		/// 재료를 합쳤을 때 용량을 넘어가게 되면 합치지 않는다.
		/// 재료를 합쳤을 때 해당 스테이지에 레시피가 존재하지 않는 경우 합치지 않습니다.
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

			RPC_UpdateIngredient(foundRecipe.Name);
			return true;
		}

		/// <summary>
		/// [RPC] 레시피에 맞추어서 재료의 정보와 비주얼 데이터를 업데이트 합니다.
		/// </summary>
		/// <param name="recipeName"></param>
		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_UpdateIngredient(NetworkString<_32> recipeName)
		{
			Recipe newRecipe = StageRecipeManager.Instance.RecipeTable[recipeName.ToString()];
			MixDataList.Clear();
			foreach (var item in newRecipe.MixList)
			{
				MixDataList.Add(item);
			}

			UpdattingMixData?.Invoke(MixDataList);
			gameObject.name = newRecipe.Name;
			Name = newRecipe.Name;
			UpdateModel(newRecipe);
		}

		/// <summary>
		/// 레시피를 기반으로 해서 그에 맞는 오브젝트로 업데이트 합니다.
		/// </summary>
		/// <param name="newRecipe"></param>
		private void UpdateModel(Recipe newRecipe)
		{
			if (_modelObject != null)
			{
				_modelObject.Relese();
			}

			_modelObject = PoolManager.Instance.GetPooledObject(newRecipe.PoolObjectType);
			_modelObject.transform.parent = _modelParent;
			_modelObject.transform.localPosition = Vector3.zero;
			_modelObject.transform.rotation = Quaternion.identity;
		}

		/// <summary>
		/// 재료를 합치고 정렬을 진행합니다.
		/// </summary>
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

		/// <summary>
		/// [RPC] 조리 도구에 있는 레시피를 가져와서 조리를 시작합니다.
		/// </summary>
		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_StartCook(NetworkObject cookingToolID, int recipeIndex)
		{
			var cookingRecipe = Runner.FindObject(cookingToolID).GetComponent<IGetCookingRecipe>();
			_cookingRecipe = cookingRecipe.GetCookingRecipe(recipeIndex);
			_currentProgress = 0.0f;
			CurrentState = CookState.Cooking;
			CookingStart?.Invoke();
		}

		/// <summary>
		/// [RPC] 조리도구를 통해서 조리를 진행합니다.
		/// 설정된 조리 레시피를 기반으로 일정 진행도를 넘어가면 재료의 데이터가 업데이트 됩니다.
		/// </summary>
		/// <param name="addProgress"></param>
		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_Cooking(float addProgress)
		{
			if (CurrentState == CookState.None || CurrentState == CookState.Fail)
				return;

			_currentProgress += addProgress;
			UpdattingProgress?.Invoke(_currentProgress);

			switch (CurrentState)
			{
				case CookState.Cooking:
					{
						if (_currentProgress >= _cookingRecipe.SucessProgress)
						{
							CurrentState = CookState.Sucess;
							RPC_UpdateIngredient(_cookingRecipe.Sucess.Name);
							CookingSucess?.Invoke();
						}
						break;
					}
				case CookState.Sucess:
					{
						if (_currentProgress >= _cookingRecipe.FailProgress)
						{
							CurrentState = CookState.Fail;
							RPC_UpdateIngredient(_cookingRecipe.Fail.Name);
							CookingFail?.Invoke();
						}
						break;
					}
			}

		}

		/// <summary>
		/// [RPC] 조리 완료시 호출됩니다.
		/// </summary>
		/// <param name="player"></param>
		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_DoneCooked(PlayerRef player)
		{
			if (HasStateAuthority == true)
			{
				if (CurrentState == CookState.Sucess)
				{
					InGameData.Instance.RPC_AddSuccesCooking(player);
				}
				else if (CurrentState == CookState.Fail)
				{
					InGameData.Instance.RPC_AddFailCooking(player);
				}
			}

			CurrentState = CookState.None;
			_cookingRecipe = null;
			DoneCooked?.Invoke();
		}

		/// <summary>
		/// [RPC] 재료를 합치는 경우 들어온 재료의 오브젝트를 삭제할 때 사용됩니다.
		/// </summary>
		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_DespawnIngredient(NetworkObject despawnObject)
		{
			Runner.Despawn(despawnObject);
		}

		/// <summary>
		/// 오브젝트가 Despawn시 MixDataList를 초기화 합니다.
		/// </summary>
		/// <param name="runner"></param>
		/// <param name="hasState"></param>
		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			base.Despawned(runner, hasState);
			MixDataList.Clear();
		}
		#endregion
	}
}
