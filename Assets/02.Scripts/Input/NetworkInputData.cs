using Fusion;
using UnityEngine;

namespace BumblingKitchen
{
	public struct NetworkInputData : INetworkInput
	{
		public const byte INTERACTION_BUTTON = 1;
		
		public Vector3 direction;
		public NetworkButtons buttons;
	}
}
