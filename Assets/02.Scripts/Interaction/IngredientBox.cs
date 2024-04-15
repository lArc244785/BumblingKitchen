using Fusion;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public class IngredientBox : NetworkBehaviour, IInteractable
	{
		[SerializeField] private Transform _spawnPoint;
		[SerializeField] private Ingredient _prefab;
		[SerializeField] private Recipe _initRecipe;

		public InteractionType Type => InteractionType.Box;

		public NetworkId NetworkId => Object.Id;

		public bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if (interactor.HasPickUpObject == true)
				return false;

			InGameData.Instance.RPC_AddSpawnObject(Runner.LocalPlayer);
			RPC_SpawnIngredient(interactor.Object, GameManager.Instance.GetDetalPlayTime());
			return true;
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void RPC_SpawnIngredient(NetworkId pickUpUserID, float callTime)
		{
			var interactor = Runner.FindObject(pickUpUserID).GetComponent<Interactor>();
			var netObject = Runner.Spawn(
				_prefab,
				interactor.PickUpPoint.position,
				interactor.PickUpPoint.rotation,
				inputAuthority: null,
				(runner, obj) =>
				{
					obj.GetComponent<Ingredient>().Init(_initRecipe);
					obj.GetComponent<TestRate>().CallTime = callTime;
				}
				).GetComponent<NetworkObject>();

			interactor.RPC_PickUp(netObject);
		}
	}
}
