using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class AuctionableInteractable : NetworkBehaviour, PurchaseInterface
{
	[SerializeField]
	private TextMeshPro priceTag;

	[SerializeField]
	private TextMeshPro auctionTimeout;
	[SerializeField]
	private PickupPriceLibrary baseCosts;

	[SerializeField]
	private UnityEvent<PlayerController> output;

	private Dictionary<PlayerController, int> bidders = new Dictionary<PlayerController, int>();

	private int baseCost = 10;


	private PlayerController localPlayer;

	private int localBid;

	private int timer = -1;

	[SyncVar(hook = nameof(UpdatePrice))]
	private int cost;
	[SerializeField] private bool confirmPurchase;
	[SerializeField] private UnityEvent<float> onConfirm;

	private Vector3 pricePos;

	private void Start()
	{
		if (isServer)
			CalculateCost();
		auctionTimeout.text = "10s";
		auctionTimeout.color = Color.green;

		pricePos = priceTag.transform.position;
	}

	public void PriceByItem(PickupType type) {
		baseCost = baseCosts[type];
		CalculateCost();
	}

	private void CalculateCost() {
		int num = Mathf.Min(GameManager.Instance.GetLevelIndex(), 10);
		cost = num * (int)(2f + 0.25f * (float)num) + baseCost - UnityEngine.Random.Range(0, 2 * num);
	}

	private void FixedUpdate()
	{
		if (!isServer)
		{
			return;
		}
		if (timer > 0)
		{
			if (timer % 50 == 0)
			{
				UpdateTime(timer / 50);
			}
			timer--;
		}
		else if (timer == 0)
		{
			RunAuctionComplete();
			timer--;
		}
	}

	private void UpdatePrice(int oldValue, int newValue)
	{
		priceTag.text = newValue.ToString();
	}

	public void OnInteract(PlayerController owner)
	{
		if (TryGetComponent(out ShopInteractable shop))
			shop.HideTooltip();

		if (confirmPurchase && !owner.IsShopConfirmed(this))
		{
			onConfirm?.Invoke(10);
			owner.ConfirmShop(this);
			return;
		}
		owner.ConfirmShop(null);

		if (owner.TrySpendMoney(cost - localBid))
		{
			localPlayer = owner;
			localBid = cost;
			AddBid(localPlayer, localBid);

			priceTag.transform.DOJump(pricePos, 0.5f, 2, 0.5f);
			priceTag.color = Color.green;
			priceTag.DOColor(Color.white, 0.5f);
		}
		else
		{
			priceTag.transform.DOShakePosition(0.5f, randomness: 20).onComplete += () =>
			{
				priceTag.transform.position = pricePos;
			};
			priceTag.color = Color.red;
			priceTag.DOColor(Color.white, 0.5f);
		}
	}

	[Command(requiresAuthority = false)]
	private void AddBid(PlayerController player, int amount)
	{
		bidders[player] = amount;
		cost = cost + 10;
		timer = 500;
	}

	[ClientRpc]
	private void UpdateTime(int val)
	{
		auctionTimeout.text = $"{val}s";
		auctionTimeout.color = Color.green;
		if (val <= 5)
		{
			auctionTimeout.color = Color.red;
		}
	}

	public void RunAuctionComplete()
	{
		if (bidders.Count <= 0)
		{
			return;
		}
		int num = 0;
		PlayerController playerController = null;
		foreach (KeyValuePair<PlayerController, int> bidder in bidders)
		{
			if (playerController == null || bidder.Value > num)
			{
				num = bidder.Value;
				playerController = bidder.Key;
			}
		}
		CompleteAuction(playerController);
		[ClientRpc]
		void CompleteAuction(PlayerController winner)
		{
			if (TryGetComponent(out ShopInteractable shop))
				shop.HideTooltip();
			if (!(localPlayer == null))
			{
				if (winner == localPlayer)
				{
					output.Invoke(localPlayer);
					Cleanup();
				}
				else
				{
					localPlayer.TryAddMoney(localBid);
				}
			}

			[Command(requiresAuthority = false)]
			void Cleanup()
			{
				NetworkServer.Destroy(gameObject);
			}
		}
	}
}
