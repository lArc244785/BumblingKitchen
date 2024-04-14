using Fusion;
using System;
using UnityEngine;

namespace BumblingKitchen.Interaction
{
	public class Sink : NetworkBehaviour, IInteractable
	{
		public InteractionType Type => InteractionType.Sink;
		public NetworkId NetworkId => Object.Id;

		[Networked, Capacity(5), OnChangedRender(nameof(UpdateDirtyPlate))]
		private NetworkLinkedList<NetworkId> DirtyPlates { get; }
		= MakeInitializer(new NetworkId[] { });

		[SerializeField] private float _addCleaning;
		[field: SerializeField] public float CleanPlateProgress { get; private set; }

		[SerializeField] private DryingRack _dryingRack;

		[Networked, OnChangedRender(nameof(UpdateProgress))] private float CurrentCleaningProgress { get; set; }

		public event Action<int, int> OnUpdateDirtyPlates;
		public event Action<float> OnUpdateProgress;

		[SerializeField] private Transform _effectPoint;

		public bool TryInteraction(Interactor interactor, IInteractable interactable)
		{
			if (interactor.HasPickUpObject == false)
			{
				if (DirtyPlates.Count > 0)
				{
					interactor.RPC_OnCleanEvent();
					RPC_Cleaning(Runner.LocalPlayer);
					RPC_EffectPlay();
					return true;
				}
				return false;
			}
			else
			{
				switch (interactable.Type)
				{
					case InteractionType.Plate:
						if (DirtyPlates.Count == DirtyPlates.Capacity)
							return false;

						Plate plate = interactor.Drop() as Plate;
						RPC_AddDirtyPlates(plate.Object);
						plate.RPC_SetActive(false);
						return true;
						break;
				}
			}

			return false;
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_EffectPlay()
		{
			var effect = PoolManager.Instance.GetPooledObject(PoolObjectType.Effect_WashDish);
			effect.transform.position = _effectPoint.position;
			effect.transform.rotation = _effectPoint.rotation;
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_AddDirtyPlates(NetworkId plateID)
		{
			DirtyPlates.Add(plateID);
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_Cleaning(PlayerRef player)
		{
			if (_dryingRack.CanSendCleanPlate() == false)
				return;

			CurrentCleaningProgress += _addCleaning;

			if (CurrentCleaningProgress >= CleanPlateProgress)
			{
				var cleaningPlateID = DirtyPlates[0];
				Runner.FindObject(cleaningPlateID).GetComponent<Plate>().RPC_Clear();
				DirtyPlates.Remove(cleaningPlateID);
				_dryingRack.RPC_SendCleanPlate(cleaningPlateID);
				CurrentCleaningProgress = 0.0f;
				Debug.Log($"Dirty Plate {DirtyPlates.Count}");
				InGameData.Instance.RPC_AddCleanPlate(player);
			}
		}

		private void UpdateDirtyPlate()
		{
			OnUpdateDirtyPlates?.Invoke(DirtyPlates.Count, DirtyPlates.Capacity);
		}

		private void UpdateProgress()
		{
			OnUpdateProgress?.Invoke(CurrentCleaningProgress);
		}
	}
}
