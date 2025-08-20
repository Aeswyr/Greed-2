using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class TriggerAction : MonoBehaviour
{
	[SerializeField]
	private UnityEvent action;

	[SerializeField]
	private UnityEvent exitAction;

	List<Collider2D> colliding = new();

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (colliding.Contains(other))
			return;
		action?.Invoke();
		colliding.Add(other);
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if (colliding.Contains(other))
			return;
		action?.Invoke();
		colliding.Add(other);
    }

    private void OnTriggerExit2D(Collider2D other)
	{
		colliding.Remove(other);
		exitAction?.Invoke();
	}
}
