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
			item = (PickupType)UnityEngine.Random.Range((int)PickupType.SKILL_MAGNET, (int)PickupType.MAX);
			break;
		case "crown":
			item = PickupType.ITEM_CROWN;
			break;
		default:
			item = (PickupType)UnityEngine.Random.Range((int)PickupType.ITEM_CROWN, (int)PickupType.MAX);
			break;
		}
	}

	public void UpdateMerchandise(PickupType oldvalue, PickupType newValue)
	{
		sprite.sprite = iconLibrary[(int)(newValue + 4)];
	}

	public void OnInteract(PlayerController owner)
	{
		//TODO once bow is implemented, just get rid of this block
		if (item == PickupType.WEAPON_BOW) {
			VFXManager.Instance.SyncFloatingText("You should be ashamed.", transform.position + 5 * Vector3.up, Color.red);
		}
		owner.GetItem(item);
	
		if (singleUse)
		{
			Cleanup();
		}
	}

	[Command(requiresAuthority = false)]
	private void Cleanup()
	{
		NetworkServer.Destroy(gameObject);
	}
}
