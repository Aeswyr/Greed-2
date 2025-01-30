using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class DestructableController : NetworkBehaviour
{
	[SerializeField]
	private PickupType dropType;

	[SerializeField]
	private int dropAmount;

	[SerializeField]
	private PickupVariant dropVariant;

	public void OnHit(HitboxData data)
	{
		if (data.Owner.TryGetComponent<PlayerController>(out var component) && component.isLocalPlayer)
		{
			if (data.transform.parent != null && data.transform.parent.TryGetComponent<ProjectileData>(out var component2))
			{
				component2.OnEntityCollide();
			}
			VFXManager.Instance.SyncVFX(ParticleType.HITSPARK, 0.5f * (base.transform.position + data.transform.position), flip: false);
			BreakObject();
		}
		[Command(requiresAuthority = false)]
		void BreakObject()
		{
			GameManager.Instance.SpawnGoldBurst(base.transform.position, dropAmount, dropVariant);
			NetworkServer.Destroy(base.gameObject);
		}
	}
}
