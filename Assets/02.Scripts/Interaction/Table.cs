using UnityEngine;
using Fusion;
using System;

namespace BumblingKitchen.Interaction
{
	public class Table : NetworkBehaviour, IInteractable
	{
		[SerializeField] private LayerMask _putObjectLayer;
		[SerializeField] private Transform _putPoint;

		public InteractionType Type => InteractionType.Table;
		[SerializeField] private PickableInteractable _putObject;

		void Start()
		{
			if (Physics.Raycast(_putPoint.position, Vector3.up, out var hit, 5.0f, _putObjectLayer) == true)
			{
				_putObject = hit.collider.GetComponent<PickableInteractable>();
				_putObject.transform.SetParent(_putPoint);
				_putObject.transform.localPosition = Vector3.zero;
				_putObject.transform.localRotation = Quaternion.identity;
			}
		}

		public bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			//interactor 상호 작용 - 손에 아무것도 없는 경우
			if (interactor.HasPickUpObject == false)
			{
				if (_putObject == null)
				{
					return false;
				}
				else
				{
					interactor.RPC_PickUp(_putObject.Object);
					RPC_RelesePutObject();
				}
			}
			//interactor 상호 작용 - 손에 무언가 있는 경우
			else
			{
				if (_putObject == null)
				{
					RPC_PutObject(interactor.Drop().Object);
				}
				else
				{
					interactor.Interaction(_putObject);
				}
			}
			return true;
		}


		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_PutObject(NetworkId id)
		{
			_putObject = Runner.FindObject(id).GetComponent<PickableInteractable>();
			if (_putObject == null)
			{
				throw new Exception("This object isn't PickableInteractable");
			}

			_putObject.transform.SetParent(_putPoint);
			_putObject.transform.localPosition = Vector3.zero;
			_putObject.transform.localRotation = Quaternion.identity;
		}
		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_RelesePutObject()
		{
			_putObject = null;
		}

	}
}
