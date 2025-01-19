using UnityEngine;
using UnityEngine.Events;

public class InteractableData : MonoBehaviour
{
	[SerializeField]
	private UnityEvent<PlayerController> action;

	public void FireInteraction(PlayerController target)
	{
		action.Invoke(target);
	}
}
