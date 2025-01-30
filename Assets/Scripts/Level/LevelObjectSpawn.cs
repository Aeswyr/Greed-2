using UnityEngine;

public class LevelObjectSpawn : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private GameObject[] spawnables;

	[SerializeField]
	private string data;

	public GameObject GetSpawn()
	{
		if (spawnables != null && spawnables.Length != 0)
		{
			return spawnables[Random.Range(0, spawnables.Length)];
		}
		return null;
	}

	public string GetExtraData()
	{
		return data;
	}

	private void OnValidate()
	{
		if (spawnables != null && spawnables.Length > 0 && spawnables[0].TryGetComponent(out SpriteRenderer render)) {
			sprite.sprite = render.sprite;
		}
	}
}
