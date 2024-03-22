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

        private IInteractable _pickUpObject = null;
        public bool HasPickUpObject => _pickUpObject != null;

        public bool IsPickUpInteractor(IInteractable obj)
		{
            return _pickUpObject == obj;
		}

		public event Action OnPickUp;
		public event Action OnDrop;


		[Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_PickUp(NetworkId id)
		{
            if (HasPickUpObject == true)
                return;

            var obj = Runner.FindObject(id);
            obj.transform.SetParent(_pickUpPoint);
            obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			OnPickUp?.Invoke();

			if (obj.TryGetComponent<IInteractable>(out var pickUpObject))
			{
				_pickUpObject = pickUpObject;
			}
			else
			{
				throw new System.Exception("This object isn't IInteractable");
			}
		}

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_Drop()
		{
            if (HasPickUpObject == false)
                return;

            var obj = _pickUpObject as NetworkObject;
            obj.transform.SetParent(null);
			OnDrop?.Invoke();

			RPC_ClearPickUpObject();
		}

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ClearPickUpObject()
		{
            _pickUpObject = null;

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

				if(interactableObject == _pickUpObject)
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

			if (_pickUpObject == null)
			{
				interactionObject.Interaction(this, null);
			}
			else if (_pickUpObject.Type > interactionObject.Type)
			{
				_pickUpObject.Interaction(this, interactionObject);
			}
			else
			{
				interactionObject.Interaction(this, _pickUpObject);
			}
		}

        public InteractionType GetPickUpObjectType()
		{
            if (HasPickUpObject == false)
                return InteractionType.None;
            return _pickUpObject.Type;
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
