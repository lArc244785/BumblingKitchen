using Fusion;
using BumblingKitchen.Interaction;
using UnityEngine;

namespace BumblingKitchen
{
	public class Server : NetworkBehaviour, IInteractable
	{
		public InteractionType Type => InteractionType.Server;

		[SerializeField] private Outlet _outLet;
		[SerializeField] private OrderManger _orderManger;

		public bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if (interactor.HasPickUpObject == false)
				return false;

			if (interactable.Type != InteractionType.Plate)
				return false;
			
			Plate plate = interactor.GetPickObject() as Plate;

			if (plate.IsDirty == true || plate.IsPutIngredient() == false)
				return false;
			interactor.Drop();
			var ingredient = plate.SpillIngredient();
			_outLet.SendPlate(plate);
			RPC_DeSpawn(ingredient.Object);
			_orderManger.OrderCheck(ingredient.Name);
			InGameData.Instance.RPC_AddSendToOrder(Runner.LocalPlayer);
			return false;
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_DeSpawn(NetworkObject netObject)
		{
			Runner.Despawn(netObject);
		}
	}
}
