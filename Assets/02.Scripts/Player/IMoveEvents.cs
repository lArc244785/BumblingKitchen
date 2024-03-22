
using System;

namespace BumblingKitchen.Player
{
	public interface IMoveEvents
	{
		event Action OnBegineMove;
		event Action OnEndMove;
	}
}
