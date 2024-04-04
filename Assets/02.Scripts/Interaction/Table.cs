using Fusion;
using System;
using UnityEngine;

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
			if (_putObject == null)
			{
				if(interactor.HasPickUpObject == true)
				{
					RPC_PutObject(interactor.Drop().Object);
					return true;
				}
			}
			else
			{
				var putObject = _putObject;
				if (interactor.HasPickUpObject == false)
				{
					RPC_RelesePutObject();
				}
				return interactor.Interaction(putObject);
			}

			return false;
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

			_putObject.OnPickUpObject += RPC_RelesePutObject;
		}
		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_RelesePutObject()
		{
			_putObject.OnPickUpObject -= RPC_RelesePutObject;
			_putObject = null;
		}

	}
}
