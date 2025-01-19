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

	public static ProjectileBuilder GetProjectile(Vector3 pos)
	{
		ProjectileBuilder result = default(ProjectileBuilder);
		result.pos = pos;
		result.destroyOnWorldImpact = true;
		result.destroyOnEntityImpact = true;
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

	public void Finish()
	{
		NetworkSingleton<GameManager>.Instance.SpawnProjectile(this);
	}

	public GameObject Apply(GameObject prefab)
	{
		GameObject gameObject = Object.Instantiate(prefab, pos, Quaternion.identity);
		ProjectileData component = gameObject.GetComponent<ProjectileData>();
		component.Init(animationIndex, hitbox, destroyOnWorldImpact, destroyOnEntityImpact, flipSprite, (particleType == 0) ? ParticleType.HITSPARK : ((ParticleType)particleType));
		if (lifetime > 0f)
		{
			component.ApplyLifetime(lifetime);
		}
		gameObject.GetComponent<Rigidbody2D>().velocity = velocity;
		if (rotateWithVelocity)
		{
			gameObject.transform.localRotation = Quaternion.FromToRotation(Vector2.right, velocity);
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
		hitbox.Read(reader);
		return this;
	}
}
