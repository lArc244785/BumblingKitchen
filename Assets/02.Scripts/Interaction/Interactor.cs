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
		/// 가장 우선 순위가 높은 IInteractable을 반환합니다. 단 현재 집고 있는 오브젝트는 제외입니다.
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
		/// 캐릭터가 바라보는 방향에서 일정 지점에서 있는 오브젝트를 탐색하고 가장 우선순위가 높은 오브젝트와 상호작용을 시도합니다.
		/// </summary>
		public void Interaction()
		{
            var interactionObject = DetectedInteractable();

			//상호작용 오브젝트를 찾지 못했다면 
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
				Debug.Log("상호 작용 시작! 픽업 오브젝트가 주체");
				return PickUpObject.TryInteraction(this, interactionObject);
			}
			else
			{
				Debug.Log("상호 작용 시작! 오브젝트가 주체");
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
