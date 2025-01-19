using System.Collections.Generic;
using UnityEngine;

public class InteractboxController : MonoBehaviour
{
	[SerializeField]
	private PlayerController owner;

	private List<Collider2D> activeCollisions = new List<Collider2D>();

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (owner.isLocalPlayer)
		{
			activeCollisions.Add(other);
			if (activeCollisions.Count == 1)
			{
				owner.NoitifyInteraction(state: true);
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (owner.isLocalPlayer)
		{
			activeCollisions.Remove(other);
			if (activeCollisions.Count == 0)
			{
				owner.NoitifyInteraction(state: false);
			}
		}
	}

	public void FireInteraction()
	{
		Collider2D collider2D = null;
		float num = float.MaxValue;
		foreach (Collider2D activeCollision in activeCollisions)
		{
			if (collider2D == null || Vector3.Distance(activeCollision.transform.position, owner.transform.position) < num)
			{
				num = Vector3.Distance(activeCollision.transform.position, owner.transform.position);
				collider2D = activeCollision;
			}
		}
		collider2D?.GetComponent<InteractableData>().FireInteraction(owner);
	}
}
