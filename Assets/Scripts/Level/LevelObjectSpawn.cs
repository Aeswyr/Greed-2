using UnityEngine;

public class LevelObjectSpawn : MonoBehaviour
{
	[SerializeField]
	private float noSpawnChance;
	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private GameObject[] spawnables;

	[SerializeField]
	private string data;
	private int index = -1;

	public GameObject GetSpawn()
	{
		if (spawnables != null && spawnables.Length != 0)
		{
			index = Random.Range(0, spawnables.Length);
			return spawnables[index];
		}
		return null;
	}

	public bool ShouldCancelSpawn() {
		return Random.Range(0, 1f) < noSpawnChance;
	}

	public int GetSpawnedIndex() {
		return index;
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
