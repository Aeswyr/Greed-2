using UnityEngine;
using System;

[CreateAssetMenu(fileName = "PickupPriceLibrary", menuName = "ScriptableObjects/PickupPriceLibrary", order = 1)]
public class PickupPriceLibrary : ScriptableObject
{
	[SerializeField]
	private PickupPrice[] prices;

	public int this[PickupType type] {
		get {
			for (int  i = 0; i < prices.Length; i++) {
				if (prices[i].type == type)
					return prices[i].price;
			}
			return 0;
		}
	}

	public int Length => prices.Length;

	[Serializable] private struct PickupPrice {
		public PickupType type;
		public int price;
	}
}


