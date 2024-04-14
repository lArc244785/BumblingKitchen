using Fusion;
using BumblingKitchen.Interaction;
using System;
using UnityEngine;

namespace BumblingKitchen
{

	internal class Outlet : NetworkBehaviour, IInteractable
	{
		public InteractionType Type => InteractionType.Outlet;
		public NetworkId NetworkId => Object.Id;

		private TickTimer _waitPlatTimer;

		[Networked, Capacity(20)]
		private NetworkLinkedList<NetworkId> WaitPlates { get;}
		= MakeInitializer(new NetworkId[] { });

		[Networked, Capacity(20), OnChangedRender(nameof(UpdateOutLetPlates))]
		private NetworkLinkedList<NetworkId> OutLetPlates { get;}
		= MakeInitializer(new NetworkId[] { });


		public event Action<int, int> OnUpdattingOutletPlates;

		public bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if (interactor.HasPickUpObject == true)
				return false;

			if (OutLetPlates.Count == 0)
				return false;

			var plate = Runner.FindObject(OutLetPlates[0]).GetComponent<Plate>();
			plate.RPC_SetActive(true);
			interactor.RPC_PickUp(plate.Object);
			RPC_RemoveFirstOutLetPlate(plate.Object);

			return true;
		}

		public void SendPlate(Plate plate)
		{
			plate.RPC_SetActive(false);
			RPC_AddWaitPlates(plate.Object);
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_RemoveFirstOutLetPlate(NetworkId removeId)
		{
			OutLetPlates.Remove(removeId);
		}


		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_AddWaitPlates(NetworkId plateID)
		{
			if (WaitPlates.Count == 0)
			{
				_waitPlatTimer = TickTimer.CreateFromSeconds(Runner, 0.5f);
			}

			WaitPlates.Add(plateID);
		}

		public override void FixedUpdateNetwork()
		{
			if (HasStateAuthority == false)
				return;

			if(_waitPlatTimer.Expired(Runner) == true)
			{
				Debug.Log("접시가 대기열로 넘어 갑니다.");
				NetworkId waitPlate = WaitPlates[0];
				WaitPlates.Remove(waitPlate);
				OutLetPlates.Add(waitPlate);
				if (WaitPlates.Count > 0)
				{
					_waitPlatTimer = TickTimer.CreateFromSeconds(Runner, 0.5f);
				}
				else
				{
					_waitPlatTimer = TickTimer.None;
				}
			}
		}

		private void UpdateOutLetPlates()
		{
			OnUpdattingOutletPlates?.Invoke(OutLetPlates.Count, OutLetPlates.Capacity);
		}
	}
}
