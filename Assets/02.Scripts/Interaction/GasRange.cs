using Fusion;
using System;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public class GasRange : NetworkBehaviour, IInteractable
	{
		[SerializeField] private Transform _putPoint;

		public InteractionType Type => InteractionType.GasRange;

		public NetworkId NetworkId => Object.Id;

		[SerializeField] private FryingPan _putFryingPan;

		public bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if (_putFryingPan == null)
			{
				if (interactor.PickUpObject.Type == InteractionType.FireKitchenTool)
				{
					var fryingpan = interactor.PickUpObject as FryingPan;
					if (fryingpan.CanPutGasRange() == true)
					{
						RPC_PutObject(interactor.DropPickUpObject().NetworkId);
					}
				}
			}
			else
			{
				var putObject = _putFryingPan;
				if (interactor.HasPickUpObject == false)
				{
					RPC_ReleseObject();
				}
				interactor.TryInteraction(putObject);
			}

			return true;
		}


		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_PutObject(NetworkId id)
		{
			_putFryingPan = Runner.FindObject(id).GetComponent<FryingPan>();
			if (_putFryingPan == null)
			{
				throw new Exception("This object isn't PickableInteractable");
			}

			_putFryingPan.transform.SetParent(_putPoint);
			_putFryingPan.transform.localPosition = Vector3.zero;
			_putFryingPan.transform.localRotation = Quaternion.identity;
			_putFryingPan.IsOnGasRange = true;
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_ReleseObject()
		{
			_putFryingPan.IsOnGasRange = false;
			_putFryingPan = null;
		}

	}
}
