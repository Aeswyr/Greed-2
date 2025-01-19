using UnityEngine;

public class LevelController : MonoBehaviour
{
	[SerializeField]
	private CompositeCollider2D worldCollision;

	public bool IsPointInGeometry(Vector2 point)
	{
		return worldCollision.OverlapPoint(point);
	}
}
