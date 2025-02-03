using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
	[SerializeField]
	private CompositeCollider2D worldCollision;

	[SerializeField] 
	private List<GameObject> spawnPoints;
	public List<GameObject> SpawnPoints => spawnPoints;
	public bool IsPointInGeometry(Vector2 point)
	{
		return worldCollision.OverlapPoint(point);
	}
}
