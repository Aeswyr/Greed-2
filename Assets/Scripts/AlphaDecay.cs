using UnityEngine;

public class AlphaDecay : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private AnimationCurve alpha;

	private float startTime;

	private void Start()
	{
		startTime = Time.time;
	}

	private void FixedUpdate()
	{
		Color color = sprite.color;
		color.a = alpha.Evaluate(Time.time - startTime);
		sprite.color = color;
	}
}
