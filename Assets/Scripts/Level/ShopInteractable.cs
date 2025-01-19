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

	public Action<PickupType, PickupType> _Mirror_SyncVarHookDelegate_item;

	public PickupType Networkitem
	{
		get
		{
			return item;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref item, 1uL, _Mirror_SyncVarHookDelegate_item);
		}
	}

	private void Start()
	{
	}

	public void SetupMerchandise(string type)
	{
		switch (type)
		{
		case "weapon":
			Networkitem = (PickupType)UnityEngine.Random.Range(3, 9);
			break;
		case "skill":
			Networkitem = (PickupType)UnityEngine.Random.Range(9, 14);
			break;
		case "crown":
			Networkitem = PickupType.ITEM_CROWN;
			break;
		default:
			Networkitem = (PickupType)UnityEngine.Random.Range(2, 14);
			break;
		}
	}

	public void UpdateMerchandise(PickupType oldvalue, PickupType newValue)
	{
		sprite.sprite = iconLibrary[(int)(newValue + 4)];
	}

	public void OnInteract(PlayerController owner)
	{
		owner.GetItem(item);
		if (singleUse)
		{
			Cleanup();
		}
	}

	[Command(requiresAuthority = false)]
	private void Cleanup()
	{
		NetworkServer.Destroy(base.gameObject);
	}
}
