using UnityEngine;

public class LevelObject : MonoBehaviour
{
	private void Start()
	{
		base.transform.SetParent(NetworkSingleton<GameManager>.Instance.GetLevelObjectRoot());
	}
}
