using Fusion;
using System;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public abstract class PickableInteractable : NetworkBehaviour, IInteractable, IObjectPickUpEvent
	{
		#region Property
		/// <summary>
		/// 상호작용 오브젝트의 타입
		/// </summary>
		public virtual InteractionType Type => InteractionType.None;

		/// <summary>
		/// Network 환경에서의 오브젝트 ID
		/// </summary>
		public NetworkId NetworkId => Object.Id;
		#endregion


		#region Event
		/// <summary>
		/// 픽업 시 발생하는 이벤트
		/// </summary>
		public event Action PickupingObject;
		#endregion


		#region Method
		/// <summary>
		/// 상호작용 주체가 픽업이 가능한 경우 해당 오브젝트를 픽업합니다.
		/// </summary>
		public virtual bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			//상호작용 주체가 픽업한 객체가 없는 경우 픽업 실행한다.
			if (interactor.HasPickUpObject == false)
			{
				interactor.RPC_Pickup(Object);
				OnPickupingObject();
				return true;
			}
			return false;
		}

		/// <summary>
		/// [RPC] 해당 오브젝트를 비활성화 합니다.
		/// </summary>
		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_SetActive(bool isActive)
		{
			gameObject.SetActive(isActive);
		}

		/// <summary>
		/// 픽업 이벤트 호출 함수입니다.
		/// </summary>
		public void OnPickupingObject()
		{
			PickupingObject?.Invoke();
		}
		#endregion
	}
}
