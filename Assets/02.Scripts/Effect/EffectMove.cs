using BumblingKitchen.Player;
using System;
using System.Collections;
using UnityEngine;

namespace BumblingKitchen
{
	internal class EffectMove : MonoBehaviour
	{
		private PlayerControl _playerControl;

		private IEnumerator _dustCoroutine;

		private void Awake()
		{
			_playerControl = transform.GetComponentInParent<PlayerControl>();
			_playerControl.OnBegineMove += StartMove;
			_playerControl.OnEndMove += EndMove;
		}

		private void StartMove()
		{
			if(_dustCoroutine != null)
			{
				StopCoroutine(_dustCoroutine);	
			}
			_dustCoroutine = DustCoroutine();
			StartCoroutine(_dustCoroutine);
		}

		private void EndMove()
		{
			if (_dustCoroutine != null)
			{
				StopCoroutine(_dustCoroutine);
			}
			_dustCoroutine = null;
		}

		private IEnumerator DustCoroutine()
		{
			while (GameManager.Instance.State == GameState.Play)
			{
				SpawnDust();
				yield return new WaitForSeconds(0.25f);
			}
			_dustCoroutine = null;
		}

		private void SpawnDust()
		{
			var dust = PoolManager.Instance.GetPooledObject(PoolObjectType.Effect_Dust);
			dust.transform.position = transform.position;
			dust.transform.rotation = transform.rotation;
		}
	}
}
