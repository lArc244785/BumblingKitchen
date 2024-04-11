using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BumblingKitchen
{
	public class JoyStick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
	{
		private Canvas _canvas;
		[SerializeField] private RectTransform _background;
		[SerializeField] private RectTransform _handle;

		private Camera _cam;

		private Vector2 _input = Vector2.zero;

		public Vector2 Diraction { get; private set; }

		private void Start()
		{
			_canvas = GetComponentInParent<Canvas>();
			if (_canvas.renderMode == RenderMode.ScreenSpaceCamera)
				_cam = _canvas.worldCamera;

			Vector2 _center = new Vector2(0.5f, 0.5f);
			_background.pivot = _center;
			_handle.anchorMin = _center;
			_handle.anchorMax = _center;
			_handle.pivot = _center;
			_handle.anchoredPosition = Vector2.zero;
		}

		public void OnDrag(PointerEventData eventData)
		{
			Vector2 position = RectTransformUtility.WorldToScreenPoint(_cam, _background.position);
			Vector2 radius = _background.sizeDelta / 2;
			Vector2 movePos = eventData.position - position;

			_input = movePos / (radius * _canvas.scaleFactor);

			if (_input.magnitude > 1)
				_input = _input.normalized;

			_handle.anchoredPosition = _input * radius;
			UpdateDiraction();
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			_input = Vector2.zero;
			_handle.anchoredPosition = Vector2.zero;
			UpdateDiraction();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			OnDrag(eventData);
		}


		private void UpdateDiraction()
		{
			Diraction = _input.normalized;
		}
	}
}
