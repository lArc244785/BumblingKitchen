using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [SerializeField] private Button _button;

    // Start is called before the first frame update
    void Start()
    {
        _button.onClick.AddListener(()=>FusionConnection.Instance.ExitSessionToTitle());
    }

}
