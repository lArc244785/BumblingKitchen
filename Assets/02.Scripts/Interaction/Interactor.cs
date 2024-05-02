using UnityEngine;
using Fusion;
using System;

namespace BumblingKitchen.Interaction
{
    public class Interactor : NetworkBehaviour, IHandEvents, ICutEvent, ICleanEvent
	{
        [field:SerializeField] public Transform PickUpPoint { get; private set; }
		public IInteractable PickupObject { get; private set; }
		public bool HasPickUpObject => PickupObject != null;

		//픽업 오브젝트를 탐색 시작 위치
        [SerializeField] private Vector3 _detectedStartLocalPoint;
		//픽업 오브젝트의 탐색 길이
        [SerializeField] private float _detectDistance;
		//픽업 오브젝트의 레이어 마스크
        [SerializeField] private LayerMask _detectLayerMask;

		#region events
		public event Action Pickuping;
		public event Action DropedPickupobject;
		public event Action Cutting;
		public event Action Cleaning;
		#endregion

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
		
		/// <summary>
		/// 픽업된 오브젝트가 있는 경우 파라미터로 들어온 오브젝트와 우선 순위를 비교해서 상호작용 주체가 결정하여 상호작용을 시도해봅니다.
		/// </summary>
		public bool TryInteraction(IInteractable interactionObject)
		{
			if (PickupObject?.Type > interactionObject.Type)
			{
				Debug.Log("상호 작용 시작! 픽업 오브젝트가 주체");
				return PickupObject.TryInteraction(this, interactionObject);
			}
			else
			{
				Debug.Log("상호 작용 시작! 오브젝트가 주체");
				return interactionObject.TryInteraction(this, PickupObject);
			}
		}

		/// <summary>
		/// 해당 오브젝트가 현재 픽업한 오브젝트인지 확인합니다.
		/// </summary>
		public bool IsPickUpInteractor(IInteractable target)
		{
			if (target == null)
			{
				return false;
			}

			if (PickupObject == target)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// [RPC] 네트워크 오브젝트를 픽업 오브젝트로 지정합니다.
		/// </summary>
		[Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_Pickup(NetworkId id)
		{
            var obj = Runner.FindObject(id);

			if(obj.TryGetComponent<IInteractable>(out var pickUpObject) == true)
			{
				obj.transform.SetParent(PickUpPoint);
				obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
				PickupObject = pickUpObject;
				Pickuping?.Invoke();
			}
			else
			{
				throw new System.Exception("This object isn't IInteractable");
			}
		}

		/// <summary>
	/// 픽업한 오브젝트를 드롭하고 해당 오브젝트를 반환합니다.
	/// </summary>
		public IInteractable DropPickUpObject()
		{
			if (HasPickUpObject == false)
			{
				return null;
			}

			var dropObject = PickupObject;
			RPC_OnDroped();
			return dropObject;
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
			//RayCast된 오브젝트들 중에서 가장 우선 순위가 높은 객체를 찾습니다.
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
		/// <summary>
		/// 현재 위치에서의 탐색 시작 위치를 가져옵니다.
		/// </summary>
		private Vector3 GetDetectPoint()
		{
			return transform.position + (transform.forward + _detectedStartLocalPoint);
		}

		/// <summary>
		/// [RPC] 픽업 오브젝트를 드롭하는 경우 호출 됩니다.
		/// </summary>
		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_OnDroped()
		{
			DropedPickupobject?.Invoke();
			PickupObject = null;
		}

		/// <summary>
		/// 손질과 관련된 로직 호출시 호출되어야됩니다.
		/// </summary>

		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_OnCuttingEvent()
		{
			Cutting?.Invoke();
		}

		/// <summary>
		/// 청소 관련 로직 호출시 호출되어야됩니다.
		/// </summary>

		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_OnCleaningEvent()
		{
			Cleaning?.Invoke();
		}

		//Debug 용
		#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			Vector3 detectPoint = GetDetectPoint();
			Gizmos.color = Color.green;
			Ray ray = new(detectPoint, Vector3.down);
			Gizmos.DrawLine(ray.origin, ray.GetPoint(_detectDistance));
		}
		#endif
	}
}
