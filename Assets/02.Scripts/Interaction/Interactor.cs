using UnityEngine;
using Fusion;
using System;

namespace BumblingKitchen.Interaction
{
    public class Interactor : NetworkBehaviour, IHandEvents, ICutEvent, ICleanEvent
	{
        [field:SerializeField] public Transform PickUpPoint { get; private set; }

        [SerializeField] private Vector3 _detectedStartLocalPoint;
        [SerializeField] private float _detectDistance;
        [SerializeField] private LayerMask _detectLayerMask;

		public IInteractable PickUpObject { get; private set; }

		//private NetworkId _pickObjectID;
		public bool HasPickUpObject => PickUpObject != null;

		public event Action OnPickUp;
		public event Action OnDrop;
		public event Action OnCutEvent;
		public event Action OnCleanEvent;

		public bool IsPickUpInteractor(IInteractable target)
		{
			if (target == null)
				return false;

			if (PickUpObject == target)
				return true;
			else
				return false;
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_PickUp(NetworkId id)
		{
			Debug.Log("PickUp!");

            var obj = Runner.FindObject(id);
			if(obj.TryGetComponent<IInteractable>(out var pickUpObject) == true)
			{
				obj.transform.SetParent(PickUpPoint);
				obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
				PickUpObject = pickUpObject;
				OnPickUp?.Invoke();
			}
			else
			{
				throw new System.Exception("This object isn't IInteractable");
			}
		}

	
		public IInteractable Drop()
		{
			if (HasPickUpObject == false)
				return null;
			var dropObject = PickUpObject;
			RPC_RelesePickUpObject();
			return dropObject;
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_RelesePickUpObject()
		{
			var pickedObject = Runner.FindObject(PickUpObject.NetworkId);
			pickedObject.transform.SetParent(null);
			OnDrop?.Invoke();

			PickUpObject = null;
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_OnCutEvent()
		{
			OnCutEvent?.Invoke();
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_OnCleanEvent()
		{
			OnCleanEvent?.Invoke();
		}

		/// <summary>
		/// ���� �켱 ������ ���� IInteractable�� ��ȯ�մϴ�. �� ���� ���� �ִ� ������Ʈ�� �����Դϴ�.
		/// </summary>
		private IInteractable DetectedInteractable()
		{
			IInteractable detedObject = null;

			Vector3 detectPoint = GetDetectPoint();

			Ray ray = new(detectPoint, Vector3.down);
			var hits = Physics.RaycastAll(ray, _detectDistance, _detectLayerMask);
			Debug.Log($"Detect {hits.Length}");

			foreach (var hit in hits)
			{
				IInteractable interactableObject = hit.collider.GetComponent<IInteractable>();
				if (interactableObject == null)
				{
					throw new MissingComponentException($"{hit.collider.gameObject}");
				}

				if(IsPickUpInteractor(interactableObject) == true)
				{
					continue;
				}	

				if (detedObject == null)
				{
					detedObject = hit.collider.GetComponent<IInteractable>();
				}
				else
				{
					if (detedObject.Type < interactableObject.Type)
					{
						detedObject = interactableObject;
					}
				}
			}

			return detedObject;
		}

		private Vector3 GetDetectPoint()
		{
			return transform.position + (transform.forward + _detectedStartLocalPoint);
		}

		/// <summary>
		/// ĳ���Ͱ� �ٶ󺸴� ���⿡�� ���� �������� �ִ� ������Ʈ�� Ž���ϰ� ���� �켱������ ���� ������Ʈ�� ��ȣ�ۿ��� �õ��մϴ�.
		/// </summary>
		public void Interaction()
		{
            var interactionObject = DetectedInteractable();

			//��ȣ�ۿ� ������Ʈ�� ã�� ���ߴٸ� 
			if (interactionObject == null)
			{
				return;
			}

			TryInteraction(interactionObject);
		}

		public bool TryInteraction(IInteractable interactionObject)
		{
			if (PickUpObject?.Type > interactionObject.Type)
			{
				Debug.Log("��ȣ �ۿ� ����! �Ⱦ� ������Ʈ�� ��ü");
				return PickUpObject.TryInteraction(this, interactionObject);
			}
			else
			{
				Debug.Log("��ȣ �ۿ� ����! ������Ʈ�� ��ü");
				return interactionObject.TryInteraction(this, PickUpObject);
			}
		}

		private void OnDrawGizmos()
		{
			Vector3 detectPoint = GetDetectPoint();
			Gizmos.color = Color.green;
			Ray ray = new(detectPoint, Vector3.down);
			Gizmos.DrawLine(ray.origin, ray.GetPoint(_detectDistance));
		}
	}
}
