using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class AuctionableInteractable : NetworkBehaviour
{
	[SerializeField]
	private TextMeshPro priceTag;

	[SerializeField]
	private TextMeshPro auctionTimeout;

	[SerializeField]
	private UnityEvent<PlayerController> output;

	private Dictionary<PlayerController, int> bidders = new Dictionary<PlayerController, int>();

	private int baseCost = 10;

	private PlayerController localPlayer;

	private int localBid;

	private int timer = -1;

	[SyncVar(hook = nameof(UpdatePrice))]
	private int cost;

	private void Start()
	{
		if (base.isServer)
		{
			int num = Mathf.Min(GameManager.Instance.GetLevelIndex(), 10);
			cost = num * (int)(2f + 0.25f * (float)num) + baseCost - UnityEngine.Random.Range(0, 2 * num);
		}
		auctionTimeout.text = "10s";
		auctionTimeout.color = Color.green;
	}

	private void FixedUpdate()
	{
		if (!base.isServer)
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
		if (owner.TrySpendMoney(cost - localBid))
		{
			localPlayer = owner;
			localBid = cost;
			AddBid(localPlayer, localBid);
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
			NetworkServer.Destroy(base.gameObject);
		}
		}
	}
}
