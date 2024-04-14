using Fusion;
using System;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public class DryingRack : NetworkBehaviour, IInteractable
	{
		public InteractionType Type => InteractionType.DryingReck;
		public NetworkId NetworkId => Object.Id;

		[Networked, Capacity(5),OnChangedRender(nameof(UpdateCleanPlats))]
		private NetworkLinkedList<NetworkId> CleanPlates { get; } = MakeInitializer(new NetworkId[] { });


		public Action<int, int> OnUpdateCleanPlates;

		public bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if (interactor.HasPickUpObject == true)
				return false;
			if (CleanPlates.Count == 0)
				return false;

			var plat = Runner.FindObject(PopCleanPlate()).GetComponent<PickableInteractable>();
			plat.RPC_SetActive(true);
			interactor.RPC_PickUp(plat.Object);

			return false;
		}

		private NetworkId PopCleanPlate()
		{
			var popPlateID = CleanPlates[0];
			RPC_PopCleanPlate(popPlateID);
			return popPlateID;
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_PopCleanPlate(NetworkId popId)
		{
			CleanPlates.Remove(popId);
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void RPC_SendCleanPlate(NetworkId cleaningPlateID)
		{
			CleanPlates.Add(cleaningPlateID);
			Debug.Log($"CleanPlates Plate {CleanPlates.Count}");
		}

		public bool CanSendCleanPlate()
		{
			return CleanPlates.Count < CleanPlates.Capacity;
		}

		private void UpdateCleanPlats()
		{
			OnUpdateCleanPlates?.Invoke(CleanPlates.Count, CleanPlates.Capacity);
		}
	}
}
