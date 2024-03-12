using UnityEngine.UI;
using UnityEngine;
using Fusion;
using Sirenix.OdinInspector;
using BumblingKitchen.PopUp;

namespace BumblingKitchen.SessionLobby {
	public class SessionManager : MonoBehaviour
	{
		[SerializeField] private Button createSession;
		[SerializeField] private CreateSessionPopUp _createSessionPopUp;
		[SerializeField] private MessagePopUp _errorMessage;

		private void Awake()
		{
			createSession.onClick.AddListener(PopUpCreateSession);
		}

		public void ConnectToSession(string sessionName)
		{
			string playerName = PlayerPrefs.GetString("Name");
			FusionConnection.Instance.ConnectToSession(playerName, sessionName);
		}


		public void CreateSession(string sessionName)
		{
			string playerName = PlayerPrefs.GetString("Name");

			if (!FusionConnection.Instance.CreateSession(playerName, sessionName))
			{
				PopUpCreateMessage("Fail", "A session with the same name exists.");
			}
		}

		private void PopUpCreateSession()
		{
			var createSesssionPopUp = Instantiate(_createSessionPopUp);
			createSesssionPopUp.OnCreate += CreateSession;

			PopupManger.Instance.RegistrationPopUp(createSesssionPopUp);
		}

		private void PopUpCreateMessage(string title, string message)
		{
			var messagePopUp = Instantiate(_errorMessage);
			messagePopUp.Init(title, message);

			PopupManger.Instance.RegistrationPopUp(messagePopUp);
		}

	}
}