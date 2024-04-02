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

		[field: SerializeField]
		[Networked] private NetworkId NetPickObjectID { set; get; } = default(NetworkId);
		public bool HasPickUpObject => NetPickObjectID != default;

		public event Action OnPickUp;
		public event Action OnDrop;
		public event Action OnCutEvent;
		public event Action OnCleanEvent;

		public bool IsPickUpInteractor(PickableInteractable target)
		{
			if (target == null)
				return false;

			if (NetPickObjectID == null)
				return false;

            return GetPickObject() == target;
		}

		public bool IsPickUpInteractor(IInteractable target)
		{
			return IsPickUpInteractor(target as PickableInteractable);
		}

		public PickableInteractable GetPickObject()
		{
			if (NetPickObjectID == default)
				return null;

			return Runner.FindObject(NetPickObjectID).GetComponent<PickableInteractable>();
		}


		[Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_PickUp(NetworkId id)
		{
			Debug.Log("PickUp!");

            var obj = Runner.FindObject(id);
			if(obj.TryGetComponent<PickableInteractable>(out var pickUpObject) == true)
			{
				obj.transform.SetParent(PickUpPoint);
				obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

				if(HasStateAuthority == true)
				{
					NetPickObjectID = obj;
					OnPickUp?.Invoke();
				}
			}
			else
			{
				throw new System.Exception("This object isn't IInteractable");
			}
		}

	
		public PickableInteractable Drop()
		{
			if (HasPickUpObject == false)
				return null;
			var dropObject = GetPickObject();
			RPC_RelesePickUpObject();
			return dropObject;
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_RelesePickUpObject()
		{
			var pickedObject = Runner.FindObject(NetPickObjectID);
			pickedObject.transform.SetParent(null);

			if(HasInputAuthority == true)
			{
				NetPickObjectID = default;
				OnDrop?.Invoke();
			}
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
		public void OnInteraction()
		{
            var interactionObject = DetectedInteractable();

			//��ȣ�ۿ� ������Ʈ�� ã�� ���ߴٸ� 
			if (interactionObject == null)
			{
				return;
			}

			Interaction(interactionObject);
		}

		public bool Interaction(IInteractable interactionObject)
		{
			if (GetPickObject()?.Type > interactionObject.Type)
			{
				Debug.Log("��ȣ �ۿ� ����! �Ⱦ� ������Ʈ�� ��ü");
				return GetPickObject().TryInteraction(this, interactionObject);
			}
			else
			{
				Debug.Log("��ȣ �ۿ� ����! ������Ʈ�� ��ü");
				return interactionObject.TryInteraction(this, GetPickObject());
			}
		}


        public InteractionType GetPickUpObjectType()
		{
            if (HasPickUpObject == false)
                return InteractionType.None;
            return GetPickObject().Type;
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
