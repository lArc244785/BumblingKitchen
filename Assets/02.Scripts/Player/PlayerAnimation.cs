using UnityEngine;
using Fusion;
using BumblingKitchen.Interaction;
using System;

namespace BumblingKitchen.Player
{
	public class PlayerAnimation : MonoBehaviour
	{
		[SerializeField] private Animator _animator;
		private IMoveEvents _move;
		private IHandEvents _hand;
		private ICutEvent _cut;
		private ICleanEvent _clean;

		private void Awake()
		{
			_move = GetComponent<IMoveEvents>();
			_hand = GetComponent<IHandEvents>();
			_cut = GetComponent<ICutEvent>();
			_clean = GetComponent<ICleanEvent>();

			_move.OnBegineMove += Move;
			_move.OnEndMove += Stop;
			_hand.OnPickUp += PickUp;
			_hand.OnDrop += Drop;
			_cut.OnCutEvent += Cut;
			_clean.OnCleanEvent += Clean;
		}

		private void Clean()
		{
			_animator.SetTrigger("Clean");
		}

		private void Cut()
		{
			_animator.SetTrigger("Cut");
		}

		private void Move()
		{
			_animator.SetFloat("Move", 1.0f);
		}

		private void Stop()
		{
			_animator.SetFloat("Move", 0.0f);
		}

		private void PickUp()
		{
			_animator.SetFloat("PickUp Item", 1.0f);
		}

		private void Drop()
		{
			_animator.SetFloat("PickUp Item", 0.0f);
		}



		private void OnDestroy()
		{
			_move.OnBegineMove -= Move;
			_move.OnEndMove -= Stop;

			_hand.OnPickUp -= PickUp;
			_hand.OnDrop -= Drop;
		}
	}
}
