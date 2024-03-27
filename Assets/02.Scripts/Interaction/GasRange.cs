using UnityEngine;
using Fusion;
using System;

namespace BumblingKitchen.Interaction
{
	public class GasRange : NetworkBehaviour, IInteractable
	{
		[SerializeField] private Transform _putPoint;

		public InteractionType Type => InteractionType.GasRange;
		[SerializeField] private FryingPan _putFryingPan;

		public bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			//interactor ��ȣ �ۿ� - �տ� �ƹ��͵� ���� ���
			if (interactor.HasPickUpObject == false)
			{
				if (_putFryingPan == null)
				{
					return false;
				}
				else
				{
					interactor.RPC_PickUp(_putFryingPan.Object);
					RPC_DropObject();
				}
			}
			//interactor ��ȣ �ۿ� - �տ� ���� �ִ� ���
			else
			{
				Debug.Log("��!�� " +interactor.GetPickObject().Type);
				if (_putFryingPan == null && interactor.GetPickObject().Type == InteractionType.FireKitchenTool)
				{
					RPC_PutObject(interactor.Drop().Object);
				}
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
		private void RPC_DropObject()
		{
			_putFryingPan.IsOnGasRange = false;
			_putFryingPan = null;
		}

	}
}
