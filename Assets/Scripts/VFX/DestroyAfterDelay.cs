using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
	[SerializeField]
	private float lifetime;

	private void Start()
	{
		lifetime += Time.time;
	}

	public void Init(float lifetime)
	{
		this.lifetime = lifetime;
	}

	private void FixedUpdate()
	{
		if (Time.time > lifetime)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
