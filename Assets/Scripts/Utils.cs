using UnityEngine;
using Steamworks;

public class Utils
{
	public static RaycastHit2D Raycast(Vector3 start, Vector2 dir, float dist, LayerMask mask)
	{
		RaycastHit2D result = Physics2D.Raycast(start, dir, dist, mask);
		Debug.DrawRay(start, dir * dist, Color.green);
		return result;
	}

	public static RaycastHit2D Boxcast(Vector3 start, Vector2 size, Vector2 dir, float dist, LayerMask mask)
	{
		RaycastHit2D result = Physics2D.BoxCast(start, size, 0f, dir, dist, mask);
		Debug.DrawRay(start - (Vector3)(0.5f * size), size.x * Vector2.right, Color.green);
		Debug.DrawRay(start - (Vector3)(0.5f * size), size.y * Vector2.up, Color.green);
		Debug.DrawRay(start + (Vector3)(0.5f * size), size.y * Vector2.down, Color.green);
		Debug.DrawRay(start + (Vector3)(0.5f * size), size.x * Vector2.left, Color.green);
		Debug.DrawRay(start - (Vector3)(0.5f * size) + (Vector3)(dir * dist), size.x * Vector2.right, Color.red);
		Debug.DrawRay(start - (Vector3)(0.5f * size) + (Vector3)(dir * dist), size.y * Vector2.up, Color.red);
		Debug.DrawRay(start + (Vector3)(0.5f * size) + (Vector3)(dir * dist), size.y * Vector2.down, Color.red);
		Debug.DrawRay(start + (Vector3)(0.5f * size) + (Vector3)(dir * dist), size.x * Vector2.left, Color.red);
		return result;
	}

	public static string GetSteamName(ulong id) {
		return SteamFriends.GetFriendPersonaName(new CSteamID(id));
	}

	public static string GetLocalSteamName() {
		return SteamFriends.GetFriendPersonaName(new CSteamID(SteamUser.GetSteamID().m_SteamID));
	}
}
