using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class ProjectileData : NetworkBehaviour
{
	[SerializeField]
	private AnimationLibrary anims;

	[SerializeField]
	private Animator animator;

	[SyncVar]
	private int animIndex;

	[SyncVar]
	private AttackBuilder hitbox;

	[SyncVar]
	private bool destroyOnWorldImpact;

	[SyncVar]
	private bool destroyOnEntityImpact;

	[SyncVar]
	private bool flipSprite;

	[SyncVar]
	private ParticleType particleType;

	private bool destroyAfterDelay = false;

	private float destroyDelay;

	private void Start()
	{
		hitbox.SetParent(base.transform).Finish();
		AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
		animatorOverrideController["projectile"] = anims[animIndex];
		animator.runtimeAnimatorController = animatorOverrideController;
		base.transform.GetComponent<SpriteRenderer>().flipX = flipSprite;
	}

	public void Init(int animIndex, AttackBuilder hitbox, bool destroyOnWorldImpact, bool destroyOnEntityImpact, bool flipSprite, ParticleType particleType)
	{
		this.hitbox = hitbox;
		this.destroyOnEntityImpact = destroyOnEntityImpact;
		this.destroyOnWorldImpact = destroyOnWorldImpact;
		this.animIndex = animIndex;
		this.flipSprite = flipSprite;
		this.particleType = particleType;
	}

	public void ApplyLifetime(float lifetime)
	{
		destroyAfterDelay = true;
		destroyDelay = Time.time + lifetime;
	}

	private void FixedUpdate()
	{
		if (base.isServer && destroyAfterDelay && Time.time > destroyDelay)
		{
			destroyAfterDelay = false;
			NetworkServer.Destroy(base.gameObject);
			NetworkSingleton<VFXManager>.Instance.CreateVFX(particleType, base.transform.position, flip: false);
		}
	}

	[Command(requiresAuthority = false)]
	public void OnWorldCollide()
	{
		if (destroyOnWorldImpact)
		{
			NetworkServer.Destroy(base.gameObject);
			NetworkSingleton<VFXManager>.Instance.CreateVFX(particleType, base.transform.position, flip: false);
		}
	}

	[Command(requiresAuthority = false)]
	public void OnEntityCollide()
	{
		if (destroyOnEntityImpact)
		{
			NetworkServer.Destroy(base.gameObject);
			NetworkSingleton<VFXManager>.Instance.CreateVFX(particleType, base.transform.position, flip: false);
		}
	}
}
