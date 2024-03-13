using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Collections.Generic;

namespace BumblingKitchen.SessionRoom
{
    public class PlayerCard : MonoBehaviour
    {
        [SerializeField] private TMP_Text _name;
        [SerializeField] private Image _characterPreview;
        [SerializeField] private Image _ready;
        [SerializeField] private GameObject _textFream;

        [SerializeField] private List<Sprite> _previewCharacters;


		public void Draw(string player, int id, bool isReady)
		{
            _textFream.SetActive(true);
            _name.text = player;
            _characterPreview.sprite = _previewCharacters[id];
            _characterPreview.color = Color.white;

            Color readyColor = isReady ? Color.white : Color.clear;
            _ready.color = readyColor;
        }

        public void Blank()
		{
            _textFream.SetActive(false);
            _characterPreview.color = Color.clear;
            _ready.color = Color.clear;
        }



    }
}
