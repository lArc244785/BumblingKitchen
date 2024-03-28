using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BumblingKitchen
{
    public class GoldUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _gold;

		public void Start()
		{
			//InGameData.Instance.OnUpdateGold += DrawGold;
		}

		private void DrawGold(int gold)
		{
			_gold.text = gold.ToString("D4");
		}

		private void OnDestroy()
		{
			//InGameData.Instance.OnUpdateGold -= DrawGold;
		}
	}
}
