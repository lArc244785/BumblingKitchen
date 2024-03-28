using Fusion;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public class IngredientBox : NetworkBehaviour, IInteractable
	{
		[SerializeField] private Transform _spawnPoint;
		[SerializeField] private NetworkPrefabRef _prefab;
		[SerializeField] private Recipe _spawnIngredient;

		public InteractionType Type => InteractionType.Box;

		public bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if (interactor.HasPickUpObject == true)
				return false;

			InGameData.Instance.RPC_AddSpawnObject(Runner.LocalPlayer);
			RPC_SpawnIngredient(interactor.Object);
			return true;
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void RPC_SpawnIngredient(NetworkId pickUpUserID)
		{
			var interactor = Runner.FindObject(pickUpUserID).GetComponent<Interactor>();
			var netObject = Runner.Spawn(
				_prefab, 
				interactor.PickUpPoint.position,
				interactor.PickUpPoint.rotation, 
				inputAuthority : null,
				InitBeforeSpawn);
			
			interactor.RPC_PickUp(netObject);
		}

		private void InitBeforeSpawn(NetworkRunner runner, NetworkObject obj)
		{
			Debug.Log("스폰이 완료되고 초기화를 진행합니다.");
			var ingredeint = obj.GetComponent<Ingredient>();
			ingredeint.InitSetting(_spawnIngredient.Name);
		}
	}
}
