

using System;

namespace BumblingKitchen.Interaction
{
	public interface IHandEvents
	{
		event Action OnPickUp;
		event Action OnDrop;
	}
}
