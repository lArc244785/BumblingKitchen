using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public class Ingredient : PickableInteractable
	{
		public override InteractionType Type => InteractionType.Ingredient;

		[SerializeField] Transform _modelParent;

		/// <summary>
		/// �� ��ῡ ���� ������ ��´� �߰��� �� ���� ���ĵȴ�.
		/// </summary>
		public List<IngredientData> MixDataList { get; } = new();
		private PooledObject _modelObject;


		private bool _isInitModel = false;

		public string Name { private set; get; }

		//���� �����Ƿ� ��Ḧ ������ �� ���ȴ�.
		private CookingRecipe _cookingRecipe;

		private float _currentProgress;
		public CookState CurrentState { get; private set; }

		public Action OnCookingStart;
		public Action<float> OnUpdattingProgress;
		public Action OnCookingSucess;
		public Action OnDoneCooked;
		public Action OnCookingFail;
		public Action<List<IngredientData>> OnUpdateMixData;

		[Networked] private NetworkString<_32> RecipeName { get; set; }

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
			OnUpdateMixData?.Invoke(MixDataList);
			gameObject.name = recipe.Name;
		}

		public void Init(Recipe recipe)
		{
			RecipeName = recipe.Name;

			//MixDataList.Clear();
			//foreach (var item in recipe.MixList)
			//{
			//	MixDataList.Add(item);
			//}
			//Name = recipe.Name;
			//UpdateModel(recipe);
			//OnUpdateMixData?.Invoke(MixDataList);
			//gameObject.name = recipe.Name;
		}


		/// <summary>
		/// �ΰ��� ��Ḧ ��ĥ������ �� ���.
		/// �ΰ��� ������ �� �뷮�� �Ѿ�� �Ǹ� ��ġ�� �ʴ´�.
		/// </summary>
		/// <param name="inputMixList"></param>
		/// <returns></returns>
		public bool TryMix(Ingredient ingredinet)
		{
			//�뷮�� �������
			if (MixDataList.Count + ingredinet.MixDataList.Count > MixDataList.Capacity)
			{
				return false;
			}

			var newMixIngredientList = CreateMixIngredient(ingredinet.MixDataList);
			//�����ǿ� �ִ� �������� Ȯ��
			var foundRecipe = StageRecipeManager.Instance.FindRecipe(newMixIngredientList);
			if (foundRecipe == null)
				return false;

			RPC_ChangeIngredient(foundRecipe.Name);
			return true;
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_ChangeIngredient(NetworkString<_32> name)
		{
			Debug.Log($"��� ������Ʈ {name}");
			Recipe newRecipe = StageRecipeManager.Instance.RecipeTable[name.ToString()];
			MixDataList.Clear();
			foreach (var item in newRecipe.MixList)
			{
				MixDataList.Add(item);
			}

			OnUpdateMixData?.Invoke(MixDataList);
			gameObject.name = newRecipe.Name;
			Name = newRecipe.Name;
			UpdateModel(newRecipe);
		}

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

		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_DoneCook(PlayerRef player)
		{
			if(HasStateAuthority == true)
			{
				if(CurrentState == CookState.Sucess)
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


		private List<IngredientData> CreateMixIngredient(List<IngredientData> mixDataList)
		{
			List<IngredientData> newMixIngredient = new();
			//����
			foreach (var ingredient in MixDataList)
			{
				newMixIngredient.Add(ingredient);
			}

			//�߰�
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

			//��Ḹ ��ȣ�ۿ��� �����ϴ�.
			if (interactable.Type != InteractionType.Ingredient)
			{
				return false;
			}

			Ingredient ingredient = interactable as Ingredient;

			// ��� ���� �õ�
			if (TryMix(ingredient) == true)
			{
				PickableInteractable pickableObject = interactable as PickableInteractable;
				
				if(pickableObject == null)
				{
					throw new Exception("This ingredient isn't PickableInteractable!");
				}

				if(interactor.IsPickUpInteractor(pickableObject) == true)
				{
					interactor.DropPickUpObject();
				}

				RPC_DespawnIngredient(ingredient.Object);
			}

			return false;
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_StartCook(NetworkObject cookingToolID, int recipeIndex)
		{
			var cookingRecipe = Runner.FindObject(cookingToolID).GetComponent<IGetCookingRecipe>();
			_cookingRecipe = cookingRecipe.GetCookingRecipe(recipeIndex);
			_currentProgress = 0.0f;
			CurrentState = CookState.Cooking;
			OnCookingStart?.Invoke();
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			base.Despawned(runner, hasState);
			MixDataList.Clear();
		}
	}
}
