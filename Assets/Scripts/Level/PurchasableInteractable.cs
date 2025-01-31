using System;
using System.Runtime.InteropServices;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PurchasableInteractable : NetworkBehaviour
{
	[SerializeField]
	private TextMeshPro priceTag;

	[SerializeField]
	private UnityEvent<PlayerController> output;

	private int baseCost = 10;

	[SyncVar(hook = nameof(UpdatePrice))]
	private int cost;

	public Action<int, int> _Mirror_SyncVarHookDelegate_cost;

	private void Start()
	{
		if (isServer)
		{
			int num = Mathf.Min(GameManager.Instance.GetLevelIndex(), 15);
			cost = num * (int)(2f + 0.25f * (float)num) + baseCost - UnityEngine.Random.Range(0, 2 * num);
		}
	}

	private void UpdatePrice(int oldValue, int newValue)
	{
		priceTag.text = newValue.ToString();
	}

	public void OnInteract(PlayerController owner)
	{
		if (owner.TrySpendMoney(cost))
		{
			output.Invoke(owner);
		}
	}


}
