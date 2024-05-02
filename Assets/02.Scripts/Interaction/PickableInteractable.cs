using Fusion;
using System;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public abstract class PickableInteractable : NetworkBehaviour, IInteractable, IObjectPickUpEvent
	{
		public virtual InteractionType Type => InteractionType.None;

		public NetworkId NetworkId => Object.Id;

		public event Action OnPickUpObject;

		public virtual bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if(interactor.HasPickUpObject == false)
			{
				interactor.RPC_OnPickuping(Object);
				OnPickUpObject?.Invoke();
				return true;
			}
			return false;
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_SetActive(bool isActive)
		{
			gameObject.SetActive(isActive);
		}

		public void InvokePickUpObject()
		{
			OnPickUpObject?.Invoke();
		}
	}
}
