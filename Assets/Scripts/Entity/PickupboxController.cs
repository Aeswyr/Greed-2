using UnityEngine;

public class PickupboxController : MonoBehaviour
{
	[SerializeField]
	private PlayerController owner;

	private void OnTriggerEnter2D(Collider2D other)
	{
		owner.DoPickup(other.GetComponent<PickupData>());
	}

    private void OnTriggerStay2D(Collider2D other)
    {
        owner.DoPickup(other.GetComponent<PickupData>());
    }
}
