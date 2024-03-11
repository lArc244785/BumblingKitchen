using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollManager : MonoBehaviour
{
	[SerializeField] private RectTransform _contentRectTransform;

	[SerializeField] private List<RectTransform> datas;
	[SerializeField] private float _topDeadY;
	[SerializeField] private float _prevY;


	[SerializeField] private int _topIndex = 0;

	[Title("Debug"),SerializeField] private float speed;

	[SerializeField] private bool isAutoMove;
	[SerializeField] private Vector2 pos;


	private void Update()
	{

		if (isAutoMove)
			_contentRectTransform.anchoredPosition += Vector2.up * speed * Time.deltaTime;

		if (_contentRectTransform.anchoredPosition.y >= _topDeadY)
		{
			Debug.Log($"SHIT {_contentRectTransform.anchoredPosition.y}");
			var data = datas[_topIndex];
			data.SetAsLastSibling();
			pos = _contentRectTransform.anchoredPosition;
			pos.y -= _prevY;
			_contentRectTransform.anchoredPosition = pos;

			_topIndex++;
			if (_topIndex == datas.Count)
				_topIndex = 0;
		}

	}
}
