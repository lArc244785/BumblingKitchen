using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

namespace BumblingKitchen.SessionLobby
{
	public class SessionPage : MonoBehaviour
	{
		private int _topIndex = 0;
		[SerializeField] private Transform cellsParent;
		private SessionCell[] cells;

		[SerializeField] private Button _next;
		[SerializeField] private Button _prev;
		[SerializeField] private Button _refresh;

		private SessionManager _sessionManager;

		void Start()
		{
			_sessionManager = GetComponent<SessionManager>();
			_next.onClick.AddListener(Next);
			_prev.onClick.AddListener(Prev);
			_refresh.onClick.AddListener(Refresh);

			cells = cellsParent.GetComponentsInChildren<SessionCell>();

			UpdatePage();
		}

		private void SetTopIndex(int index)
		{
			_topIndex = Mathf.Clamp(index, 0, FusionConnection.Instance.sessionList.Count - 1);
			UpdatePage();
		}

		public void UpdatePage()
		{
			List<SessionInfo> sessionList = FusionConnection.Instance.sessionList;
			
			if(sessionList == null)
			{
				return;
			}

			for (int i = 0; i < cells.Length; i++)
			{
				int sessionIndex = _topIndex + i;
				var session = sessionIndex < sessionList.Count ? sessionList[sessionIndex] : null;

				cells[i].SetUp(session, _sessionManager.ConnectToSession);
			}
		}

		private void Next()
		{
			int nextTopIndex = _topIndex + cells.Length;
			if(nextTopIndex >= FusionConnection.Instance.sessionList.Count)
			{
				return;
			}


			SetTopIndex(nextTopIndex);
		}
		private void Prev()
		{
			int prevTopIndex = _topIndex - cells.Length;
			if (prevTopIndex < 0)
			{
				return;
			}

			SetTopIndex(prevTopIndex);
		}

		private void Refresh()
		{
			SetTopIndex(0);
		}

	}
}
