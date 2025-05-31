using UnityEngine;

[CreateAssetMenu(fileName = "AnimationLibrary", menuName = "ScriptableObjects/AnimationLibrary", order = 2)]
public class AnimationLibrary : ScriptableObject
{
	[SerializeField]
	private AnimationClip[] anims;

	public AnimationClip this[int i] => anims[i];

	public int Length => anims.Length;
}
