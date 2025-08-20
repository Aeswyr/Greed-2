using System;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
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
	[SerializeField] private GameObject[] spawnablePrefabs;
	[SerializeField] private AnimationCurve boomerangFire;
	[SerializeField] private AnimationCurve boomerangReturn;

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

	private Vector2 speed;
	private float startTime;
	private bool returning;


	private void Start()
	{
		hitbox.SetParent(transform).Finish();
		AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
		animatorOverrideController["projectile"] = anims[animIndex];
		animator.runtimeAnimatorController = animatorOverrideController;
		transform.GetComponent<SpriteRenderer>().flipX = flipSprite;

		speed = rbody.linearVelocity;

		startTime = Time.time;
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

		if (isServer && uniqueFunction == UniqueProjectile.BOOMERANG)
		{

			if (returning)
			{
				rbody.linearVelocity = boomerangReturn.Evaluate(Time.time - startTime) * speed;

				if ((owner.transform.position - transform.position).sqrMagnitude < 9f)
				{
					owner.GetComponent<PlayerController>().ResetCooldown();
					NetworkServer.Destroy(gameObject);
				}
			}
			else
				rbody.linearVelocity = boomerangFire.Evaluate(Time.time - startTime) * speed;

			if (!returning && Time.time > startTime + boomerangFire[boomerangFire.length - 1].time)
				StartBoomerangReturning();
			
		}
	}

	private void StartBoomerangReturning()
	{
		returning = true;
		startTime = Time.time;
		speed = speed.magnitude * (owner.transform.position - transform.position).normalized;
		transform.localRotation = Quaternion.FromToRotation(Vector2.right, speed);
	}

	[Command(requiresAuthority = false)]
	public void OnWorldCollide()
	{
		if (uniqueFunction == UniqueProjectile.BOOMERANG && !returning)
		{
			transform.position += - 0.02f * (Vector3)speed;
			StartBoomerangReturning();
			return;
		}


		if (destroyOnWorldImpact)
		{
			NetworkServer.Destroy(gameObject);
			if (particleType != ParticleType.NONE)
				VFXManager.Instance.CreateVFX(particleType, transform.position, flip: false);
		}

		if (uniqueFunction == UniqueProjectile.BOMB)
		{
			AttackBuilder.GetAttack(owner).SetParent(transform).SetSize(new Vector2(10f, 10f)).SetDuration(0.2f)
				.MakeProjectile(transform.position)
				.DisableWorldImpact()
				.DisableEntityImpact()
				.SetAnimation(5)
				.SetLifetime(0.85f)
				.Finish();
		}

		if (uniqueFunction == UniqueProjectile.DRILL)
		{
			NotifyVFXStart();
			rbody.linearVelocity = 20 * rbody.linearVelocity.normalized;

			[ClientRpc] void NotifyVFXStart()
			{
				drillVFX.Play();
			}
		}

		if (uniqueFunction == UniqueProjectile.ARROW)
		{
			CreateArrowPickup();
		}
		
	}

	private void CreateArrowPickup() {
		var arrowSpawn = Instantiate(spawnablePrefabs[0], transform.position, transform.rotation, GameManager.Instance.GetLevelObjectRoot());

		var arrow = arrowSpawn.GetComponent<ArrowPickupController>();
		arrow.SetOwner(owner);

		NetworkServer.Spawn(arrowSpawn);
	}

	[Command(requiresAuthority = false)] public void OnWorldExit() {
		if (uniqueFunction == UniqueProjectile.DRILL) {
			NotifyVFXStop();
			rbody.linearVelocity = 40 * rbody.linearVelocity.normalized;

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

		if (uniqueFunction == UniqueProjectile.ARROW)
		{
			CreateArrowPickup();
		}
	}
}

public enum UniqueProjectile {
	DEFAULT, DRILL, BOMB, ARROW, BOOMERANG
}
