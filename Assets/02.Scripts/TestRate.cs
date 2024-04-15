using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace BumblingKitchen
{
	public class TestRate : NetworkBehaviour
	{
		[Networked] public float CallTime { get; set; }

		private float spawnRate;
		Rect rect;

		private void Start()
		{
			rect = new Rect(Screen.width * 0.5f, Screen.height * 0.8f, 100, 100);
		}

		public override void Spawned()
		{
			if (GameManager.Instance.State != GameState.Play)
				return;

			float time = GameManager.Instance.GetDetalPlayTime();
			spawnRate = time - CallTime;
		}


		private void OnGUI()
		{
			string text = $"Spawn Rate: {spawnRate}";

			GUIStyle style = new GUIStyle();
			style.fontSize = 50;
			style.normal.textColor = Color.cyan;

			GUI.Label(rect, text, style);
		}
	}
}