using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BumblingKitchen
{
	public class TimeUI : MonoBehaviour
	{
		[SerializeField] private TMP_Text _text;
		private bool isRun = false;

		private void Start()
		{
			GameManager.Instance.OnReadying += () => isRun = true;
		}

		private void Update()
		{
			if (GameManager.Instance == null)
				return;
			int time = GameManager.Instance.GetEndTime();
			if (time == -1)
			{
				_text.text = $"--:--";
			}
			else
			{
				int min = time / 60;
				int sec = time % 60;
				_text.text = $"{min.ToString("D2")}:{sec.ToString("D2")}";
			}
		}
	}
}
