using TMPro;
using UnityEngine;

namespace BumblingKitchen
{
	public class GoldUI : MonoBehaviour
	{
		[SerializeField] private TMP_Text _gold;

		public void Init(InGameData data)
		{
			data.OnUpdateGold += DrawGold;
		}

		private void DrawGold(int gold)
		{
			_gold.text = gold.ToString("D4");
		}
	}
}
