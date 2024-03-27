using UnityEngine;
using TMPro;
using BumblingKitchen.Interaction;
using System;

namespace BumblingKitchen
{
	public class DryingReckUI : InGameUIBase
	{
		[SerializeField] private DryingRack _dryingReck;
		[SerializeField] private TMP_Text _text;

		protected override void Awake()
		{
			base.Awake();
			_dryingReck.OnUpdateCleanPlates += DrawCleanPlateInfo;
		}

		private void DrawCleanPlateInfo(int count, int capacity)
		{
			_text.text = $"CLEAN\n{count}/{capacity}";
		}
	}
}
