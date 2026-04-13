using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
	[SerializeField]
	private CompositeCollider2D worldCollision;

	[SerializeField]
	private bool cameraFollowLevel;
	[SerializeField]
	private Vector2 cameraXBounds;

	[SerializeField]
	private GameObject[] parallaxLayers;

	[SerializeField] 
	private List<GameObject> spawnPoints;
	public List<GameObject> SpawnPoints => spawnPoints;

	public bool IsPointInGeometry(Vector2 point)
	{
		return worldCollision.OverlapPoint(point);
	}

	public bool ShouldCameraFollow()
	{
		return cameraFollowLevel;
	}

	public void BoundsPosition(ref Vector3 pos)
	{
		if (pos.x < cameraXBounds.x)
		{
			pos.x = cameraXBounds.x;
		} else if (pos.x > cameraXBounds.y)
		{
			pos.x = cameraXBounds.y;
		}
	}

    void LateUpdate()
    {
        if (parallaxLayers != null)
		{
			for (int i = 0; i < parallaxLayers.Length; i++)
			{
				Vector3 pos = parallaxLayers[i].transform.position;
				pos.x = Camera.main.transform.position.x;
				parallaxLayers[i].transform.position = pos;
			}
		}
    }
}
