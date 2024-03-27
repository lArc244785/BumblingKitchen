using UnityEngine;
using Fusion;

namespace BumblingKitchen.Interaction
{
    public class Plate : PickableInteractable
    {
		public override InteractionType Type => InteractionType.Plate;

		private Ingredient _putIngredient;
		[SerializeField] private Transform _putPoint;

		public override bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if(base.TryInteraction(interactor, interactable) == true)
			{
				return true;
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
							RPC_PutIngredient(ingredient.Object);
						}
						else if(_putIngredient.TryMix(ingredient) == true)
						{
							Debug.Log("접시에 재료를 추가합니다.");
							if (interactor.IsPickUpInteractor(interactable) == true)
							{
								interactor.Drop();
							}
							Runner.Despawn(ingredient.Object);
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
							fryingPan.RPC_RelesePutIngredient();
						}
						else if(_putIngredient.TryMix(ingredient) == true)
						{
							fryingPan.RPC_RelesePutIngredient();
							Runner.Despawn(ingredient.Object);
						}
					}
					break;
			}




			return false;
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_PutIngredient(NetworkId id)
		{
			_putIngredient = Runner.FindObject(id).GetComponent<Ingredient>();
			_putIngredient.transform.SetParent(_putPoint);
			_putIngredient.transform.localPosition = Vector3.zero;
			_putIngredient.transform.localRotation = Quaternion.identity;
		}
	}
}
