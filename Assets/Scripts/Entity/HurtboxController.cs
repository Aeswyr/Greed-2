using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HurtboxController : MonoBehaviour
{
	[SerializeField]
	private UnityEvent<HitboxData> action;

	private List<Transform> seenHitboxes = new();
	private bool isPlayerOwned;

    void Start()
    {
        isPlayerOwned = transform.parent != null && transform.parent.TryGetComponent(out PlayerController owner);
    }

    private void OnTriggerEnter2D(Collider2D other)
	{
		var data = other.GetComponent<HitboxData>();
		PlayerController player;
		if (other.transform.parent != null && other.transform.parent.TryGetComponent(out player))
		{
			player.DoHitstop(0.15f);
		}

		if (data.Owner != null && !isPlayerOwned && data.Owner.TryGetComponent(out player)) {
			player.OnHitTrigger(data.transform);
		}

		action?.Invoke(data);
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
