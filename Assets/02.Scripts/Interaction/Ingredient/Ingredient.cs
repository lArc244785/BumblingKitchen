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
		/// ���յ� ��� �̸�
		/// </summary>
		public string Name { private set; get; }
		/// <summary>
		/// �� ��� ����Ʈ �߰��� �� ���� ���ĵȴ�.
		/// </summary>
		public List<IngredientData> MixDataList { get; } = new();
		/// <summary>
		/// ����� ���� ����
		/// </summary>
		public CookState CurrentState { get; private set; }
		/// <summary>
		/// [Networked] ���� ���յ� ������ ������ �̸�
		/// </summary>
		[Networked] private NetworkString<_32> RecipeName { get; set; }
		#endregion


		#region Field
		/// <summary>
		/// �� ������Ʈ�� ��ġ�� �θ� Transfrom
		/// </summary>
		[SerializeField] private Transform _modelParent;

		/// <summary>
		/// �� ������Ʈ �ش� ������Ʈ�� ��� PooledObject�� ������ �־�ߵ˴ϴ�.
		/// </summary>
		private PooledObject _modelObject;
		
		/// <summary>
		/// ���� �� ���Ǵ� ������
		/// </summary>
		private CookingRecipe _cookingRecipe;
		
		/// <summary>
		/// ���� ������ �̿��� ���� ���൵
		/// </summary>
		private float _currentProgress;
		#endregion


		#region Event
		/// <summary>
		/// ���� ���� �� ȣ��Ǵ� �̺�Ʈ
		/// </summary>
		public Action CookingStart;

		/// <summary>
		/// ������ ���൵�� ������Ʈ �� ȣ��Ǵ� �̺�Ʈ
		/// </summary>
		public Action<float> UpdattingProgress;
		
		/// <summary>
		/// ���� ���� �� ȣ��Ǵ� �̺�Ʈ
		/// </summary>
		public Action CookingSucess;
		
		/// <summary>
		/// ������ ������ �� ȣ��Ǵ� �̺�Ʈ
		/// </summary>
		public Action DoneCooked;
		
		/// <summary>
		/// ���� ���� �� ȣ��Ǵ� �̺�Ʈ
		/// </summary>
		public Action CookingFail;
		
		/// <summary>
		/// ����� ���� ��ᰡ ������Ʈ �� ȣ��Ǵ� �̺�Ʈ
		/// </summary>
		public Action<List<IngredientData>> UpdattingMixData;
		#endregion


		#region Method
		/// <summary>
		/// �ʱ� ���� �� �ش� ����� ������ �̸��� �����մϴ�.
		/// </summary>
		/// <param name="recipe"></param>
		public void Init(Recipe recipe)
		{
			RecipeName = recipe.Name;
		}

		/// <summary>
		/// ��Ʈ��ũ ������Ʈ�� ���� �� ȣ��˴ϴ�.
		/// ����ȭ �� �������� �̸� ���ؼ� ���� ����� ���¸� ������Ʈ �մϴ�.
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
		/// 1. ��ȣ�ۿ� ��ü�� �Ⱦ��� ������ ��� �ش� ������Ʈ�� �Ⱦ��մϴ�.
		/// 2. ��ȣ�ۿ� ��ü�� ��Ḧ ��� �ִ� ��� �ش� ��Ḧ ���⸦ �õ��غ��� ������ ��� RPC�� ���ؼ� ���⸦ �����մϴ�.
		/// </summary>
		/// <returns>��ȣ �ۿ� ���� ����</returns>
		/// <exception cref="��ü�� �Ⱦ��� ������Ʈ�� PickableInteractable���� �Ļ��� ��ü�� �ƴ� ���"></exception>
		public override bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if (base.TryInteraction(interactor, interactable) == true)
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

		/// <summary>
		/// �����Ǹ� ������� �ؼ� �׿� �´� ������Ʈ�� ������Ʈ �մϴ�.
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

		/// <summary>
		/// [RPC] ���������� ���ؼ� ������ �����մϴ�.
		/// ������ ���� �����Ǹ� ������� ���� ���൵�� �Ѿ�� ����� �����Ͱ� ������Ʈ �˴ϴ�.
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
		/// [RPC] ���� �Ϸ�� ȣ��˴ϴ�.
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
		/// [RPC] ��Ḧ ��ġ�� ��� ���� ����� ������Ʈ�� ������ �� ���˴ϴ�.
		/// </summary>
		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_DespawnIngredient(NetworkObject despawnObject)
		{
			Runner.Despawn(despawnObject);
		}

		/// <summary>
		/// ������Ʈ�� Despawn�� MixDataList�� �ʱ�ȭ �մϴ�.
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
