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
				OnPickUpCall();
				return true;
			}
			return false;
		}

		public virtual void OnPickUpCall() { }

		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_SetActive(bool isActive)
		{
			gameObject.SetActive(isActive);
		}
	}
}
