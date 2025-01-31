using UnityEngine;

public class LevelObject : MonoBehaviour
{
	private void Start()
	{
		transform.SetParent(GameManager.Instance.GetLevelObjectRoot());
	}
}
