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
	[SerializeField] private Rigidbody2D rbody;
	[SerializeField]
	private ParticleSystem drillVFX;

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
	
	[SyncVar]
	private UniqueProjectile uniqueFunction;
	[SyncVar]
	private Transform owner;

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

	public void Init(int animIndex, AttackBuilder hitbox, bool destroyOnWorldImpact, bool destroyOnEntityImpact, bool flipSprite, int uniqueFunction, ParticleType particleType, Transform owner)
	{
		this.hitbox = hitbox;
		this.destroyOnEntityImpact = destroyOnEntityImpact;
		this.destroyOnWorldImpact = destroyOnWorldImpact;
		this.animIndex = animIndex;
		this.flipSprite = flipSprite;
		this.particleType = particleType;
		this.uniqueFunction = (UniqueProjectile)uniqueFunction;
		this.owner = owner;
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
			if (particleType != ParticleType.NONE)
				VFXManager.Instance.CreateVFX(particleType, transform.position, flip: false);
		}

		if (uniqueFunction == UniqueProjectile.BOMB) {
			AttackBuilder.GetAttack(owner).SetParent(transform).SetSize(new Vector2(10f, 10f)).SetDuration(0.2f)
				.MakeProjectile(transform.position)
				.DisableWorldImpact()
				.DisableEntityImpact()
				.SetAnimation(5)
				.SetLifetime(0.85f)
				.Finish();
		}

		if (uniqueFunction == UniqueProjectile.DRILL) {
			NotifyVFXStart();
			rbody.velocity = 20 * rbody.velocity.normalized;

			[ClientRpc] void NotifyVFXStart() {
				drillVFX.Play();
			}
		}
	}

	[Command(requiresAuthority = false)] public void OnWorldExit() {
		if (uniqueFunction == UniqueProjectile.DRILL) {
			NotifyVFXStop();
			rbody.velocity = 40 * rbody.velocity.normalized;

			[ClientRpc] void NotifyVFXStop() {
				drillVFX.Stop();
			}
		}
	}

	[Command(requiresAuthority = false)]
	public void OnEntityCollide()
	{
		if (destroyOnEntityImpact)
		{
			NetworkServer.Destroy(gameObject);
			if (particleType != ParticleType.NONE)
				VFXManager.Instance.CreateVFX(particleType, transform.position, flip: false);
		}
	}
}

public enum UniqueProjectile {
	DEFAULT, DRILL, BOMB
}
