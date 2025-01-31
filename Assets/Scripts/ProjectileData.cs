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
		hitbox.SetParent(transform).Finish();
		AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
		animatorOverrideController["projectile"] = anims[animIndex];
		animator.runtimeAnimatorController = animatorOverrideController;
		transform.GetComponent<SpriteRenderer>().flipX = flipSprite;
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
		if (isServer && destroyAfterDelay && Time.time > destroyDelay)
		{
			destroyAfterDelay = false;
			NetworkServer.Destroy(gameObject);
			VFXManager.Instance.CreateVFX(particleType, transform.position, flip: false);
		}
	}

	[Command(requiresAuthority = false)]
	public void OnWorldCollide()
	{
		if (destroyOnWorldImpact)
		{
			NetworkServer.Destroy(gameObject);
			VFXManager.Instance.CreateVFX(particleType, transform.position, flip: false);
		}
	}

	[Command(requiresAuthority = false)]
	public void OnEntityCollide()
	{
		if (destroyOnEntityImpact)
		{
			NetworkServer.Destroy(gameObject);
			VFXManager.Instance.CreateVFX(particleType, transform.position, flip: false);
		}
	}
}
