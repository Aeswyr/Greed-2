using UnityEngine;

public class AlphaDecay : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private AnimationCurve alpha;

	private float startTime;
	private float duration;

	private void Start()
	{
		startTime = Time.time;
		if (duration == 0) {
			duration = 1;
		}
	}

	public void SetDuration(float duration) {
		this.duration = duration;
	}

	private void FixedUpdate()
	{
		Color color = sprite.color;
		color.a = alpha.Evaluate((Time.time - startTime) / duration);
		sprite.color = color;
	}
}
