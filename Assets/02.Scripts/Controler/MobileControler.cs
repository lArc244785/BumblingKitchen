using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BumblingKitchen
{
    public class MobileControler : MonoBehaviour
    {
		[SerializeField] private Canvas _canvas;
		private LocalInput _localInput;
		[SerializeField] JoyStick _joystick;
		[SerializeField] Button _interaction;

		private void Awake()
		{
			if (SystemInfo.deviceType != DeviceType.Handheld)
			{
				_canvas.gameObject.SetActive(false);
				return;
			}

			_localInput = FindAnyObjectByType<LocalInput>();
			_interaction.onClick.AddListener(() => _localInput.Interaction = true);
		}

		private void Update()
		{
			Vector3 diraction3D = new Vector3(_joystick.Diraction.x, 0.0f, _joystick.Diraction.y);
			_localInput.Dircation = diraction3D;
		}
	}
}
