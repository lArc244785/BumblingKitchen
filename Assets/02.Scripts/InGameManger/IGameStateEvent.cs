using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BumblingKitchen
{
	internal interface IGameStateEvent
	{
		public event Action OnFinshedReady;
		public event Action OnStarttingGame;
		public event Action OnEnddingGame;
	}
}
