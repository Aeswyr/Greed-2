using UnityEngine;
using UnityEngine.Events;

public class TriggerAction : MonoBehaviour
{
	[SerializeField]
	private UnityEvent action;

	private void OnTriggerEnter2D(Collider2D other)
	{
		action.Invoke();
	}
}
