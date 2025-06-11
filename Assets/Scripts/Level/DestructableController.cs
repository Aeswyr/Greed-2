using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class DestructableController : NetworkBehaviour
{
	[SerializeField]
	private SpriteRenderer sprite;
	[SerializeField]
	private BoxCollider2D hurtbox;
	[SerializeField]
	private PickupType dropType;

	[SerializeField]
	private int dropAmount;

	[SerializeField]
	private PickupVariant dropVariant;

	[SerializeField]
	private bool swapSpriteOnDestroy;

	[SerializeField]
	private Sprite destroySprite;


	[SerializeField]
	private ParticlePrefabType destryParticle;

	[SyncVar] bool broken;

	public void OnHit(HitboxData data)
	{
		if (broken)
			return;

		if (data.Owner.TryGetComponent<PlayerController>(out var component) && component.isLocalPlayer)
		{
			if (data.transform.parent != null && data.transform.parent.TryGetComponent<ProjectileData>(out var component2))
			{
				component2.OnEntityCollide();
			}
			VFXManager.Instance.SyncVFX(ParticleType.HITSPARK, 0.5f * (transform.position + data.transform.position), flip: false);
			BreakObject();
			if (destryParticle != ParticlePrefabType.NONE)
				VFXManager.Instance.SyncPrefabVFX(destryParticle, transform.position);
		}

		[Command(requiresAuthority = false)]
		void BreakObject()
		{
			if (broken)
				return;
			broken = true;
			GameManager.Instance.SpawnGoldBurst(transform.position, dropAmount, dropVariant);

			if (swapSpriteOnDestroy)
			{
				SwapSprite();
				hurtbox.enabled = false;
			}
			else
			{
				NetworkServer.Destroy(gameObject);
			}

			[ClientRpc] void SwapSprite()
			{
				sprite.sprite = destroySprite;
			}
		}
	}
}
