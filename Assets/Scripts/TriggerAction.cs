using UnityEngine;
using UnityEngine.Events;

public class TriggerAction : MonoBehaviour
{
	[SerializeField]
	private UnityEvent action;

	[SerializeField]
	private UnityEvent exitAction;

	private void OnTriggerEnter2D(Collider2D other)
	{
		action?.Invoke();
	}

	private void OnTriggerExit2D(Collider2D other) {
		exitAction?.Invoke();
	}
}
