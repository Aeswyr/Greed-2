using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HurtboxController : MonoBehaviour
{
	[SerializeField]
	private UnityEvent<HitboxData> action;

	private List<Collider2D> hitColliders = new();

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (hitColliders.Contains(other))
			return;

		if (other.transform.parent != null && other.transform.parent.TryGetComponent<PlayerController>(out var component))
		{
			component.DoHitstop(0.15f);
		}
		action?.Invoke(other.GetComponent<HitboxData>());

		CleanColliders();
		hitColliders.Add(other);
	}

	public void CleanColliders() {
		for (int i = 0; i < hitColliders.Count; i++) {
			if (hitColliders == null) {
				hitColliders.RemoveAt(i);
				i--;
			}
		}
	}
}
