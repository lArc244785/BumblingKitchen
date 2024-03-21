using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace BumblingKitchen.Player
{
    public class PlayerControl : NetworkBehaviour
    {
		private NetworkCharacterController _controller;
		[SerializeField] private NetworkMecanimAnimator _animator;

		private void Awake()
		{
			_controller = GetComponent<NetworkCharacterController>();
		}

		public override void Spawned()
		{
			base.Spawned();
		}

		public override void FixedUpdateNetwork()
		{
			Debug.Log(GetInput(out NetworkInputData d));
			if (GetInput(out NetworkInputData data))
			{
				if(data.direction.sqrMagnitude > 0.1f)
				{
					data.direction.Normalize();
					_controller.Move(data.direction * Runner.DeltaTime * 10);
					_animator.Animator.SetFloat("Move", 1.0f);
				}
				else
				{
					_animator.Animator.SetFloat("Move", 0.0f);
				}

				if(data.buttons.IsSet(NetworkInputData.INTERACTION_BUTTON) == true)
				{
					_animator.Animator.SetFloat("Pickup Item", 1.0f);
				}
				else
				{
					_animator.Animator.SetFloat("Pickup Item", 0.0f);
				}
			}
		}


	}
}
