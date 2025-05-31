using System;
using System.Runtime.InteropServices;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PurchasableInteractable : NetworkBehaviour, PurchaseInterface
{
	[SerializeField]
	private TextMeshPro priceTag;
	[SerializeField]
	private BoxCollider2D interactBox;

	[SerializeField]
	private UnityEvent<PlayerController> output;
	[SerializeField]
	private float purchaseLockout;
	[SerializeField]
	private bool oneTimePurchase;
	[SerializeField]
	private PickupPriceLibrary baseCosts;

	private float lastPurchase;
	private int baseCost = 10;
	[SerializeField] private bool levelPurchasable;

	[SyncVar(hook = nameof(UpdatePrice))]
	private int cost;
	[SerializeField] private bool confirmPurchase;
	[SerializeField] private UnityEvent<float> onConfirm;

	private void Start()
	{
		if (isServer)
			CalculateCost();
	}

	public void PriceByItem(PickupType type)
	{
		baseCost = baseCosts[type];
		CalculateCost();
	}

	private void CalculateCost()
	{
		int num = Mathf.Min(GameManager.Instance.GetLevelIndex(), 15);
		cost = (int)((num * (int)(2f + 0.25f * (float)num) + baseCost - UnityEngine.Random.Range(0, 2 * num)) * (levelPurchasable ? 0.5f : 1f));
	}

	private void UpdatePrice(int oldValue, int newValue)
	{
		priceTag.text = newValue.ToString();
	}

	public void OnInteract(PlayerController owner)
	{
		if (confirmPurchase && !owner.IsShopConfirmed(this))
		{
			onConfirm?.Invoke(6f);
			owner.ConfirmShop(this);
			return;
		}
		owner.ConfirmShop(null);

		if (PurchaseUnlocked() && owner.TrySpendMoney(cost))
		{
			output.Invoke(owner);
			lastPurchase = Time.time + purchaseLockout;
			if (oneTimePurchase)
				SendActivation();
		}

		[Command(requiresAuthority = false)] void SendActivation()
		{
			RecieveActivation();
		}

		[ClientRpc] void RecieveActivation()
		{
			priceTag.transform.parent.gameObject.SetActive(false);
			interactBox.enabled = false;
			lastPurchase = Time.time + purchaseLockout;
		}
	}

	private bool PurchaseUnlocked()
	{
		return purchaseLockout == 0 || Time.time > lastPurchase;
	}


}

public interface PurchaseInterface
{
	
}
