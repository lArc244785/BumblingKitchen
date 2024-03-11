using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecycleScrollRect : ScrollRect
{
	protected override void Start()
	{
		base.Start();
		onValueChanged.AddListener(OnValueChanged);
	}

	private void OnValueChanged(Vector2 normalizedPos)
	{
		if (content.anchoredPosition.y >= 800.0f)
		{
			Debug.Log("Call");
			content.anchoredPosition += Vector2.down * 400;

			m_ContentStartPosition = content.anchoredPosition;
		}
	}

	private void Update()
	{
		//Debug.Log($"{content.anchoredPosition.y} {m_ContentStartPosition}");
		
		//if (content.anchoredPosition.y >= 400.0f)
		//{
		//	content.anchoredPosition = Vector2.zero;
		//}
	}
}
