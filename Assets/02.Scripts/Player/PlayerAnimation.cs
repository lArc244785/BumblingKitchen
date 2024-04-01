using UnityEngine;
using Fusion;
using BumblingKitchen.Interaction;
using System;

namespace BumblingKitchen.Player
{
	public class PlayerAnimation : NetworkBehaviour
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

			GameManager.Instance.OnEnddingGame += Stop;
		}

		private void Clean()
		{
			RPC_Clean();
		}

		private void Cut()
		{
			RPC_Cut();
		}

		private void Move()
		{
			RPC_SetMove(1.0f);
		}

		private void Stop()
		{
			RPC_SetMove(0.0f);
		}
		
		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_Cut()
		{
			_animator.SetTrigger("Cut");
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_Clean()
		{
			_animator.SetTrigger("Clean");
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_SetMove(float value)
		{
			_animator.SetFloat("Move", value);
		}

		private void PickUp()
		{
			RPC_SetPickUpItem(1.0f);
		}

		private void Drop()
		{
			RPC_SetPickUpItem(0.0f);
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_SetPickUpItem(float value)
		{
			_animator.SetFloat("PickUp Item", value);
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
