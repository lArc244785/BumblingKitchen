using Fusion;
using UnityEngine;

namespace BumblingKitchen.Player
{
	public class PlayerControl : NetworkBehaviour
	{
		private NetworkCharacterController _controller;
		[Networked] private TickTimer _stepSoundTimer { set; get; }
		[SerializeField] private NetworkMecanimAnimator _animator;
		[SerializeField] private PlayerSound _sound;

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
			if (GetInput(out NetworkInputData data))
			{
				if (data.direction.sqrMagnitude > 0.1f)
				{
					data.direction.Normalize();
					_controller.Move(data.direction * Runner.DeltaTime * 10);
					_animator.Animator.SetFloat("Move", 1.0f);

					if (_stepSoundTimer.IsRunning == false)
					{
						SetStepSound(TickTimer.CreateFromSeconds(Runner, 0.5f));
					}
				}
				else
				{
					_animator.Animator.SetFloat("Move", 0.0f);
					SetStepSound(TickTimer.None);
				}

				if (data.buttons.IsSet(NetworkInputData.INTERACTION_BUTTON) == true)
				{
					_animator.Animator.SetFloat("Pickup Item", 1.0f);
				}
				else
				{
					_animator.Animator.SetFloat("Pickup Item", 0.0f);
				}
			}
		}

		public override void Render()
		{
			base.Render();
			if (_stepSoundTimer.Expired(Runner) == true)
			{
				_sound.PlayStep();
				SetStepSound(TickTimer.None);
			}
		}

		private void SetStepSound(TickTimer timer)
		{
			if (HasStateAuthority == true)
			{
				_stepSoundTimer = timer;
			}
		}

	}
}
