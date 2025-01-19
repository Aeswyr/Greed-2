using UnityEngine;
using UnityEngine.Events;

public class HurtboxController : MonoBehaviour
{
	[SerializeField]
	private UnityEvent<HitboxData> action;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.transform.parent != null && other.transform.parent.TryGetComponent<PlayerController>(out var component))
		{
			component.DoHitstop(0.15f);
		}
		action?.Invoke(other.GetComponent<HitboxData>());
	}
}
