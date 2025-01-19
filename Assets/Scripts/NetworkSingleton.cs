using Mirror;
using UnityEngine;

public class NetworkSingleton<T> : NetworkBehaviour where T : Component
{
	private static T instance;

	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Object.FindObjectOfType<T>();
			}
			return instance;
		}
	}
}
