using Mirror;
using UnityEngine;

public struct ProjectileBuilder
{
	private Vector2 velocity;

	private Vector3 pos;

	private AttackBuilder hitbox;

	private bool destroyOnWorldImpact;

	private bool destroyOnEntityImpact;

	private float lifetime;

	private int animationIndex;

	private bool rotateWithVelocity;

	private bool flipSprite;

	private int particleType;

	private float gravity;
	private Transform owner;
	private int uniqueFunction;

	public static ProjectileBuilder GetProjectile(Vector3 pos)
	{
		ProjectileBuilder result = default(ProjectileBuilder);
		result.pos = pos;
		result.destroyOnWorldImpact = true;
		result.destroyOnEntityImpact = true;
		result.particleType = (int)ParticleType.NONE;
		return result;
	}

	public ProjectileBuilder SetHitbox(AttackBuilder attack)
	{
		hitbox = attack;
		return this;
	}

	public ProjectileBuilder SetVelocity(Vector2 vel)
	{
		velocity = vel;
		return this;
	}

	public ProjectileBuilder SetLifetime(float val)
	{
		lifetime = val;
		return this;
	}

	public ProjectileBuilder SetAnimation(int index)
	{
		animationIndex = index;
		return this;
	}

	public ProjectileBuilder DisableWorldImpact()
	{
		destroyOnWorldImpact = false;
		return this;
	}

	public ProjectileBuilder DisableEntityImpact()
	{
		destroyOnEntityImpact = false;
		return this;
	}

	public ProjectileBuilder FlipSprite(bool flip)
	{
		flipSprite = flip;
		return this;
	}

	public ProjectileBuilder SetParticleType(ParticleType type)
	{
		particleType = (int)type;
		return this;
	}

	public ProjectileBuilder RotateWithVelocity()
	{
		rotateWithVelocity = true;
		return this;
	}

	public ProjectileBuilder SetGravity(float gravity) {
		this.gravity = gravity;
		return this;
	}

	public ProjectileBuilder SetUnique(UniqueProjectile id) {
		uniqueFunction = (int)id;
		return this;
	}

	public ProjectileBuilder SetOwner(Transform owner) {
		this.owner = owner;
		return this;
	}

	public void Finish()
	{
		GameManager.Instance.SpawnProjectile(this);
	}

	public GameObject Apply(GameObject prefab)
	{
		GameObject gameObject = Object.Instantiate(prefab, pos, Quaternion.identity);
		ProjectileData component = gameObject.GetComponent<ProjectileData>();
		component.Init(animationIndex, hitbox, destroyOnWorldImpact, destroyOnEntityImpact, flipSprite, uniqueFunction, (ParticleType)particleType, owner);
		if (lifetime > 0f)
		{
			component.ApplyLifetime(lifetime);
		}
		var rbody = gameObject.GetComponent<Rigidbody2D>();
		rbody.velocity = velocity;
		rbody.gravityScale = gravity;
		if (rotateWithVelocity)
		{
			gameObject.transform.localRotation = Quaternion.FromToRotation(Vector2.right, velocity);
			if (velocity.x < 0 && velocity.y != 0) {
				gameObject.GetComponent<SpriteRenderer>().flipY = true;
			}
		}
		return gameObject;
	}

	public void Write(NetworkWriter writer)
	{
		writer.Write(velocity);
		writer.Write(pos);
		writer.Write(destroyOnWorldImpact);
		writer.Write(destroyOnEntityImpact);
		writer.Write(lifetime);
		writer.Write(animationIndex);
		writer.Write(flipSprite);
		writer.Write(rotateWithVelocity);
		writer.Write(particleType);
		writer.Write(gravity);
		writer.Write(uniqueFunction);
		writer.Write(owner);

		hitbox.Write(writer);
	}

	public ProjectileBuilder Read(NetworkReader reader)
	{
		velocity = reader.Read<Vector2>();
		pos = reader.Read<Vector3>();
		destroyOnWorldImpact = reader.Read<bool>();
		destroyOnEntityImpact = reader.Read<bool>();
		lifetime = reader.Read<float>();
		animationIndex = reader.Read<int>();
		flipSprite = reader.Read<bool>();
		rotateWithVelocity = reader.Read<bool>();
		particleType = reader.Read<int>();
		gravity = reader.Read<float>();
		uniqueFunction = reader.Read<int>();
		owner = reader.Read<Transform>();

		hitbox.Read(reader);
		return this;
	}
}
