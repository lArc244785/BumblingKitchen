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
		[SerializeField] Joystick _joystick;
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
			_localInput.Dircation = _joystick.Direction3D.normalized;
		}
	}
}
