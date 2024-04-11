using Fusion;
using UnityEngine;

namespace BumblingKitchen
{
	internal class InGameSetUpNetworkObject:NetworkBehaviour
	{
		public override void Spawned()
		{
			Runner.Despawn(Object);
		}
	}
}
