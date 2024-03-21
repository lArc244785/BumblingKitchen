using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
	[SerializeField] private PlayerSound _sound;

	public void OnSetping()
	{
		_sound.PlayStep();
	}

	public void OnChopping()
	{
		_sound.PlayChrop();
	}
}
