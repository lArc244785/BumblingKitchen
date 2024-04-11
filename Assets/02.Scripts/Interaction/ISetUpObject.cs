using Fusion;
using System;

namespace BumblingKitchen
{
	internal interface ISetUpObject: IAfterSpawned
	{
		public event Action OnSpawned;
	}
}
