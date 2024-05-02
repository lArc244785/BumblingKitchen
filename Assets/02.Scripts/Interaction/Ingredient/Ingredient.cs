using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public class Ingredient : PickableInteractable
	{
		// PUBLIC	=======================================
		public override InteractionType Type => InteractionType.Ingredient;
		/// <summary>
		/// ���յ� ��� �̸�
		/// </summary>
		public string Name { private set; get; }
		/// <summary>
		/// �� ��� ����Ʈ �߰��� �� ���� ���ĵȴ�.
		/// </summary>
		public List<IngredientData> MixDataList { get; } = new();
		public CookState CurrentState { get; private set; }

		// PRIVATE	======================================
		[SerializeField] private Transform _modelParent;
		private PooledObject _modelObject;
		//���� �����Ƿ� ��Ḧ ������ �� ���ȴ�.
		private CookingRecipe _cookingRecipe;
		/// <summary>
		/// ���� ������ �̿��� ���� ���൵
		/// </summary>
		private float _currentProgress;
		/// <summary>
		/// [Networked] ���� ���յ� ������ ������ �̸�
		/// </summary>
		[Networked] private NetworkString<_32> RecipeName { get; set; }
		
		// EVENT	=======================================
		public Action CookingStart;
		public Action<float> UpdattingProgress;
		public Action CookingSucess;
		public Action DoneCooked;
		public Action CookingFail;
		public Action<List<IngredientData>> UpdattingMixData;
		//================================================

		public void Init(Recipe recipe)
		{
			RecipeName = recipe.Name;
		}
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


		/// <summary>
		/// �ΰ��� ��Ḧ ��ġ�⸦ �õ��մϴ�.
		/// ��Ḧ ������ �� �뷮�� �Ѿ�� �Ǹ� ��ġ�� �ʴ´�.
		/// ��Ḧ ������ �� �ش� ���������� �����ǰ� �������� �ʴ� ��� ��ġ�� �ʽ��ϴ�.
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

			RPC_UpdateIngredient(foundRecipe.Name);
			return true;
		}
		/// <summary>
		/// [RPC] �����ǿ� ���߾ ����� ������ ���־� �����͸� ������Ʈ �մϴ�.
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
		/// ��Ḧ ��ġ�� ������ �����մϴ�.
		/// </summary>
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

		/// <summary>
		/// [RPC] ���� ������ �ִ� �����Ǹ� �����ͼ� ������ �����մϴ�.
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
						if(_currentProgress >= _cookingRecipe.SucessProgress)
						{
							CurrentState = CookState.Sucess;
							RPC_UpdateIngredient(_cookingRecipe.Sucess.Name);
							CookingSucess?.Invoke();
						}
						break;
					}
				case CookState.Sucess:
					{
						if(_currentProgress >= _cookingRecipe.FailProgress)
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
		/// [RPC] ���� �Ϸ�
		/// </summary>
		/// <param name="player"></param>
		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_DoneCooked(PlayerRef player)
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
			DoneCooked?.Invoke();
		}


		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_DespawnIngredient(NetworkObject despawnObject)
		{
			Runner.Despawn(despawnObject);
		}
		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			base.Despawned(runner, hasState);
			MixDataList.Clear();
		}
	}
}
