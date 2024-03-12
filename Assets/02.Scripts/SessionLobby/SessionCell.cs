using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using System;

#if UNITY_EDITOR // Editor namespaces can only be used in the editor.
using Sirenix.OdinInspector.Editor.Examples;
#endif

namespace BumblingKitchen.SessionLobby
{
    public enum SessionState
    {
        None,
        Open,
        Close,
        Full
    }

    public class SessionCell : SerializedMonoBehaviour
    {
        [SerializeField] private Dictionary<SessionState, Sprite> _sessionStateBackgroundSprites = new();
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _user;
        [SerializeField] private Button _connectSession;

        private SessionState _state;

        public void SetUp(SessionInfo info, Action<string> onConnect)
        {
            _connectSession.interactable = false;
            _connectSession.onClick.RemoveAllListeners();

            if (info == null)
            {
                SetState(SessionState.None);
                _name.text = null;
                _user.text = null;
            }
            else
            {
                _name.text = info.Name;
                _user.text = $"{info.PlayerCount} / {info.MaxPlayers}";

                if (info.IsOpen && info.IsValid)
                {
                    if (info.PlayerCount == info.MaxPlayers)
                    {
                        SetState(SessionState.Full);
                    }
                    else
                    {
                        SetState(SessionState.Open);
                        _connectSession.interactable = true;
                        _connectSession.onClick.AddListener(() => onConnect(info.Name));
                    }
                }
                else
                {
                    SetState(SessionState.Close);
                }
            }


        }

        private void SetState(SessionState state)
        {
            _state = state;
            _connectSession.image.sprite = _sessionStateBackgroundSprites[_state];

        }
    }
}