using Fusion;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public class Box : NetworkBehaviour, IInteractable
	{
		[SerializeField] private Transform _spawnPoint;
		[SerializeField] private NetworkPrefabRef _prefab;
		public InteractionType Type => InteractionType.Box;

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void RPC_PickUpObject(NetworkId interactorID)
		{
			var instance = Runner.Spawn(_prefab);
			Runner.FindObject(interactorID).GetComponent<Interactor>().RPC_PickUp(instance);
		}

		public bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if (interactor.HasPickUpObject == true)
				return false;

			RPC_SpawnPickUpObject(interactor.Object);
			return true;
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void RPC_SpawnPickUpObject(NetworkId pickUpUserID)
		{
			var interactor = Runner.FindObject(pickUpUserID).GetComponent<Interactor>();
			var pickUpObject = Runner.Spawn(_prefab, _spawnPoint.position, _spawnPoint.rotation);
			pickUpObject.transform.position = _spawnPoint.position;
			interactor.RPC_PickUp(pickUpObject);
		}

	}
}
