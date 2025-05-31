using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PickupPriceLibrary", menuName = "ScriptableObjects/PickupPriceLibrary", order = 1)]
public class PickupPriceLibrary : ScriptableObject
{
	[SerializeField]
	private PickupPrice[] prices;
	private static Dictionary<string, int> priceLookup;

	public int this[PickupType type]
	{
		get
		{
			if (priceLookup == null)
			{
				priceLookup = new();
				foreach (var price in prices)
					priceLookup.Add(price.type, price.price);
			}
			if (priceLookup.ContainsKey(type.ToString()))
				return priceLookup[type.ToString()];
			return 0;
		}
	}

	public int Length => prices.Length;

	[Serializable] private struct PickupPrice {
		public string type;
		public int price;
	}
}


