using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

namespace BumblingKitchen.SessionLobby
{
	public class PlayerEditor : MonoBehaviour
	{
		[SerializeField] private TMP_InputField _inputName;
		[SerializeField] private Button _nextCharacter;
		[SerializeField] private Image _preview;
		[SerializeField] private TMP_Text _characterName;

		[SerializeField] private List<Sprite> _previewCharacters;
		[SerializeField] private List<String> _characterNames;

		private int _characterID;
		private string _playerName;

		private void Awake()
		{
			//이름 데이터가 없는 경우 초기화 후 다시 생성
			if (!PlayerPrefs.HasKey("Name"))
			{
				PlayerPrefs.DeleteAll();
				PlayerPrefs.SetString("Name", RandomName());
				PlayerPrefs.SetInt("CharacterID", 0);
			}

			_playerName = PlayerPrefs.GetString("Name");
			SetCharacterID(PlayerPrefs.GetInt("CharacterID"));

			_inputName.text = _playerName;

			_inputName.onEndEdit.AddListener(SetCharacterName);
			_nextCharacter.onClick.AddListener(NextCharacter);

			
		}

		private string RandomName()
		{
			int random = Random.Range(0, 999);
			return "User-" + random.ToString("D3");
		}

		/// <summary>
		/// 마지막 캐릭터인 경우 처음 캐릭터로 돌아간다.
		/// </summary>
		private void NextCharacter()
		{
			_characterID++;
			if (_characterID == _previewCharacters.Count)
			{
				_characterID = 0;
			}
			SetCharacterID(_characterID);
		}

		private void SetCharacterID(int id)
		{
			_characterID = id;
			_preview.sprite = _previewCharacters[_characterID];
			_characterName.text = _characterNames[_characterID];
			PlayerPrefs.SetInt("CharacterID", id);
		}

		public void SetCharacterName(string name)
		{
			_playerName = name;
			PlayerPrefs.SetString("Name", _playerName);
		}
	}
}
