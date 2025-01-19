using Mirror;

public static class NetworkReadWrite
{
	public static void WriteProjectileBuilder(this NetworkWriter writer, ProjectileBuilder value)
	{
		value.Write(writer);
	}

	public static ProjectileBuilder ReadProjectileBuilder(this NetworkReader reader)
	{
		return default(ProjectileBuilder).Read(reader);
	}

	public static void WriteAttackBuilder(this NetworkWriter writer, AttackBuilder value)
	{
		value.Write(writer);
	}

	public static AttackBuilder ReadAttackBuilder(this NetworkReader reader)
	{
		return default(AttackBuilder).Read(reader);
	}
}
