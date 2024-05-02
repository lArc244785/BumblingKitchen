using Fusion;
using System;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public abstract class PickableInteractable : NetworkBehaviour, IInteractable, IObjectPickUpEvent
	{
		public virtual InteractionType Type => InteractionType.None;

		public NetworkId NetworkId => Object.Id;

		public event Action PickupingObject;

		public virtual bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			//상호작용 주체가 픽업한 객체가 없는 경우 픽업 실행한다.
			if(interactor.HasPickUpObject == false)
			{
				interactor.RPC_OnPickuping(Object);
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

		public void OnPickupingObject()
		{
			PickupingObject?.Invoke();
		}
	}
}
