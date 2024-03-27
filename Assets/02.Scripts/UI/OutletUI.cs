using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace BumblingKitchen
{
    public class OutletUI : InGameUIBase
    {
		[SerializeField] private Outlet _outLet;
		[SerializeField] private TMP_Text _text;

		protected override void Awake()
		{
			base.Awake();
			_outLet.OnUpdattingOutletPlates += DrawOutletPlatCount;
		}

		private void DrawOutletPlatCount(int count, int capacity)
		{
			_text.text = $"PLATE {count.ToString("D2")}/{capacity.ToString("D2")}";
		}
	}
}
