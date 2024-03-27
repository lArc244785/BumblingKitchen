using UnityEngine;
using Fusion;
using System;

namespace BumblingKitchen.Interaction
{
    public class Interactor : NetworkBehaviour, IHandEvents
    {
        [SerializeField] private Transform _pickUpPoint;

        [SerializeField] private Vector3 _detectedStartLocalPoint;
        [SerializeField] private float _detectDistance;
        [SerializeField] private LayerMask _detectLayerMask;

		[field: SerializeField]
		[Networked] private NetworkId NetPickObjectID { set; get; } = default(NetworkId);
		public bool HasPickUpObject => NetPickObjectID != default;

        public bool IsPickUpInteractor(PickableInteractable target)
		{
			if (target == null)
				return false;

			if (NetPickObjectID == null)
				return false;

            return GetPickObject() == target;
		}

		private bool IsPickUpInteractor(IInteractable target)
		{
			return IsPickUpInteractor(target as PickableInteractable);
		}


		public event Action OnPickUp;
		public event Action OnDrop;

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
				obj.transform.SetParent(_pickUpPoint);
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
		public void OnInteraction()
		{
            var interactionObject = DetectedInteractable();

			//상호작용 오브젝트를 찾지 못했다면 
			if (interactionObject == null)
			{
				return;
			}

			Interaction(interactionObject);
		}

		public void Interaction(IInteractable interactionObject)
		{
			if (GetPickObject()?.Type > interactionObject.Type)
			{
				Debug.Log("상호 작용 시작! 픽업 오브젝트가 주체");
				GetPickObject().TryInteraction(this, interactionObject);
			}
			else
			{
				Debug.Log("상호 작용 시작! 오브젝트가 주체");
				interactionObject.TryInteraction(this, GetPickObject());
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
