using Fusion;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public class Box : NetworkBehaviour, IInteractable
	{
		[SerializeField] private NetworkPrefabRef _prefab;
		public InteractionType Type => InteractionType.Box;

		public bool CanInteraction(InteractionType type)
		{
			return type == InteractionType.Interactor;
		}

		public bool CanInteraction(Interactor interactor)
		{
			InteractionType type = interactor.HasPickUpObject ? interactor.GetPickUpObjectType() : InteractionType.Interactor;
			return CanInteraction(type);
		}

		public void Interaction(Interactor interactor, IInteractable interactionObject)
		{
			if(CanInteraction(interactor) == true)
			{
				RPC_PickUpObject(interactor.GetComponent<NetworkObject>());
			}
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void RPC_PickUpObject(NetworkId interactorID)
		{
			var instance = Runner.Spawn(_prefab);
			Runner.FindObject(interactorID).GetComponent<Interactor>().RPC_PickUp(instance);
		}
	}
}
