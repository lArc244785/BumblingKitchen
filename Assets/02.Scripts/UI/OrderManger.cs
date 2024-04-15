using BumblingKitchen.Interaction;
using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace BumblingKitchen
{
	public struct OrderData : INetworkStruct
	{
		public int orderInex;
		public float startTime;
		public float endTime;

		public OrderData(int orderInex, float startTime, float endTime)
		{
			this.orderInex = orderInex;
			this.startTime = startTime;
			this.endTime = endTime;
		}
	}


	public class OrderManger : NetworkBehaviour
	{
		private const int MAX_ORDER = 7;

		[SerializeField] private Order _orderPrefab;

		[SerializeField] private List<Recipe> _orderableList;

		[SerializeField] private Transform _orderPerent;
		
		private List<Order> _orderList = new List<Order>();

		private TickTimer _singUpOrderTimer;

		private bool _isOrderRun = false;

		private AudioSource _audioSource;
		[SerializeField] private AudioClip _succeseSound;
		[SerializeField] private AudioClip _failSound;

		private float _minOrderTime = 10.0f;
		private float _maxOrderTime = 20.0f;

		private float _orderEndTime = 30.0f;

		private void Awake()
		{
			_audioSource = GetComponent<AudioSource>();
		}

		private void Start()
		{
			GameManager.Instance.OnPlaying += StartOrder;
			GameManager.Instance.OnEnddingGame += StopOrder;
		}

		private void StartOrder()
		{
			if (HasStateAuthority == false)
				return;
			_isOrderRun = true;
			RandomOrder();

		}

		private void SetRandomSingUpOrderTimer()
		{
			float random = Random.Range(_minOrderTime, _maxOrderTime);
			_singUpOrderTimer = TickTimer.CreateFromSeconds(Runner, random);
		}

		private void StopOrder()
		{
			if (HasStateAuthority == false)
				return;
			_isOrderRun = false;
		}

		public void RandomOrder()
		{
			int randomIndex = Random.Range(0, _orderableList.Count);
			float startTime = GameManager.Instance.GetPlayTime();
			float endTime = startTime + _orderEndTime;

			OrderData newOrder = new OrderData(
				randomIndex,
				startTime,
				endTime);
			RPC_SingUpOrder(newOrder);
			SetRandomSingUpOrderTimer();
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_SingUpOrder(OrderData data)
		{
			var newOrderGameObject = PoolManager.Instance.GetPooledObject(PoolObjectType.UI_Order);
			RectTransform newOrderRectTransfrom = newOrderGameObject.GetComponent<RectTransform>();
			newOrderRectTransfrom.SetParent(_orderPerent);
			newOrderRectTransfrom.localPosition = Vector3.one;
			newOrderRectTransfrom.localScale = Vector3.one;
			newOrderRectTransfrom.localRotation = Quaternion.identity;

			var newOrder = newOrderGameObject.GetComponent<Order>();

			Recipe reciep = _orderableList[data.orderInex];
			newOrder.InitSetting(reciep, data.startTime, data.endTime);
			newOrder.name = reciep.Name;
			_orderList.Add(newOrder);
		}

		public override void FixedUpdateNetwork()
		{
			if (HasStateAuthority == false)
				return;
			if (_isOrderRun == false)
				return;

			if (_orderList.Count < MAX_ORDER)
			{
				if (_singUpOrderTimer.Expired(Runner) == true)
				{
					RandomOrder();
				}
			}

			if (_orderList.Count == 0)
				return;

			if (GameManager.Instance.GetPlayTime() >= _orderList[0].EndTiem)
			{
				RPC_RemoveOrderReuslt(0, false);
			}

		}

		public override void Render()
		{
			UpdateOrderTime();
		}


		[Rpc(RpcSources.All, RpcTargets.All)]
		private void RPC_RemoveOrderReuslt(int index,NetworkBool isSucess)
		{
			string recipeName = _orderList[index].RecipeName;
			_orderList[index].gameObject.SetActive(false);
			GameObject destoryGameObejct = _orderList[index].gameObject;
			_orderList.RemoveAt(index);
			if(destoryGameObejct.TryGetComponent<PooledObject>(out var pooled))
			{
				pooled.Relese();
			}
			else
			{
				Destroy(destoryGameObejct);
			}
		}

		public void OrderCheck(string recipeName)
		{
			for (int i = 0; i < _orderList.Count; i++)
			{
				//오더가 있는 음식인 경우
				if (_orderList[i].RecipeName == recipeName)
				{
					RPC_RemoveOrderReuslt(i, true);
					var recipe = StageRecipeManager.Instance.RecipeTable[recipeName];
					InGameData.Instance.RPC_AddGold(recipe.SellGold, Runner.LocalPlayer);
					RPC_OrderCheckResult(true);
					return;
				}
			}
			RPC_OrderCheckResult(false);
		}

		private void RPC_OrderCheckResult(NetworkBool isSucess)
		{
			if (isSucess == true)
			{
				_audioSource.PlayOneShot(_succeseSound);
			}
			else
			{
				_audioSource.PlayOneShot(_failSound);
			}
		}

		private void UpdateOrderTime()
		{
			foreach(var order in _orderList)
			{
				order.Draw(GameManager.Instance.GetPlayTime());
			}
		}
	}
}
