using Mirror;
using UnityEngine;

public struct AttackBuilder
{
	private Vector3 position;

	private Vector3 size;

	private Transform parent;

	private float duration;

	private Transform owner;

	private bool friendlyFire;

	public AttackBuilder SetPosition(Vector3 pos)
	{
		position = pos;
		return this;
	}

	public AttackBuilder SetParent(Transform parent)
	{
		this.parent = parent;
		return this;
	}

	public AttackBuilder SetDuration(float duration)
	{
		this.duration = duration;
		return this;
	}

	public AttackBuilder EnableFriendlyFire()
	{
		friendlyFire = true;
		return this;
	}

	public AttackBuilder SetSize(Vector2 size)
	{
		this.size = size;
		return this;
	}

	public static AttackBuilder GetAttack(Transform owner)
	{
		AttackBuilder result = default(AttackBuilder);
		result.owner = owner;
		return result;
	}

	public ProjectileBuilder MakeProjectile(Vector3 startPos)
	{
		return ProjectileBuilder.GetProjectile(startPos).SetHitbox(this);
	}

	public void Finish()
	{
		GameObject hitbox = NetworkSingleton<GameManager>.Instance.GetHitbox(position, Quaternion.identity, parent);
		hitbox.GetComponent<BoxCollider2D>().size = size;
		if (duration != 0f)
		{
			hitbox.AddComponent<DestroyAfterDelay>().Init(duration);
		}
		HitboxData component = hitbox.GetComponent<HitboxData>();
		component.Owner = owner;
		component.FriendlyFire = friendlyFire;
	}

	public void Write(NetworkWriter writer)
	{
		writer.Write(position);
		writer.Write(size);
		writer.Write(parent);
		writer.Write(duration);
		writer.Write(owner);
		writer.Write(friendlyFire);
	}

	public AttackBuilder Read(NetworkReader reader)
	{
		position = reader.Read<Vector3>();
		size = reader.Read<Vector3>();
		parent = reader.Read<Transform>();
		duration = reader.Read<float>();
		owner = reader.Read<Transform>();
		friendlyFire = reader.Read<bool>();
		return this;
	}
}
