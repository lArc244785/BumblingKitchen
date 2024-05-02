using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BumblingKitchen
{
	public interface IObjectPickUpEvent
	{
		event Action PickupingObject;
	}
}
