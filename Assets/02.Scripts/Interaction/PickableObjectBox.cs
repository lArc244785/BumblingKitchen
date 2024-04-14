using Fusion;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public class PickableObjectBox : NetworkBehaviour, IInteractable
	{
		[SerializeField] private Transform _spawnPoint;
		[SerializeField] private PickableInteractable _prefab;

		public InteractionType Type => InteractionType.Box;
		public NetworkId NetworkId => Object.Id;

		public bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if (interactor.HasPickUpObject == true)
				return false;

			InGameData.Instance.RPC_AddSpawnObject(Runner.LocalPlayer);
			RPC_Spawn(interactor.Object);
			return true;
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void RPC_Spawn(NetworkId pickUpUserID)
		{
			var interactor = Runner.FindObject(pickUpUserID).GetComponent<Interactor>();
			var netObject = Runner.Spawn(
				_prefab, 
				interactor.PickUpPoint.position,
				interactor.PickUpPoint.rotation).GetComponent<NetworkObject>();
			
			interactor.RPC_PickUp(netObject);
		}
	}
}
