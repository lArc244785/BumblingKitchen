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
		[Networked] private TickTimer _stepSoundTimer { set; get; }
		
		private PlayerSound _sound;
		private Interactor _interactor;

		private bool _isPrevMove = false;

		public event Action OnBegineMove;
		public event Action OnEndMove;

		private void Awake()
		{
			_controller = GetComponent<NetworkCharacterController>();
			_sound = GetComponent<PlayerSound>();
			_interactor = GetComponent<Interactor>();
		}

		private void Start()
		{
			GameManager.Instance.OnEnddingGame += () => SetStepSound(TickTimer.None);
		}

		public override void Spawned()
		{
			base.Spawned();
			if(HasInputAuthority == true)
			{
				GameObject.FindAnyObjectByType<CinemachineVirtualCamera>().Follow = transform;
			}
		}

		public override void FixedUpdateNetwork()
		{
			if ((GameManager.Instance.State == GameState.Play) == false)
				return;

			if (GetInput(out NetworkInputData data))
			{
				if (data.direction.sqrMagnitude > 0.1f)
				{
					data.direction.Normalize();
					_controller.Move(data.direction * Runner.DeltaTime * 10);
					
					if(_isPrevMove == false && HasStateAuthority == true)
					{
						OnBegineMove?.Invoke();
						_isPrevMove = true;
					}

					if (_stepSoundTimer.IsRunning == false)
					{
						SetStepSound(TickTimer.CreateFromSeconds(Runner, 0.5f));
					}
				}
				else
				{
					if(_isPrevMove == true && HasStateAuthority == true)
					{
						OnEndMove?.Invoke();
						_isPrevMove = false;
					}

					SetStepSound(TickTimer.None);
				}

				if (data.buttons.IsSet(NetworkInputData.INTERACTION_BUTTON) == true)
				{
					_interactor.OnInteraction();
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
