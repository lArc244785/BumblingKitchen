using UnityEngine;
using Fusion;
using System;

namespace BumblingKitchen.Interaction
{
    public class Plate : PickableInteractable, ISpillIngredient
    {
		public override InteractionType Type => InteractionType.Plate;

		private Ingredient _putIngredient;
		[SerializeField] private Transform _putPoint;
		[SerializeField] private MeshFilter _model;
		public bool IsDirty { get; private set; } = false;

		[SerializeField] private Mesh _clearMesh;
		[SerializeField] private Mesh _dirtyMesh;

		public event Action OnSpawned;

		public override bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if(base.TryInteraction(interactor, interactable) == true)
			{
				return true;
			}

			if(IsDirty == true)
			{
				return false;
			}

			switch(interactable.Type)
			{
				case InteractionType.Ingredient:
					{
						var ingredient = (Ingredient)interactable;

						if(_putIngredient == null)
						{
							if (interactor.IsPickUpInteractor(interactable) == true)
							{
								interactor.Drop();
							}
							ingredient.InvokePickUpObject();
							RPC_PutIngredient(ingredient.Object);
						}
						else if(_putIngredient.TryMix(ingredient) == true)
						{
							Debug.Log("접시에 재료를 추가합니다.");
							if (interactor.IsPickUpInteractor(interactable) == true)
							{
								interactor.Drop();
							}
							ingredient.InvokePickUpObject();
							RPC_DespawnObject(ingredient.Object);
						}
					}
					break;
				case InteractionType.FireKitchenTool:
					{
						var fryingPan = (FryingPan)interactable;
						if (fryingPan.CanDropIngredient() == false)
							return false;

						Ingredient ingredient = fryingPan.GetIngredient();

						if (_putIngredient == null)
						{
							RPC_PutIngredient(ingredient.Object);
							fryingPan.RPC_RelesePutIngredient(Runner.LocalPlayer);
						}
						else if(_putIngredient.TryMix(ingredient) == true)
						{
							fryingPan.RPC_RelesePutIngredient(Runner.LocalPlayer);
							RPC_DespawnObject(ingredient.Object);
						}
					}
					break;
			}

			return false;
		}

		public Ingredient SpillIngredient()
		{
			var spill = _putIngredient;
			RPC_RelesePutIngredient();
			return spill;
		}
		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_DespawnObject(NetworkId despawnObjectId)
		{
			var despawnObject = Runner.FindObject(despawnObjectId);
			Runner.Despawn(despawnObject);
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_RelesePutIngredient()
		{
			if(_putIngredient != null)
			{
				_putIngredient.transform.SetParent(null);
				_putIngredient = null;
			}

			SetDirty(true);
		}
		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_Clear()
		{
			SetDirty(false);
		}

		internal bool IsPutIngredient()
		{
			return _putIngredient != null;
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_PutIngredient(NetworkId id)
		{
			_putIngredient = Runner.FindObject(id).GetComponent<Ingredient>();
			_putIngredient.transform.SetParent(_putPoint);
			_putIngredient.transform.localPosition = Vector3.zero;
			_putIngredient.transform.localRotation = Quaternion.identity;
		}

		public void SetDirty(bool isDirty)
		{
			IsDirty = isDirty;
			_model.mesh = IsDirty ? _dirtyMesh : _clearMesh;
		}
	}
}
