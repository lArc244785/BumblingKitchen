using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

namespace BumblingKitchen
{

	public class TestAA : NetworkBehaviour
	{
		public override void Spawned()
		{
			Debug.Log("InitData!");
		}
	}


}
