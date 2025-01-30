using UnityEngine;

public class LevelObject : MonoBehaviour
{
	private void Start()
	{
		base.transform.SetParent(GameManager.Instance.GetLevelObjectRoot());
	}
}
