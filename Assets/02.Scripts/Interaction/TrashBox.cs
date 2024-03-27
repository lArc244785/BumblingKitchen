using UnityEngine;
using BumblingKitchen.Interaction;
using Fusion;

namespace BumblingKitchen
{
	public class TrashBox : NetworkBehaviour, IInteractable
	{
		public InteractionType Type => InteractionType.TrashBox;

		public bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			switch (interactable.Type)
			{
				case InteractionType.Ingredient:
					{
						Runner.Despawn(interactor.Drop().Object);
						return true;
					}
					break;
				case InteractionType.Plate:
					{
						Plate plate = interactable as Plate;
						if(plate.IsPutIngredient() == true)
						{
							RPC_Despawn(plate.SpillIngredient().Object);
						}
						else
						{
							return false;
						}
						return true;
					}
					break;
				case InteractionType.FireKitchenTool:
					{
						FryingPan pan = interactable as FryingPan;
						RPC_Despawn(pan.SpillIngredient().Object);
					}
					break;
			}

			return false;
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_Despawn(NetworkObject netObject)
		{
			Runner.Despawn(netObject);
		}
	}
}
