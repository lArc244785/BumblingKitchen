
using System;

namespace BumblingKitchen.Interaction
{
	internal interface ICutEvent
	{
		public event Action OnCutEvent;
	}
}
