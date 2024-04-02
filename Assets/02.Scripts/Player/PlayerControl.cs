using Fusion;
using UnityEngine;
using Cinemachine;
using BumblingKitchen.Interaction;
using System;

namespace BumblingKitchen.Player
{
	public class PlayerControl : NetworkBehaviour, IMoveEvents
	{
		private NetworkCharacterController _controller;
		private LocalInput _localInput;

		private Interactor _interactor;

		private bool _isPrevMove = false;

		public event Action OnBegineMove;
		public event Action OnEndMove;

		private void Awake()
		{
			_controller = GetComponent<NetworkCharacterController>();
			_interactor = GetComponent<Interactor>();
		}

		public override void Spawned()
		{
			base.Spawned();
			if(HasInputAuthority == true)
			{
				GameObject.FindAnyObjectByType<CinemachineVirtualCamera>().Follow = transform;
				_localInput = FindAnyObjectByType<LocalInput>();
			}
		}

		public override void FixedUpdateNetwork()
		{
			if ((GameManager.Instance.IsMove) == false)
				return;

			if (GetInput(out NetworkInputData data))
			{
				if (data.direction.sqrMagnitude > 0.1f)
				{
					data.direction.Normalize();
					_controller.Move(data.direction * Runner.DeltaTime * 10);
					
					if(_isPrevMove == false && HasStateAuthority == true)
					{
						if(HasStateAuthority == true)
						{
							RPC_OnBegineMove();
						}
						_isPrevMove = true;
					}
				}
				else
				{
					if(_isPrevMove == true && HasStateAuthority == true)
					{
						if(HasStateAuthority == true)
						{
							RPC_OnEndMove();
						}
						_isPrevMove = false;
					}
				}

				if (data.buttons.IsSet(NetworkInputData.INTERACTION_BUTTON) == true)
				{
					_interactor.OnInteraction();
				}
			}
		}

		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		private void RPC_OnBegineMove()
		{
			OnBegineMove?.Invoke();
		}

		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		private void RPC_OnEndMove()
		{
			OnEndMove?.Invoke();
		}

	}
}
