using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen
{
	public class GameStartEnd : MonoBehaviour
	{
		[SerializeField] private Canvas _canvas;
		[SerializeField] private GameStartUI _startUI;
		[SerializeField] private GameEndUI _endUI;

		private void Awake()
		{
			_startUI.Init(_canvas);
			_endUI.Init(_canvas);
		}

		private void Start()
		{
			GameManager.Instance.OnReadying += OnGameStartUI;
			GameManager.Instance.OnEnddingGame += OnGameEndUI;
			_canvas.enabled = false;
		}

		private void OnGameStartUI()
		{
			_canvas.enabled = true;
			_startUI.gameObject.SetActive(true);
		}

		private void OnGameEndUI()
		{
			_canvas.enabled = true;
			_endUI.gameObject.SetActive(true);
		}
	}
}
