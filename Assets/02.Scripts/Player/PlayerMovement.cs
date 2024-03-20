using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace BumblingKitchen.Player
{
    public class PlayerMovement : NetworkBehaviour
    {
		private NetworkCharacterController _controller;

		private void Awake()
		{
			_controller = GetComponent<NetworkCharacterController>();
		}

		public override void Spawned()
		{
			base.Spawned();
		}

		public override void FixedUpdateNetwork()
		{
			Debug.Log(GetInput(out NetworkInputData d));
			if(GetInput(out NetworkInputData data))
			{
				Debug.Log(data.direction);
				_controller.Move(data.direction);
			}
		}
	}
}
