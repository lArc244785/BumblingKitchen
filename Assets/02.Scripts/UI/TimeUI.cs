using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BumblingKitchen
{
	public class TimeUI : MonoBehaviour
	{
		[SerializeField] private TMP_Text _text;

		private bool _isTimerRuner = false;
		
		private void Start()
		{
			GameManager.Instance.OnStarttingGame += () => _isTimerRuner = true;
		}

		private void Update()
		{
			if (_isTimerRuner == false)
				return;

			int time = GameManager.Instance.GetEndTime();
			int min = time / 60;
			int sec = time % 60;
			_text.text = $"{min.ToString("D2")}:{sec.ToString("D2")}";
		}
	}
}
