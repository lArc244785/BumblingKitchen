using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BumblingKitchen
{
    public class PlayerDataUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _userName;
        [SerializeField] private TMP_Text _gold;
        [SerializeField] private TMP_Text _spawn;
        [SerializeField] private TMP_Text _succesCook;
        [SerializeField] private TMP_Text _failCook;
        [SerializeField] private TMP_Text _sendOrder;
        [SerializeField] private TMP_Text _cleanPlate;

        public void InitDataSetting(string userName, string gold, string spawn, string succesCook, string failCook, string sendOrder, string cleanPlate)
		{
            _userName.text = userName;
            _gold.text = gold;
            _spawn.text = spawn;
            _failCook.text = failCook;
            _sendOrder.text = sendOrder;
            _cleanPlate.text = cleanPlate;
        }
    }
}
