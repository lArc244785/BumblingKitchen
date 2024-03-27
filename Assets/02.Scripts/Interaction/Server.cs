using Fusion;
using BumblingKitchen.Interaction;
using UnityEngine;

namespace BumblingKitchen
{
	public class Server : NetworkBehaviour, IInteractable
	{
		public InteractionType Type => InteractionType.Server;

		[SerializeField] private Outlet _outLet;

		public bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if (interactable.Type != InteractionType.Plate)
				return false;
			
			Plate plate = interactor.Drop() as Plate;

			if (plate.IsDirty == true || plate.IsPutIngredient() == false)
				return false;

			var ingredient = plate.SpillIngredient();
			_outLet.SendPlate(plate);
			RPC_DeSpawn(ingredient.Object);
			//_order.SendOrder(ingredient);

			return false;
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_DeSpawn(NetworkObject netObject)
		{
			Runner.Despawn(netObject);
		}
	}
}
