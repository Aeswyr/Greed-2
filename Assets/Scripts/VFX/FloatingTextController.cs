using TMPro;
using UnityEngine;

public class FloatingTextController : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro text;

	[SerializeField]
	private AnimationCurve scale;

	[SerializeField]
	private AnimationCurve alpha;

	[SerializeField]
	private AnimationCurve yPos;

	private float startTime;

	private Vector3 basePos;

	private Vector3 baseScale;

	private void Start()
	{
		startTime = Time.time;
		basePos = transform.position;
		baseScale = transform.localScale;
	}

	public void SetText(string text, Color color)
	{
		this.text.text = text;
		this.text.color = color;
	}

	private void FixedUpdate()
	{
		float num = Time.time - startTime;
		if (num >= scale.keys[scale.keys.Length - 1].time || num >= yPos.keys[yPos.keys.Length - 1].time)
		{
			Destroy(gameObject);
			return;
		}
		transform.position = basePos + yPos.Evaluate(num) * Vector3.up;
		transform.localScale = scale.Evaluate(num) * baseScale;
		Color color = text.color;
		color.a = alpha.Evaluate(num);
		text.color = color;
	}
}
