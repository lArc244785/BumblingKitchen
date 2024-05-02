

using System;

namespace BumblingKitchen.Interaction
{
	public interface IHandEvents
	{
		event Action Pickuping;
		event Action Droped;
	}
}
