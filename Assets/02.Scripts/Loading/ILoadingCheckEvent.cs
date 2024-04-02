using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BumblingKitchen
{
	internal interface ILoadingCheckEvent
	{
		event Action OnLoadSceneStabilization;
		event Action OnInGameLoaded;
		event Action OnSpawnedPlayer;
		event Action OnSpawnedAllPlayer;
	}
}
