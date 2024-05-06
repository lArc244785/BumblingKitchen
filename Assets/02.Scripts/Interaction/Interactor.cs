using Fusion;
using System;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public class Interactor : NetworkBehaviour, IHandEvents, ICutEvent, ICleanEvent
	{
		#region Property
		/// <summary>
		/// �Ⱦ� ������Ʈ�� �θ� Transfrom
		/// </summary>
		[field: SerializeField] public Transform PickUpPoint { get; private set; }

		/// <summary>
		/// �Ⱦ� ������Ʈ
		/// </summary>
		public IInteractable PickupObject { get; private set; }
		
		/// <summary>
		/// ���� �Ⱦ� ������Ʈ�� �ִ���
		/// </summary>
		public bool HasPickUpObject => PickupObject != null;
		#endregion


		#region Field
		/// <summary>
		/// ������Ʈ Ž�� �����ϴ� ��ġ
		/// </summary>
		[SerializeField] private Vector3 _detectedStartLocalPoint;
		
		/// <summary>
		/// Ž�� Ray�� ����
		/// </summary>
		[SerializeField] private float _detectDistance;
		
		/// <summary>
		/// Ž�� Ray�� �ɸ��� ������Ʈ���� ����ũ
		/// </summary>
		[SerializeField] private LayerMask _detectLayerMask;
		#endregion


		#region Event
		/// <summary>
		/// �Ⱦ� �� ȣ��Ǵ� �̺�Ʈ �̺�Ʈ�Դϴ�.
		/// </summary>
		public event Action Pickuping;
		
		/// <summary>
		/// �Ⱦ��� ������Ʈ�� ������ �� ȣ��Ǵ� �̺�Ʈ �Դϴ�.
		/// </summary>
		public event Action DropedPickupobject;

		/// <summary>
		/// ��Ḧ �ڸ��� ��ȣ�ۿ��� ����� ȣ��Ǵ� �̺�Ʈ�Դϴ�.
		/// </summary>
		public event Action Cutting;

		/// <summary>
		/// ���� �۱� ��ȣ�ۿ��� ����� ȣ��Ǵ� �̺�Ʈ�Դϴ�.
		/// </summary>
		public event Action Cleaning;
		#endregion


		#region Method
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

		/// <summary>
		/// �Ⱦ��� ������Ʈ�� �ִ� ��� �Ķ���ͷ� ���� ������Ʈ�� �켱 ������ ���ؼ� ��ȣ�ۿ� ��ü�� �����Ͽ� ��ȣ�ۿ��� �õ��غ��ϴ�.
		/// </summary>
		public bool TryInteraction(IInteractable interactionObject)
		{
			if (PickupObject?.Type > interactionObject.Type)
			{
				Debug.Log("��ȣ �ۿ� ����! �Ⱦ� ������Ʈ�� ��ü");
				return PickupObject.TryInteraction(this, interactionObject);
			}
			else
			{
				Debug.Log("��ȣ �ۿ� ����! ������Ʈ�� ��ü");
				return interactionObject.TryInteraction(this, PickupObject);
			}
		}


		/// <summary>
		/// �ش� ������Ʈ�� ���� �Ⱦ��� ������Ʈ���� Ȯ���մϴ�.
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
		/// [RPC] ��Ʈ��ũ ������Ʈ�� �Ⱦ� ������Ʈ�� �����մϴ�.
		/// </summary>
		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_Pickup(NetworkId id)
		{
			var obj = Runner.FindObject(id);

			if (obj.TryGetComponent<IInteractable>(out var pickUpObject) == true)
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
		/// �Ⱦ��� ������Ʈ�� ����ϰ� �ش� ������Ʈ�� ��ȯ�մϴ�.
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
		/// ���� �켱 ������ ���� IInteractable�� ��ȯ�մϴ�. �� ���� ���� �ִ� ������Ʈ�� �����Դϴ�.
		/// </summary>
		private IInteractable DetectedInteractable()
		{
			IInteractable detedObject = null;

			Vector3 detectPoint = GetDetectPoint();

			Ray ray = new(detectPoint, Vector3.down);
			var hits = Physics.RaycastAll(ray, _detectDistance, _detectLayerMask);
			//RayCast�� ������Ʈ�� �߿��� ���� �켱 ������ ���� ��ü�� ã���ϴ�.
			foreach (var hit in hits)
			{
				IInteractable interactableObject = hit.collider.GetComponent<IInteractable>();
				if (interactableObject == null)
				{
					throw new MissingComponentException($"{hit.collider.gameObject}");
				}

				if (IsPickUpInteractor(interactableObject) == true)
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
		/// ���� ��ġ������ Ž�� ���� ��ġ�� �����ɴϴ�.
		/// </summary>
		private Vector3 GetDetectPoint()
		{
			return transform.position + (transform.forward + _detectedStartLocalPoint);
		}

		/// <summary>
		/// [RPC] �Ⱦ� ������Ʈ�� ����ϴ� ��� ȣ�� �˴ϴ�.
		/// </summary>
		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_OnDroped()
		{
			DropedPickupobject?.Invoke();
			PickupObject = null;
		}

		/// <summary>
		/// ������ ���õ� ���� ȣ��� ȣ��Ǿ�ߵ˴ϴ�.
		/// </summary>

		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_OnCuttingEvent()
		{
			Cutting?.Invoke();
		}


		/// <summary>
		/// û�� ���� ���� ȣ��� ȣ��Ǿ�ߵ˴ϴ�.
		/// </summary>

		[Rpc(RpcSources.All, RpcTargets.All)]
		public void RPC_OnCleaningEvent()
		{
			Cleaning?.Invoke();
		}
		#endregion
	}
}
