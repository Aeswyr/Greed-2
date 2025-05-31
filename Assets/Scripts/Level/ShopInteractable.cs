using System;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class ShopInteractable : NetworkBehaviour
{
	[SerializeField]
	private SpriteRenderer sprite;

	[SyncVar(hook = nameof(UpdateMerchandise))]
	private PickupType item;

	[SerializeField]
	private SpriteLibrary iconLibrary;

	[SerializeField]
	private bool singleUse;

	private ToolTip currentToolTip;
	
	private void Start()
	{
	}

	public void SetupMerchandise(string type)
	{
		switch (type)
		{
		case "weapon":
			item = (PickupType)UnityEngine.Random.Range((int)PickupType.WEAPON_PICK, (int)PickupType.SKILL_MAGNET);
			break;
		case "skill":
			item = (PickupType)UnityEngine.Random.Range((int)PickupType.SKILL_MAGNET, (int)PickupType.ITEM_POTION_HEALTH);
			break;
		case "crown":
			item = PickupType.ITEM_CROWN;
			break;
		case "potion":
			item = (PickupType)UnityEngine.Random.Range((int)PickupType.ITEM_POTION_HEALTH, (int)PickupType.MAX);
			break;
		default:
			item = (PickupType)UnityEngine.Random.Range((int)PickupType.ITEM_CROWN, (int)PickupType.MAX);
			break;
		}

		if (gameObject.TryGetComponent(out PurchasableInteractable purchase)) {
			purchase.PriceByItem(item);
		} else if (gameObject.TryGetComponent(out AuctionableInteractable auction)) {
			auction.PriceByItem(item);
		}
	}

	public void UpdateMerchandise(PickupType oldvalue, PickupType newValue)
	{
		sprite.sprite = iconLibrary[(int)(newValue + 6)];
	}

	public void OnInteract(PlayerController owner)
	{
		ToolTipManager.Instance.ClearTooltip(currentToolTip);
		owner.GetItem(item);
	
		if (singleUse)
		{
			Cleanup();
		}
	}

	public void ShowTooltip(float offset)
	{
		foreach (var shop in FindObjectsByType<ShopInteractable>(FindObjectsSortMode.None))
			shop.HideTooltip();
		currentToolTip = ToolTipManager.Instance.CreateTooltip(item, transform.position + offset * Vector3.up);
	}

	public void HideTooltip()
	{
		ToolTipManager.Instance.ClearTooltip(currentToolTip);
		currentToolTip = null;
	}

	[Command(requiresAuthority = false)]
	private void Cleanup()
	{
		NetworkServer.Destroy(gameObject);
	}
}
