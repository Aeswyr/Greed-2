using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HurtboxController : MonoBehaviour
{
	[SerializeField]
	private UnityEvent<HitboxData> action;

	private List<Transform> seenHitboxes = new();

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.transform.parent != null && other.transform.parent.TryGetComponent<PlayerController>(out var component))
		{
			component.DoHitstop(0.15f);
		}
		action?.Invoke(other.GetComponent<HitboxData>());
	}

	public void MarkHitboxSeen(Transform hitbox) {
		seenHitboxes.Add(hitbox);

		while (seenHitboxes.Count > 10) {
			seenHitboxes.RemoveAt(0);
		}
	}

	public bool HasSeenHitbox(Transform hitbox) {
		return seenHitboxes.Contains(hitbox);
	}
}
