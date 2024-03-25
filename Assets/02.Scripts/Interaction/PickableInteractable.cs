using Fusion;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public abstract class PickableInteractable : NetworkBehaviour, IInteractable
	{
		public virtual InteractionType Type => InteractionType.None;

		public virtual bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if(interactor.HasPickUpObject == false)
			{
				interactor.RPC_PickUp(Object);
				return true;
			}
			return false;
		}
	}
}
